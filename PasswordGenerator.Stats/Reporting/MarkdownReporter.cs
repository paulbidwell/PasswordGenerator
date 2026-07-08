using PasswordGenerator.Stats.Models;
using System.Text;

namespace PasswordGenerator.Stats.Reporting;

public static class MarkdownReporter
{
    private const string Pass = "✅";
    private const string Fail = "❌";

    public static void Write(GenerationReport report, string path)
    {
        var sb = new StringBuilder();

        WriteHeader(sb, report);
        WriteConfiguration(sb, report);
        WriteFrequency(sb, report.FrequencyStats);
        WriteRepetition(sb, report.RepetitionStats);
        WriteEntropy(sb, report.EntropyStats, report.SampleSize);
        WriteDuplicates(sb, report.DuplicateStats);
        WritePolicyVerification(sb, report.PolicyVerification);
        WriteOverallVerdict(sb, report);

        File.WriteAllText(path, sb.ToString());
    }

    private static void WriteHeader(StringBuilder sb, GenerationReport report)
    {
        sb.AppendLine("# Password Generator — Statistics Report");
        sb.AppendLine();
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine($"|--------|-------|");
        sb.AppendLine($"| Mode | **{report.Mode}** |");
        sb.AppendLine($"| Sample size | {report.SampleSize:N0} |");
        sb.AppendLine($"| Generation time | {report.ElapsedMilliseconds} ms |");
        sb.AppendLine();
    }

    private static void WriteConfiguration(StringBuilder sb, GenerationReport report)
    {
        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine("```text");
        sb.AppendLine(report.ConfigSummary);
        sb.AppendLine("```");
        sb.AppendLine();
    }

    private static void WriteFrequency(StringBuilder sb, FrequencyStats stats)
    {
        sb.AppendLine("## Character Frequency");
        sb.AppendLine();

        sb.AppendLine("| Category | Expected % | Actual % | Deviation | Status |");
        sb.AppendLine("|----------|----------:|--------:|---------:|:------:|");

        foreach (var (_, cat) in stats.CategoryDistribution)
        {
            var deviation = Math.Abs(cat.ActualPercent - cat.ExpectedPercent);
            var status = deviation < 5.0 ? Pass : Fail;
            sb.AppendLine($"| {cat.Category} | {cat.ExpectedPercent:F1} | {cat.ActualPercent:F1} | {deviation:F1} | {status} |");
        }

        sb.AppendLine();

        var chiVerdict = stats.ChiSquaredPValue > 0.01 ? $"No significant bias {Pass}" : $"Possible bias {Fail}";
        sb.AppendLine($"**Chi-squared:** {stats.ChiSquaredStatistic:F1} &nbsp;(p = {stats.ChiSquaredPValue:F4}) — {chiVerdict}");
        sb.AppendLine();

        sb.AppendLine("### Character Set Coverage");
        sb.AppendLine();
        sb.AppendLine("| Set | Used / Total | Coverage | Status |");
        sb.AppendLine("|-----|------------:|---------:|:------:|");

        foreach (var cov in stats.CharacterSetCoverage)
        {
            var pct = cov.TotalChars > 0 ? 100.0 * cov.UsedChars / cov.TotalChars : 0;
            var status = cov.UsedChars == cov.TotalChars ? Pass : Fail;
            sb.AppendLine($"| {cov.SetIndex + 1} – {cov.Label} | {cov.UsedChars}/{cov.TotalChars} | {pct:F0}% | {status} |");
        }

        sb.AppendLine();
    }

    private static void WriteRepetition(StringBuilder sb, RepetitionStats stats)
    {
        sb.AppendLine("## Repetition");
        sb.AppendLine();
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine($"|--------|------:|");
        sb.AppendLine($"| Max consecutive | {stats.MaxConsecutive} |");
        sb.AppendLine($"| Min consecutive | {stats.MinConsecutive} |");
        sb.AppendLine($"| Avg consecutive | {stats.AverageConsecutive:F2} |");

        if (stats.ConfiguredMaxRepetition >= 0)
        {
            var status = stats.AllWithinConstraint ? Pass : Fail;
            sb.AppendLine($"| Constraint (max {stats.ConfiguredMaxRepetition}) | {(stats.AllWithinConstraint ? "Honoured" : "**Violated**")} {status} |");
        }
        else
        {
            sb.AppendLine($"| Constraint | Unlimited |");
        }

        sb.AppendLine();
    }

