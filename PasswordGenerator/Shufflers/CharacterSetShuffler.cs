using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers
{
    /// <summary>
    /// Shuffles character sets using the collection shuffler.
    /// </summary>
    public class CharacterSetShuffler(ICollectionShuffler collectionShuffler) : ICharacterSetShuffler
    {
        /// <summary>
        /// Shuffles the characters within a character set.
        /// </summary>
        /// <param name="characterSet">The character set to shuffle.</param>
        /// <exception cref="ArgumentException">Thrown when character set is invalid or empty.</exception>
        public void ShuffleCharacterSet(ICharacterSet characterSet)
        {
            if (characterSet.Set is { Length: > 0 })
            {
                collectionShuffler.Shuffle(characterSet.Set, true, true);
            }
            else
            {
                throw new ArgumentException("Invalid character set.");
            }
        }
    }
}