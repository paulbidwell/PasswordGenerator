using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class Generator : IGenerator
    {
        private readonly IGeneratorConfig _config;
        private readonly ICollectionShuffler _collectionShuffler;
        private readonly ICharacterSelector _characterSelector;
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public Generator(IGeneratorConfig config, ICollectionShuffler collectionShuffler, ICharacterSelector characterSelector, IRandomNumberGenerator randomNumberGenerator)
        {
            _config = config;
            _collectionShuffler = collectionShuffler;
            _characterSelector = characterSelector;
            _randomNumberGenerator = randomNumberGenerator;
        }

        public string Generate()
        {
            var generated = GeneratePassword();
            return new string(generated.ToArray());
        }

        private List<char> GeneratePassword()
        {
            ValidateConfiguration();

            var generated = new List<char>();
            var characterCount = new Dictionary<char, int>();

            GenerateRequiredCharacters(generated, characterCount);
            GenerateRandomCharacters(generated, characterCount);

            _collectionShuffler.Shuffle(generated, _config.AllowSequences, _config.AllowUpperLowerSequences);

            return generated;
        }

        private void ValidateConfiguration()
        {
            if (_config.CharacterSets.Any(characterSet => !characterSet.Set.Any()))
            {
                throw new ArgumentException("Character sets cannot be empty or null.");
            }

            var minUseTotal = _config.CharacterSets.Sum(characterSet => characterSet.Min);

            if (minUseTotal > _config.Length)
            {
                throw new ArgumentException("The minimum usage of the character sets is more than the maximum length of the generated password.");
            }

            if (_config.CharacterSets.Any(characterSet => characterSet.Min < 0 || characterSet.Min > _config.Length))
            {
                throw new ArgumentException("Minimum usage of a character set must be between 0 and the password length, inclusive.");
            }

            if (_config.MaxRepetition < -1)
            {
                throw new ArgumentException("The maximum repetition should be -1 or more.");
            }
        }

        private void GenerateRequiredCharacters(ICollection<char> generated, IDictionary<char, int> characterCount)
        {
            var requiredSets = _config.CharacterSets.Where(characterSet => characterSet.Min > 0).ToArray();

            foreach (var requiredSet in requiredSets)
            {
                AddRequiredCharactersFromSet(generated, characterCount, requiredSet);
            }
        }

        private void AddRequiredCharactersFromSet(ICollection<char> generated, IDictionary<char, int> characterCount, ICharacterSet requiredSet)
        {
            for (var i = 0; i < requiredSet.Min;)
            {
                char? nextCharacter = _characterSelector.GetNextCharacter(requiredSet.Set);

                if (_config.MaxRepetition > -1)
                {
                    nextCharacter = HandleRepetition((char)nextCharacter, characterCount);
                }

                if (nextCharacter.HasValue)
                {
                    generated.Add((char)nextCharacter);
                    i++;
                }
            }
        }

        private void GenerateRandomCharacters(ICollection<char> generated, IDictionary<char, int> characterCount)
        {
            while (generated.Count < _config.Length)
            {
                var randomSetIndex = _randomNumberGenerator.GetRandomIntInRange(0, _config.CharacterSets.Count - 1);
                var characterSet = _config.CharacterSets[randomSetIndex];

                char? nextCharacter = _characterSelector.GetNextCharacter(characterSet.Set);

                if (_config.MaxRepetition >= 0)
                {
                    nextCharacter = HandleRepetition((char)nextCharacter, characterCount);
                }

                if (nextCharacter.HasValue)
                {
                    generated.Add((char)nextCharacter);
                }
            }
        }

        private char? HandleRepetition(char character, IDictionary<char, int> characterCount)
        {
            return character.IsRepeated(characterCount, _config.MaxRepetition)
                ? null
                : character;
        }
    }
}