    private static void WriteEntropy(StringBuilder sb, EntropyStats stats, int sampleSize)
    {
        sb.AppendLine("## Entropy");
        sb.AppendLine();
        sb.AppendLine($"| Statistic | Bits |");
        sb.AppendLine($"|-----------|-----:|");
        sb.AppendLine($"| Average | {stats.AverageEntropy:F2} |");
        sb.AppendLine($"| Min | {stats.MinEntropy:F2} |");
        sb.AppendLine($"| Max | {stats.MaxEntropy:F2} |");
        sb.AppendLine($"| Std Dev (σ) | {stats.StdDevEntropy:F2} |");
        sb.AppendLine();

        sb.AppendLine("### Strength Distribution");
        sb.AppendLine();
        sb.AppendLine("| Strength | Count | % |");
        sb.AppendLine("|----------|------:|--:|");

        foreach (var (strength, count) in stats.StrengthDistribution)
        {
            var pct = sampleSize > 0 ? 100.0 * count / sampleSize : 0;
            sb.AppendLine($"| {strength} | {count:N0} | {pct:F1} |");
        }

        sb.AppendLine();

        if (stats.WarningCounts.Count > 0)
        {
            sb.AppendLine("### Warnings");
            sb.AppendLine();
            foreach (var (warning, count) in stats.WarningCounts)
                sb.AppendLine($"- **{count:N0}×** {warning}");
            sb.AppendLine();
        }
    }

    private static void WriteDuplicates(StringBuilder sb, DuplicateStats stats)
    {
        sb.AppendLine("## Duplicates");
        sb.AppendLine();

        var status = stats.DuplicateCount == 0 ? Pass : Fail;

        sb.AppendLine("| Metric | Value | Status |");
        sb.AppendLine("|--------|------:|:------:|");
        sb.AppendLine($"| Total generated | {stats.TotalGenerated:N0} | |");
        sb.AppendLine($"| Unique | {stats.UniqueCount:N0} | |");
        sb.AppendLine($"| Duplicates | {stats.DuplicateCount:N0} ({stats.DuplicatePercent:F2}%) | {status} |");

        if (stats.MostRepeatedExample is not null)
            sb.AppendLine($"| Most repeated | {stats.MostRepeatedCount}\u00d7 | |");

        sb.AppendLine();
    }

    private static void WritePolicyVerification(StringBuilder sb, PolicyVerificationStats stats)
    {
        sb.AppendLine("## Policy Verification");
        sb.AppendLine();

        var overallStatus = stats.FailedCount == 0 ? Pass : Fail;

        sb.AppendLine("| Metric | Value | Status |");
        sb.AppendLine("|--------|------:|:------:|");
        sb.AppendLine($"| Checked | {stats.TotalChecked:N0} | |");
        sb.AppendLine($"| Passed | {stats.PassedCount:N0} | |");
        sb.AppendLine($"| Failed | {stats.FailedCount:N0} | {overallStatus} |");
        sb.AppendLine();

        if (stats.RuleViolationCounts.Count > 0)
        {
            sb.AppendLine("### Rule Violations");
            sb.AppendLine();
            foreach (var (rule, count) in stats.RuleViolationCounts.OrderByDescending(kv => kv.Value))
                sb.AppendLine($"- **{count:N0}×** {rule}");
            sb.AppendLine();

            var shown = Math.Min(stats.Violations.Count, 10);
            sb.AppendLine($"### First {shown} Failing Password(s)");
            sb.AppendLine();
            sb.AppendLine("| # | Password | Violations |");
            sb.AppendLine("|--:|----------|------------|");
            foreach (var v in stats.Violations.Take(10))
                sb.AppendLine($"| {v.Index} | `{v.Password}` | {string.Join("; ", v.Rules)} |");
            sb.AppendLine();
        }
    }

    private static void WriteOverallVerdict(StringBuilder sb, GenerationReport report)
    {
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Overall Verdict");
        sb.AppendLine();

        var issues = new List<string>();

        foreach (var (_, cat) in report.FrequencyStats.CategoryDistribution)
        {
            var deviation = Math.Abs(cat.ActualPercent - cat.ExpectedPercent);
            if (deviation >= 5.0)
                issues.Add($"Category **{cat.Category}** deviates by {deviation:F1}%");
        }

        if (report.FrequencyStats.ChiSquaredPValue <= 0.01)
            issues.Add("Chi-squared test indicates possible bias");

        foreach (var cov in report.FrequencyStats.CharacterSetCoverage)
        {
            if (cov.UsedChars < cov.TotalChars)
                issues.Add($"Set {cov.SetIndex + 1} ({cov.Label}): {cov.TotalChars - cov.UsedChars} unused characters");
        }

        if (report.RepetitionStats.ConfiguredMaxRepetition >= 0 && !report.RepetitionStats.AllWithinConstraint)
            issues.Add("Repetition constraint was violated");

        if (report.DuplicateStats.DuplicateCount > 0)
            issues.Add($"{report.DuplicateStats.DuplicateCount:N0} duplicate password(s) detected ({report.DuplicateStats.DuplicatePercent:F2}%)");

        if (report.PolicyVerification.FailedCount > 0)
            issues.Add($"{report.PolicyVerification.FailedCount:N0} password(s) failed policy verification");

        if (issues.Count == 0)
        {
            sb.AppendLine($"{Pass} **All checks passed** — distribution, coverage, and constraints look good.");
        }
        else
        {
            sb.AppendLine($"{Fail} **{issues.Count} issue(s) detected:**");
            sb.AppendLine();
            foreach (var issue in issues)
                sb.AppendLine($"- {issue}");
        }

        sb.AppendLine();
        sb.AppendLine($"> Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
    }
}
