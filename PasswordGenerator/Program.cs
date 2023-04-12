using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PasswordGenerator;

internal class Program
{
    private static void Main()
    {
        var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        Directory.SetCurrentDirectory(Path.GetDirectoryName(applicationPath) ?? string.Empty);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.Configure<PasswordGeneratorOptions>(configuration.GetSection("PasswordGenerator"));
                services.AddTransient<IGenerator, Generator>();
                services.AddTransient<IUtility, Utility>();
            })
            .Build();

        var config = configuration.GetSection("PasswordGenerator").Get<PasswordGeneratorOptions>();

        var passwords = new List<string>();

        if (config != null)
        {
            for (var i = 0; i < config.PasswordsToGenerate; i++)
            {
                var generator = host.Services.GetRequiredService<IGenerator>();

                var password = generator
                    .Configure()
                    .Generate();

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