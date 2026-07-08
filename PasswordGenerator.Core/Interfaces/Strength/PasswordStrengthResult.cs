namespace PasswordGenerator.Core.Interfaces.Strength;

/// <summary>Immutable result of a password strength analysis.</summary>
/// <param name="EntropyBits">Estimated entropy in bits.</param>
/// <param name="Strength">The qualitative <see cref="PasswordStrength"/> rating.</param>
/// <param name="GuessesLog10">Log-base-10 of the estimated number of guesses required.</param>
/// <param name="Warnings">Human-readable warnings about weaknesses found.</param>
public record PasswordStrengthResult(
    double EntropyBits,
    PasswordStrength Strength,
    double GuessesLog10,
    IReadOnlyList<string> Warnings);
