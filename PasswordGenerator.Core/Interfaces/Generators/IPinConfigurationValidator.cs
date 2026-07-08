namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>Validates a <see cref="IPinGeneratorConfig"/> before PIN generation begins.</summary>
public interface IPinConfigurationValidator
{
    /// <summary>Validates <paramref name="config"/> and throws if the configuration is invalid.</summary>
    /// <param name="config">The PIN generator configuration to validate.</param>
    void Validate(IPinGeneratorConfig config);
}
