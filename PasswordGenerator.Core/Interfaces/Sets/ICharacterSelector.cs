namespace PasswordGenerator.Core.Interfaces.Sets;

/// <summary>Selects a single character from a set of candidates.</summary>
public interface ICharacterSelector
{
    /// <summary>Returns a randomly selected character from <paramref name="characters"/>.</summary>
    /// <param name="characters">The candidate character array, or <see langword="null"/> if none are available.</param>
    public char GetNextCharacter(char[]? characters);
}