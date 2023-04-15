using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class CharacterSelector : ICharacterSelector
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public CharacterSelector(IRandomNumberGenerator randomNumberGenerator)
        {
            _randomNumberGenerator = randomNumberGenerator;
        }

        public char GetNextCharacter(char[]? characters)
        {
            if (characters != null)
            {
                var randomCharacterIndex = _randomNumberGenerator.GetRandomIntInRange(0, characters.Length - 1);
                return characters[randomCharacterIndex];
            }

            throw new NullReferenceException("Could not select a next character. The character set is null.");
        }
    }
}