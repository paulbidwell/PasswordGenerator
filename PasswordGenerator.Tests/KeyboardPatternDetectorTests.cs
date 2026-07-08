using PasswordGenerator.Strength.Detectors;
using Xunit;

namespace PasswordGenerator.Tests;

public class KeyboardPatternDetectorTests
{
    [Fact]
    public void Detect_EmptyString_NoPenalty()
    {
        var result = KeyboardPatternDetector.Detect("");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_ThreeAdjacentKeys_NoPenalty()
    {
        var result = KeyboardPatternDetector.Detect("qwe");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_FourAdjacentKeys_DetectsPattern()
    {
        var result = KeyboardPatternDetector.Detect("qwer");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_Qwerty_DetectsPattern()
    {
        var result = KeyboardPatternDetector.Detect("qwerty");
        Assert.True(result.PenaltyBits > 0);
        Assert.Contains("keyboard", result.Warning!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Detect_Asdf_DetectsPattern()
    {
        var result = KeyboardPatternDetector.Detect("asdf");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_CaseInsensitive_DetectsPattern()
    {
        var result = KeyboardPatternDetector.Detect("QWER");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_NoPattern_NoPenalty()
    {
        var result = KeyboardPatternDetector.Detect("axbycz");
        Assert.Equal(0, result.PenaltyBits);
        Assert.Null(result.Warning);
    }

    [Fact]
    public void Detect_LongerPattern_HigherPenalty()
    {
        var short4 = KeyboardPatternDetector.Detect("qwer");
        var long6 = KeyboardPatternDetector.Detect("qwerty");
        Assert.True(long6.PenaltyBits > short4.PenaltyBits);
    }

    [Fact]
    public void Detect_NumberRow_DetectsPattern()
    {
        var result = KeyboardPatternDetector.Detect("1234");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Detect_ReverseQwerty_DetectsPattern()
    {
        var result = KeyboardPatternDetector.Detect("rewq");
        Assert.True(result.PenaltyBits > 0);
        Assert.NotNull(result.Warning);
    }
}
