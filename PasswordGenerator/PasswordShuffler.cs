using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class PasswordShuffler(ICollectionShuffler collectionShuffler) : IPasswordShuffler
    {
        public void Shuffle(char[] buffer, IGeneratorConfig config)
        {
            collectionShuffler.Shuffle(buffer, config.AllowSequences, config.AllowUpperLowerSequences);
        }
    }
}