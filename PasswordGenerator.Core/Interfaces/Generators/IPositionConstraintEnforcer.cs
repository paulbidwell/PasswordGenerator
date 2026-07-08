namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>Enforces position-based constraints (e.g. required first/last characters) on a password buffer.</summary>
public interface IPositionConstraintEnforcer
{
    /// <summary>Mutates <paramref name="buffer"/> in-place to satisfy the constraints in <paramref name="config"/>.</summary>
    /// <param name="buffer">The mutable character buffer representing the password.</param>
    /// <param name="config">The generator configuration containing position constraints.</param>
    void Enforce(char[] buffer, IGeneratorConfig config);
}
