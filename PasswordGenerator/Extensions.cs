namespace PasswordGenerator
{
    public static class Extensions
    {
        public static bool IsRepeated(this char character, IDictionary<char, int> characterCount, int maxRepetition)
        {
            if (characterCount.TryGetValue(character, out var count))
            {
                if (count > maxRepetition)
                {
                    return true;
                }
                characterCount[character] = count + 1;
            }
            else
            {
                characterCount.Add(character, 1);
            }
            return false;
        }
    }
}