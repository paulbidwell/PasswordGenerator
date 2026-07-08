using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators;

/// <summary>Enforces position-based constraints on a password buffer by swapping characters.</summary>
public class PositionConstraintEnforcer : IPositionConstraintEnforcer
{
    /// <inheritdoc />
    public void Enforce(char[] buffer, IGeneratorConfig config)
    {
        if (buffer.Length == 0)
            return;

        if (config.MustStartWithLetter)
        {
            EnforceStart(buffer, static c => char.IsLetter(c));
        }

        if (config.ExcludeLeadingTrailingSymbols)
        {
            EnforceStart(buffer, static c => char.IsLetterOrDigit(c));
            EnforceEnd(buffer, static c => char.IsLetterOrDigit(c));
        }
    }

    private static void EnforceStart(char[] buffer, Func<char, bool> predicate)
    {
        if (predicate(buffer[0]))
            return;

        for (var i = 1; i < buffer.Length; i++)
        {
            if (predicate(buffer[i]))
            {
                (buffer[0], buffer[i]) = (buffer[i], buffer[0]);
                return;
            }
        }
    }

    private static void EnforceEnd(char[] buffer, Func<char, bool> predicate)
    {
        var last = buffer.Length - 1;

        if (predicate(buffer[last]))
            return;

        for (var i = last - 1; i >= 0; i--)
        {
            if (predicate(buffer[i]) && i != 0)
            {
                (buffer[last], buffer[i]) = (buffer[i], buffer[last]);
                return;
            }
        }
    }
}
