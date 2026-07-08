using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Strength;
using PasswordGenerator.Stats.Analysis;
using PasswordGenerator.Stats.Models;
using PasswordGenerator.Stats.Reporting;
using Spectre.Console;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .Build();

var statsOptions = new StatsOptions();
configuration.GetSection(StatsOptions.SectionName).Bind(statsOptions);

var count = statsOptions.Count;
var mode = statsOptions.Mode;
var runAllPolicies = true;
PasswordGeneratorOptions? passwordOptions = null;
PinGeneratorOptions? pinOptions = null;

if (statsOptions.Policy is not null)
{
    if (statsOptions.Policy.Equals("all", StringComparison.OrdinalIgnoreCase))
    {
        runAllPolicies = true;
    }
    else
    {
        runAllPolicies = false;
        switch (statsOptions.Policy.ToLowerInvariant())
        {
            case "default":
                passwordOptions = new PasswordGeneratorOptions();
                break;
            case "owasp":
                passwordOptions = PasswordPolicy.Owasp;
                break;
            case "nist":
                passwordOptions = PasswordPolicy.Nist80063B;
                break;
            case "pci":
                passwordOptions = PasswordPolicy.Pci;
                break;
            case "pin":
                mode = "pin";
                pinOptions = new PinGeneratorOptions();
                break;
            default:
                throw new ArgumentException($"Unknown policy in appsettings: {statsOptions.Policy}. Valid: default, owasp, nist, pci, pin, all");
        }
    }
}

if (statsOptions.Length is { } cfgLen)
{
    if (mode == "pin")
        (pinOptions ??= new PinGeneratorOptions()).Length = cfgLen;
    else
        (passwordOptions ??= new PasswordGeneratorOptions()).Length = cfgLen;
}

if (statsOptions.MaxRepetition is { } cfgMaxRep)
    (passwordOptions ??= new PasswordGeneratorOptions()).MaxRepetition = cfgMaxRep;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i].ToLowerInvariant())
    {
        case "--count" or "-n" when i + 1 < args.Length:
            count = int.Parse(args[++i]);
            break;

        case "--mode" or "-m" when i + 1 < args.Length:
            mode = args[++i].ToLowerInvariant();
            runAllPolicies = false;
            break;

        case "--length" or "-l" when i + 1 < args.Length:
            var length = int.Parse(args[++i]);
            runAllPolicies = false;
            if (mode == "pin")
                (pinOptions ??= new PinGeneratorOptions()).Length = length;
            else
                (passwordOptions ??= new PasswordGeneratorOptions()).Length = length;
            break;

        case "--max-repetition" when i + 1 < args.Length:
            runAllPolicies = false;
            (passwordOptions ??= new PasswordGeneratorOptions()).MaxRepetition = int.Parse(args[++i]);
            break;

        case "--policy" when i + 1 < args.Length:
            var policyArg = args[++i].ToLowerInvariant();
            if (policyArg == "all")
            {
                runAllPolicies = true;
            }
            else
            {
                runAllPolicies = false;
                switch (policyArg)
                {
                    case "default":
                        passwordOptions = new PasswordGeneratorOptions();
                        break;
                    case "owasp":
                        passwordOptions = PasswordPolicy.Owasp;
                        break;
                    case "nist":
                        passwordOptions = PasswordPolicy.Nist80063B;
                        break;
                    case "pci":
                        passwordOptions = PasswordPolicy.Pci;
                        break;
                    case "pin":
                        mode = "pin";
                        pinOptions = new PinGeneratorOptions();
                        break;
                    default:
                        throw new ArgumentException($"Unknown policy: {policyArg}. Valid: default, owasp, nist, pci, pin, all");
                }
            }
            break;

        case "--output" or "-o" when i + 1 < args.Length:
            statsOptions.OutputDirectory = args[++i];
            break;

        case "--help" or "-h":
            PrintUsage();
            return;
    }
}

