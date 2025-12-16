using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers
{
    /// <summary>
    /// Shuffles password character arrays while respecting sequence constraints.
    /// </summary>
    public class PasswordShuffler(ICollectionShuffler collectionShuffler) : IPasswordShuffler
    {
        /// <summary>
        /// Shuffles a password buffer according to the configuration.
        /// </summary>
        /// <param name="buffer">The password character buffer to shuffle.</param>
        /// <param name="config">The generator configuration containing sequence rules.</param>
        public void Shuffle(char[] buffer, IGeneratorConfig config)
        {
            collectionShuffler.Shuffle(buffer, config.AllowSequences, config.AllowUpperLowerSequences);
        }
    }
}