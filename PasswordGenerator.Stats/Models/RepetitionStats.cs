namespace PasswordGenerator.Stats.Models;

public record RepetitionStats(
    int MinConsecutive,
    int MaxConsecutive,
    double AverageConsecutive,
    IReadOnlyDictionary<int, int> Histogram,
    bool AllWithinConstraint,
    int ConfiguredMaxRepetition);
