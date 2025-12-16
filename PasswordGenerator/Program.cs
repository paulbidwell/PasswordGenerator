using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordGenerator.Core;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;
using PasswordGenerator.Generators;
using PasswordGenerator.Sets;
using PasswordGenerator.Shufflers;

namespace PasswordGenerator;

internal class Program
{
    private const int BatchSize = 10000;

    private static void Main()
    {
        try
        {
            var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(applicationPath) ?? string.Empty);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddTransient<ICharacterGenerator, CharacterGenerator>();
                    services.AddTransient<ICharacterSelector, CharacterSelector>();
                    services.AddTransient<ICharacterSetManager, CharacterSetManager>();
                    services.AddTransient<ICharacterSetShuffler, CharacterSetShuffler>();
                    services.AddTransient<ICollectionShuffler, CollectionShuffler>();
                    services.AddTransient<IConfigurationValidator, ConfigurationValidator>();
                    services.AddTransient<IGenerator, Generator>();
                    services.AddTransient<IGeneratorConfig, GeneratorConfig>();
                    services.AddTransient<IPasswordShuffler, PasswordShuffler>();
                    services.AddSingleton<IRandomNumberGenerator, SecureRng>();

                    services.Configure<PasswordGeneratorOptions>(configuration.GetSection("PasswordGenerator"));
                })
                .Build();

            var config = configuration.GetSection("PasswordGenerator").Get<PasswordGeneratorOptions>();

            if (config == null)
            {
                Console.Error.WriteLine("Failed to load configuration from appsettings.json");
                return;
            }

            ValidateConfiguration(config);

            GeneratePasswords(host, config);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void GeneratePasswords(IHost host, PasswordGeneratorOptions config)
    {
        if (config.OutputToFile)
        {
            GeneratePasswordsToFile(host, config);
        }
        else
        {
            GeneratePasswordsToConsole(host, config);
        }
    }

    private static void GeneratePasswordsToFile(IHost host, PasswordGeneratorOptions config)
    {
        var passwordsBatch = new List<string>(BatchSize);
        var generator = host.Services.GetRequiredService<IGenerator>();

        try
        {
            using var writer = new StreamWriter(config.OutputPath, false);

            for (var i = 0; i < config.PasswordsToGenerate; i++)
            {
                var password = generator.Generate();

                passwordsBatch.Add(password);

                if (config.OutputToConsole)
                {
                    Console.WriteLine(password);
                }

                if (passwordsBatch.Count >= BatchSize || i == config.PasswordsToGenerate - 1)
                {
                    foreach (var pwd in passwordsBatch)
                    {
                        writer.WriteLine(pwd);
                    }
                    passwordsBatch.Clear();
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Access denied writing to file: {config.OutputPath}", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"Failed to write passwords to file: {config.OutputPath}", ex);
        }
    }

    private static void GeneratePasswordsToConsole(IHost host, PasswordGeneratorOptions config)
    {
        var generator = host.Services.GetRequiredService<IGenerator>();

        for (var i = 0; i < config.PasswordsToGenerate; i++)
        {
            var password = generator.Generate();

            if (config.OutputToConsole)
            {
                Console.WriteLine(password);
            }
        }
    }

    private static void ValidateConfiguration(PasswordGeneratorOptions config)
    {
        const int maxPasswordsToGenerate = 10_000_000;
        
        if (config.PasswordsToGenerate < 1)
        {
            throw new ArgumentException("PasswordsToGenerate must be at least 1.");
        }

        if (config.PasswordsToGenerate > maxPasswordsToGenerate)
        {
            throw new ArgumentException($"PasswordsToGenerate cannot exceed {maxPasswordsToGenerate} to prevent resource exhaustion.");
        }

        if (config.Length < 1)
        {
            throw new ArgumentException("Password length must be at least 1.");
        }

        if (config.OutputToFile && string.IsNullOrWhiteSpace(config.OutputPath))
        {
            throw new ArgumentException("OutputPath must be specified when OutputToFile is true.");
        }

        if (config.OutputToFile)
        {
            ValidateOutputPath(config.OutputPath);
        }
    }

    private static void ValidateOutputPath(string outputPath)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Invalid output path specified.");
        }

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Output directory does not exist: {directory}");
        }
    }
}