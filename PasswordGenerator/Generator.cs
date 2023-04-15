using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class Generator : IGenerator
    {
        private readonly Configuration _config;
        private readonly ICollectionShuffler _collectionShuffler;
        private readonly ICharacterSelector _characterSelector;
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public Generator(Configuration config, ICollectionShuffler collectionShuffler, ICharacterSelector characterSelector, IRandomNumberGenerator randomNumberGenerator)
        {
            _config = config;
            _collectionShuffler = collectionShuffler;
            _characterSelector = characterSelector;
            _randomNumberGenerator = randomNumberGenerator;
        }

        public string Generate()
        {
            var generated = GeneratePassword(_config);
            return new string(generated.ToArray());
        }

        private List<char> GeneratePassword(Configuration config)
        {
            ValidateConfiguration(config);

            var generated = new List<char>();
            var characterCount = new Dictionary<char, int>();

            GenerateRequiredCharacters(generated, characterCount, config);
            GenerateRandomCharacters(generated, characterCount, config);

            _collectionShuffler.Shuffle(generated, config.AllowSequences, config.AllowUpperLowerSequences);

            return generated;
        }

        private void ValidateConfiguration(Configuration config)
        {
            if (config.CharacterSets.Any(characterSet => !characterSet.Set.Any()))
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

            if (config.MaxRepetition < -1)
            {
                throw new ArgumentException("The maximum repetition should be -1 or more.");
            }
        }

        private void GenerateRequiredCharacters(ICollection<char> generated, IDictionary<char, int> characterCount, Configuration config)
        {
            var requiredSets = config.CharacterSets.Where(characterSet => characterSet.Min > 0).ToArray();

            foreach (var requiredSet in requiredSets)
            {
                AddRequiredCharactersFromSet(generated, characterCount, config, requiredSet);
            }
        }

        private void AddRequiredCharactersFromSet(ICollection<char> generated, IDictionary<char, int> characterCount, Configuration config, ICharacterSet requiredSet)
        {
            for (var i = 0; i < requiredSet.Min;)
            {
                char? nextCharacter = _characterSelector.GetNextCharacter(requiredSet.Set);

                if (config.MaxRepetition > -1)
                {
                    nextCharacter = HandleRepetition((char)nextCharacter, characterCount, config.MaxRepetition);
                }

                if (nextCharacter.HasValue)
                {
                    generated.Add((char)nextCharacter);
                    i++;
                }
            }
        }

        private void GenerateRandomCharacters(ICollection<char> generated, IDictionary<char, int> characterCount, Configuration config)
        {
            while (generated.Count < config.Length)
            {
                var randomSetIndex = _randomNumberGenerator.GetRandomIntInRange(0, config.CharacterSets.Count - 1);
                var characterSet = config.CharacterSets[randomSetIndex];

                char? nextCharacter = _characterSelector.GetNextCharacter(characterSet.Set);

                if (config.MaxRepetition >= 0)
                {
                    nextCharacter = HandleRepetition((char)nextCharacter, characterCount, config.MaxRepetition);
                }

                if (nextCharacter.HasValue)
                {
                    generated.Add((char)nextCharacter);
                }
            }
        }

        private char? HandleRepetition(char character, IDictionary<char, int> characterCount, int maxRepetition)
        {
            return character.IsRepeated(characterCount, maxRepetition)
                ? null
                : character;
        }
    }
}