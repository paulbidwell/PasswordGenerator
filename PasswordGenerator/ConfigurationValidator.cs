using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class ConfigurationValidator : IConfigurationValidator
    {
        public void Validate(IGeneratorConfig config)
        {
            if (config.CharacterSets.Any(characterSet => characterSet.Set.Length <= 0))
            {
                throw new ArgumentException("Character sets cannot be empty or null.");
            }

            var minUseTotal = config.CharacterSets.Sum(characterSet => characterSet.Min);

            if (minUseTotal > config.Length)
            {
                throw new ArgumentException("The minimum usage of the character sets is more than the maximum length of the generated password.");
            }

            if (config.CharacterSets.Any(characterSet => characterSet.Min < 0 || characterSet.Min > config.Length))
            {
                throw new ArgumentException("Minimum usage of a character set must be between 0 and the password length, inclusive.");
            }

            switch (config.MaxRepetition)
            {
                case < -1:
                    throw new ArgumentException("The maximum repetition should be -1 or more.");

                case 0:
                {
                    var totalUnique = config.CharacterSets.Sum(cs => cs.Set.Distinct().Count());
                    if (totalUnique < config.Length)
                    {
                        throw new InvalidOperationException("Not enough unique characters to satisfy length without repetition.");
                    }

                    break;
                }
            }
        }
    }
}