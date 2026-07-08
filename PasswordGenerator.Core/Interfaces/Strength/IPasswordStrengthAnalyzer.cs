using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Core.Interfaces.Strength;

/// <summary>Analyses a password and returns entropy, strength rating, and warnings.</summary>
public interface IPasswordStrengthAnalyzer
{
    /// <summary>Analyses <paramref name="password"/> without knowledge of the generator configuration.</summary>
    /// <param name="password">The password to analyse.</param>
    PasswordStrengthResult Analyze(string password);

    /// <summary>Analyses <paramref name="password"/> using the generator <paramref name="config"/> for more accurate entropy estimation.</summary>
    /// <param name="password">The password to analyse.</param>
    /// <param name="config">Optional generator configuration used to refine the analysis.</param>
    PasswordStrengthResult Analyze(string password, IGeneratorConfig? config);
}
