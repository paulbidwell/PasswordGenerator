using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers
{
    public class CharacterSetShuffler(ICollectionShuffler collectionShuffler) : ICharacterSetShuffler
    {
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