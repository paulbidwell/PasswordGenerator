using PasswordGenerator.Core;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Generators
{
    public class CharacterGenerator(IGeneratorConfig config, ICharacterSelector characterSelector, IRandomNumberGenerator randomNumberGenerator, IPasswordShuffler passwordShuffler) : ICharacterGenerator
    {
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

            while (added < set.Min)
            {
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
            while (index < config.Length)
            {
                var characterSet = PickRandomCharacterSet();
                var next = GetValidCharacter(characterSet.Set, characterCount);

                if (next.HasValue)
                {
                    buffer[index++] = next.Value;
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

            if (config.MaxRepetition > -1)
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