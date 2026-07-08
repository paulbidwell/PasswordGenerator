using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers;

/// <summary>Delegates password buffer shuffling to <see cref="ICollectionShuffler"/> using config constraints.</summary>
public class PasswordShuffler(ICollectionShuffler collectionShuffler) : IPasswordShuffler
{
    /// <inheritdoc />
    public void Shuffle(char[] buffer, IGeneratorConfig config)
    {
        collectionShuffler.Shuffle(buffer, config.AllowSequences, config.AllowUpperLowerSequences);
    }
}