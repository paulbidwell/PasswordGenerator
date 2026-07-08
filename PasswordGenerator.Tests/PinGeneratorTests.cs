using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Generators;
using Xunit;

namespace PasswordGenerator.Tests;

public class PinGeneratorTests
{
    private static PinGenerator CreateGenerator(
        int length = 6,
        bool rejectTrivialPatterns = false,
        int maxRetries = 10,
        IRandomNumberGenerator? rng = null,
        IPinTrivialPatternDetector? detector = null,
        IPinConfigurationValidator? validator = null)
    {
        var config = new FakePinGeneratorConfig
        {
            Length = length,
            RejectTrivialPatterns = rejectTrivialPatterns,
            MaxRetries = maxRetries
        };

        return new PinGenerator(
            config,
            validator ?? new StubPinConfigurationValidator(),
            rng ?? new Generators.SecureRng(),
            detector ?? new PinTrivialPatternDetector());
    }

    [Fact]
    public void Generate_ReturnsStringOfConfiguredLength()
    {
        var generator = CreateGenerator(length: 8);
        var pin = generator.Generate();
        Assert.Equal(8, pin.Length);
    }

    [Fact]
    public void Generate_ReturnsDigitsOnly()
    {
        var generator = CreateGenerator(length: 6);
        var pin = generator.Generate();
        Assert.All(pin.AsEnumerable(), c => Assert.True(char.IsAsciiDigit(c)));
    }

    [Fact]
    public void Generate_DefaultLength_ReturnsSixDigitPin()
    {
        var generator = CreateGenerator();
        var pin = generator.Generate();
        Assert.Equal(6, pin.Length);
    }

    [Fact]
    public void Generate_WithRejectTrivial_NeverReturnsTrivialPin()
    {
        var generator = CreateGenerator(length: 4, rejectTrivialPatterns: true);

        for (var i = 0; i < 100; i++)
        {
            var pin = generator.Generate();
            var detector = new PinTrivialPatternDetector();
            Assert.False(detector.IsTrivial(pin), $"Trivial PIN generated: {pin}");
        }
    }

    [Fact]
    public void Generate_WithRejectTrivialDisabled_DoesNotReject()
    {
        var rng = new FakeRandomNumberGenerator(0);
        var generator = CreateGenerator(length: 4, rejectTrivialPatterns: false, rng: rng);

        var pin = generator.Generate();
        Assert.Equal("0000", pin);
    }

    [Fact]
    public void Generate_WhenAlwaysTrivial_ThrowsInvalidOperationException()
    {
        var rng = new FakeRandomNumberGenerator(0);
        var generator = CreateGenerator(length: 4, rejectTrivialPatterns: true, maxRetries: 3, rng: rng);

        Assert.Throws<InvalidOperationException>(() => generator.Generate());
    }

    [Fact]
    public void Constructor_CallsValidator()
    {
        var validator = new StubPinConfigurationValidator();
        _ = CreateGenerator(validator: validator);

        Assert.True(validator.WasCalled);
    }

    [Fact]
    public void Constructor_WhenValidatorThrows_ExceptionPropagates()
    {
        var validator = new StubPinConfigurationValidator(new ArgumentException("bad config"));

        Assert.Throws<ArgumentException>(() => CreateGenerator(validator: validator));
    }

    [Fact]
    public void GenerateBatch_ReturnsRequestedCount()
    {
        var generator = CreateGenerator();
        var result = generator.GenerateBatch(5);
        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public void GenerateBatch_AllPinsAreUnique()
    {
        var generator = CreateGenerator(length: 8);
        var result = generator.GenerateBatch(10);
        Assert.Equal(result.Items.Count, new HashSet<string>(result.Items, StringComparer.Ordinal).Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GenerateBatch_ZeroOrNegative_ThrowsArgumentOutOfRangeException(int count)
    {
        var generator = CreateGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateBatch(count));
    }

    [Fact]
    public void GenerateBatch_SingleItem_ReturnsSinglePin()
    {
        var generator = CreateGenerator();
        var result = generator.GenerateBatch(1);
        Assert.Single(result.Items);
    }

    [Fact]
    public void GenerateBatch_WhenDuplicatesExhausted_ReturnsPartialResultWithWarning()
    {
        var rng = new FakeRandomNumberGenerator(0);
        var generator = CreateGenerator(length: 4, rejectTrivialPatterns: false, rng: rng);

        var result = generator.GenerateBatch(2);

        Assert.True(result.Items.Count < 2);
        Assert.NotNull(result.Warning);
    }
}

internal class FakePinGeneratorConfig : IPinGeneratorConfig
{
    public int Length { get; init; } = 6;
    public bool RejectTrivialPatterns { get; init; }
    public int MaxRetries { get; init; } = 10;
}

internal class StubPinConfigurationValidator : IPinConfigurationValidator
{
    public bool WasCalled { get; private set; }
    private readonly Exception? _toThrow;

    public StubPinConfigurationValidator(Exception? toThrow = null) => _toThrow = toThrow;

    public void Validate(IPinGeneratorConfig config)
    {
        WasCalled = true;
        if (_toThrow is not null) throw _toThrow;
    }
}

internal class AlwaysTrivialDetector : IPinTrivialPatternDetector
{
    public bool IsTrivial(ReadOnlySpan<char> pin) => true;
}

internal class NeverTrivialDetector : IPinTrivialPatternDetector
{
    public bool IsTrivial(ReadOnlySpan<char> pin) => false;
}
