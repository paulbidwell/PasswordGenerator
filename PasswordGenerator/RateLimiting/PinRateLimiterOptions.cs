namespace PasswordGenerator.RateLimiting;

/// <summary>Options for the <see cref="PinRateLimiter"/> sliding-window rate limiter.</summary>
public class PinRateLimiterOptions
{
    /// <summary>Maximum generation attempts allowed within the sliding <see cref="Window"/>.</summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>Duration of the sliding rate-limit window.</summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(5);
}
