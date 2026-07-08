using PasswordGenerator.Core.Interfaces.Strength;

namespace PasswordGenerator.Stats.Models;

public record EntropyStats(
    double MinEntropy,
    double MaxEntropy,
    double AverageEntropy,
    double StdDevEntropy,
    IReadOnlyDictionary<PasswordStrength, int> StrengthDistribution,
    IReadOnlyDictionary<string, int> WarningCounts,
    IReadOnlyList<PasswordDiagnostic> WarnedPasswords);
