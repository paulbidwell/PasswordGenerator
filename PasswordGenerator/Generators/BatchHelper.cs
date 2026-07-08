using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators;

/// <summary>
/// Shared helper for generating a batch of unique values with retry logic.
/// </summary>
internal static class BatchHelper
{
    private const int BatchRetryMultiplier = 10;

    /// <summary>
    /// Generates up to <paramref name="count"/> unique strings by repeatedly invoking
    /// <paramref name="generate"/>, retrying up to <c>count * 10</c> times.
    /// Returns a partial result with a warning if not all items could be produced.
    /// </summary>
    internal static BatchResult GenerateUniqueBatch(
        int count,
        Func<string> generate,
        string itemName,
        string hint)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var results = new HashSet<string>(count, StringComparer.Ordinal);
        var maxAttempts = count * BatchRetryMultiplier;
        var attempts = 0;

        while (results.Count < count)
        {
            if (++attempts > maxAttempts)
            {
                return new BatchResult(
                    [.. results],
                    $"Could only generate {results.Count} of {count} unique {itemName} within {maxAttempts} attempts. " +
                    hint);
            }

            results.Add(generate());
        }

        return new BatchResult([.. results]);
    }
}
