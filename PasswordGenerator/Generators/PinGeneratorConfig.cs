using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators;

/// <summary>Snapshot of PIN generator configuration resolved from the current <see cref="IPinGeneratorOptionsManager"/>.</summary>
public class PinGeneratorConfig(IPinGeneratorOptionsManager optionsManager) : IPinGeneratorConfig
{
    public int Length { get; } = optionsManager.Current.Length;
    public bool RejectTrivialPatterns { get; } = optionsManager.Current.RejectTrivialPatterns;
    public int MaxRetries { get; } = optionsManager.Current.MaxRetries;
}
