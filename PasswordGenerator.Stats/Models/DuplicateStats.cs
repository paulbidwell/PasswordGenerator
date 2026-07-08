namespace PasswordGenerator.Stats.Models;

public record DuplicateStats(
    int TotalGenerated,
    int UniqueCount,
    int DuplicateCount,
    double DuplicatePercent,
    int MostRepeatedCount,
    string? MostRepeatedExample,
    IReadOnlyDictionary<int, int> OccurrenceHistogram);
