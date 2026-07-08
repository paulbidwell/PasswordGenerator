using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers;

/// <summary>Fisher–Yates shuffle with optional adjacent-duplicate elimination.</summary>
public class CollectionShuffler(IRandomNumberGenerator randomNumberGenerator) : ICollectionShuffler
{
    /// <inheritdoc />
    public void Shuffle<T>(IList<T> collection, bool allowSequences, bool allowUpperLower)
    {
        ShuffleCollection(collection);

        if (!allowSequences)
        {
            var stringComparison = allowUpperLower
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            var maxAttempts = collection.Count * collection.Count;
            var attempts = 0;

            while (HasSequences(collection, stringComparison))
            {
                if (++attempts >= maxAttempts)
                {
                    throw new InvalidOperationException(
                        "Unable to eliminate adjacent duplicate characters after maximum shuffle attempts.");
                }

                ShuffleCollection(collection);
            }
        }
    }

    private void ShuffleCollection<T>(IList<T> collection)
    {
        for (var i = collection.Count - 1; i > 0; i--)
        {
            var randomIndex = randomNumberGenerator.GetRandomIntInRange(0, i);
            (collection[i], collection[randomIndex]) = (collection[randomIndex], collection[i]);
        }
    }

    private static bool HasSequences<T>(IList<T> collection, StringComparison stringComparison)
    {
        if (collection is IList<char> chars)
        {
            var ignoreCase = stringComparison == StringComparison.OrdinalIgnoreCase;

            for (var i = 0; i < chars.Count - 1; i++)
            {
                var a = chars[i];
                var b = chars[i + 1];

                if (ignoreCase
                    ? char.ToLowerInvariant(a) == char.ToLowerInvariant(b)
                    : a == b)
                {
                    return true;
                }
            }

            return false;
        }

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