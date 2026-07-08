namespace PasswordGenerator.Stats.Models;

public record FrequencyStats(
    IReadOnlyDictionary<char, int> CharacterCounts,
    int TotalCharacters,
    IReadOnlyDictionary<string, CategoryFrequency> CategoryDistribution,
    IReadOnlyList<CharacterSetCoverage> CharacterSetCoverage,
    double ChiSquaredStatistic,
    double ChiSquaredPValue);

public record CategoryFrequency(
    string Category,
    int Count,
    double ActualPercent,
    double ExpectedPercent);

public record CharacterSetCoverage(
    int SetIndex,
    string Label,
    int TotalChars,
    int UsedChars);
