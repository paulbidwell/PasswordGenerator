using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Core.Interfaces.Shufflers;

/// <summary>Shuffles the characters within an individual <see cref="ICharacterSet"/>.</summary>
public interface ICharacterSetShuffler
{
    /// <summary>Randomises the order of characters in <paramref name="characterSet"/>.</summary>
    /// <param name="characterSet">The character set whose <see cref="ICharacterSet.Set"/> array will be shuffled.</param>
    void ShuffleCharacterSet(ICharacterSet characterSet);
}