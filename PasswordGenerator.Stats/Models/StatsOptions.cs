namespace PasswordGenerator.Stats.Models;

public sealed class StatsOptions
{
    public const string SectionName = "Stats";

    public int Count { get; set; } = 10_000;
    public string Mode { get; set; } = "password";
    public int? Length { get; set; }
    public int? MaxRepetition { get; set; }
    public string? Policy { get; set; }
    public string OutputDirectory { get; set; } = "./output";
    public string MarkdownFileName { get; set; } = "report.md";
    public string CsvFileName { get; set; } = "report.csv";
    public string ErrorCsvFileName { get; set; } = "errors.csv";
}
