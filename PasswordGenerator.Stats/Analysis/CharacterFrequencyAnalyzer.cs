using PasswordGenerator.Stats.Models;

namespace PasswordGenerator.Stats.Analysis;

public static class CharacterFrequencyAnalyzer
{
    public static FrequencyStats Analyze(
        IReadOnlyList<string> passwords,
        IReadOnlyList<(int Index, string Label, string Characters)> characterSets)
    {
        var counts = new Dictionary<char, int>();
        var totalChars = 0;

        foreach (var password in passwords)
        {
            foreach (var c in password)
            {
                counts[c] = counts.TryGetValue(c, out var n) ? n + 1 : 1;
                totalChars++;
            }
        }

        var categoryDistribution = BuildCategoryDistribution(counts, totalChars, characterSets);
        var coverage = BuildCoverage(counts, characterSets);
        var (chiSquared, pValue) = ComputeChiSquared(counts, totalChars, characterSets);

        return new FrequencyStats(
            counts.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value),
            totalChars,
            categoryDistribution,
            coverage,
            Math.Round(chiSquared, 2),
            Math.Round(pValue, 4));
    }

    private static Dictionary<string, CategoryFrequency> BuildCategoryDistribution(
        Dictionary<char, int> counts,
        int totalChars,
        IReadOnlyList<(int Index, string Label, string Characters)> characterSets)
    {
        var result = new Dictionary<string, CategoryFrequency>();
        var totalPoolSize = characterSets.Sum(cs => cs.Characters.Length);

        foreach (var (_, label, characters) in characterSets)
        {
            var charSet = new HashSet<char>(characters);
            var count = counts.Where(kv => charSet.Contains(kv.Key)).Sum(kv => kv.Value);
            var actualPercent = totalChars > 0 ? 100.0 * count / totalChars : 0;
            var expectedPercent = totalPoolSize > 0 ? 100.0 * characters.Length / totalPoolSize : 0;

            result[label] = new CategoryFrequency(label, count, Math.Round(actualPercent, 1), Math.Round(expectedPercent, 1));
        }

        return result;
    }

    private static List<CharacterSetCoverage> BuildCoverage(
        Dictionary<char, int> counts,
        IReadOnlyList<(int Index, string Label, string Characters)> characterSets)
    {
        var result = new List<CharacterSetCoverage>();

        foreach (var (index, label, characters) in characterSets)
        {
            var used = characters.Count(c => counts.ContainsKey(c));
            result.Add(new CharacterSetCoverage(index, label, characters.Length, used));
        }

        return result;
    }

    private static (double ChiSquared, double PValue) ComputeChiSquared(
        Dictionary<char, int> counts,
        int totalChars,
        IReadOnlyList<(int Index, string Label, string Characters)> characterSets)
    {
        var allChars = new HashSet<char>();
        foreach (var (_, _, characters) in characterSets)
        {
            foreach (var c in characters)
                allChars.Add(c);
        }

        if (allChars.Count == 0 || totalChars == 0)
            return (0, 1);

        var expected = (double)totalChars / allChars.Count;
        var chiSquared = 0.0;

        foreach (var c in allChars)
        {
            var observed = counts.TryGetValue(c, out var n) ? n : 0;
            chiSquared += (observed - expected) * (observed - expected) / expected;
        }

        var degreesOfFreedom = allChars.Count - 1;
        var pValue = ChiSquaredSurvival(chiSquared, degreesOfFreedom);

        return (chiSquared, pValue);
    }

    /// <summary>
    /// Approximates the chi-squared survival function (1 - CDF) using the
    /// regularised incomplete gamma function. Uses a series expansion when
    /// x &lt;= a+1 and a continued-fraction (modified Lentz) when x &gt; a+1.
    /// Good enough for diagnostic reporting without pulling in a stats library.
    /// </summary>
    private static double ChiSquaredSurvival(double x, int k)
    {
        if (k <= 0 || x < 0)
            return 1;

        var a = k / 2.0;
        var halfX = x / 2.0;

        if (halfX <= a + 1)
        {
            var sum = 0.0;
            var term = 1.0 / a;
            sum += term;

            for (var n = 1; n < 300; n++)
            {
                term *= halfX / (a + n);
                sum += term;
                if (Math.Abs(term) < 1e-14 * Math.Abs(sum))
                    break;
            }

            var logPrefix = -halfX + a * Math.Log(halfX) - LogGamma(a);
            if (logPrefix < -700)
                return 1.0;

            var lowerIncomplete = Math.Exp(logPrefix) * sum;
            return Math.Max(0, Math.Min(1, 1.0 - lowerIncomplete));
        }
        else
        {
            var logPrefix = -halfX + a * Math.Log(halfX) - LogGamma(a);
            if (logPrefix < -700)
                return 0.0;

            const double tiny = 1e-30;
            var b = halfX + 1.0 - a;
            var c = 1.0 / tiny;
            var d = 1.0 / b;
            var h = d;

            for (var n = 1; n <= 300; n++)
            {
                var an = -n * (n - a);
                b += 2.0;
                d = an * d + b;
                if (Math.Abs(d) < tiny) d = tiny;
                c = b + an / c;
                if (Math.Abs(c) < tiny) c = tiny;
                d = 1.0 / d;
                var delta = d * c;
                h *= delta;
                if (Math.Abs(delta - 1.0) < 1e-14)
                    break;
            }

            return Math.Max(0, Math.Min(1, Math.Exp(logPrefix) * h));
        }
    }

    /// <summary>
    /// Stirling-series approximation for ln(Gamma(x)).
    /// </summary>
    private static double LogGamma(double x)
    {
        if (x <= 0)
            return 0;

        ReadOnlySpan<double> c =
        [
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716e-6,
            1.5056327351493116e-7
        ];

        var y = x;
        var tmp = x + 7.5;
        tmp = (y + 0.5) * Math.Log(tmp) - tmp;

        var ser = c[0];
        for (var j = 1; j < c.Length; j++)
            ser += c[j] / (y + j);

        return tmp + Math.Log(Math.Sqrt(2 * Math.PI) * ser / y);
    }
}
