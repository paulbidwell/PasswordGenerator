namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Read-only snapshot of the PIN generator configuration used during a single generation pass.
/// </summary>
public interface IPinGeneratorConfig
{
    /// <summary>Number of digits in the generated PIN.</summary>
    int Length { get; }

    /// <summary>Whether trivial patterns (e.g. 1234, 0000) are rejected.</summary>
    bool RejectTrivialPatterns { get; }

    /// <summary>Maximum regeneration attempts when rejecting trivial patterns.</summary>
    int MaxRetries { get; }
}
