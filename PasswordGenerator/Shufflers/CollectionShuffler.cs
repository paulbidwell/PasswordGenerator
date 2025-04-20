using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers
{
    public class CollectionShuffler(IRandomNumberGenerator randomNumberGenerator) : ICollectionShuffler
    {
        public void Shuffle<T>(IList<T> collection, bool allowSequences, bool allowUpperLower)
        {
            ShuffleCollection(collection);

            if (!allowSequences)
            {
                var stringComparison = allowUpperLower
                    ? StringComparison.CurrentCulture
                    : StringComparison.CurrentCultureIgnoreCase;

                while (HasSequences(collection, stringComparison))
                {
                    ShuffleCollection(collection);
                }
            }
        }

        private void ShuffleCollection<T>(IList<T> collection)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                var random = randomNumberGenerator.GetRandomIntInRange(0, i);
                (collection[i], collection[random]) = (collection[random], collection[i]);
            }
        }

        private static bool HasSequences<T>(IList<T> collection, StringComparison stringComparison)
        {
            for (var i = 0; i < collection.Count - 1; i++)
            {
                if (string.Equals(collection[i]?.ToString(), collection[i + 1]?.ToString(), stringComparison))
                {
                    return true;
                }
            }

            return false;
        }
    }
}