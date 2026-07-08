using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator;

/// <summary>Validates <see cref="IGeneratorConfig"/> before password generation.</summary>
public class ConfigurationValidator : IConfigurationValidator
{
    /// <inheritdoc />
    public void Validate(IGeneratorConfig config)
    {
        if (config.CharacterSets.Count > 0 && config.Length < 1)
        {
            throw new ArgumentException("Password length must be at least 1 when character sets are provided.");
        }

        if (config.CharacterSets.Any(characterSet => characterSet.Set.Length <= 0))
        {
            throw new ArgumentException("Character sets cannot be empty or null.");
        }

        var minUseTotal = config.CharacterSets.Sum(characterSet => characterSet.Min);

        if (minUseTotal > config.Length)
        {
            throw new ArgumentException("The minimum usage of the character sets is more than the maximum length of the generated password.");
        }

        if (config.CharacterSets.Any(characterSet => characterSet.Min < 0 || characterSet.Min > config.Length))
        {
            throw new ArgumentException("Minimum usage of a character set must be between 0 and the password length, inclusive.");
        }

        switch (config.MaxRepetition)
        {
            case < -1:
                throw new ArgumentException("The maximum repetition should be -1 or more.");

            case >= 0:
            {
                var totalSupply = config.CharacterSets
                    .Sum(characterSet => (long)characterSet.Set.Distinct().Count() * (config.MaxRepetition + 1));

                if (totalSupply < config.Length)
                {
                    throw new InvalidOperationException(
                        $"Not enough characters to fill a password of length {config.Length} " +
                        $"with MaxRepetition {config.MaxRepetition}. " +
                        $"Total available slots: {totalSupply}.");
                }

                break;
            }
        }

        if (config.MustStartWithLetter || config.ExcludeLeadingTrailingSymbols)
        {
            var hasLetterOrDigit = config.CharacterSets
                .Any(cs => cs.Set.Any(char.IsLetterOrDigit));

            if (!hasLetterOrDigit)
            {
                throw new ArgumentException(
                    "Position constraints require at least one character set containing letters or digits.");
            }
        }

        if (config.MustStartWithLetter)
        {
            var hasLetter = config.CharacterSets
                .Any(cs => cs.Set.Any(char.IsLetter));

            if (!hasLetter)
            {
                throw new ArgumentException(
                    "MustStartWithLetter requires at least one character set containing letters.");
            }
        }

        if (config.ExcludeLeadingTrailingSymbols && config.Length >= 2)
        {
            var letterOrDigitCount = config.CharacterSets
                .SelectMany(cs => cs.Set)
                .Count(char.IsLetterOrDigit);

            if (letterOrDigitCount < 2)
            {
                throw new ArgumentException(
                    "ExcludeLeadingTrailingSymbols requires at least two letter or digit characters across all character sets.");
            }
        }
    }
}