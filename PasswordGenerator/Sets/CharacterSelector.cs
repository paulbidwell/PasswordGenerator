using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Sets
{
    /// <summary>
    /// Selects random characters from a character array using cryptographically secure randomness.
    /// </summary>
    public class CharacterSelector(IRandomNumberGenerator randomNumberGenerator) : ICharacterSelector
    {
        /// <summary>
        /// Gets a random character from the provided character array.
        /// </summary>
        /// <param name="characters">The array of characters to select from.</param>
        /// <returns>A randomly selected character.</returns>
        /// <exception cref="ArgumentNullException">Thrown when characters is null.</exception>
        /// <exception cref="ArgumentException">Thrown when characters array is empty.</exception>
        public char GetNextCharacter(char[]? characters)
        {
            ArgumentNullException.ThrowIfNull(characters);

            if (characters.Length == 0)
            {
                throw new ArgumentException("Character array must not be empty", nameof(characters));
            }

            var randomCharacterIndex = randomNumberGenerator.GetRandomIntInRange(0, characters.Length - 1);
            return characters[randomCharacterIndex];
        }
    }
}