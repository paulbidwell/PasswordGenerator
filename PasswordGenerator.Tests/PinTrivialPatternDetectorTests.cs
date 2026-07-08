using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Generators;
using Xunit;

namespace PasswordGenerator.Tests;

public class PinTrivialPatternDetectorTests
{
    private readonly IPinTrivialPatternDetector _detector = new PinTrivialPatternDetector();

    [Theory]
    [InlineData("0000")]
    [InlineData("1111")]
    [InlineData("999999")]
    public void IsTrivial_AllSameDigits_ReturnsTrue(string pin)
    {
        Assert.True(_detector.IsTrivial(pin));
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("4567")]
    [InlineData("012345")]
    public void IsTrivial_AscendingSequence_ReturnsTrue(string pin)
    {
        Assert.True(_detector.IsTrivial(pin));
    }

    [Theory]
    [InlineData("4321")]
    [InlineData("9876")]
    [InlineData("543210")]
    public void IsTrivial_DescendingSequence_ReturnsTrue(string pin)
    {
        Assert.True(_detector.IsTrivial(pin));
    }

    [Theory]
    [InlineData("1212")]
    [InlineData("5656")]
    [InlineData("373737")]
    public void IsTrivial_RepeatingPair_ReturnsTrue(string pin)
    {
        Assert.True(_detector.IsTrivial(pin));
    }

    [Theory]
    [InlineData("3847")]
    [InlineData("920516")]
    [InlineData("7041")]
    [InlineData("583921")]
    public void IsTrivial_NonTrivialPin_ReturnsFalse(string pin)
    {
        Assert.False(_detector.IsTrivial(pin));
    }

    [Fact]
    public void IsTrivial_SingleDigit_ReturnsFalse()
    {
        Assert.False(_detector.IsTrivial("5"));
    }

    [Theory]
    [InlineData("1235")]
    [InlineData("1243")]
    public void IsTrivial_PartialSequence_ReturnsFalse(string pin)
    {
        Assert.False(_detector.IsTrivial(pin));
    }

    [Theory]
    [InlineData("12123")]
    public void IsTrivial_OddLengthRepeatingPair_ReturnsFalse(string pin)
    {
        Assert.False(_detector.IsTrivial(pin));
    }
}
