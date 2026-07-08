namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Assembles a character buffer that satisfies the generator configuration constraints.
/// </summary>
public interface ICharacterGenerator
{
    /// <summary>
    /// Generates a password as a character array satisfying all configured constraints.
    /// </summary>
    char[] GeneratePassword();
}