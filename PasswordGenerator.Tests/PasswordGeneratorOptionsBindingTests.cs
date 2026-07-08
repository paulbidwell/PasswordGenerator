using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PasswordGenerator.Core.Interfaces.Generators;
using Xunit;

namespace PasswordGenerator.Tests;

public class PasswordGeneratorOptionsBindingTests
{
    [Fact]
    public void AddPasswordGenerator_Registers_IOptions()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PasswordGeneratorOptions>>();

        Assert.NotNull(options);
        Assert.Equal(22, options.Value.Length);
    }

    [Fact]
    public void AddPasswordGenerator_Registers_IOptionsMonitor()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .BuildServiceProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<PasswordGeneratorOptions>>();

        Assert.NotNull(monitor);
        Assert.Equal(22, monitor.CurrentValue.Length);
    }

    [Fact]
    public void AddPasswordGenerator_WithDelegate_IOptions_ReflectsConfiguredValues()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator(o => o.Length = 16)
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PasswordGeneratorOptions>>();

        Assert.Equal(16, options.Value.Length);
    }

    [Fact]
    public void AddPasswordGenerator_WithDelegate_BridgeAndIOptions_AreConsistent()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator(o =>
            {
                o.Length = 30;
                o.MaxRepetition = 3;
            })
            .BuildServiceProvider();

        var manager = provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
        var options = provider.GetRequiredService<IOptions<PasswordGeneratorOptions>>();

        Assert.Equal(manager.Current.Length, options.Value.Length);
        Assert.Equal(manager.Current.MaxRepetition, options.Value.MaxRepetition);
    }

    [Fact]
    public void AddPasswordGenerator_WithConfigSection_BindsScalarProperties()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PasswordGenerator:Length"] = "32",
                ["PasswordGenerator:MaxRepetition"] = "3",
                ["PasswordGenerator:AllowSequences"] = "true",
                ["PasswordGenerator:ExcludeAmbiguous"] = "true",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddPasswordGenerator("PasswordGenerator")
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PasswordGeneratorOptions>>();

        Assert.Equal(32, options.Value.Length);
        Assert.Equal(3, options.Value.MaxRepetition);
        Assert.True(options.Value.AllowSequences);
        Assert.True(options.Value.ExcludeAmbiguous);
    }

    [Fact]
    public void AddPasswordGenerator_WithConfigSection_BridgeReflectsBoundValues()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Pwd:Length"] = "28",
                ["Pwd:MustStartWithLetter"] = "true",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddPasswordGenerator("Pwd")
            .BuildServiceProvider();

        var manager = provider.GetRequiredService<IPasswordGeneratorOptionsManager>();

        Assert.Equal(28, manager.Current.Length);
        Assert.True(manager.Current.MustStartWithLetter);
    }

    [Fact]
    public void AddPasswordGenerator_WithIConfigurationSection_BindsValues()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MySection:Length"] = "18",
                ["MySection:ExcludeLeadingTrailingSymbols"] = "true",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddPasswordGenerator(config.GetSection("MySection"))
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PasswordGeneratorOptions>>();

        Assert.Equal(18, options.Value.Length);
        Assert.True(options.Value.ExcludeLeadingTrailingSymbols);
    }

    [Fact]
    public void AddPasswordGenerator_WithConfigSection_GeneratesCorrectLengthPassword()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PG:Length"] = "16",
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddPasswordGenerator("PG")
            .BuildServiceProvider();

        var password = provider.GetRequiredService<IGenerator>().Generate();

        Assert.Equal(16, password.Length);
    }

    [Fact]
    public void ManagerConfigure_MutatesIOptionsMonitorCurrentValue()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .BuildServiceProvider();

        var manager = provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
        var monitor = provider.GetRequiredService<IOptionsMonitor<PasswordGeneratorOptions>>();

        manager.Configure(o => o.Length = 40);

        Assert.Equal(40, monitor.CurrentValue.Length);
    }

    [Fact]
    public void IOptionsSnapshot_ResolvesInScope()
    {
        using var provider = new ServiceCollection()
            .AddPasswordGenerator(o => o.Length = 14)
            .BuildServiceProvider();

        using var scope = provider.CreateScope();
        var snapshot = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PasswordGeneratorOptions>>();

        Assert.Equal(14, snapshot.Value.Length);
    }
}
