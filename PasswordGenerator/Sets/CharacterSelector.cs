using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Sets
{
    public class CharacterSelector(IRandomNumberGenerator randomNumberGenerator) : ICharacterSelector
    {
        public char GetNextCharacter(char[]? characters)
        {
            if (characters != null)
            {
                var randomCharacterIndex = randomNumberGenerator.GetRandomIntInRange(0, characters.Length - 1);
                return characters[randomCharacterIndex];
            }

            if (characters is { Length: 0 })
            {
                throw new ArgumentException("Character array must not be empty", nameof(characters));
            }

            throw new ArgumentNullException(nameof(characters));
            
        }
    }
}