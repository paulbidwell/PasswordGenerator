using PasswordGenerator.Strength.Detectors;
using Xunit;

namespace PasswordGenerator.Tests;

public class RepeatDetectorTests
{
    [Fact]
    public void Detect_EmptyString_NoPenalty()
    {
        var result = RepeatDetector.Detect("");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_TwoRepeats_NoPenalty()
    {
        var result = RepeatDetector.Detect("aa");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_ThreeRepeats_DetectsPattern()
    {
        var result = RepeatDetector.Detect("aaa");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_LongerRepeat_HigherPenalty()
    {
        var short3 = RepeatDetector.Detect("aaa");
        var long6 = RepeatDetector.Detect("aaaaaa");
        Assert.True(long6.PenaltyBits > short3.PenaltyBits);
    }

    [Fact]
    public void Detect_NoRepeats_NoPenalty()
    {
        var result = RepeatDetector.Detect("abcdef");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_MultipleRepeatRuns_ReportsMultiple()
    {
        var result = RepeatDetector.Detect("aaaxbbb");
        Assert.True(result.PenaltyBits > 0);
        Assert.Contains("2", result.Warning!);
    }

    [Fact]
    public void Detect_MixedContent_DetectsEmbeddedRepeat()
    {
        var result = RepeatDetector.Detect("xaaay");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }
}
