namespace PasswordGenerator.Strength.Detectors;

internal static class KeyboardPatternDetector
{
    private const int MinRunLength = 4;
    private const double PenaltyPerRun = 10.0;

    private static readonly string[] QwertyRows =
    [
        "qwertyuiop",
        "asdfghjkl",
        "zxcvbnm",
        "1234567890"
    ];

    private static readonly HashSet<(char, char)> AdjacencySet = BuildAdjacencySet();

    private static HashSet<(char, char)> BuildAdjacencySet()
    {
        var set = new HashSet<(char, char)>();

        foreach (var row in QwertyRows)
        {
            for (var i = 0; i < row.Length - 1; i++)
            {
                set.Add((row[i], row[i + 1]));
                set.Add((row[i + 1], row[i]));
            }
        }

        return set;
    }

    public static DetectionResult Detect(ReadOnlySpan<char> password)
    {
        if (password.Length < MinRunLength)
            return new DetectionResult(0, null);

        var totalPenalty = 0.0;
        var runCount = 0;
        var currentRun = 1;

        for (var i = 1; i < password.Length; i++)
        {
            var a = char.ToLowerInvariant(password[i - 1]);
            var b = char.ToLowerInvariant(password[i]);

            if (AdjacencySet.Contains((a, b)))
            {
                currentRun++;
            }
            else
            {
                if (currentRun >= MinRunLength)
                {
                    runCount++;
                    totalPenalty += PenaltyPerRun + (currentRun - MinRunLength) * 3.0;
                }

                currentRun = 1;
            }
        }

        if (currentRun >= MinRunLength)
        {
            runCount++;
            totalPenalty += PenaltyPerRun + (currentRun - MinRunLength) * 3.0;
        }

        if (runCount == 0)
            return new DetectionResult(0, null);

        var warning = runCount == 1
            ? "Contains a keyboard pattern (e.g., qwerty, asdf)."
            : $"Contains {runCount} keyboard patterns.";

        return new DetectionResult(totalPenalty, warning);
    }
}
