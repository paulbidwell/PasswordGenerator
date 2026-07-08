using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Core.Interfaces.Generators;
using Xunit;

namespace PasswordGenerator.Tests;

public class PinServiceCollectionExtensionsTests : IDisposable
{
    private readonly ServiceProvider _provider;

    public PinServiceCollectionExtensionsTests()
    {
        _provider = new ServiceCollection()
            .AddPinGenerator()
            .BuildServiceProvider();
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public void AddPinGenerator_ResolvesPinGenerator()
    {
        var generator = _provider.GetRequiredService<IPinGenerator>();
        Assert.NotNull(generator);
    }

    [Fact]
    public void AddPinGenerator_ResolvesOptionsManager()
    {
        var manager = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        Assert.NotNull(manager);
    }

    [Fact]
    public void AddPinGenerator_DefaultOptions_GeneratesSixDigitPin()
    {
        var generator = _provider.GetRequiredService<IPinGenerator>();
        var pin = generator.Generate();
        Assert.Equal(6, pin.Length);
    }

    [Fact]
    public void AddPinGenerator_DefaultOptions_GeneratesDigitsOnly()
    {
        var generator = _provider.GetRequiredService<IPinGenerator>();
        var pin = generator.Generate();
        Assert.All(pin.AsEnumerable(), c => Assert.True(char.IsAsciiDigit(c)));
    }

    [Fact]
    public void AddPinGenerator_WithConfigure_AppliesLengthAtRegistration()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator(o => o.Length = 8)
            .BuildServiceProvider();

        var pin = provider.GetRequiredService<IPinGenerator>().Generate();
        Assert.Equal(8, pin.Length);
    }

    [Fact]
    public void OptionsManager_Current_ReflectsDefaultLength()
    {
        var manager = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        Assert.Equal(6, manager.Current.Length);
    }

    [Fact]
    public void OptionsManager_Current_ReflectsDefaultRejectTrivialPatterns()
    {
        var manager = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        Assert.True(manager.Current.RejectTrivialPatterns);
    }

    [Fact]
    public void OptionsManager_Configure_UpdatesCurrentLength()
    {
        var manager = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        manager.Configure(o => o.Length = 8);
        Assert.Equal(8, manager.Current.Length);
    }

    [Fact]
    public void OptionsManager_Configure_ChangesReflectedInNextGenerate()
    {
        var manager = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        manager.Configure(o => o.Length = 8);

        var pin = _provider.GetRequiredService<IPinGenerator>().Generate();
        Assert.Equal(8, pin.Length);
    }

    [Fact]
    public void OptionsManager_Configure_CanBeCalledMultipleTimes()
    {
        var manager = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        manager.Configure(o => o.Length = 8);
        manager.Configure(o => o.Length = 10);
        Assert.Equal(10, manager.Current.Length);
    }

    [Fact]
    public void OptionsManager_IsSingleton_SameInstanceResolvedTwice()
    {
        var first = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        var second = _provider.GetRequiredService<IPinGeneratorOptionsManager>();
        Assert.Same(first, second);
    }

    [Fact]
    public void AddPinGenerator_ResolvesPinRateLimiter()
    {
        var limiter = _provider.GetRequiredService<IPinRateLimiter>();
        Assert.NotNull(limiter);
    }

    [Fact]
    public void PinRateLimiter_IsSingleton()
    {
        var first = _provider.GetRequiredService<IPinRateLimiter>();
        var second = _provider.GetRequiredService<IPinRateLimiter>();
        Assert.Same(first, second);
    }

    [Fact]
    public void AddPinGenerator_CoexistsWithPasswordGenerator()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .AddPinGenerator()
            .BuildServiceProvider();

        var pinGen = provider.GetRequiredService<IPinGenerator>();
        var pwdGen = provider.GetRequiredService<IGenerator>();

        Assert.NotNull(pinGen);
        Assert.NotNull(pwdGen);
    }

    [Fact]
    public void AddPinGenerator_DefaultRejectTrivial_NeverReturnsTrivialPin()
    {
        var generator = _provider.GetRequiredService<IPinGenerator>();

        for (var i = 0; i < 50; i++)
        {
            var pin = generator.Generate();
            var detector = new Generators.PinTrivialPatternDetector();
            Assert.False(detector.IsTrivial(pin), $"Trivial PIN generated: {pin}");
        }
    }
}
