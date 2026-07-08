using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator;

/// <summary>Validates <see cref="IPinGeneratorConfig"/> before PIN generation.</summary>
public class PinConfigurationValidator : IPinConfigurationValidator
{
    private const int MinPinLength = 4;
    private const int MaxPinLength = 12;

    /// <inheritdoc />
    public void Validate(IPinGeneratorConfig config)
    {
        if (config.Length < MinPinLength)
        {
            throw new ArgumentException($"PIN length must be at least {MinPinLength} digits.");
        }

        if (config.Length > MaxPinLength)
        {
            throw new ArgumentException($"PIN length must not exceed {MaxPinLength} digits.");
        }

        if (config.MaxRetries < 1)
        {
            throw new ArgumentException("MaxRetries must be at least 1.");
        }
    }
}
