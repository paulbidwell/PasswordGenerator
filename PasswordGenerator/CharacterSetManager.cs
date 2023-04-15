using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class CharacterSetManager : ICharacterSetManager
    {
        private readonly ICharacterSetShuffler _characterSetShuffler;
        private readonly ICollectionShuffler _collectionShuffler;

        public CharacterSetManager(ICharacterSetShuffler characterSetShuffler, ICollectionShuffler collectionShuffler)
        {
            _characterSetShuffler = characterSetShuffler;
            _collectionShuffler = collectionShuffler;
        }

        public List<ICharacterSet> CreateAndShuffleCharacterSets(IEnumerable<ICharacterSet> characterSets)
        {
            var result = new List<ICharacterSet>();

            foreach (var characterSet in characterSets)
            {
                characterSet.Set = characterSet.Characters.ToCharArray();
                result.Add(characterSet);
            }

            _collectionShuffler.Shuffle(result, true, true);

            foreach (var charSet in result)
            {
                _characterSetShuffler.ShuffleCharacterSet(charSet);
            }

            return result;
        }
    }
}