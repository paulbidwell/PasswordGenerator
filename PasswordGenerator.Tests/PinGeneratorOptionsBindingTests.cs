using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.RateLimiting;
using Xunit;

namespace PasswordGenerator.Tests;

public class PinGeneratorOptionsBindingTests
{
    [Fact]
    public void AddPinGenerator_Registers_IOptions()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator()
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PinGeneratorOptions>>();

        Assert.NotNull(options);
        Assert.Equal(6, options.Value.Length);
    }

    [Fact]
    public void AddPinGenerator_Registers_IOptionsMonitor()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator()
            .BuildServiceProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<PinGeneratorOptions>>();

        Assert.NotNull(monitor);
        Assert.True(monitor.CurrentValue.RejectTrivialPatterns);
    }

    [Fact]
    public void AddPinGenerator_WithDelegate_IOptions_ReflectsConfiguredValues()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator(o => o.Length = 8)
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PinGeneratorOptions>>();

        Assert.Equal(8, options.Value.Length);
    }

    [Fact]
    public void AddPinGenerator_WithDelegate_BridgeAndIOptions_AreConsistent()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator(o =>
            {
                o.Length = 10;
                o.MaxRetries = 20;
            })
            .BuildServiceProvider();

        var manager = provider.GetRequiredService<IPinGeneratorOptionsManager>();
        var options = provider.GetRequiredService<IOptions<PinGeneratorOptions>>();

        Assert.Equal(manager.Current.Length, options.Value.Length);
        Assert.Equal(manager.Current.MaxRetries, options.Value.MaxRetries);
    }

    [Fact]
    public void AddPinGenerator_WithConfigSection_BindsScalarProperties()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PinGenerator:Length"] = "8",
                ["PinGenerator:RejectTrivialPatterns"] = "false",
                ["PinGenerator:MaxRetries"] = "5",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddPinGenerator("PinGenerator")
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PinGeneratorOptions>>();

        Assert.Equal(8, options.Value.Length);
        Assert.False(options.Value.RejectTrivialPatterns);
        Assert.Equal(5, options.Value.MaxRetries);
    }

    [Fact]
    public void AddPinGenerator_WithConfigSection_BridgeReflectsBoundValues()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Pin:Length"] = "10",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddPinGenerator("Pin")
            .BuildServiceProvider();

        var manager = provider.GetRequiredService<IPinGeneratorOptionsManager>();

        Assert.Equal(10, manager.Current.Length);
    }

    [Fact]
    public void AddPinGenerator_WithIConfigurationSection_BindsValues()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MyPin:Length"] = "8",
                ["MyPin:MaxRetries"] = "3",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddPinGenerator(config.GetSection("MyPin"))
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PinGeneratorOptions>>();

        Assert.Equal(8, options.Value.Length);
        Assert.Equal(3, options.Value.MaxRetries);
    }

    [Fact]
    public void AddPinGenerator_WithConfigSection_GeneratesCorrectLengthPin()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PG:Length"] = "8",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddPinGenerator("PG")
            .BuildServiceProvider();

        var pin = provider.GetRequiredService<IPinGenerator>().Generate();

        Assert.Equal(8, pin.Length);
    }

    [Fact]
    public void ManagerConfigure_MutatesIOptionsMonitorCurrentValue()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator()
            .BuildServiceProvider();

        var manager = provider.GetRequiredService<IPinGeneratorOptionsManager>();
        var monitor = provider.GetRequiredService<IOptionsMonitor<PinGeneratorOptions>>();

        manager.Configure(o => o.Length = 10);

        Assert.Equal(10, monitor.CurrentValue.Length);
    }

    [Fact]
    public void PinRateLimiterOptions_Registered_ViaOptionsPattern()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator()
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PinRateLimiterOptions>>();

        Assert.Equal(5, options.Value.MaxAttempts);
        Assert.Equal(TimeSpan.FromMinutes(5), options.Value.Window);
    }

    [Fact]
    public void IOptionsSnapshot_ResolvesInScope()
    {
        using var provider = new ServiceCollection()
            .AddPinGenerator(o => o.Length = 8)
            .BuildServiceProvider();

        using var scope = provider.CreateScope();
        var snapshot = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PinGeneratorOptions>>();

        Assert.Equal(8, snapshot.Value.Length);
    }
}
