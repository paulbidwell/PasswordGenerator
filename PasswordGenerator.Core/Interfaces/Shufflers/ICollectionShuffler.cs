namespace PasswordGenerator.Core.Interfaces.Shufflers;

public interface ICollectionShuffler
{
    public void Shuffle<T>(IList<T> collection, bool allowSequences, bool allowUpperLower);
}