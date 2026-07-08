namespace PasswordGenerator.Strength.Detectors;

internal static class SequenceDetector
{
    private const int MinRunLength = 3;
    private const double PenaltyPerRun = 6.0;

    public static DetectionResult Detect(ReadOnlySpan<char> password)
    {
        if (password.Length < MinRunLength)
            return new DetectionResult(0, null);

        var totalPenalty = 0.0;
        var runCount = 0;
        var currentRun = 1;

        for (var i = 1; i < password.Length; i++)
        {
            var delta = password[i] - password[i - 1];

            if (delta is 1 or -1 && (currentRun == 1 || delta == password[i - 1] - password[i - 2]))
            {
                currentRun++;
            }
            else
            {
                if (currentRun >= MinRunLength)
                {
                    runCount++;
                    totalPenalty += PenaltyPerRun + (currentRun - MinRunLength) * 2.0;
                }

                currentRun = 1;
            }
        }

        if (currentRun >= MinRunLength)
        {
            runCount++;
            totalPenalty += PenaltyPerRun + (currentRun - MinRunLength) * 2.0;
        }

        if (runCount == 0)
            return new DetectionResult(0, null);

        var warning = runCount == 1
            ? "Contains a sequential character pattern (e.g., abc, 321)."
            : $"Contains {runCount} sequential character patterns.";

        return new DetectionResult(totalPenalty, warning);
    }
}
