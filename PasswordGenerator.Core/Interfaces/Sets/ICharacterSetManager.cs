namespace PasswordGenerator.Core.Interfaces.Sets;

public interface ICharacterSetManager
{
    List<ICharacterSet> CreateAndShuffleCharacterSets(IEnumerable<ICharacterSet> characterSets);
}