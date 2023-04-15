using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class CharacterSetShuffler : ICharacterSetShuffler
    {
        private readonly ICollectionShuffler _collectionShuffler;

        public CharacterSetShuffler(ICollectionShuffler collectionShuffler)
        {
            _collectionShuffler = collectionShuffler;
        }

        public void ShuffleCharacterSet(ICharacterSet characterSet)
        {
            if (characterSet.Set is { Length: > 0 })
            {
                _collectionShuffler.Shuffle(characterSet.Set, true, true);
            }
            else
            {
                throw new ArgumentException("Invalid character set.");
            }
        }
    }
}