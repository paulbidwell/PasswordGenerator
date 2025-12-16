using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators
{
    /// <summary>
    /// Orchestrates password generation by validating configuration and delegating to character generator.
    /// </summary>
    public class Generator(IGeneratorConfig config, IConfigurationValidator validator, ICharacterGenerator characterGenerator) : IGenerator
    {
        /// <summary>
        /// Generates a password string based on the configuration.
        /// </summary>
        /// <returns>A generated password string.</returns>
        /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when configuration is impossible to satisfy.</exception>
        public string Generate()
        {
            validator.Validate(config);
            var generated = characterGenerator.GeneratePassword();
            return new string(generated);
        }
    }
}