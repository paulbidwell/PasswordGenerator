using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Strength;
using PasswordGenerator.Strength.Detectors;

namespace PasswordGenerator.Strength;

/// <summary>Estimates password entropy and classifies strength, with optional pattern-based penalties.</summary>
public class PasswordStrengthAnalyzer : IPasswordStrengthAnalyzer
{
    private const int LowercasePoolSize = 26;
    private const int UppercasePoolSize = 26;
    private const int DigitPoolSize = 10;
    private const int AsciiSymbolPoolSize = 33;
    private const int ExtendedUnicodePoolSize = 128;

    /// <inheritdoc />
    public PasswordStrengthResult Analyze(string password)
        => Analyze(password, null);

    /// <inheritdoc />
    public PasswordStrengthResult Analyze(string password, IGeneratorConfig? config)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (password.Length == 0)
            return new PasswordStrengthResult(0, PasswordStrength.VeryWeak, 0, []);

        var poolSize = config is not null
            ? ComputePoolSizeFromConfig(config)
            : InferPoolSize(password);

        var entropyBits = password.Length * Math.Log2(Math.Max(poolSize, 1));

        var warnings = new List<string>();
        var totalPenalty = 0.0;

        ApplyDetection(SequenceDetector.Detect(password), warnings, ref totalPenalty);
        ApplyDetection(RepeatDetector.Detect(password), warnings, ref totalPenalty);
        ApplyDetection(KeyboardPatternDetector.Detect(password), warnings, ref totalPenalty);

        var adjustedEntropy = Math.Max(entropyBits - totalPenalty, 0);
        var strength = ClassifyStrength(adjustedEntropy);
        var guessesLog10 = adjustedEntropy * Math.Log10(2);

        return new PasswordStrengthResult(
            Math.Round(adjustedEntropy, 2),
            strength,
            Math.Round(guessesLog10, 2),
            warnings);
    }

    private static void ApplyDetection(DetectionResult result, List<string> warnings, ref double totalPenalty)
    {
        totalPenalty += result.PenaltyBits;

        if (result.Warning is not null)
            warnings.Add(result.Warning);
    }

    private static int InferPoolSize(ReadOnlySpan<char> password)
    {
        var hasLower = false;
        var hasUpper = false;
        var hasDigit = false;
        var hasAsciiSymbol = false;
        var hasExtended = false;

        foreach (var c in password)
        {
            if (char.IsAsciiLetterLower(c)) hasLower = true;
            else if (char.IsAsciiLetterUpper(c)) hasUpper = true;
            else if (char.IsAsciiDigit(c)) hasDigit = true;
            else if (c <= 127) hasAsciiSymbol = true;
            else hasExtended = true;
        }

        var pool = 0;
        if (hasLower) pool += LowercasePoolSize;
        if (hasUpper) pool += UppercasePoolSize;
        if (hasDigit) pool += DigitPoolSize;
        if (hasAsciiSymbol) pool += AsciiSymbolPoolSize;
        if (hasExtended) pool += ExtendedUnicodePoolSize;

        return pool;
    }

    private static int ComputePoolSizeFromConfig(IGeneratorConfig config)
    {
        var uniqueChars = new HashSet<char>();

        foreach (var characterSet in config.CharacterSets)
        {
            foreach (var c in characterSet.Characters)
                uniqueChars.Add(c);
        }

        return uniqueChars.Count > 0 ? uniqueChars.Count : 1;
    }

    private static PasswordStrength ClassifyStrength(double entropyBits) => entropyBits switch
    {
        < 28 => PasswordStrength.VeryWeak,
        < 36 => PasswordStrength.Weak,
        < 60 => PasswordStrength.Fair,
        < 128 => PasswordStrength.Strong,
        _ => PasswordStrength.VeryStrong
    };
}
