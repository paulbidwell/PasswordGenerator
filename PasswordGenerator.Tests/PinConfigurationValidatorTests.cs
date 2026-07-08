using PasswordGenerator.Core.Interfaces.Generators;
using Xunit;

namespace PasswordGenerator.Tests;

public class PinConfigurationValidatorTests
{
    private readonly PinConfigurationValidator _validator = new();

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    public void Validate_ValidLength_DoesNotThrow(int length)
    {
        var config = new FakePinGeneratorConfig { Length = length, MaxRetries = 10 };
        _validator.Validate(config);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void Validate_LengthBelowMinimum_ThrowsArgumentException(int length)
    {
        var config = new FakePinGeneratorConfig { Length = length, MaxRetries = 10 };
        Assert.Throws<ArgumentException>(() => _validator.Validate(config));
    }

    [Theory]
    [InlineData(13)]
    [InlineData(20)]
    [InlineData(100)]
    public void Validate_LengthAboveMaximum_ThrowsArgumentException(int length)
    {
        var config = new FakePinGeneratorConfig { Length = length, MaxRetries = 10 };
        Assert.Throws<ArgumentException>(() => _validator.Validate(config));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_MaxRetriesBelowOne_ThrowsArgumentException(int maxRetries)
    {
        var config = new FakePinGeneratorConfig { Length = 6, MaxRetries = maxRetries };
        Assert.Throws<ArgumentException>(() => _validator.Validate(config));
    }

    [Fact]
    public void Validate_MaxRetriesOne_DoesNotThrow()
    {
        var config = new FakePinGeneratorConfig { Length = 6, MaxRetries = 1 };
        _validator.Validate(config);
    }
}
