using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Strength.Detectors;

namespace PasswordGenerator.Generators;

/// <summary>Generates cryptographically random passwords from configured character sets.</summary>
public class Generator : IGenerator
{
    private const int MaxPatternRetries = 100;

    private readonly IGeneratorConfig config;
    private readonly ICharacterGenerator characterGenerator;

    /// <summary>Initialises a new <see cref="Generator"/> and validates the configuration eagerly.</summary>
    public Generator(IGeneratorConfig config, IConfigurationValidator validator, ICharacterGenerator characterGenerator)
    {
        this.config = config;
        this.characterGenerator = characterGenerator;

        validator.Validate(config);
    }

    /// <inheritdoc />
    public string Generate()
    {
        var maxAttempts = config.AllowSequences ? 1 : MaxPatternRetries;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var generated = characterGenerator.GeneratePassword();
            try
            {
                if (config.AllowSequences || !HasUnwantedPatterns(generated))
                    return new string(generated);
            }
            finally
            {
                Array.Clear(generated);
            }
        }

        throw new InvalidOperationException(
            $"Could not generate a password without unwanted patterns within {MaxPatternRetries} attempts. " +
            "Consider increasing password length or relaxing pattern constraints.");
    }

    /// <inheritdoc />
    public BatchResult GenerateBatch(int count)
        => BatchHelper.GenerateUniqueBatch(
            count,
            Generate,
            "passwords",
            "Consider increasing password length or expanding character sets.");

    private static bool HasUnwantedPatterns(char[] password)
    {
        ReadOnlySpan<char> span = password;
        return SequenceDetector.Detect(span).Warning is not null
            || KeyboardPatternDetector.Detect(span).Warning is not null;
    }
}