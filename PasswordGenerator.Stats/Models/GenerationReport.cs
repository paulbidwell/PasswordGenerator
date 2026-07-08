namespace PasswordGenerator.Stats.Models;

public record GenerationReport(
    string Mode,
    int SampleSize,
    long ElapsedMilliseconds,
    string ConfigSummary,
    FrequencyStats FrequencyStats,
    RepetitionStats RepetitionStats,
    EntropyStats EntropyStats,
    DuplicateStats DuplicateStats,
    PolicyVerificationStats PolicyVerification);
