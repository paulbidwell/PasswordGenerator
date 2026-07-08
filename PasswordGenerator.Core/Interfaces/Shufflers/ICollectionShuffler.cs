namespace PasswordGenerator.Core.Interfaces.Shufflers;

/// <summary>Performs an in-place shuffle on a generic collection.</summary>
public interface ICollectionShuffler
{
    /// <summary>Shuffles <paramref name="collection"/> in-place using a cryptographically secure algorithm.</summary>
    /// <param name="collection">The collection to shuffle.</param>
    /// <param name="allowSequences">When <see langword="false"/>, adjacent sequential characters are broken up.</param>
    /// <param name="allowUpperLower">When <see langword="false"/>, adjacent upper/lower pairs are broken up.</param>
    public void Shuffle<T>(IList<T> collection, bool allowSequences, bool allowUpperLower);
}