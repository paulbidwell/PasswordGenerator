using PasswordGenerator.Core;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Generators
{
    /// <summary>
    /// Generates passwords based on configured character sets and constraints.
    /// </summary>
    public class CharacterGenerator(IGeneratorConfig config, ICharacterSelector characterSelector, IRandomNumberGenerator randomNumberGenerator, IPasswordShuffler passwordShuffler) : ICharacterGenerator
    {
        private const int MaxAttemptsPerCharacter = 1000;

        /// <summary>
        /// Generates a password as a character array based on the configuration.
        /// </summary>
        /// <returns>A character array representing the generated password.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the password cannot be generated due to impossible configuration.</exception>
        public char[] GeneratePassword()
        {
            var buffer = new char[config.Length];
            var index = 0;
            var characterCount = new Dictionary<char, int>();

            GenerateRequiredCharacters(buffer, ref index, characterCount);
            GenerateRandomCharacters(buffer, ref index, characterCount);

            passwordShuffler.Shuffle(buffer, config);
            return buffer;
        }

        private void GenerateRequiredCharacters(char[] buffer, ref int index, IDictionary<char, int> characterCount)
        {
            var requiredSets = config.CharacterSets.Where(set => set.Min > 0).ToArray();
            foreach (var set in requiredSets)
            {
                AddRequiredCharactersFromSet(buffer, ref index, characterCount, set);
            }
        }

        private void AddRequiredCharactersFromSet(char[] buffer, ref int index, IDictionary<char, int> characterCount, ICharacterSet set)
        {
            var added = 0;
            var maxAttempts = MaxAttemptsPerCharacter * set.Min;
            var attempts = 0;

            while (added < set.Min)
            {
                if (++attempts > maxAttempts)
                {
                    throw new InvalidOperationException(
                        $"Unable to satisfy minimum character requirements after {attempts} attempts. " +
                        "This may indicate the configuration is impossible to satisfy (e.g., MaxRepetition too low).");
                }

                var next = GetValidCharacter(set.Set, characterCount);

                if (next.HasValue)
                {
                    buffer[index++] = next.Value;
                    added++;
                }
            }
        }

        private void GenerateRandomCharacters(char[] buffer, ref int index, IDictionary<char, int> characterCount)
        {
            var attempts = 0;

            while (index < config.Length)
            {
                var remainingCharacters = config.Length - index;
                var maxAttempts = MaxAttemptsPerCharacter * remainingCharacters;

                if (++attempts > maxAttempts)
                {
                    throw new InvalidOperationException(
                        $"Unable to complete password generation after {attempts} attempts. " +
                        "This may indicate the configuration is impossible to satisfy (e.g., MaxRepetition too low).");
                }

                var characterSet = PickRandomCharacterSet();
                var next = GetValidCharacter(characterSet.Set, characterCount);

                if (next.HasValue)
                {
                    buffer[index++] = next.Value;
                    attempts = 0; // Reset attempts counter after successful character addition
                }
            }
        }

        private ICharacterSet PickRandomCharacterSet()
        {
            var randomIndex = randomNumberGenerator.GetRandomIntInRange(0, config.CharacterSets.Count - 1);
            return config.CharacterSets[randomIndex];
        }

        private char? GetValidCharacter(char[] characterSet, IDictionary<char, int> characterCount)
        {
            var next = characterSelector.GetNextCharacter(characterSet);

            if (config.MaxRepetition >= 0)
            {
                return HandleRepetition(next, characterCount);
            }

            return next;
        }

        private char? HandleRepetition(char character, IDictionary<char, int> characterCount)
        {
            if (!characterCount.TryAddCount(character, config.MaxRepetition, out _))
            {
                return null;
            }
            return character;
        }
    }
}