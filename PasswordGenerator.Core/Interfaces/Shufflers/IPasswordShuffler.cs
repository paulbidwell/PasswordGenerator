using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Core.Interfaces.Shufflers;

/// <summary>Shuffles a completed password buffer according to generator constraints.</summary>
public interface IPasswordShuffler
{
    /// <summary>Shuffles <paramref name="buffer"/> in-place, respecting the rules in <paramref name="config"/>.</summary>
    /// <param name="buffer">The mutable character buffer representing the password.</param>
    /// <param name="config">The generator configuration containing shuffle constraints.</param>
    void Shuffle(char[] buffer, IGeneratorConfig config);
}