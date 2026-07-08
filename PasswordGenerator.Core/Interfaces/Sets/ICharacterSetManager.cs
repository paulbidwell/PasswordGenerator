namespace PasswordGenerator.Core.Interfaces.Sets;

/// <summary>Creates and shuffles deep copies of character sets for a single generation pass.</summary>
public interface ICharacterSetManager
{
    /// <summary>Clones the source <paramref name="characterSets"/>, shuffles each clone's characters, and returns them in randomised order.</summary>
    /// <param name="characterSets">The original character set definitions.</param>
    List<ICharacterSet> CreateAndShuffleCharacterSets(IEnumerable<ICharacterSet> characterSets);
}