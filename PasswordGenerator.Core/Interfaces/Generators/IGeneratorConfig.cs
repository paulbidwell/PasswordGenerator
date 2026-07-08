using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Read-only snapshot of the password generator configuration used during a single generation pass.
/// </summary>
public interface IGeneratorConfig
{
    /// <summary>Character sets available for password composition.</summary>
    public List<ICharacterSet> CharacterSets { get; }

    /// <summary>Maximum number of times any single character may appear, or -1 for unlimited.</summary>
    public int MaxRepetition { get; }

    /// <summary>Desired length of the generated password.</summary>
    public int Length { get; }

    /// <summary>Whether sequential character patterns are permitted.</summary>
    public bool AllowSequences { get; }

    /// <summary>Whether case-insensitive sequential patterns are permitted.</summary>
    public bool AllowUpperLowerSequences { get; }

    /// <summary>Whether the password must begin with a letter.</summary>
    public bool MustStartWithLetter { get; }

    /// <summary>Whether the first and last characters must be alphanumeric.</summary>
    public bool ExcludeLeadingTrailingSymbols { get; }
}