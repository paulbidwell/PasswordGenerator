using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Core.Interfaces.Shufflers;

public interface ICharacterSetShuffler
{
    void ShuffleCharacterSet(ICharacterSet characterSet);
}