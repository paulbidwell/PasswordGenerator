    namespace PasswordGenerator.Core
{
    /// <summary>
    /// Provides extension methods for common operations.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Attempts to increment the count for a character in the dictionary, checking against a maximum value.
        /// </summary>
        /// <param name="counts">The dictionary tracking character counts.</param>
        /// <param name="c">The character to track.</param>
        /// <param name="max">The maximum allowed count.</param>
        /// <param name="exceeds">Output parameter indicating if the count exceeds the maximum.</param>
        /// <returns>True if the character was added without exceeding the maximum; otherwise, false.</returns>
        public static bool TryAddCount(this IDictionary<char, int> counts, char c, int max, out bool exceeds)
        {
            counts.TryGetValue(c, out var count);
            count++;
            counts[c] = count;
            exceeds = count > max;
            return !exceeds;
        }
    }
}