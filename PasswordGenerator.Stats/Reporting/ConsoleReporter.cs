using PasswordGenerator.Stats.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace PasswordGenerator.Stats.Reporting;

public static class ConsoleReporter
{
    private const string PassIcon = "[green]✓[/]";
    private const string FailIcon = "[red]✗[/]";

    public static void Print(GenerationReport report)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.Write(
            new FigletText("PassStats")
                .Color(Color.Cyan1)
                .Centered());

        AnsiConsole.Write(new Rule("[dim]Password Generator — Statistics Dashboard[/]") { Style = Style.Parse("cyan") });
        AnsiConsole.WriteLine();

        PrintTopCards(report);

        PrintCategoryDistribution(report.FrequencyStats);

        PrintCoverageAndRepetition(report.FrequencyStats, report.RepetitionStats);

        PrintStrengthBreakdown(report.EntropyStats, report.SampleSize);

        PrintEntropyAndDuplicates(report.EntropyStats, report.DuplicateStats, report.SampleSize);

        PrintPolicyDashboard(report.PolicyVerification);

        PrintWarnings(report.EntropyStats);

        AnsiConsole.Write(new Rule("[dim]End of Report[/]") { Style = Style.Parse("cyan") });
        AnsiConsole.WriteLine();
    }

    private static void PrintTopCards(GenerationReport report)
    {
        var dominant = GetDominantStrength(report.EntropyStats, report.SampleSize);
        var biasOk = report.FrequencyStats.ChiSquaredPValue > 0.01;
        var policyOk = report.PolicyVerification.FailedCount == 0;
        var dupsOk = report.DuplicateStats.DuplicateCount == 0;
        var coverageOk = report.FrequencyStats.CharacterSetCoverage.All(c => c.UsedChars == c.TotalChars);

        var runGrid = new Grid();
        runGrid.AddColumn(new GridColumn().NoWrap());
        runGrid.AddColumn();
        runGrid.AddRow("[bold]Mode[/]", $"[yellow]{Markup.Escape(report.Mode)}[/]");
        runGrid.AddRow("[bold]Sample[/]", $"[yellow]{report.SampleSize:N0}[/]");
        runGrid.AddRow("[bold]Time[/]", $"[yellow]{report.ElapsedMilliseconds:N0} ms[/]");

        var configLines = report.ConfigSummary.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).Take(3);
        foreach (var line in configLines)
            runGrid.AddRow("", $"[dim]{Markup.Escape(line.Trim())}[/]");

        var runPanel = new Panel(runGrid)
        {
            Header = new PanelHeader("[bold white] Run Info [/]"),
            Border = BoxBorder.Heavy,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        var quickGrid = new Grid();
        quickGrid.AddColumn(new GridColumn().NoWrap());
        quickGrid.AddColumn();
        quickGrid.AddRow("[bold]Generated[/]", $"[yellow]{report.SampleSize:N0}[/]");
        quickGrid.AddRow("[bold]Unique[/]", $"[green]{report.DuplicateStats.UniqueCount:N0}[/]");
        quickGrid.AddRow("[bold]Avg Entropy[/]", $"[yellow]{report.EntropyStats.AverageEntropy:F1}[/] bits");
        quickGrid.AddRow("[bold]Strength[/]", $"[cyan]{Markup.Escape(dominant)}[/]");

        var quickPanel = new Panel(quickGrid)
        {
            Header = new PanelHeader("[bold white] Quick Stats [/]"),
            Border = BoxBorder.Heavy,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        var healthGrid = new Grid();
        healthGrid.AddColumn(new GridColumn().NoWrap());
        healthGrid.AddColumn();
        healthGrid.AddRow($"{(policyOk ? PassIcon : FailIcon)} [bold]Policy[/]", policyOk ? "[green]All pass[/]" : $"[red]{report.PolicyVerification.FailedCount} failed[/]");
        healthGrid.AddRow($"{(biasOk ? PassIcon : FailIcon)} [bold]Bias[/]", biasOk ? "[green]None detected[/]" : "[red]Possible bias[/]");
        healthGrid.AddRow($"{(dupsOk ? PassIcon : FailIcon)} [bold]Duplicates[/]", dupsOk ? "[green]0[/]" : $"[red]{report.DuplicateStats.DuplicateCount:N0}[/]");
        healthGrid.AddRow($"{(coverageOk ? PassIcon : FailIcon)} [bold]Coverage[/]", coverageOk ? "[green]100%[/]" : "[yellow]Partial[/]");

        var healthPanel = new Panel(healthGrid)
        {
            Header = new PanelHeader("[bold white] Health [/]"),
            Border = BoxBorder.Heavy,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        AnsiConsole.Write(new Columns(runPanel, quickPanel, healthPanel));
        AnsiConsole.WriteLine();
    }

    private static void PrintCategoryDistribution(FrequencyStats stats)
    {
        AnsiConsole.Write(new Rule("[bold white] Category Distribution [/]") { Style = Style.Parse("grey"), Justification = Justify.Left });
        AnsiConsole.WriteLine();

        var table = new Table { Border = TableBorder.None, ShowHeaders = false, Expand = true };
        table.AddColumn(new TableColumn("").Width(12).NoWrap());
        table.AddColumn(new TableColumn("").Width(40));
        table.AddColumn(new TableColumn("").Width(10).RightAligned());
        table.AddColumn(new TableColumn("").Width(18).RightAligned());
        table.AddColumn(new TableColumn("").Width(4).Centered());

        foreach (var (_, cat) in stats.CategoryDistribution)
        {
            var deviation = Math.Abs(cat.ActualPercent - cat.ExpectedPercent);
            var status = deviation < 5.0 ? PassIcon : FailIcon;
            var barColor = deviation < 2.0 ? "green" : deviation < 5.0 ? "yellow" : "red";
            var bar = ProgressBar(cat.ActualPercent, 100.0, 30, barColor);

            table.AddRow(
                $"[bold]{Markup.Escape(cat.Category)}[/]",
                bar,
                $"[{barColor}]{cat.ActualPercent:F1}%[/]",
                $"[dim](expect {cat.ExpectedPercent:F1}%)[/]",
                status);
        }

        AnsiConsole.Write(table);

        var chiOk = stats.ChiSquaredPValue > 0.01;
        AnsiConsole.MarkupLine($"  [dim]χ²={stats.ChiSquaredStatistic:F1}  p={stats.ChiSquaredPValue:F4}[/]  {(chiOk ? "[green]No bias ✓[/]" : "[red]Possible bias ✗[/]")}");
        AnsiConsole.WriteLine();
    }

    private static void PrintCoverageAndRepetition(FrequencyStats freq, RepetitionStats rep)
    {
        var covGrid = new Grid();
        covGrid.AddColumn(new GridColumn().NoWrap());
        covGrid.AddColumn();
        covGrid.AddColumn(new GridColumn().NoWrap());

        foreach (var cov in freq.CharacterSetCoverage)
        {
            var pct = cov.TotalChars > 0 ? 100.0 * cov.UsedChars / cov.TotalChars : 0;
            var color = pct >= 100 ? "green" : pct >= 80 ? "yellow" : "red";
            var status = cov.UsedChars == cov.TotalChars ? PassIcon : FailIcon;
            var bar = ProgressBar(pct, 100.0, 20, color);
            covGrid.AddRow(
                new Markup($"[bold]{Markup.Escape(cov.Label)}[/]"),
                new Markup($"{bar} [{color}]{pct:F0}%[/] [dim]({cov.UsedChars}/{cov.TotalChars})[/]"),
                new Markup(status));
        }

        var covPanel = new Panel(covGrid)
        {
            Header = new PanelHeader("[bold white] Character Set Coverage [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        var repTable = new Grid();
        repTable.AddColumn(new GridColumn().NoWrap());
        repTable.AddColumn();
        repTable.AddRow("[bold]Max[/]", $"[yellow]{rep.MaxConsecutive}[/]");
        repTable.AddRow("[bold]Min[/]", $"[yellow]{rep.MinConsecutive}[/]");
        repTable.AddRow("[bold]Avg[/]", $"[yellow]{rep.AverageConsecutive:F2}[/]");

        if (rep.ConfiguredMaxRepetition >= 0)
        {
            var ok = rep.AllWithinConstraint;
            repTable.AddRow(
                $"[bold]Limit ({rep.ConfiguredMaxRepetition})[/]",
                ok ? "[green]HONOURED ✓[/]" : "[red]VIOLATED ✗[/]");
        }
        else
        {
            repTable.AddRow("[bold]Limit[/]", "[dim]Unlimited[/]");
        }

        if (rep.Histogram.Count > 0)
        {
            repTable.AddRow("", "");
            repTable.AddRow("[dim]Run[/]", "[dim]Count[/]");
            var maxVal = rep.Histogram.Values.Max();
            foreach (var (run, cnt) in rep.Histogram.OrderBy(kv => kv.Key))
            {
                var barLen = maxVal > 0 ? (int)Math.Round(15.0 * cnt / maxVal) : 0;
                var miniBar = $"[cyan]{new string('█', barLen)}[/][dim]{new string('░', 15 - barLen)}[/]";
                repTable.AddRow($"[bold]{run}[/]", $"{miniBar} {cnt:N0}");
            }
        }

        var repPanel = new Panel(repTable)
        {
            Header = new PanelHeader("[bold white] Repetition [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        AnsiConsole.Write(new Columns(covPanel, repPanel));
        AnsiConsole.WriteLine();
    }

    private static void PrintStrengthBreakdown(EntropyStats stats, int sampleSize)
    {
        AnsiConsole.Write(new Rule("[bold white] Strength Distribution [/]") { Style = Style.Parse("grey"), Justification = Justify.Left });
        AnsiConsole.WriteLine();

        var chart = new BreakdownChart().Width(60);

        foreach (var (strength, count) in stats.StrengthDistribution)
        {
            if (count == 0) continue;
            var (color, _) = StrengthStyle(strength.ToString());
            chart.AddItem(strength.ToString(), count, color);
        }

        AnsiConsole.Write(chart);
        AnsiConsole.WriteLine();

        var legendGrid = new Grid();
        legendGrid.AddColumn();
        legendGrid.AddColumn();
        legendGrid.AddColumn();
        legendGrid.AddColumn();
        legendGrid.AddColumn();

        var markups = new List<IRenderable>();
        foreach (var (strength, count) in stats.StrengthDistribution)
        {
            var pct = sampleSize > 0 ? 100.0 * count / sampleSize : 0;
            var (_, colorName) = StrengthStyle(strength.ToString());
            markups.Add(new Markup($"[{colorName}]● {strength}[/] [dim]{pct:F1}%[/]"));
        }

        while (markups.Count < 5) markups.Add(new Markup(""));
        legendGrid.AddRow(markups.Take(5).ToArray());

        AnsiConsole.Write(legendGrid);
        AnsiConsole.WriteLine();
    }

    private static void PrintEntropyAndDuplicates(EntropyStats entropy, DuplicateStats dups, int sampleSize)
    {
        var eGrid = new Grid();
        eGrid.AddColumn(new GridColumn().NoWrap());
        eGrid.AddColumn();

        eGrid.AddRow("[bold]Average[/]", $"[yellow]{entropy.AverageEntropy:F2}[/] bits");
        eGrid.AddRow("[bold]Min[/]", $"[yellow]{entropy.MinEntropy:F2}[/] bits");
        eGrid.AddRow("[bold]Max[/]", $"[yellow]{entropy.MaxEntropy:F2}[/] bits");
        eGrid.AddRow("[bold]Std Dev[/]", $"[yellow]{entropy.StdDevEntropy:F2}[/]");

        var entropyPct = Math.Min(entropy.AverageEntropy / 128.0 * 100.0, 100.0);
        var entropyColor = entropyPct >= 60 ? "green" : entropyPct >= 40 ? "yellow" : "red";
        eGrid.AddRow("", "");
        eGrid.AddRow(new Markup("[bold]Quality[/]"), new Markup($"{ProgressBar(entropyPct, 100.0, 20, entropyColor)} [{entropyColor}]{entropyPct:F0}%[/]"));

        var ePanel = new Panel(eGrid)
        {
            Header = new PanelHeader("[bold white] Entropy [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        var dGrid = new Grid();
        dGrid.AddColumn(new GridColumn().NoWrap());
        dGrid.AddColumn();

        var dupOk = dups.DuplicateCount == 0;
        var uniquePct = sampleSize > 0 ? 100.0 * dups.UniqueCount / sampleSize : 100.0;

        dGrid.AddRow("[bold]Total[/]", $"[yellow]{dups.TotalGenerated:N0}[/]");
        dGrid.AddRow("[bold]Unique[/]", $"[green]{dups.UniqueCount:N0}[/]");

        var dupColor = dupOk ? "green" : "red";
        dGrid.AddRow("[bold]Duplicates[/]", $"[{dupColor}]{dups.DuplicateCount:N0}[/] {(dupOk ? PassIcon : FailIcon)}");

        var barColor = uniquePct >= 99.9 ? "green" : uniquePct >= 95 ? "yellow" : "red";
        dGrid.AddRow("", "");
        dGrid.AddRow(new Markup("[bold]Uniqueness[/]"), new Markup($"{ProgressBar(uniquePct, 100.0, 20, barColor)} [{barColor}]{uniquePct:F2}%[/]"));

        if (dups.MostRepeatedExample is not null)
        {
            dGrid.AddRow("", "");
            dGrid.AddRow("[dim]Most seen[/]", $"[dim]{dups.MostRepeatedCount}× —[/] \"{Markup.Escape(dups.MostRepeatedExample)}\"");
        }

        var dPanel = new Panel(dGrid)
        {
            Header = new PanelHeader("[bold white] Duplicates [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1, 0),
            Expand = true
        };

        AnsiConsole.Write(new Columns(ePanel, dPanel));
        AnsiConsole.WriteLine();
    }

    private static void PrintPolicyDashboard(PolicyVerificationStats stats)
    {
        AnsiConsole.Write(new Rule("[bold white] Policy Verification [/]") { Style = Style.Parse("grey"), Justification = Justify.Left });
        AnsiConsole.WriteLine();

        var passRate = stats.TotalChecked > 0 ? 100.0 * stats.PassedCount / stats.TotalChecked : 100.0;
        var ok = stats.FailedCount == 0;
        var color = ok ? "green" : passRate >= 95 ? "yellow" : "red";

        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap());
        grid.AddColumn();
        grid.AddColumn(new GridColumn().NoWrap());

        grid.AddRow(
            new Markup($"[bold]Pass Rate[/]"),
            new Markup(ProgressBar(passRate, 100.0, 40, color)),
            new Markup($"[{color} bold]{passRate:F1}%[/] {(ok ? PassIcon : FailIcon)}"));

        grid.AddRow(new Markup(""),
            new Markup($"[dim]{stats.PassedCount:N0} passed  ·  {stats.FailedCount:N0} failed  ·  {stats.TotalChecked:N0} checked[/]"),
            new Markup(""));

        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        if (stats.RuleViolationCounts.Count > 0)
        {
            var violationTable = new Table { Border = TableBorder.Rounded, BorderStyle = Style.Parse("red") };
            violationTable.AddColumn(new TableColumn("[bold]Rule[/]").LeftAligned());
            violationTable.AddColumn(new TableColumn("[bold]Violations[/]").RightAligned());
            violationTable.AddColumn(new TableColumn("[bold]Bar[/]").LeftAligned());

            var maxViolations = stats.RuleViolationCounts.Values.Max();
            foreach (var (rule, count) in stats.RuleViolationCounts.OrderByDescending(kv => kv.Value))
            {
                var pct = maxViolations > 0 ? 100.0 * count / maxViolations : 0;
                violationTable.AddRow(
                    Markup.Escape(rule),
                    $"[red]{count:N0}[/]",
                    ProgressBar(pct, 100.0, 15, "red"));
            }

            AnsiConsole.Write(violationTable);
            AnsiConsole.WriteLine();

            var shown = Math.Min(stats.Violations.Count, 5);
            if (shown > 0)
            {
                var failTable = new Table { Border = TableBorder.Simple, BorderStyle = Style.Parse("grey") };
                failTable.AddColumn(new TableColumn("[bold]#[/]").RightAligned());
                failTable.AddColumn(new TableColumn("[bold]Password[/]").LeftAligned());
                failTable.AddColumn(new TableColumn("[bold]Broken Rules[/]").LeftAligned());

                foreach (var v in stats.Violations.Take(5))
                    failTable.AddRow($"{v.Index}", $"[dim]{Markup.Escape(v.Password)}[/]", $"[red]{Markup.Escape(string.Join("; ", v.Rules))}[/]");

                AnsiConsole.Write(failTable);
                AnsiConsole.WriteLine();
            }
        }
    }

    private static void PrintWarnings(EntropyStats stats)
    {
        if (stats.WarningCounts.Count == 0)
            return;

        AnsiConsole.Write(new Rule("[bold yellow] (!) Warnings [/]") { Style = Style.Parse("yellow"), Justification = Justify.Left });
        AnsiConsole.WriteLine();

        var table = new Table { Border = TableBorder.Rounded, BorderStyle = Style.Parse("yellow") };
        table.AddColumn(new TableColumn("[bold]Count[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Warning[/]").LeftAligned());

        foreach (var (warning, count) in stats.WarningCounts.OrderByDescending(kv => kv.Value))
            table.AddRow($"[yellow]{count:N0}[/]", Markup.Escape(warning));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static string ProgressBar(double value, double max, int width, string color)
    {
        var ratio = max > 0 ? Math.Clamp(value / max, 0, 1) : 0;
        var filled = (int)Math.Round(ratio * width);
        var empty = width - filled;
        return $"[{color}]{new string('━', filled)}[/][dim]{new string('─', empty)}[/]";
    }

    private static string GetDominantStrength(EntropyStats stats, int sampleSize)
    {
        if (stats.StrengthDistribution.Count == 0) return "N/A";
        var dominant = stats.StrengthDistribution.OrderByDescending(kv => kv.Value).First();
        return dominant.Key.ToString();
    }

    private static (Color Color, string Name) StrengthStyle(string strength) => strength switch
    {
        "VeryWeak" => (Color.Red, "red"),
        "Weak" => (Color.OrangeRed1, "orangered1"),
        "Fair" => (Color.Yellow, "yellow"),
        "Strong" => (Color.Green, "green"),
        "VeryStrong" => (Color.Cyan1, "cyan1"),
        _ => (Color.White, "white")
    };
}
