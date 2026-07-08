using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;
using PasswordGenerator.Core.Interfaces.Strength;
using PasswordGenerator.Generators;
using PasswordGenerator.RateLimiting;
using PasswordGenerator.Sets;
using PasswordGenerator.Shufflers;
using PasswordGenerator.Strength;

namespace PasswordGenerator;

/// <summary>Extension methods for registering password and PIN generators with <see cref="IServiceCollection"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers a password generator with default options.</summary>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services)
        => services.AddPasswordGenerator(_ => { });

    /// <summary>Registers a password generator configured via <paramref name="configure"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate that configures <see cref="PasswordGeneratorOptions"/>.</param>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services, Action<PasswordGeneratorOptions> configure)
    {
        services.AddOptions<PasswordGeneratorOptions>().Configure(configure);

        return services.AddPasswordGeneratorCore();
    }

    /// <summary>Registers a password generator bound to the configuration section at <paramref name="configurationSectionPath"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSectionPath">Path to the configuration section (e.g. <c>"PasswordGenerator"</c>).</param>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services, string configurationSectionPath)
    {
        services.AddOptions<PasswordGeneratorOptions>()
                .BindConfiguration(configurationSectionPath);

        return services.AddPasswordGeneratorCore();
    }

    /// <summary>Registers a password generator bound to <paramref name="configurationSection"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section to bind.</param>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services, IConfiguration configurationSection)
    {
        services.AddOptions<PasswordGeneratorOptions>()
                .Bind(configurationSection);

        return services.AddPasswordGeneratorCore();
    }

    /// <summary>Registers a password generator from a pre-built <paramref name="options"/> instance (snapshot is taken).</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The options instance to snapshot.</param>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services, PasswordGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var snapshot = options.Snapshot();
        return services.AddPasswordGenerator(snapshot.CopyTo);
    }

    private static IServiceCollection AddPasswordGeneratorCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IPasswordGeneratorOptionsManager, PasswordGeneratorOptionsManager>();

        services.AddTransient<ICharacterGenerator, CharacterGenerator>();
        services.AddTransient<ICharacterSelector, CharacterSelector>();
        services.AddTransient<ICharacterSetManager, CharacterSetManager>();
        services.AddTransient<ICharacterSetShuffler, CharacterSetShuffler>();
        services.AddTransient<ICollectionShuffler, CollectionShuffler>();
        services.AddTransient<IConfigurationValidator, ConfigurationValidator>();
        services.AddTransient<IGenerator, Generator>();
        services.AddTransient<IGeneratorConfig, GeneratorConfig>();
        services.AddTransient<IPasswordShuffler, PasswordShuffler>();
        services.AddTransient<IPositionConstraintEnforcer, PositionConstraintEnforcer>();
        services.AddTransient<IRandomNumberGenerator, SecureRng>();
        services.AddTransient<IPasswordStrengthAnalyzer, PasswordStrengthAnalyzer>();

        return services;
    }

    /// <summary>Registers a PIN generator with default options.</summary>
    public static IServiceCollection AddPinGenerator(this IServiceCollection services)
        => services.AddPinGenerator(_ => { });

    /// <summary>Registers a PIN generator configured via <paramref name="configure"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate that configures <see cref="PinGeneratorOptions"/>.</param>
    public static IServiceCollection AddPinGenerator(this IServiceCollection services, Action<PinGeneratorOptions> configure)
    {
        services.AddOptions<PinGeneratorOptions>().Configure(configure);

        return services.AddPinGeneratorCore();
    }

    /// <summary>Registers a PIN generator bound to the configuration section at <paramref name="configurationSectionPath"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSectionPath">Path to the configuration section.</param>
    public static IServiceCollection AddPinGenerator(this IServiceCollection services, string configurationSectionPath)
    {
        services.AddOptions<PinGeneratorOptions>()
                .BindConfiguration(configurationSectionPath);

        return services.AddPinGeneratorCore();
    }

    /// <summary>Registers a PIN generator bound to <paramref name="configurationSection"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section to bind.</param>
    public static IServiceCollection AddPinGenerator(this IServiceCollection services, IConfiguration configurationSection)
    {
        services.AddOptions<PinGeneratorOptions>()
                .Bind(configurationSection);

        return services.AddPinGeneratorCore();
    }

    private static IServiceCollection AddPinGeneratorCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IPinGeneratorOptionsManager, PinGeneratorOptionsManager>();

        services.TryAddTransient<IRandomNumberGenerator, SecureRng>();
        services.AddTransient<IPinGeneratorConfig, PinGeneratorConfig>();
        services.AddTransient<IPinConfigurationValidator, PinConfigurationValidator>();
        services.AddTransient<IPinTrivialPatternDetector, PinTrivialPatternDetector>();
        services.AddTransient<IPinGenerator, PinGenerator>();

        services.AddOptions<PinRateLimiterOptions>();
        services.TryAddSingleton<IPinRateLimiter, PinRateLimiter>();

        return services;
    }
}
