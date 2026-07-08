using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Sets;

/// <summary>Concrete implementation of <see cref="ICharacterSet"/>.</summary>
public class CharacterSet : ICharacterSet
{
    /// <inheritdoc />
    public char[] Set { get; set; } = [];

    /// <inheritdoc />
    public required string Characters { get; set; }

    /// <inheritdoc />
    public int Min { get; set; }

    /// <inheritdoc />
    public int Weight { get; set; }
}