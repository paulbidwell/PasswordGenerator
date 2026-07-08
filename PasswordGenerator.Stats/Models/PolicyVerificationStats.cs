namespace PasswordGenerator.Stats.Models;

public record PolicyVerificationStats(
    int TotalChecked,
    int PassedCount,
    int FailedCount,
    IReadOnlyDictionary<string, int> RuleViolationCounts,
    IReadOnlyList<PolicyViolation> Violations);

public record PolicyViolation(
    int Index,
    string Password,
    IReadOnlyList<string> Rules);
