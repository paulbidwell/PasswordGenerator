using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Core.Interfaces.Shufflers
{
    public interface IPasswordShuffler
    {
        void Shuffle(char[] buffer, IGeneratorConfig config);
    }
}