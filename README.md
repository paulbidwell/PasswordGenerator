# PasswordGenerator
A cryptographically secure password and PIN generator library for .NET.
## Overview
PasswordGenerator is a .NET 10 library for generating random passwords and numeric PINs with a cryptographically secure random number generator. It plugs into `Microsoft.Extensions.DependencyInjection` and gives you control over composition, character sets, length, repetition limits, sequence rules. There's also a PIN generator for MFA and verification flows, with trivial pattern rejection and rate limiting hooks built in.
## Project Structure
| Project | Description |
|---|---|
| `PasswordGenerator` | Main library containing the password generator, PIN generator, strength analyzer, options, and DI registration |
| `PasswordGenerator.Core` | Interfaces and shared utilities |
| `PasswordGenerator.Tests` | Unit tests (xUnit v3) |
## Getting Started
### Prerequisites
- .NET 10 SDK or later
### Building
```bash
dotnet build
```
### Running Tests
```bash
dotnet test
```
## Usage
### Fluent Builder API (No DI Required)
The fluent builder provides a standalone way to configure and create generators without a dependency-injection container; ideal for console apps, scripts, unit tests, and libraries that don't use `Microsoft.Extensions.DependencyInjection`.
#### Password Builder
```csharp
using PasswordGenerator;
// One-liner with defaults (22 chars, mixed case + digits + symbols)
string password = PasswordGeneratorBuilder.Create()
    .Build()
    .Generate();
// Customised
string password = PasswordGeneratorBuilder.Create()
    .WithLength(18)
    .WithMaxRepetition(2)
    .AllowSequences()
    .ExcludeAmbiguousCharacters()
    .MustStartWithLetter()
    .Build()
    .Generate();
// Batch generation
IReadOnlyList<string> batch = PasswordGeneratorBuilder.Create()
    .WithLength(24)
    .AsciiOnly()
    .Build()
    .GenerateBatch(50);
// Start from a compliance preset, then override
string password = PasswordGeneratorBuilder.From(PasswordPolicy.Owasp)
    .WithLength(20)
    .Build()
    .Generate();
// Custom character sets
string password = PasswordGeneratorBuilder.Create()
    .ClearCharacterSets()
    .AddCharacterSet("ABCDEFGHIJKLMNOPQRSTUVWXYZ", min: 5)
    .AddCharacterSet("0123456789", min: 5)
    .WithLength(14)
    .WithMaxRepetition(-1)
    .AllowSequences()
    .Build()
    .Generate();
```
Each call to `Build()` returns an independent `IGenerator`, further mutations on the builder do not affect previously built generators.
#### Password Builder API Reference
| Method | Description | Default |
|---|---|---|
| `WithLength(int)` | Set password length | `22` |
| `WithMaxRepetition(int)` | Max times any character may appear (`-1` = unlimited) | `1` |
| `WithUnlimitedRepetition()` | Shorthand for `WithMaxRepetition(-1)` | `1` |
| `AllowSequences(bool)` | Allow adjacent identical characters | `false` |
| `AllowUpperLowerSequences(bool)` | Treat upper/lower as distinct for sequence checks | `false` |
| `AsciiOnly(bool)` | Restrict to printable ASCII | `false` |
| `ExcludeAmbiguousCharacters(bool)` | Remove ambiguous characters (`0O1lI\|S5B8Z2`) | `false` |
| `ExcludeCharacters(string)` | Remove specific characters from all sets | `""` |
| `MustStartWithLetter(bool)` | Force the first character to be a letter | `false` |
| `ExcludeLeadingTrailingSymbols(bool)` | Force first and last characters to be alphanumeric | `false` |
| `ClearCharacterSets()` | Remove all default character sets | — |
| `AddCharacterSet(string, int)` | Add a character set with a minimum count | — |
| `Build()` | Create an `IGenerator` snapshot | — |
#### PIN Builder
```csharp
using PasswordGenerator;
string pin = PinGeneratorBuilder.Create()
    .WithLength(8)
    .RejectTrivialPatterns()
    .Build()
    .Generate();
IReadOnlyList<string> pins = PinGeneratorBuilder.Create()
    .WithLength(6)
    .Build()
    .GenerateBatch(10);
```
#### PIN Builder API Reference
| Method | Description | Default |
|---|---|---|
| `WithLength(int)` | Set PIN length (4–12) | `6` |
| `RejectTrivialPatterns(bool)` | Reject PINs like `1234`, `0000`, `1212` | `true` |
| `WithMaxRetries(int)` | Max regeneration attempts when rejecting trivial patterns | `10` |
| `Build()` | Create an `IPinGenerator` snapshot | — |
### Dependency Injection
#### Register Services
Add the password generator to your dependency injection container using the provided extension method:
```csharp
using PasswordGenerator;
// Register with default options
services.AddPasswordGenerator();
```
Or configure options at registration time:
```csharp
services.AddPasswordGenerator(options =>
{
    options.Length = 30;
    options.MaxRepetition = 2;
});
```
#### Bind from Configuration
Options can be bound directly from `appsettings.json`, environment variables, or any other `IConfiguration` source using the built-in `Microsoft.Extensions.Options` integration.
Pass a configuration section path:
```csharp
services.AddPasswordGenerator("PasswordGenerator");
```
Or pass an `IConfiguration` section instance:
```csharp
services.AddPasswordGenerator(builder.Configuration.GetSection("PasswordGenerator"));
```
With a corresponding `appsettings.json`:
```json
{
  "PasswordGenerator": {
    "Length": 32,
    "MaxRepetition": 2,
    "AllowSequences": true,
    "ExcludeAmbiguous": true
  }
}
```
#### IOptions&lt;T&gt; / IOptionsSnapshot&lt;T&gt; / IOptionsMonitor&lt;T&gt;
All registration overloads wire options through the standard `Microsoft.Extensions.Options` pipeline. This means `IOptions<PasswordGeneratorOptions>`, `IOptionsSnapshot<PasswordGeneratorOptions>`, and `IOptionsMonitor<PasswordGeneratorOptions>` are all available for injection alongside the existing `IPasswordGeneratorOptionsManager`:
```csharp
using Microsoft.Extensions.Options;
public class MyService(IOptionsSnapshot<PasswordGeneratorOptions> options)
{
    public int CurrentLength => options.Value.Length;
}
```
The existing `IPasswordGeneratorOptionsManager` continues to work and shares the same underlying options instance, so both APIs stay consistent.
### Generate a Password
Inject `IGenerator` and call `Generate()`:
```csharp
using PasswordGenerator.Core.Interfaces.Generators;
public class MyService(IGenerator generator)
{
    public string CreatePassword()
    {
        return generator.Generate();
    }
}
```
### Generate Multiple Passwords
Call `GenerateBatch(count)` to generate multiple unique passwords in a single call:
```csharp
public class MyService(IGenerator generator)
{
    public IReadOnlyList<string> CreatePasswords(int count)
    {
        return generator.GenerateBatch(count);
    }
}
```
All passwords in the returned list are guaranteed to be unique. If the configuration produces a password space too small to satisfy the requested count, an `InvalidOperationException` is thrown.
### Analyze Password Strength
Inject `IPasswordStrengthAnalyzer` to score any password, whether generated by this library or supplied by a user. This is useful for building strength meters or enforcing minimum strength requirements.
```csharp
using PasswordGenerator.Core.Interfaces.Strength;
public class MyService(IPasswordStrengthAnalyzer analyzer)
{
    public PasswordStrengthResult CheckPassword(string password)
    {
        return analyzer.Analyze(password);
    }
}
```
The returned `PasswordStrengthResult` contains:
| Property | Type | Description |
|---|---|---|
| `EntropyBits` | `double` | Estimated entropy in bits (higher is better) |
| `Strength` | `PasswordStrength` | Bucketed score: `VeryWeak`, `Weak`, `Fair`, `Strong`, or `VeryStrong` |
| `GuessesLog10` | `double` | Estimated `log₁₀(guesses)`, useful for display |
| `Warnings` | `IReadOnlyList<string>` | Human-readable notes about detected weaknesses |
#### Strength Thresholds
| Strength | Entropy Bits |
|---|---|
| `VeryWeak` | < 28 |
| `Weak` | 28 – 35 |
| `Fair` | 36 – 59 |
| `Strong` | 60 – 127 |
| `VeryStrong` | ≥ 128 |
#### Config-Aware Analysis
When you have access to the generator configuration, pass it for a more accurate entropy calculation that uses the exact character pool size instead of inferring it from the password content:
```csharp
var result = analyzer.Analyze(password, generatorConfig);
```
#### Pattern Detection
The analyzer detects and warns about common weaknesses:
- **Sequential characters** ascending or descending runs of 3+ characters (e.g., `abc`, `321`)
- **Repeated characters** runs of 3+ identical characters (e.g., `aaa`)
- **Keyboard patterns** QWERTY-adjacent key runs of 4+ characters (e.g., `qwerty`, `asdf`)
Each detected pattern reduces the effective entropy estimate and adds a human-readable warning to the result.
### Reconfigure at Runtime
Inject `IPasswordGeneratorOptionsManager` to change options after initial registration:
```csharp
optionsManager.Configure(options =>
{
    options.Length = 40;
    options.AllowSequences = true;
});
```
## Compliance Presets
The `PasswordPolicy` class provides ready-made `PasswordGeneratorOptions` instances for common compliance standards. Each property returns a fresh defensive copy, so callers can safely mutate the result without affecting other consumers.
```csharp
using PasswordGenerator;
// Use a preset with the fluent builder
string password = PasswordGeneratorBuilder.From(PasswordPolicy.Owasp)
    .WithLength(20) // extend beyond the baseline
    .Build()
    .Generate();
// Use a preset directly with DI
services.AddPasswordGenerator(PasswordPolicy.Owasp);
// Or copy and customise
var options = PasswordPolicy.Nist80063B;
options.Length = 20; // extend beyond the baseline
```
### Available Presets
| Preset | Standard | Length | Min per Set | Key Characteristics |
|---|---|---|---|---|
| `PasswordPolicy.Owasp` | OWASP ASVS 4.0.3 §V2.1 | 16 | 2 | Ambiguous excluded, no sequences, max repetition 1 |
| `PasswordPolicy.Nist80063B` | NIST SP 800-63B §5.1.1.1 | 15 | 1 | ASCII-only, unrestricted repetition, sequences allowed |
| `PasswordPolicy.Pci` | PCI DSS v4.0 Req 8.3.6 | 14 | 2 | Ambiguous excluded, no sequences, max repetition 1 |
### Preset Details
#### OWASP (ASVS 4.0.3)
Follows OWASP ASVS 4.0.3 §V2.1: at least 2 characters from each of the four categories (uppercase, lowercase, digits, special), no ambiguous characters, no sequential or repeated runs.
#### NIST SP 800-63B
Follows NIST SP 800-63B §5.1.1.1 (2017, updated 2020), which prioritizes length over composition rules. All printable ASCII is allowed, and there's no restriction on repeated or sequential characters, NIST's guidance found composition rules add little in practice.
#### PCI DSS v4.0
Follows PCI DSS v4.0 Requirement 8.3.6 (March 2022), which requires numeric and alphabetic characters with a 12-character minimum. This preset uses 14 characters plus symbols as a stricter baseline, and excludes ambiguous characters and sequences.
## Configuration Options
Options are set through the `PasswordGeneratorOptions` class.
| Property | Type | Default | Description |
|---|---|---|---|
| `Length` | `int` | `22` | Total length of the generated password |
| `MaxRepetition` | `int` | `1` | Maximum number of times a single character can appear. Use `-1` for unlimited |
| `AllowSequences` | `bool` | `false` | Allow sequential characters (e.g., `abc`, `123`) |
| `AllowUpperLowerSequences` | `bool` | `false` | Allow case-insensitive sequential characters (e.g., `aBc`) |
| `AsciiOnly` | `bool` | `false` | Restrict special character sets to printable ASCII characters only |
| `ExcludeAmbiguous` | `bool` | `false` | Remove visually ambiguous characters (`0O1lI\|S5B8Z2`) from all character sets |
| `ExcludedCharacters` | `string` | `""` | Custom characters to remove from all character sets |
| `MustStartWithLetter` | `bool` | `false` | First character of the password must be a letter (`a-z` or `A-Z`) |
| `ExcludeLeadingTrailingSymbols` | `bool` | `false` | First and last characters must not be symbols (must be a letter or digit) |
| `CharacterSets` | `List<CharacterSet>` | See below | Character pools and their minimum usage requirements |
### Default Character Sets
| Characters | Minimum |
|---|---|
| `ABCDEFGHIJKLMNOPQRSTUVWXYZ` | 5 |
| `abcdefghijklmnopqrstuvwxyz` | 5 |
| `0123456789` | 2 |
| `!$%^&*()-_=+[]{}@#~;:,.?/` | 2 |
### Custom Character Sets
Override the default character sets by replacing the `CharacterSets` list:
```csharp
services.AddPasswordGenerator(options =>
{
    options.Length = 16;
    options.CharacterSets =
    [
        new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 4 },
        new CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz", Min = 4 },
        new CharacterSet { Characters = "0123456789",                 Min = 4 },
        new CharacterSet { Characters = "!@#$%",                      Min = 2 }
    ];
});
```
The sum of all `Min` values across character sets must not exceed `Length`.
### ASCII-Only Mode
When `AsciiOnly` is set to `true`, any character set containing non-ASCII characters (code point > 127) is replaced with the standard printable ASCII special characters:
```
!"#$%&'()*+,-./:;<=>?@[\]^_`{|}~
```
### Character Exclusion
#### Ambiguous Character Exclusion
Enable `ExcludeAmbiguous` to strip visually similar characters that are easily confused when a password is read aloud or typed manually:
```csharp
services.AddPasswordGenerator(options =>
{
    options.ExcludeAmbiguous = true;
});
```
The ambiguous character set is defined as `PasswordGeneratorOptions.AmbiguousCharacters` and removes these commonly confused groups:
| Characters | Confused With |
|---|---|
| `0` / `O` | Zero vs. uppercase O |
| `1` / `l` / `I` / `\|` | One vs. lowercase L vs. uppercase I vs. pipe |
| `S` / `5` | Uppercase S vs. five |
| `B` / `8` | Uppercase B vs. eight |
| `Z` / `2` | Uppercase Z vs. two |
#### Custom Exclusion List
Use `ExcludedCharacters` to ban specific characters, useful when a target system disallows certain symbols:
```csharp
services.AddPasswordGenerator(options =>
{
    options.ExcludedCharacters = "<>\"\'";
});
```
#### Combining Exclusions
Both exclusion features compose naturally. When both are set, ambiguous characters and custom exclusions are merged into a single filter:
```csharp
services.AddPasswordGenerator(options =>
{
    options.ExcludeAmbiguous = true;
    options.ExcludedCharacters = "<>{}";
});
```
Exclusion is applied after the `AsciiOnly` transform, so all three features work together predictably.
> **Note:** if you exclude too many characters from a set, you can empty it out, and generation will fail with a descriptive error.
### Start/End Position Constraints
Some systems reject passwords that begin or end with a special character. Two options control boundary character placement:
#### MustStartWithLetter
Force the first character to be a letter, useful when a target system requires passwords to begin with an alphabetic character:
```csharp
services.AddPasswordGenerator(options =>
{
    options.MustStartWithLetter = true;
});
```
#### ExcludeLeadingTrailingSymbols
Ensure neither the first nor the last character is a symbol (both must be a letter or digit):
```csharp
services.AddPasswordGenerator(options =>
{
    options.ExcludeLeadingTrailingSymbols = true;
});
```
#### Combining Both
The two options compose naturally. When both are enabled the first character is guaranteed to be a letter and the last character is guaranteed to be a letter or digit:
```csharp
services.AddPasswordGenerator(options =>
{
    options.MustStartWithLetter = true;
    options.ExcludeLeadingTrailingSymbols = true;
});
```
Position constraints are enforced after the password is shuffled by swapping boundary characters with suitable interior characters. This preserves all character-set minimum requirements and does not reduce the password's randomness.
> **Note:** Enabling `MustStartWithLetter` requires at least one character set containing letters. Enabling `ExcludeLeadingTrailingSymbols` requires at least two letter-or-digit characters across all character sets (when password length ≥ 2). Validation will throw an `ArgumentException` if these conditions are not met.
## PIN Generator
The PIN generator produces cryptographically secure numeric-only codes suitable for MFA, verification flows, and similar scenarios.
### Register PIN Services
```csharp
using PasswordGenerator;
// Register with default options (6-digit PINs, trivial pattern rejection enabled)
services.AddPinGenerator();
```
Or configure options at registration time:
```csharp
services.AddPinGenerator(options =>
{
    options.Length = 8;
    options.RejectTrivialPatterns = true;
    options.MaxRetries = 10;
});
```
#### Bind PIN Options from Configuration
Like the password generator, PIN options can be bound from configuration:
```csharp
services.AddPinGenerator("PinGenerator");
```
Or via an `IConfiguration` section:
```csharp
services.AddPinGenerator(builder.Configuration.GetSection("PinGenerator"));
```
With a corresponding `appsettings.json`:
```json
{
  "PinGenerator": {
    "Length": 8,
    "RejectTrivialPatterns": true,
    "MaxRetries": 5
  }
}
```
`IOptions<PinGeneratorOptions>`, `IOptionsSnapshot<PinGeneratorOptions>`, and `IOptionsMonitor<PinGeneratorOptions>` are all available for direct injection.
### Generate a PIN
Inject `IPinGenerator` and call `Generate()`:
```csharp
using PasswordGenerator.Core.Interfaces.Generators;
public class VerificationService(IPinGenerator pinGenerator)
{
    public string CreateVerificationCode()
    {
        return pinGenerator.Generate(); // e.g. "384719"
    }
}
```
### Generate Multiple PINs
Call `GenerateBatch(count)` to generate multiple unique PINs:
```csharp
public class VerificationService(IPinGenerator pinGenerator)
{
    public IReadOnlyList<string> CreateBatch(int count)
    {
        return pinGenerator.GenerateBatch(count);
    }
}
```
All PINs in the returned list are guaranteed to be unique.
### PIN Configuration Options
| Property | Type | Default | Description |
|---|---|---|---|
| `Length` | `int` | `6` | Length of the generated PIN (4–12 digits) |
| `RejectTrivialPatterns` | `bool` | `true` | Automatically reject and regenerate trivial PINs |
| `MaxRetries` | `int` | `10` | Maximum regeneration attempts when rejecting trivial patterns |
### Trivial Pattern Detection
When `RejectTrivialPatterns` is enabled, the generator automatically rejects PINs matching these patterns and regenerates:
| Pattern | Examples |
|---|---|
| All same digits | `0000`, `1111`, `999999` |
| Sequential ascending | `1234`, `4567`, `012345` |
| Sequential descending | `4321`, `9876`, `543210` |
| Repeating pair | `1212`, `5656`, `373737` |
### Reconfigure at Runtime
Inject `IPinGeneratorOptionsManager` to change options after initial registration:
```csharp
pinOptionsManager.Configure(options =>
{
    options.Length = 8;
    options.RejectTrivialPatterns = false;
});
```
### Rate Limiting
The library registers an `IPinRateLimiter` for protecting PIN generation and verification endpoints. A default in-memory implementation is included:
```csharp
using PasswordGenerator.Core.Interfaces.Generators;
public class VerificationEndpoint(IPinGenerator pinGenerator, IPinRateLimiter rateLimiter)
{
    public string GenerateForUser(string userId)
    {
        if (!rateLimiter.IsAllowed(userId))
            throw new InvalidOperationException("Rate limit exceeded. Try again later.");
        rateLimiter.RecordAttempt(userId);
        return pinGenerator.Generate();
    }
}
```
The default rate limiter allows **5 attempts per 5-minute window** per key. For distributed scenarios, replace the default with your own `IPinRateLimiter` implementation (e.g., Redis-backed) via DI.
### Using Both Generators Together
The password and PIN generators can coexist in the same DI container:
```csharp
services
    .AddPasswordGenerator()
    .AddPinGenerator();
// Resolve independently
var passwordGenerator = provider.GetRequiredService<IGenerator>();
var pinGenerator = provider.GetRequiredService<IPinGenerator>();
```
## Validation
The library validates the configuration before each password generation. An `ArgumentException` is thrown if:
- Any character set is empty
- The sum of minimum character set usages exceeds the password length
- A character set minimum is negative or exceeds the password length
- `MaxRepetition` is less than `-1`
- `MustStartWithLetter` is enabled but no character set contains letters
- `ExcludeLeadingTrailingSymbols` is enabled but fewer than two letter-or-digit characters are available (when password length ≥ 2)
PIN configuration is also validated before each generation. An `ArgumentException` is thrown if:
- PIN length is less than 4 or greater than 12
- `MaxRetries` is less than 1
## License
This project is licensed under the MIT License.