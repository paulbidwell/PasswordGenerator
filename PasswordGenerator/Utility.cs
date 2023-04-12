using System.Security.Cryptography;

namespace PasswordGenerator
{
    public class Utility : IUtility
    {
        public int GetRandomIntInRange(int min, int max)
        {
            var randomNumberGenerator = RandomNumberGenerator.Create();
            var range = max - min;

            var bytes = new byte[4];
            randomNumberGenerator.GetBytes(bytes);

            var generatedInteger = BitConverter.ToInt32(bytes, 0);
            return Math.Abs(generatedInteger) % range + min;
        }

        public char GetNextChar(char[]? characters)
        {
            if (characters != null)
            {
                var randomCharacterIndex = GetRandomIntInRange(0, characters.Length - 1);
                return characters[randomCharacterIndex];
            }

            throw new NullReferenceException("Could not select a next char. Chars is null.");
        }

        public void Shuffle<T>(IList<T> collection, bool allowSequences, bool allowUpperLower)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                var random = GetRandomIntInRange(0, i + 1);
                (collection[i], collection[random]) = (collection[random], collection[i]);
            }

            if (!allowSequences)
            {
                var stringComparison = allowUpperLower
                    ? StringComparison.CurrentCulture
                    : StringComparison.CurrentCultureIgnoreCase;

                for (var i = 0; i < collection.Count - 1; i++)
                {
                    if (string.Equals(collection[i]?.ToString(), collection[i + 1]?.ToString(), stringComparison))
                    {
                        Shuffle(collection, allowSequences, allowUpperLower);
                    }
                }
            }
        }
    }
}