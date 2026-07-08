namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>Detects trivial PIN patterns such as repeated digits or sequential runs.</summary>
public interface IPinTrivialPatternDetector
{
    /// <summary>Returns <see langword="true"/> when <paramref name="pin"/> matches a trivial pattern.</summary>
    /// <param name="pin">The PIN digits to evaluate.</param>
    bool IsTrivial(ReadOnlySpan<char> pin);
}
