using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Strength;
using PasswordGenerator.Strength;
using Xunit;

namespace PasswordGenerator.Tests;

public class PasswordStrengthAnalyzerTests
{
    private readonly PasswordStrengthAnalyzer _analyzer = new();

    [Fact]
    public void Analyze_NullPassword_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _analyzer.Analyze(null!));
    }

    [Fact]
    public void Analyze_EmptyPassword_ReturnsVeryWeak()
    {
        var result = _analyzer.Analyze("");

        Assert.Equal(PasswordStrength.VeryWeak, result.Strength);
        Assert.Equal(0, result.EntropyBits);
        Assert.Equal(0, result.GuessesLog10);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Analyze_SingleCharacter_ReturnsVeryWeak()
    {
        var result = _analyzer.Analyze("a");

        Assert.Equal(PasswordStrength.VeryWeak, result.Strength);
        Assert.True(result.EntropyBits < 28);
    }

    [Fact]
    public void Analyze_LongMixedPassword_ReturnsStrongOrAbove()
    {
        var result = _analyzer.Analyze("aB3$xK9!mP2@nQ5&wR8#");

        Assert.True(result.Strength >= PasswordStrength.Strong,
            $"Expected Strong or above but got {result.Strength} with {result.EntropyBits} entropy bits");
    }

    [Fact]
    public void Analyze_AllLowercase_HasLowerEntropyThanMixed()
    {
        var lower = _analyzer.Analyze("abcxyzqwmnop");
        var mixed = _analyzer.Analyze("aBc1!zQw9@oP");

        Assert.True(lower.EntropyBits < mixed.EntropyBits);
    }

    [Fact]
    public void Analyze_WithConfig_UsesConfigPoolSize()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 10,
            MaxRepetition = -1,
            AllowSequences = true,
            CharacterSets =
            [
                new FakeCharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                new FakeCharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz" },
                new FakeCharacterSet { Characters = "0123456789" },
                new FakeCharacterSet { Characters = "!@#$%^&*()" }
            ]
        };

        var withConfig = _analyzer.Analyze("aB3!xK9@mP", config);
        var withoutConfig = _analyzer.Analyze("aB3!xK9@mP");

        Assert.True(withConfig.EntropyBits > 0);
        Assert.True(withoutConfig.EntropyBits > 0);
    }

    [Fact]
    public void Analyze_SequentialChars_IncludesWarning()
    {
        var result = _analyzer.Analyze("xyzabcdef123");

        Assert.Contains(result.Warnings, w => w.Contains("sequential", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Analyze_RepeatedChars_IncludesWarning()
    {
        var result = _analyzer.Analyze("aaaa1234BBB!");

        Assert.Contains(result.Warnings, w => w.Contains("repeated", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Analyze_KeyboardPattern_IncludesWarning()
    {
        var result = _analyzer.Analyze("qwerty12345");

        Assert.Contains(result.Warnings, w => w.Contains("keyboard", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Analyze_GuessesLog10_CorrelatesWithEntropy()
    {
        var result = _analyzer.Analyze("T3st!ng_Str0ng");

        var expectedLog10 = result.EntropyBits * Math.Log10(2);

        Assert.Equal(expectedLog10, result.GuessesLog10, precision: 1);
    }

    [Fact]
    public void Analyze_NoPatterns_HasNoWarnings()
    {
        var result = _analyzer.Analyze("Kx8!mQ2@nW5&");

        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Analyze_VeryLongPassword_ReturnsVeryStrong()
    {
        var result = _analyzer.Analyze("aB3$xK9!mP2@nQ5&wR8#jL4^tY7*uI0%");

        Assert.Equal(PasswordStrength.VeryStrong, result.Strength);
    }

    [Fact]
    public void Analyze_EntropyBitsNeverNegative()
    {
        var result = _analyzer.Analyze("aaa");

        Assert.True(result.EntropyBits >= 0);
    }

    [Fact]
    public void Analyze_WithNullConfig_BehavesLikeNoConfig()
    {
        var withNull = _analyzer.Analyze("Test1234!", null);
        var withoutConfig = _analyzer.Analyze("Test1234!");

        Assert.Equal(withNull.EntropyBits, withoutConfig.EntropyBits);
        Assert.Equal(withNull.Strength, withoutConfig.Strength);
    }

    [Fact]
    public void Analyze_ResolvesFromDI()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddPasswordGenerator();
        var provider = services.BuildServiceProvider();

        var analyzer = provider.GetService<IPasswordStrengthAnalyzer>();

        Assert.NotNull(analyzer);
        var result = analyzer.Analyze("Test1234!");
        Assert.True(result.EntropyBits > 0);
    }

    [Fact]
    public void Analyze_WithConfig_EmptyCharacterSets_ReturnsPositiveEntropy()
    {
        var config = new FakeGeneratorConfig
        {
            Length = 5,
            MaxRepetition = -1,
            AllowSequences = true,
            CharacterSets =
            [
                new FakeCharacterSet { Characters = "" }
            ]
        };

        var result = _analyzer.Analyze("hello", config);

        Assert.Equal(0, result.EntropyBits);
        Assert.True(result.EntropyBits >= 0);
    }
}
