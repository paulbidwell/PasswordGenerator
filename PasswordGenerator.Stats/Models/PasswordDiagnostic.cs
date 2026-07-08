using PasswordGenerator.Core.Interfaces.Strength;

namespace PasswordGenerator.Stats.Models;

public record PasswordDiagnostic(
    int Index,
    string Password,
    int Length,
    double EntropyBits,
    PasswordStrength Strength,
    double GuessesLog10,
    int MaxConsecutiveRepetition,
    IReadOnlyList<string> Warnings);
