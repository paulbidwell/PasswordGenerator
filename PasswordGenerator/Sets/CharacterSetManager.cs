using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Sets
{
    /// <summary>
    /// Manages character set operations including creation and shuffling.
    /// </summary>
    public class CharacterSetManager(ICharacterSetShuffler characterSetShuffler, ICollectionShuffler collectionShuffler)
        : ICharacterSetManager
    {
        /// <summary>
        /// Creates copies of character sets and shuffles them to ensure randomness.
        /// </summary>
        /// <param name="characterSets">The source character sets to copy and shuffle.</param>
        /// <returns>A shuffled list of character set copies.</returns>
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