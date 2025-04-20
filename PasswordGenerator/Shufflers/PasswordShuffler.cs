using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Shufflers
{
    public class PasswordShuffler(ICollectionShuffler collectionShuffler) : IPasswordShuffler
    {
        public void Shuffle(char[] buffer, IGeneratorConfig config)
        {
            collectionShuffler.Shuffle(buffer, config.AllowSequences, config.AllowUpperLowerSequences);
        }
    }
}