using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers;

/// <summary>Shuffles the character array within an individual <see cref="ICharacterSet"/>.</summary>
public class CharacterSetShuffler(ICollectionShuffler collectionShuffler) : ICharacterSetShuffler
{
    /// <inheritdoc />
    public void ShuffleCharacterSet(ICharacterSet characterSet)
    {
        if (characterSet.Set is not { Length: > 0 })
            throw new ArgumentException("Invalid character set.");

        collectionShuffler.Shuffle(characterSet.Set, true, true);
    }
}