using Microsoft.Extensions.Options;

namespace PasswordGenerator
{
    public class Generator : IGenerator
    {
        private readonly List<CharacterSet> _characterSets = new();
        private int _maxRepetition;
        private char[]? _masterSet;

        private readonly IOptions<PasswordGeneratorOptions> _options;
        private readonly IUtility _utility;

        public Generator(IOptions<PasswordGeneratorOptions> options, IUtility utility)
        {
            _options = options;
            _utility = utility;
        }

        public IGenerator Configure()
        {
            _maxRepetition = _options.Value.MaxRepetition;
            var characterSets = _options.Value.CharactersSets;

            foreach (var characterSet in characterSets)
            {
                _characterSets.Add(new CharacterSet
                {
                    Characters = characterSet.Characters.ToCharArray(),
                    Min = characterSet.Min
                });
            }

            _utility.Shuffle(_characterSets, true);

            foreach (var charSet in _characterSets.Where(characterSet => characterSet.Characters != null))
            {
                if (charSet.Characters is { Length: > 0 })
                {
                    _utility.Shuffle(charSet.Characters, true);
                }
                else
                {
                    throw new ArgumentException("Invalid character set.");
                }
            }

            _masterSet = _characterSets.SelectMany(characterSet => characterSet.Characters ?? throw new InvalidOperationException()).ToArray();
            _utility.Shuffle(_masterSet, true);

            return this;
        }

        public string Generate()
        {
            var minUseTotal = _characterSets.Sum(c => c.Min);

            if (minUseTotal > _options.Value.Length)
            {
                throw new ArgumentException("The minimum usage of the character sets is more than the maximum length of the generated password.");
            }

            if (_maxRepetition < -1)
            {
                throw new ArgumentException("The maximum repetition should be -1 or more.");
            }

            var generated = new List<char>();
            var characterCount = new Dictionary<char, int>();
            var requiredSets = _characterSets.Where(x => x.Min > 0).ToArray();
            
            foreach (var requiredSet in requiredSets)
            {
                for (var i = 0; i < requiredSet.Min; i++)
                {
                    var nextCharacter = _utility.GetNextChar(requiredSet.Characters);

                    if (_maxRepetition > -1)
                    {
                        var isRepeated = nextCharacter.IsRepeated(characterCount, _maxRepetition);

                        if (!isRepeated)
                        {
                            generated.Add(nextCharacter);
                        }
                        else
                        {
                            i--;
                        }
                    }
                    else
                    {
                        generated.Add(nextCharacter);
                    }
                }
            }

            while (generated.Count < _options.Value.Length)
            {
                var randomSetIndex = _utility.GetRandomIntInRange(0, _characterSets.Count - 1);
                var characterSet = _characterSets[randomSetIndex];

                var nextCharacter = _utility.GetNextChar(characterSet.Characters);

                if (_maxRepetition >= 0)
                {
                    var isRepeated = nextCharacter.IsRepeated(characterCount, _maxRepetition);

                    if (!isRepeated)
                    {
                        generated.Add(nextCharacter);
                    }
                }
                else
                {
                    generated.Add(nextCharacter);
                }
            }
            
            _utility.Shuffle(generated, _options.Value.AllowSequences);

            return new string(generated.ToArray());
        }
    }
}