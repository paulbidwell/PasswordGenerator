using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Sets;

/// <summary>Selects a random character from a candidate array using a secure RNG.</summary>
public class CharacterSelector(IRandomNumberGenerator randomNumberGenerator) : ICharacterSelector
{
    /// <inheritdoc />
    public char GetNextCharacter(char[]? characters)
    {
        ArgumentNullException.ThrowIfNull(characters);

        if (characters.Length == 0)
        {
            throw new ArgumentException("Character array must not be empty.", nameof(characters));
        }

        var randomCharacterIndex = randomNumberGenerator.GetRandomIntInRange(0, characters.Length - 1);
        return characters[randomCharacterIndex];
    }
}