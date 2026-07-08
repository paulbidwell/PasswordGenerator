namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Provides cryptographically secure random integer generation.
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>
    /// Returns a random integer in the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public int GetRandomIntInRange(int min, int max);
}