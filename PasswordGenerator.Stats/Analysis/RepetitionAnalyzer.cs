using PasswordGenerator.Stats.Models;

namespace PasswordGenerator.Stats.Analysis;

public static class RepetitionAnalyzer
{
    public static RepetitionStats Analyze(IReadOnlyList<string> passwords, int configuredMaxRepetition)
    {
        var histogram = new Dictionary<int, int>();
        var globalMin = int.MaxValue;
        var globalMax = int.MinValue;
        var sum = 0L;
        var allWithin = true;

        foreach (var password in passwords)
        {
            var maxRun = MaxConsecutiveRun(password);

            histogram[maxRun] = histogram.TryGetValue(maxRun, out var n) ? n + 1 : 1;
            sum += maxRun;

            if (maxRun < globalMin) globalMin = maxRun;
            if (maxRun > globalMax) globalMax = maxRun;

            if (configuredMaxRepetition >= 0 && maxRun > configuredMaxRepetition)
                allWithin = false;
        }

        if (passwords.Count == 0)
        {
            globalMin = 0;
            globalMax = 0;
        }

        return new RepetitionStats(
            globalMin,
            globalMax,
            passwords.Count > 0 ? Math.Round((double)sum / passwords.Count, 2) : 0,
            histogram.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
            allWithin,
            configuredMaxRepetition);
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
