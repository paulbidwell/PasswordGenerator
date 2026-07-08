namespace PasswordGenerator.Core.Interfaces.Strength;

/// <summary>Qualitative rating of a password's resistance to guessing attacks.</summary>
public enum PasswordStrength
{
    /// <summary>Entropy below 28 bits — trivially guessable.</summary>
    VeryWeak = 0,

    /// <summary>Entropy between 28 and 35 bits.</summary>
    Weak = 1,

    /// <summary>Entropy between 36 and 59 bits.</summary>
    Fair = 2,

    /// <summary>Entropy between 60 and 127 bits.</summary>
    Strong = 3,

    /// <summary>Entropy of 128 bits or higher.</summary>
    VeryStrong = 4
}
