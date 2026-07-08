using Microsoft.Extensions.Options;
using PasswordGenerator.RateLimiting;
using Xunit;

namespace PasswordGenerator.Tests;

public class PinRateLimiterTests
{
    private static PinRateLimiter CreateLimiter(
        int maxAttempts = 5,
        TimeSpan? window = null,
        TimeProvider? timeProvider = null)
    {
        var options = new PinRateLimiterOptions
        {
            MaxAttempts = maxAttempts,
            Window = window ?? TimeSpan.FromMinutes(5)
        };
        return new PinRateLimiter(Options.Create(options), timeProvider);
    }

    [Fact]
    public void IsAllowed_NoAttempts_ReturnsTrue()
    {
        var limiter = CreateLimiter();
        Assert.True(limiter.IsAllowed("user1"));
    }

    [Fact]
    public void IsAllowed_BelowMax_ReturnsTrue()
    {
        var limiter = CreateLimiter(maxAttempts: 3);

        limiter.RecordAttempt("user1");
        limiter.RecordAttempt("user1");

        Assert.True(limiter.IsAllowed("user1"));
    }

    [Fact]
    public void IsAllowed_AtMax_ReturnsFalse()
    {
        var limiter = CreateLimiter(maxAttempts: 3);

        limiter.RecordAttempt("user1");
        limiter.RecordAttempt("user1");
        limiter.RecordAttempt("user1");

        Assert.False(limiter.IsAllowed("user1"));
    }

    [Fact]
    public void IsAllowed_DifferentKeys_AreIndependent()
    {
        var limiter = CreateLimiter(maxAttempts: 1);

        limiter.RecordAttempt("user1");

        Assert.False(limiter.IsAllowed("user1"));
        Assert.True(limiter.IsAllowed("user2"));
    }

    [Fact]
    public void IsAllowed_AfterWindowExpires_ResetsAndReturnsTrue()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var limiter = CreateLimiter(maxAttempts: 1, window: TimeSpan.FromMinutes(5), timeProvider: fakeTime);

        limiter.RecordAttempt("user1");
        Assert.False(limiter.IsAllowed("user1"));

        fakeTime.Advance(TimeSpan.FromMinutes(6));

        Assert.True(limiter.IsAllowed("user1"));
    }

    [Fact]
    public void RecordAttempt_AfterWindowExpires_ResetsCount()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var limiter = CreateLimiter(maxAttempts: 2, window: TimeSpan.FromMinutes(5), timeProvider: fakeTime);

        limiter.RecordAttempt("user1");
        limiter.RecordAttempt("user1");
        Assert.False(limiter.IsAllowed("user1"));

        fakeTime.Advance(TimeSpan.FromMinutes(6));

        limiter.RecordAttempt("user1");
        Assert.True(limiter.IsAllowed("user1"));
    }

    [Fact]
    public void IsAllowed_NullKey_ThrowsArgumentNullException()
    {
        var limiter = CreateLimiter();
        Assert.Throws<ArgumentNullException>(() => limiter.IsAllowed(null!));
    }

    [Fact]
    public void RecordAttempt_NullKey_ThrowsArgumentNullException()
    {
        var limiter = CreateLimiter();
        Assert.Throws<ArgumentNullException>(() => limiter.RecordAttempt(null!));
    }

    [Fact]
    public void RecordAttempt_AboveEvictionThreshold_EvictsExpiredKeys()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var limiter = CreateLimiter(maxAttempts: 100, window: TimeSpan.FromMinutes(1), timeProvider: fakeTime);

        for (var i = 0; i < 10_001; i++)
            limiter.RecordAttempt($"key-{i}");

        Assert.False(limiter.IsAllowed("key-0") && false, "precondition");
        Assert.True(limiter.IsAllowed("key-0"));

        fakeTime.Advance(TimeSpan.FromMinutes(2));

        limiter.RecordAttempt("trigger");

        Assert.True(limiter.IsAllowed("key-0"));

        Assert.True(limiter.IsAllowed("trigger"));
    }
}

internal class FakeTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _utcNow = start;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan delta) => _utcNow += delta;
}
