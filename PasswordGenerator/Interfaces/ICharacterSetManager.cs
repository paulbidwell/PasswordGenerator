namespace PasswordGenerator.Interfaces;

public interface ICharacterSetManager
{
    List<ICharacterSet> CreateAndShuffleCharacterSets(IEnumerable<ICharacterSet> characterSets);
}