if (runAllPolicies)
{
    var policies = new (string Name, PasswordGeneratorOptions Options)[]
    {
        ("default", new PasswordGeneratorOptions()),
        ("owasp", PasswordPolicy.Owasp),
        ("nist", PasswordPolicy.Nist80063B),
        ("pci", PasswordPolicy.Pci)
    };

    foreach (var (policyName, policyOptions) in policies)
    {
        RunAndReport(count, "password", policyOptions, null, policyName, statsOptions);
    }

    RunAndReport(count, "pin", null, new PinGeneratorOptions(), "pin", statsOptions);
}
else
{
    RunAndReport(count, mode, passwordOptions, pinOptions, null, statsOptions);
}

static void RunAndReport(
    int count,
    string mode,
    PasswordGeneratorOptions? passwordOptions,
    PinGeneratorOptions? pinOptions,
    string? policyLabel,
    StatsOptions statsOptions)
{
    var services = new ServiceCollection();

    if (mode == "pin")
    {
        services.AddPinGenerator(o =>
        {
            if (pinOptions is null) return;
            o.Length = pinOptions.Length;
            o.RejectTrivialPatterns = pinOptions.RejectTrivialPatterns;
            o.MaxRetries = pinOptions.MaxRetries;
        });

        services.AddPasswordGenerator();
    }
    else
    {
        services.AddPasswordGenerator(o =>
        {
            if (passwordOptions is null) return;
            o.Length = passwordOptions.Length;
            o.MaxRepetition = passwordOptions.MaxRepetition;
            o.AllowSequences = passwordOptions.AllowSequences;
            o.AllowUpperLowerSequences = passwordOptions.AllowUpperLowerSequences;
            o.AsciiOnly = passwordOptions.AsciiOnly;
            o.ExcludeAmbiguous = passwordOptions.ExcludeAmbiguous;
            o.ExcludedCharacters = passwordOptions.ExcludedCharacters;
            o.ExcludeLeadingTrailingSymbols = passwordOptions.ExcludeLeadingTrailingSymbols;
            o.MustStartWithLetter = passwordOptions.MustStartWithLetter;
            o.CharacterSets = passwordOptions.CharacterSets;
        });

        services.AddPinGenerator();
    }

    services.AddTransient<StatisticsRunner>();

    using var provider = services.BuildServiceProvider();

    var runner = provider.GetRequiredService<StatisticsRunner>();

    var effectivePasswordOptions = passwordOptions ?? new PasswordGeneratorOptions();
    var effectivePinOptions = pinOptions ?? new PinGeneratorOptions();

    GenerationReport? report = null;

    AnsiConsole.Status()
        .Spinner(Spinner.Known.Arc)
        .SpinnerStyle(Style.Parse("cyan"))
        .Start("[cyan]Initializing…[/]", ctx =>
        {
            var progress = new Progress<string>(msg => ctx.Status($"[cyan]{Markup.Escape(msg)}[/]"));
            report = runner.Run(count, mode, effectivePasswordOptions, effectivePinOptions, progress);
        });

    if (policyLabel is not null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold yellow]Policy: {policyLabel.ToUpperInvariant()}[/]") { Style = Style.Parse("yellow"), Justification = Justify.Center });
    }

    ConsoleReporter.Print(report!);

    var suffix = policyLabel is not null ? $"-{policyLabel}" : "";
    var outputDir = Path.GetFullPath(statsOptions.OutputDirectory);
    Directory.CreateDirectory(outputDir);

    var mdPath = Path.Combine(outputDir, InsertSuffix(statsOptions.MarkdownFileName, suffix));
    MarkdownReporter.Write(report!, mdPath);

    var csvPath = Path.Combine(outputDir, InsertSuffix(statsOptions.CsvFileName, suffix));
    CsvReporter.Write(report!, csvPath);

    string? errorCsvPath = null;
    var warnedPasswords = report!.EntropyStats.WarnedPasswords;
    if (warnedPasswords.Count > 0)
    {
        errorCsvPath = Path.Combine(outputDir, InsertSuffix(statsOptions.ErrorCsvFileName, suffix));
        ErrorCsvReporter.Write(warnedPasswords, errorCsvPath);
    }

    var outputGrid = new Grid();
    outputGrid.AddColumn(new GridColumn().NoWrap());
    outputGrid.AddColumn();
    outputGrid.AddRow("[dim]Markdown[/]", $"[link={Markup.Escape(mdPath)}]{Markup.Escape(mdPath)}[/]");
    outputGrid.AddRow("[dim]CSV[/]", $"[link={Markup.Escape(csvPath)}]{Markup.Escape(csvPath)}[/]");
    if (errorCsvPath is not null)
        outputGrid.AddRow("[yellow]Error CSV[/]", $"[link={Markup.Escape(errorCsvPath)}]{Markup.Escape(errorCsvPath)}[/] [dim]({warnedPasswords.Count:N0} warnings)[/]");
    else
        outputGrid.AddRow("[dim]Error CSV[/]", "[dim]skipped — no warnings[/]");

    var outputPanel = new Panel(outputGrid)
    {
        Header = new PanelHeader("[bold white] Output Files [/]"),
        Border = BoxBorder.Rounded,
        BorderStyle = Style.Parse("grey"),
        Padding = new Padding(2, 0)
    };
    AnsiConsole.Write(outputPanel);
    AnsiConsole.WriteLine();
}

