using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Interfaces.Generators;
using System.Collections.Concurrent;

namespace PasswordGenerator.RateLimiting;

/// <summary>Sliding-window rate limiter for PIN generation attempts, backed by a <see cref="ConcurrentDictionary{TKey,TValue}"/>.</summary>
public class PinRateLimiter(IOptions<PinRateLimiterOptions> options, TimeProvider? timeProvider = null) : IPinRateLimiter
{
    private const int EvictionThreshold = 10_000;

    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private readonly ConcurrentDictionary<string, (int Count, DateTimeOffset WindowStart)> _attempts = new();

    /// <inheritdoc />
    public bool IsAllowed(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_attempts.TryGetValue(key, out var entry))
            return true;

        if (_timeProvider.GetUtcNow() - entry.WindowStart >= options.Value.Window)
            return true;

        return entry.Count < options.Value.MaxAttempts;
    }

    /// <inheritdoc />
    public void RecordAttempt(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var now = _timeProvider.GetUtcNow();

        _attempts.AddOrUpdate(
            key,
            _ => (1, now),
            (_, existing) =>
            {
                if (now - existing.WindowStart >= options.Value.Window)
                    return (1, now);

                return (existing.Count + 1, existing.WindowStart);
            });

        if (_attempts.Count > EvictionThreshold)
            EvictExpiredEntries(now);
    }

    private void EvictExpiredEntries(DateTimeOffset now)
    {
        var window = options.Value.Window;

        foreach (var kvp in _attempts)
        {
            if (now - kvp.Value.WindowStart >= window)
                _attempts.TryRemove(kvp.Key, out _);
        }
    }
}
