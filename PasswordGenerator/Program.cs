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
    private static void Main()
    {
        var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        Directory.SetCurrentDirectory(Path.GetDirectoryName(applicationPath) ?? string.Empty);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var host = Host.CreateDefaultBuilder()
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
                services.AddTransient<IRandomNumberGenerator, SecureRng>();

                services.Configure<PasswordGeneratorOptions>(configuration.GetSection("PasswordGenerator"));
            })
            .Build();

        var config = configuration.GetSection("PasswordGenerator").Get<PasswordGeneratorOptions>();

        var passwords = new List<string>();

        if (config != null)
        {
            for (var i = 0; i < config.PasswordsToGenerate; i++)
            {
                var generator = host.Services.GetRequiredService<IGenerator>();

                var password = generator.Generate();

                passwords.Add(password);

                if (config.OutputToConsole)
                {
                    Console.WriteLine(password);
                }
            }

            if (config.OutputToFile)
            {
                File.WriteAllLines(config.OutputPath, passwords);
            }
        }
    }
}