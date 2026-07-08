using PasswordGenerator.Core.Interfaces.Generators;
using System.Security.Cryptography;

namespace PasswordGenerator.Generators;

/// <summary>Cryptographically secure random number generator backed by <see cref="RandomNumberGenerator"/>.</summary>
public class SecureRng : IRandomNumberGenerator
{
    /// <inheritdoc />
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

        if (max == int.MaxValue)
        {
            if (min == int.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(min),
                    "The range [int.MinValue, int.MaxValue] is not supported.");
            }

            return RandomNumberGenerator.GetInt32(min - 1, max) + 1;
        }

        return RandomNumberGenerator.GetInt32(min, max + 1);
    }
}