static string InsertSuffix(string fileName, string suffix)
{
    if (suffix.Length == 0) return fileName;
    var ext = Path.GetExtension(fileName);
    var name = Path.GetFileNameWithoutExtension(fileName);
    return $"{name}{suffix}{ext}";
}

static void PrintUsage()
{
    AnsiConsole.Write(new Rule("[bold cyan]Password Generator — Distribution Statistics Tool[/]") { Style = Style.Parse("cyan"), Justification = Justify.Center });
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Usage:[/]");
    AnsiConsole.MarkupLine("  PasswordGenerator.Stats [dim][[options]][/]");
    AnsiConsole.WriteLine();

    var table = new Table { Border = TableBorder.Simple, BorderStyle = Style.Parse("grey") };
    table.AddColumn(new TableColumn("[bold]Option[/]").LeftAligned().NoWrap());
    table.AddColumn(new TableColumn("[bold]Description[/]").LeftAligned());

    table.AddRow("[yellow]--count, -n[/] <N>", "Number of passwords to generate [dim](default: 10000)[/]");
    table.AddRow("[yellow]--mode, -m[/] <mode>", "Generation mode: 'password' or 'pin' [dim](default: password)[/]");
    table.AddRow("[yellow]--length, -l[/] <N>", "Password/PIN length override");
    table.AddRow("[yellow]--max-repetition[/] <N>", "Max consecutive character repetition [dim](password mode)[/]");
    table.AddRow("[yellow]--policy[/] <name>", "Use a preset: default, owasp, nist, pci, pin, all");
    table.AddRow("[yellow]--output, -o[/] <dir>", "Output directory for report files [dim](default: ./output)[/]");
    table.AddRow("[yellow]--help, -h[/]", "Show this help");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine("[bold]Configuration:[/]");
    AnsiConsole.MarkupLine("  Settings can also be specified in [dim]appsettings.json[/] under the [yellow]\"Stats\"[/] section.");
    AnsiConsole.MarkupLine("  CLI arguments override appsettings values.");
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine("[bold]Examples:[/]");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats [yellow]--count 50000 --policy owasp[/]");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats [yellow]--policy all[/]");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats [yellow]--policy all --count 5000[/]");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats [yellow]--mode pin --length 8 --count 20000[/]");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats [yellow]-n 5000 -l 32 --max-repetition 2[/]");
    AnsiConsole.MarkupLine("  [dim]$[/] PasswordGenerator.Stats [yellow]-o ./reports[/]");
    AnsiConsole.WriteLine();
}
