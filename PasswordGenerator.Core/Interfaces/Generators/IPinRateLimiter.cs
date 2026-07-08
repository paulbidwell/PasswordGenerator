namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>Rate-limits PIN generation attempts per logical key.</summary>
public interface IPinRateLimiter
{
    /// <summary>Returns <see langword="true"/> when <paramref name="key"/> has not exceeded its rate limit.</summary>
    /// <param name="key">The logical key identifying the caller or context.</param>
    bool IsAllowed(string key);

    /// <summary>Records a generation attempt against <paramref name="key"/>.</summary>
    /// <param name="key">The logical key identifying the caller or context.</param>
    void RecordAttempt(string key);
}
