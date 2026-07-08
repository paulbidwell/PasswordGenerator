using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Generators;

namespace PasswordGenerator;

/// <summary>
/// Fluent builder that configures and creates an <see cref="IPinGenerator"/>
/// without requiring a dependency-injection container.
/// </summary>
public sealed class PinGeneratorBuilder
{
    private readonly PinGeneratorOptions _options = new();

    /// <summary>
    /// Creates a new builder initialised with the default
    /// <see cref="PinGeneratorOptions"/> values.
    /// </summary>
    public static PinGeneratorBuilder Create() => new();

    public PinGeneratorBuilder WithLength(int length)
    {
        _options.Length = length;
        return this;
    }

    public PinGeneratorBuilder RejectTrivialPatterns(bool reject = true)
    {
        _options.RejectTrivialPatterns = reject;
        return this;
    }

    public PinGeneratorBuilder WithMaxRetries(int maxRetries)
    {
        _options.MaxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Creates a fully wired <see cref="IPinGenerator"/> using the current
    /// builder state. The returned generator is independent — further
    /// mutations on this builder do not affect it.
    /// </summary>
    public IPinGenerator Build()
    {
        var snapshot = new PinGeneratorOptions
        {
            Length = _options.Length,
            RejectTrivialPatterns = _options.RejectTrivialPatterns,
            MaxRetries = _options.MaxRetries,
        };

        var optionsManager = new DirectPinOptionsManager(snapshot);
        var rng = new SecureRng();
        var config = new PinGeneratorConfig(optionsManager);
        var validator = new PinConfigurationValidator();
        var trivialPatternDetector = new PinTrivialPatternDetector();

        return new PinGenerator(config, validator, rng, trivialPatternDetector);
    }
}
