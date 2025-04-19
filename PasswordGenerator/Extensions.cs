    namespace PasswordGenerator
{
    public static class Extensions
    {
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