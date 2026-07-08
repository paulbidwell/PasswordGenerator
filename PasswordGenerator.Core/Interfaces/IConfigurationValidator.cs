using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Core.Interfaces;

/// <summary>
/// Validates a password generator configuration before generation begins.
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// Validates the specified <paramref name="config"/> and throws if any constraint is violated.
    /// </summary>
    void Validate(IGeneratorConfig config);
}