using PasswordGenerator.Interfaces;
using CryptoRandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace PasswordGenerator
{
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        public int GetRandomIntInRange(int min, int max)
        {
            using var randomNumberGenerator = CryptoRandomNumberGenerator.Create();
            var range = max - min;

            var bytes = new byte[4];
            randomNumberGenerator.GetBytes(bytes);

            var generatedInteger = BitConverter.ToInt32(bytes, 0);
            return Math.Abs(generatedInteger) % range + min;
        }
    }
}