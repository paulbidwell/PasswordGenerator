using PasswordGenerator.Stats.Models;
using System.Globalization;
using System.Text;

namespace PasswordGenerator.Stats.Reporting;

public static class ErrorCsvReporter
{
    public static void Write(IReadOnlyList<PasswordDiagnostic> diagnostics, string path)
    {
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(',',
            "Index",
            "Password",
            "Length",
            "EntropyBits",
            "Strength",
            "GuessesLog10",
            "MaxConsecutiveRepetition",
            "WarningCount",
            "Warnings",
            "UniqueChars",
            "UniqueRatio",
            "HasUppercase",
            "HasLowercase",
            "HasDigit",
            "HasSymbol",
            "StartsWithLetter",
            "CharCategories"));

        foreach (var d in diagnostics)
        {
            var uniqueChars = d.Password.Distinct().Count();
            var uniqueRatio = d.Length > 0
                ? (double)uniqueChars / d.Length
                : 0;

            var hasUpper = d.Password.Any(char.IsAsciiLetterUpper);
            var hasLower = d.Password.Any(char.IsAsciiLetterLower);
            var hasDigit = d.Password.Any(char.IsAsciiDigit);
            var hasSymbol = d.Password.Any(c => !char.IsLetterOrDigit(c));
            var startsWithLetter = d.Password.Length > 0 && char.IsLetter(d.Password[0]);

            var categories = 0;
            if (hasUpper) categories++;
            if (hasLower) categories++;
            if (hasDigit) categories++;
            if (hasSymbol) categories++;

            sb.AppendLine(string.Join(',',
                d.Index,
                Escape(d.Password),
                d.Length,
                d.EntropyBits.ToString("F2", CultureInfo.InvariantCulture),
                d.Strength,
                d.GuessesLog10.ToString("F2", CultureInfo.InvariantCulture),
                d.MaxConsecutiveRepetition,
                d.Warnings.Count,
                Escape(string.Join(" | ", d.Warnings)),
                uniqueChars,
                uniqueRatio.ToString("F4", CultureInfo.InvariantCulture),
                hasUpper,
                hasLower,
                hasDigit,
                hasSymbol,
                startsWithLetter,
                categories));
        }

        File.WriteAllText(path, sb.ToString());
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
