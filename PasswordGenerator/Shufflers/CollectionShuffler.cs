using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers
{
    /// <summary>
    /// Shuffles collections using the Fisher-Yates algorithm with optional sequence prevention.
    /// </summary>
    public class CollectionShuffler(IRandomNumberGenerator randomNumberGenerator) : ICollectionShuffler
    {
        private const int MaxShuffleAttempts = 1000;

        /// <summary>
        /// Shuffles the collection randomly, optionally preventing consecutive duplicate elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to shuffle.</param>
        /// <param name="allowSequences">If false, prevents consecutive duplicate elements.</param>
        /// <param name="allowUpperLower">If true, case-sensitive comparison is used when checking sequences.</param>
        /// <exception cref="InvalidOperationException">Thrown when sequences cannot be avoided after maximum attempts.</exception>
        public void Shuffle<T>(IList<T> collection, bool allowSequences, bool allowUpperLower)
        {
            if (collection.Count == 0)
            {
                return;
            }

            ShuffleCollection(collection);

            if (!allowSequences)
            {
                var stringComparison = allowUpperLower
                    ? StringComparison.CurrentCulture
                    : StringComparison.CurrentCultureIgnoreCase;

                var attempts = 0;
                while (HasSequences(collection, stringComparison))
                {
                    if (++attempts > MaxShuffleAttempts)
                    {
                        throw new InvalidOperationException(
                            $"Unable to generate password without sequences after {MaxShuffleAttempts} attempts. " +
                            "This may indicate that the configuration makes it impossible to avoid sequences.");
                    }

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