using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Sets
{
    public class CharacterSetManager(ICharacterSetShuffler characterSetShuffler, ICollectionShuffler collectionShuffler)
        : ICharacterSetManager
    {
        public List<ICharacterSet> CreateAndShuffleCharacterSets(IEnumerable<ICharacterSet> characterSets)
        {
            var result = new List<ICharacterSet>();

            foreach (var characterSet in characterSets)
            {
                var clone = new CharacterSet
                {
                    Characters = characterSet.Characters,
                    Min = characterSet.Min,
                    Set = characterSet.Characters.ToCharArray()
                };

                result.Add(clone);
            }

            collectionShuffler.Shuffle(result, true, true);

            foreach (var charSet in result)
            {
                characterSetShuffler.ShuffleCharacterSet(charSet);
            }

            return result;
        }
    }
}