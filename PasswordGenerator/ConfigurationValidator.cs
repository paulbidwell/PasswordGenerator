using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator
{
    /// <summary>
    /// Validates password generator configuration to ensure it meets all requirements and constraints.
    /// </summary>
    public class ConfigurationValidator : IConfigurationValidator
    {
        /// <summary>
        /// Validates the generator configuration for consistency and feasibility.
        /// </summary>
        /// <param name="config">The configuration to validate.</param>
        /// <exception cref="ArgumentException">Thrown when configuration contains invalid values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when configuration is logically impossible to satisfy.</exception>
        public void Validate(IGeneratorConfig config)
        {
            ValidateCharacterSets(config);
            ValidateMinimumUsage(config);
            ValidateMaxRepetition(config);
        }

        private static void ValidateCharacterSets(IGeneratorConfig config)
        {
            if (config.CharacterSets.Any(characterSet => characterSet.Set.Length <= 0))
            {
                throw new ArgumentException("Character sets cannot be empty or null.");
            }

            if (config.CharacterSets.Any(characterSet => characterSet.Min < 0 || characterSet.Min > config.Length))
            {
                throw new ArgumentException("Minimum usage of a character set must be between 0 and the password length, inclusive.");
            }
        }

        private static void ValidateMinimumUsage(IGeneratorConfig config)
        {
            var minUseTotal = config.CharacterSets.Sum(characterSet => characterSet.Min);

            if (minUseTotal > config.Length)
            {
                throw new ArgumentException("The minimum usage of the character sets is more than the maximum length of the generated password.");
            }
        }

        private static void ValidateMaxRepetition(IGeneratorConfig config)
        {
            if (config.MaxRepetition < -1)
            {
                throw new ArgumentException("The maximum repetition should be -1 or more.");
            }

            if (config.MaxRepetition == 0)
            {
                ValidateNoRepetitionIsPossible(config);
            }
        }

        private static void ValidateNoRepetitionIsPossible(IGeneratorConfig config)
        {
            var totalUnique = config.CharacterSets.Sum(cs => cs.Set.Distinct().Count());
            if (totalUnique < config.Length)
            {
                throw new InvalidOperationException("Not enough unique characters to satisfy length without repetition.");
            }
        }
    }
}