using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Generators;
using PasswordGenerator.Sets;
using Xunit;

namespace PasswordGenerator.Tests;

public class PositionConstraintEnforcerTests
{
    private readonly PositionConstraintEnforcer _enforcer = new();

    [Fact]
    public void Enforce_MustStartWithLetter_SwapsFirstCharWhenNeeded()
    {
        var buffer = "1abcde".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 6,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            CharacterSets = [new FakeCharacterSet { Set = "abcde12345".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.True(char.IsLetter(buffer[0]));
        Assert.Equal(6, buffer.Length);
    }

    [Fact]
    public void Enforce_MustStartWithLetter_NoSwapWhenAlreadySatisfied()
    {
        var buffer = "Abcde1".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 6,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            CharacterSets = [new FakeCharacterSet { Set = "Abcde12345".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.Equal('A', buffer[0]);
    }

    [Fact]
    public void Enforce_ExcludeLeadingTrailingSymbols_FixesBothEnds()
    {
        var buffer = "!abc@".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 5,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "abc!@".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.True(char.IsLetterOrDigit(buffer[0]));
        Assert.True(char.IsLetterOrDigit(buffer[^1]));
    }

    [Fact]
    public void Enforce_ExcludeLeadingTrailingSymbols_NoSwapWhenAlreadySatisfied()
    {
        var buffer = "a!b!c".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 5,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "abc!@".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.Equal('a', buffer[0]);
        Assert.Equal('c', buffer[^1]);
    }

    [Fact]
    public void Enforce_BothConstraints_FirstIsLetterAndBoundariesAreAlphanumeric()
    {
        var buffer = "!1abc@".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 6,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "abc12!@".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.True(char.IsLetter(buffer[0]));
        Assert.True(char.IsLetterOrDigit(buffer[^1]));
    }

    [Fact]
    public void Enforce_EmptyBuffer_DoesNotThrow()
    {
        var buffer = Array.Empty<char>();
        var config = new FakeGeneratorConfig
        {
            Length = 0,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            CharacterSets = []
        };

        _enforcer.Enforce(buffer, config);
    }

    [Fact]
    public void Enforce_SingleCharBuffer_MustStartWithLetter_SwapsIfNeeded()
    {
        var buffer = "a".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 1,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            CharacterSets = [new FakeCharacterSet { Set = "a".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.True(char.IsLetter(buffer[0]));
    }

    [Fact]
    public void Enforce_PreservesAllCharacters()
    {
        var buffer = "!abc@".ToCharArray();
        var original = buffer.OrderBy(c => c).ToArray();
        var config = new FakeGeneratorConfig
        {
            Length = 5,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "abc!@".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.Equal(original, buffer.OrderBy(c => c).ToArray());
    }

    [Fact]
    public void Enforce_NoConstraints_BufferUnchanged()
    {
        var buffer = "!abc@".ToCharArray();
        var original = (char[])buffer.Clone();
        var config = new FakeGeneratorConfig
        {
            Length = 5,
            MaxRepetition = -1,
            AllowSequences = true,
            CharacterSets = [new FakeCharacterSet { Set = "abc!@".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.Equal(original, buffer);
    }

    [Fact]
    public void Enforce_ExcludeLeadingTrailingSymbols_AllSymbolsExceptFirst_EndUnchanged()
    {
        var buffer = "a!@#".ToCharArray();
        var config = new FakeGeneratorConfig
        {
            Length = 4,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "a!@#".ToCharArray(), Min = 0 }]
        };

        _enforcer.Enforce(buffer, config);

        Assert.Equal('a', buffer[0]);

        Assert.Equal('#', buffer[^1]);
    }
}

public class PositionConstraintValidatorTests
{
    [Fact]
    public void Validate_MustStartWithLetter_NoLetters_ThrowsArgumentException()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 10,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            CharacterSets = [new FakeCharacterSet { Set = "0123456789".ToCharArray(), Min = 1 }]
        };

        var validator = new ConfigurationValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate(config));
    }

    [Fact]
    public void Validate_MustStartWithLetter_WithLetters_DoesNotThrow()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 10,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            CharacterSets =
            [
                new FakeCharacterSet { Set = "ABCDEFGHIJ".ToCharArray(), Min = 1 },
                new FakeCharacterSet { Set = "0123456789".ToCharArray(), Min = 1 }
            ]
        };

        var validator = new ConfigurationValidator();
        validator.Validate(config);
    }

    [Fact]
    public void Validate_ExcludeLeadingTrailingSymbols_OnlySymbols_ThrowsArgumentException()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 10,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "!@#$%^&*()".ToCharArray(), Min = 1 }]
        };

