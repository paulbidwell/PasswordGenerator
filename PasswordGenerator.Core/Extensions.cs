    namespace PasswordGenerator.Core;

    /// <summary>
    /// Extension methods for common operations used during password generation.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Attempts to increment the count for <paramref name="character"/> in <paramref name="counts"/>,
        /// returning <see langword="false"/> if the count would exceed <paramref name="maxAllowed"/>.
        /// </summary>
        public static bool TryAddCount(this IDictionary<char, int> counts, char character, int maxAllowed, out bool exceeds)
        {
            counts.TryGetValue(character, out var count);
            var next = count + 1;
            exceeds = next > maxAllowed;

            if (!exceeds)
            {
                counts[character] = next;
            }

            return !exceeds;
        }
    }