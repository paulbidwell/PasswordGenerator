using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class CollectionShuffler : ICollectionShuffler
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public CollectionShuffler(IRandomNumberGenerator randomNumberGenerator)
        {
            _randomNumberGenerator = randomNumberGenerator;
        }

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
                var random = _randomNumberGenerator.GetRandomIntInRange(0, i + 1);
                (collection[i], collection[random]) = (collection[random], collection[i]);
            }
        }

        private bool HasSequences<T>(IList<T> collection, StringComparison stringComparison)
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