        var validator = new ConfigurationValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate(config));
    }

    [Fact]
    public void Validate_ExcludeLeadingTrailingSymbols_OnlyOneAlphanumeric_Length2_ThrowsArgumentException()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 2,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "A!".ToCharArray(), Min = 0 }]
        };

        var validator = new ConfigurationValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate(config));
    }

    [Fact]
    public void Validate_ExcludeLeadingTrailingSymbols_TwoAlphanumeric_DoesNotThrow()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 5,
            MaxRepetition = -1,
            AllowSequences = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets =
            [
                new FakeCharacterSet { Set = "ABCDE".ToCharArray(), Min = 1 },
                new FakeCharacterSet { Set = "!@#".ToCharArray(), Min = 1 }
            ]
        };

        var validator = new ConfigurationValidator();
        validator.Validate(config);
    }

    [Fact]
    public void Validate_BothConstraints_SymbolsOnly_ThrowsArgumentException()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 10,
            MaxRepetition = -1,
            AllowSequences = true,
            MustStartWithLetter = true,
            ExcludeLeadingTrailingSymbols = true,
            CharacterSets = [new FakeCharacterSet { Set = "!@#$%^".ToCharArray(), Min = 1 }]
        };

        var validator = new ConfigurationValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate(config));
    }
}

public class PositionConstraintIntegrationTests
{
    [Fact]
    public void Generate_MustStartWithLetter_AlwaysStartsWithLetter()
    {
        using var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddPasswordGenerator(o =>
            {
                o.Length = 20;
                o.MustStartWithLetter = true;
            })
            .BuildServiceProvider();

        var generator = provider.GetRequiredService<IGenerator>();

        for (var i = 0; i < 50; i++)
        {
            var password = generator.Generate();
            Assert.True(char.IsLetter(password[0]),
                $"Password '{password}' does not start with a letter.");
        }
    }

    [Fact]
    public void Generate_ExcludeLeadingTrailingSymbols_BoundariesAreAlphanumeric()
    {
        using var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddPasswordGenerator(o =>
            {
                o.Length = 20;
                o.ExcludeLeadingTrailingSymbols = true;
            })
            .BuildServiceProvider();

        var generator = provider.GetRequiredService<IGenerator>();

        for (var i = 0; i < 50; i++)
        {
            var password = generator.Generate();
            Assert.True(char.IsLetterOrDigit(password[0]),
                $"Password '{password}' starts with a symbol.");
            Assert.True(char.IsLetterOrDigit(password[^1]),
                $"Password '{password}' ends with a symbol.");
        }
    }

    [Fact]
    public void Generate_BothConstraints_StartsWithLetterAndEndsAlphanumeric()
    {
        using var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddPasswordGenerator(o =>
            {
                o.Length = 20;
                o.MustStartWithLetter = true;
                o.ExcludeLeadingTrailingSymbols = true;
            })
            .BuildServiceProvider();

        var generator = provider.GetRequiredService<IGenerator>();

        for (var i = 0; i < 50; i++)
        {
            var password = generator.Generate();
            Assert.True(char.IsLetter(password[0]),
                $"Password '{password}' does not start with a letter.");
            Assert.True(char.IsLetterOrDigit(password[^1]),
                $"Password '{password}' ends with a symbol.");
        }
    }

    [Fact]
    public void Generate_ConstraintsWithMinSymbols_StillMeetsMinimumRequirements()
    {
        using var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddPasswordGenerator(o =>
            {
                o.Length = 22;
                o.MustStartWithLetter = true;
                o.ExcludeLeadingTrailingSymbols = true;
                o.CharacterSets =
                [
                    new Sets.CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 5 },
                    new Sets.CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz", Min = 5 },
                    new Sets.CharacterSet { Characters = "0123456789", Min = 2 },
                    new Sets.CharacterSet { Characters = "!$%^&*()-_=+[]{}@#~", Min = 2 }
                ];
            })
            .BuildServiceProvider();

        var generator = provider.GetRequiredService<IGenerator>();

        for (var i = 0; i < 50; i++)
        {
            var password = generator.Generate();

            Assert.True(char.IsLetter(password[0]));
            Assert.True(char.IsLetterOrDigit(password[^1]));

            var upper = password.Count(char.IsUpper);
            var lower = password.Count(char.IsLower);
            var digit = password.Count(char.IsDigit);
            var symbol = password.Count(c => !char.IsLetterOrDigit(c));

            Assert.True(upper >= 5, $"Upper count {upper} < 5 in '{password}'");
            Assert.True(lower >= 5, $"Lower count {lower} < 5 in '{password}'");
            Assert.True(digit >= 2, $"Digit count {digit} < 2 in '{password}'");
            Assert.True(symbol >= 2, $"Symbol count {symbol} < 2 in '{password}'");
        }
    }

    [Fact]
    public void Generate_DefaultOptions_NoConstraints_StillWorks()
    {
        using var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddPasswordGenerator()
            .BuildServiceProvider();

        var generator = provider.GetRequiredService<IGenerator>();
        var password = generator.Generate();

        Assert.Equal(22, password.Length);
    }
}
