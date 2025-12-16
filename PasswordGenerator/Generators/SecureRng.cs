using PasswordGenerator.Core.Interfaces.Generators;
using System.Security.Cryptography;

namespace PasswordGenerator.Generators
{
    /// <summary>
    /// Provides cryptographically secure random number generation with uniform distribution.
    /// Uses rejection sampling to avoid modulo bias.
    /// </summary>
    public class SecureRng : IRandomNumberGenerator, IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private const int BitShiftForUint32 = 32;
        private const int MaxIterations = 1000;

        /// <summary>
        /// Generates a cryptographically secure random integer within the specified range (inclusive).
        /// </summary>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value.</param>
        /// <returns>A random integer between min and max (inclusive).</returns>
        /// <exception cref="ArgumentException">Thrown when min is greater than max.</exception>
        /// <exception cref="InvalidOperationException">Thrown when unable to generate a random number after maximum iterations.</exception>
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

        /// <summary>
        /// Releases the cryptographic resources used by the random number generator.
        /// </summary>
        public void Dispose()
        {
            _rng.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}