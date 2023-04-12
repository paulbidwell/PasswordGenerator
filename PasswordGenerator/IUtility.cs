namespace PasswordGenerator
{
    public interface IUtility
    {
        int GetRandomIntInRange(int min, int max);
        char GetNextChar(char[]? characters);
        void Shuffle<T>(IList<T> collection, bool allowSequences = false, bool allowUpperLower = false);

    }
}