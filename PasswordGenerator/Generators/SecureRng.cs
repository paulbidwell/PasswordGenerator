using PasswordGenerator.Core.Interfaces.Generators;
using System.Security.Cryptography;

namespace PasswordGenerator.Generators
{
    public class SecureRng : IRandomNumberGenerator, IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private const int BitShiftForUint32 = 32;
        private const int MaxIterations = 1000;

        public int GetRandomIntInRange(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException("min cannot be greater than max", nameof(min));
            }

            if (min == max)
            {
                return min;
            }

            var diff = (long)max - min + 1;
            var uint32Buffer = new byte[4];
            const long fullRange = 1L << BitShiftForUint32;
            var iterations = 0;

            while (true)
            {
                if (++iterations > MaxIterations)
                {
                    throw new InvalidOperationException($"Failed to generate random number after {MaxIterations} iterations. This may indicate an implementation error.");
                }

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