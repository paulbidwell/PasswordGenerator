using PasswordGenerator.Stats.Models;
using System.Globalization;
using System.Text;

namespace PasswordGenerator.Stats.Reporting;

public static class CsvReporter
{
    public static void Write(GenerationReport report, string path)
    {
        var sb = new StringBuilder();

        WriteSummary(sb, report);

        WriteCategoryFrequency(sb, report.FrequencyStats);

        WriteCharacterCounts(sb, report.FrequencyStats);

        WriteCoverage(sb, report.FrequencyStats);

        WriteRepetition(sb, report.RepetitionStats);

        WriteEntropy(sb, report.EntropyStats, report.SampleSize);

        WriteWarnings(sb, report.EntropyStats);

        WriteDuplicates(sb, report.DuplicateStats);

        WritePolicyVerification(sb, report.PolicyVerification);

        File.WriteAllText(path, sb.ToString());
    }

    private static void WriteSummary(StringBuilder sb, GenerationReport report)
    {
        sb.AppendLine("[Summary]");
        sb.AppendLine("Metric,Value");
        sb.AppendLine(Csv("Mode", report.Mode));
        sb.AppendLine(Csv("SampleSize", report.SampleSize));
        sb.AppendLine(Csv("ElapsedMs", report.ElapsedMilliseconds));
        sb.AppendLine(Csv("ChiSquared", report.FrequencyStats.ChiSquaredStatistic.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("ChiSquaredPValue", report.FrequencyStats.ChiSquaredPValue.ToString("F6", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("MinConsecutiveRepetition", report.RepetitionStats.MinConsecutive));
        sb.AppendLine(Csv("MaxConsecutiveRepetition", report.RepetitionStats.MaxConsecutive));
        sb.AppendLine(Csv("AvgConsecutiveRepetition", report.RepetitionStats.AverageConsecutive.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("RepetitionConstraintHonoured", report.RepetitionStats.AllWithinConstraint));
        sb.AppendLine(Csv("EntropyAvg", report.EntropyStats.AverageEntropy.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("EntropyMin", report.EntropyStats.MinEntropy.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("EntropyMax", report.EntropyStats.MaxEntropy.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("EntropyStdDev", report.EntropyStats.StdDevEntropy.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("DuplicateCount", report.DuplicateStats.DuplicateCount));
        sb.AppendLine(Csv("DuplicatePercent", report.DuplicateStats.DuplicatePercent.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("PolicyVerificationPassed", report.PolicyVerification.PassedCount));
        sb.AppendLine(Csv("PolicyVerificationFailed", report.PolicyVerification.FailedCount));
        sb.AppendLine();
    }

    private static void WriteCategoryFrequency(StringBuilder sb, FrequencyStats stats)
    {
        sb.AppendLine("[CategoryFrequency]");
        sb.AppendLine("Category,Count,ExpectedPct,ActualPct,DeviationPct,Pass");

        foreach (var (_, cat) in stats.CategoryDistribution)
        {
            var deviation = Math.Abs(cat.ActualPercent - cat.ExpectedPercent);
            sb.AppendLine(string.Join(',',
                Escape(cat.Category),
                cat.Count,
                cat.ExpectedPercent.ToString("F4", CultureInfo.InvariantCulture),
                cat.ActualPercent.ToString("F4", CultureInfo.InvariantCulture),
                deviation.ToString("F4", CultureInfo.InvariantCulture),
                deviation < 5.0));
        }

        sb.AppendLine();
    }

    private static void WriteCharacterCounts(StringBuilder sb, FrequencyStats stats)
    {
        sb.AppendLine("[CharacterCounts]");
        sb.AppendLine("Character,Count,Pct");

        foreach (var (ch, count) in stats.CharacterCounts.OrderByDescending(p => p.Value))
        {
            var pct = stats.TotalCharacters > 0
                ? 100.0 * count / stats.TotalCharacters
                : 0;
            sb.AppendLine(string.Join(',',
                Escape(ch.ToString()),
                count,
                pct.ToString("F4", CultureInfo.InvariantCulture)));
        }

        sb.AppendLine();
    }

    private static void WriteCoverage(StringBuilder sb, FrequencyStats stats)
    {
        sb.AppendLine("[CharacterSetCoverage]");
        sb.AppendLine("SetIndex,Label,UsedChars,TotalChars,CoveragePct,FullCoverage");

        foreach (var cov in stats.CharacterSetCoverage)
        {
            var pct = cov.TotalChars > 0 ? 100.0 * cov.UsedChars / cov.TotalChars : 0;
            sb.AppendLine(string.Join(',',
                cov.SetIndex,
                Escape(cov.Label),
                cov.UsedChars,
                cov.TotalChars,
                pct.ToString("F2", CultureInfo.InvariantCulture),
                cov.UsedChars == cov.TotalChars));
        }

        sb.AppendLine();
    }

    private static void WriteRepetition(StringBuilder sb, RepetitionStats stats)
    {
        sb.AppendLine("[RepetitionHistogram]");
        sb.AppendLine("MaxConsecutiveRun,PasswordCount");

        foreach (var (run, count) in stats.Histogram.OrderBy(p => p.Key))
            sb.AppendLine($"{run},{count}");

        sb.AppendLine();
    }

    private static void WriteEntropy(StringBuilder sb, EntropyStats stats, int sampleSize)
    {
        sb.AppendLine("[StrengthDistribution]");
        sb.AppendLine("Strength,Count,Pct");

        foreach (var (strength, count) in stats.StrengthDistribution)
        {
            var pct = sampleSize > 0 ? 100.0 * count / sampleSize : 0;
            sb.AppendLine(string.Join(',',
                strength,
                count,
                pct.ToString("F4", CultureInfo.InvariantCulture)));
        }

        sb.AppendLine();
    }

    private static void WriteWarnings(StringBuilder sb, EntropyStats stats)
    {
        if (stats.WarningCounts.Count == 0)
            return;

        sb.AppendLine("[Warnings]");
        sb.AppendLine("Warning,Count");

        foreach (var (warning, count) in stats.WarningCounts)
            sb.AppendLine($"{Escape(warning)},{count}");

        sb.AppendLine();
    }

    private static void WriteDuplicates(StringBuilder sb, DuplicateStats stats)
    {
        sb.AppendLine("[Duplicates]");
        sb.AppendLine("Metric,Value");
        sb.AppendLine(Csv("TotalGenerated", stats.TotalGenerated));
        sb.AppendLine(Csv("UniqueCount", stats.UniqueCount));
        sb.AppendLine(Csv("DuplicateCount", stats.DuplicateCount));
        sb.AppendLine(Csv("DuplicatePercent", stats.DuplicatePercent.ToString("F4", CultureInfo.InvariantCulture)));
        sb.AppendLine(Csv("MostRepeatedCount", stats.MostRepeatedCount));
        sb.AppendLine();

        if (stats.OccurrenceHistogram.Count > 1)
        {
            sb.AppendLine("[DuplicateOccurrenceHistogram]");
            sb.AppendLine("TimesSeen,DistinctPasswords");
            foreach (var (occurrences, count) in stats.OccurrenceHistogram)
                sb.AppendLine($"{occurrences},{count}");
            sb.AppendLine();
        }
    }

    private static void WritePolicyVerification(StringBuilder sb, PolicyVerificationStats stats)
    {
        sb.AppendLine("[PolicyVerification]");
        sb.AppendLine("Metric,Value");
        sb.AppendLine(Csv("TotalChecked", stats.TotalChecked));
        sb.AppendLine(Csv("Passed", stats.PassedCount));
        sb.AppendLine(Csv("Failed", stats.FailedCount));
        sb.AppendLine();

        if (stats.RuleViolationCounts.Count > 0)
        {
            sb.AppendLine("[PolicyRuleViolations]");
            sb.AppendLine("Rule,Count");
            foreach (var (rule, count) in stats.RuleViolationCounts.OrderByDescending(kv => kv.Value))
                sb.AppendLine($"{Escape(rule)},{count}");
            sb.AppendLine();
        }
    }

    private static string Csv(string key, object value) => $"{Escape(key)},{Escape(value.ToString() ?? string.Empty)}";

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
