using PasswordGenerator.Strength.Detectors;
using Xunit;

namespace PasswordGenerator.Tests;

public class SequenceDetectorTests
{
    [Fact]
    public void Detect_EmptyString_NoPenalty()
    {
        var result = SequenceDetector.Detect("");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_TwoCharSequence_NoPenalty()
    {
        var result = SequenceDetector.Detect("ab");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_ThreeCharAscending_DetectsPattern()
    {
        var result = SequenceDetector.Detect("abc");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_ThreeCharDescending_DetectsPattern()
    {
        var result = SequenceDetector.Detect("cba");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_LongerSequence_HigherPenalty()
    {
        var short3 = SequenceDetector.Detect("abc");
        var long6 = SequenceDetector.Detect("abcdef");
        Assert.True(long6.PenaltyBits > short3.PenaltyBits);
    }

    [Fact]
    public void Detect_NoSequence_NoPenalty()
    {
        var result = SequenceDetector.Detect("axbycz");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_MultipleSequences_ReportsMultiple()
    {
        var result = SequenceDetector.Detect("abcXcba");
        Assert.True(result.PenaltyBits > 0);
        Assert.Contains("2", result.Warning!);
    }

    [Fact]
    public void Detect_DigitSequence_DetectsPattern()
    {
        var result = SequenceDetector.Detect("12345");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }
}
