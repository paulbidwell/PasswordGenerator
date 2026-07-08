namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Generates cryptographically random numeric PINs.
/// </summary>
public interface IPinGenerator
{
    /// <summary>
    /// Generates a single random PIN.
    /// </summary>
    string Generate();

    /// <summary>
    /// Generates a batch of up to <paramref name="count"/> unique random PINs.
    /// </summary>
    BatchResult GenerateBatch(int count);
}
