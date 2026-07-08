namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Generates cryptographically random passwords.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Generates a single random password.
    /// </summary>
    string Generate();

    /// <summary>
    /// Generates a batch of up to <paramref name="count"/> unique random passwords.
    /// </summary>
    BatchResult GenerateBatch(int count);
}