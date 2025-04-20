using System.Security.Cryptography;
using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators
{
    public class SecureRng : IRandomNumberGenerator, IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public int GetRandomIntInRange(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException("min cannot be greater than max", nameof(min));
            }

            var diff = (long)max - min + 1;
            var uint32Buffer = new byte[4];
            const long fullRange = 1L << 32;

            while (true)
            {
                _rng.GetBytes(uint32Buffer);
                var rand = BitConverter.ToUInt32(uint32Buffer, 0);
                var remainder = fullRange % diff;

                if (rand < fullRange - remainder)
                {
                    return (int)(min + rand % diff);
                }
            }
        }

        public void Dispose()
        {
            _rng.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}