namespace PasswordGenerator.Strength.Detectors;

internal static class RepeatDetector
{
    private const int MinRunLength = 3;
    private const double PenaltyPerRun = 8.0;

    public static DetectionResult Detect(ReadOnlySpan<char> password)
    {
        if (password.Length < MinRunLength)
            return new DetectionResult(0, null);

        var totalPenalty = 0.0;
        var runCount = 0;
        var currentRun = 1;

        for (var i = 1; i < password.Length; i++)
        {
            if (password[i] == password[i - 1])
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
            ? "Contains a repeated character run (e.g., aaa)."
            : $"Contains {runCount} repeated character runs.";

        return new DetectionResult(totalPenalty, warning);
    }
}
