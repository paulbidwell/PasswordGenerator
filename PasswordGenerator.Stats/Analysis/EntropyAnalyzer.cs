using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Strength;
using PasswordGenerator.Stats.Models;

namespace PasswordGenerator.Stats.Analysis;

public static class EntropyAnalyzer
{
    public static EntropyStats Analyze(IReadOnlyList<string> passwords, IPasswordStrengthAnalyzer strengthAnalyzer, IGeneratorConfig? config = null)
    {
        if (passwords.Count == 0)
        {
            return new EntropyStats(0, 0, 0, 0,
                new Dictionary<PasswordStrength, int>(),
                new Dictionary<string, int>(),
                []);
        }

        var entropies = new double[passwords.Count];
        var strengthCounts = new Dictionary<PasswordStrength, int>();
        var warningCounts = new Dictionary<string, int>();
        var diagnostics = new List<PasswordDiagnostic>();

        for (var i = 0; i < passwords.Count; i++)
        {
            var password = passwords[i];
            var result = strengthAnalyzer.Analyze(password, config);
            entropies[i] = result.EntropyBits;

            strengthCounts[result.Strength] = strengthCounts.TryGetValue(result.Strength, out var n) ? n + 1 : 1;

            foreach (var warning in result.Warnings)
                warningCounts[warning] = warningCounts.TryGetValue(warning, out var w) ? w + 1 : 1;

            if (result.Warnings.Count > 0)
            {
                diagnostics.Add(new PasswordDiagnostic(
                    i,
                    password,
                    password.Length,
                    result.EntropyBits,
                    result.Strength,
                    result.GuessesLog10,
                    MaxConsecutiveRun(password),
                    result.Warnings));
            }
        }

        var min = entropies.Min();
        var max = entropies.Max();
        var avg = entropies.Average();

        var variance = entropies.Sum(e => (e - avg) * (e - avg)) / entropies.Length;
        var stdDev = Math.Sqrt(variance);

        foreach (var level in Enum.GetValues<PasswordStrength>())
            strengthCounts.TryAdd(level, 0);

        return new EntropyStats(
            Math.Round(min, 2),
            Math.Round(max, 2),
            Math.Round(avg, 2),
            Math.Round(stdDev, 2),
            strengthCounts.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
            warningCounts.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value),
            diagnostics);
    }

    private static int MaxConsecutiveRun(string password)
    {
        if (password.Length == 0)
            return 0;

        var maxRun = 1;
        var currentRun = 1;

        for (var i = 1; i < password.Length; i++)
        {
            if (password[i] == password[i - 1])
            {
                currentRun++;
                if (currentRun > maxRun)
                    maxRun = currentRun;
            }
            else
            {
                currentRun = 1;
            }
        }

        return maxRun;
    }
}
