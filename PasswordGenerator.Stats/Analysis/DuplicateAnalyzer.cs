using PasswordGenerator.Stats.Models;

namespace PasswordGenerator.Stats.Analysis;

public static class DuplicateAnalyzer
{
    public static DuplicateStats Analyze(IReadOnlyList<string> passwords)
    {
        if (passwords.Count == 0)
        {
            return new DuplicateStats(
                TotalGenerated: 0,
                UniqueCount: 0,
                DuplicateCount: 0,
                DuplicatePercent: 0,
                MostRepeatedCount: 0,
                MostRepeatedExample: null,
                OccurrenceHistogram: new Dictionary<int, int>());
        }

        var frequencyMap = new Dictionary<string, int>(passwords.Count);
        foreach (var password in passwords)
            frequencyMap[password] = frequencyMap.TryGetValue(password, out var n) ? n + 1 : 1;

        var uniqueCount = frequencyMap.Count;
        var duplicateCount = passwords.Count - uniqueCount;
        var duplicatePercent = 100.0 * duplicateCount / passwords.Count;

        var mostRepeatedCount = 0;
        string? mostRepeatedExample = null;

        foreach (var (password, count) in frequencyMap)
        {
            if (count > mostRepeatedCount)
            {
                mostRepeatedCount = count;
                mostRepeatedExample = password;
            }
        }

        if (mostRepeatedCount <= 1)
            mostRepeatedExample = null;

        var occurrenceHistogram = new SortedDictionary<int, int>();
        foreach (var count in frequencyMap.Values)
            occurrenceHistogram[count] = occurrenceHistogram.TryGetValue(count, out var n) ? n + 1 : 1;

        return new DuplicateStats(
            TotalGenerated: passwords.Count,
            UniqueCount: uniqueCount,
            DuplicateCount: duplicateCount,
            DuplicatePercent: Math.Round(duplicatePercent, 4),
            MostRepeatedCount: mostRepeatedCount,
            MostRepeatedExample: mostRepeatedExample,
            OccurrenceHistogram: occurrenceHistogram);
    }
}
