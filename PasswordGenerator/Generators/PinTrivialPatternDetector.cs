using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators;

/// <summary>Detects trivial PIN patterns (repeated digits, sequential runs, repeating pairs).</summary>
public class PinTrivialPatternDetector : IPinTrivialPatternDetector
{
    /// <inheritdoc />
    public bool IsTrivial(ReadOnlySpan<char> pin)
    {
        if (pin.Length < 2)
            return false;

        return IsAllSameDigit(pin) || IsSequential(pin) || IsRepeatingPair(pin);
    }

    private static bool IsAllSameDigit(ReadOnlySpan<char> pin)
    {
        var first = pin[0];
        for (var i = 1; i < pin.Length; i++)
        {
            if (pin[i] != first)
                return false;
        }

        return true;
    }

    private static bool IsSequential(ReadOnlySpan<char> pin)
    {
        var delta = pin[1] - pin[0];
        if (delta is not (1 or -1))
            return false;

        for (var i = 2; i < pin.Length; i++)
        {
            if (pin[i] - pin[i - 1] != delta)
                return false;
        }

        return true;
    }

    private static bool IsRepeatingPair(ReadOnlySpan<char> pin)
    {
        if (pin.Length < 4 || pin.Length % 2 != 0)
            return false;

        var a = pin[0];
        var b = pin[1];

        if (a == b)
            return false;

        for (var i = 2; i < pin.Length; i += 2)
        {
            if (pin[i] != a || pin[i + 1] != b)
                return false;
        }

        return true;
    }
}
