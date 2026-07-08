using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators;

/// <summary>Generates cryptographically random numeric PINs.</summary>
public class PinGenerator : IPinGenerator
{
    private readonly IPinGeneratorConfig config;
    private readonly IRandomNumberGenerator randomNumberGenerator;
    private readonly IPinTrivialPatternDetector trivialPatternDetector;

    /// <summary>Initialises a new <see cref="PinGenerator"/> and validates the configuration eagerly.</summary>
    public PinGenerator(
        IPinGeneratorConfig config,
        IPinConfigurationValidator validator,
        IRandomNumberGenerator randomNumberGenerator,
        IPinTrivialPatternDetector trivialPatternDetector)
    {
        this.config = config;
        this.randomNumberGenerator = randomNumberGenerator;
        this.trivialPatternDetector = trivialPatternDetector;

        validator.Validate(config);
    }

    /// <inheritdoc />
    public string Generate()
    {
        var buffer = new char[config.Length];
        try
        {
            var attempts = 0;
            while (true)
            {
                FillWithRandomDigits(buffer);

                if (!config.RejectTrivialPatterns || !trivialPatternDetector.IsTrivial(buffer))
                    return new string(buffer);

                if (++attempts >= config.MaxRetries)
                {
                    throw new InvalidOperationException(
                        $"Could not generate a non-trivial PIN within {config.MaxRetries} attempts. " +
                        "Consider increasing MaxRetries or PIN length.");
                }
            }
        }
        finally
        {
            Array.Clear(buffer);
        }
    }

    /// <inheritdoc />
    public BatchResult GenerateBatch(int count)
        => BatchHelper.GenerateUniqueBatch(
            count,
            Generate,
            "PINs",
            "Consider increasing PIN length.");

    private void FillWithRandomDigits(char[] buffer)
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)('0' + randomNumberGenerator.GetRandomIntInRange(0, 9));
        }
    }
}
