using PasswordGenerator.Sets;

namespace PasswordGenerator;

/// <summary>
/// Provides named preset <see cref="PasswordGeneratorOptions"/> instances
/// for common password compliance standards. Each property returns a new
/// defensive copy, so callers may freely mutate the result.
/// </summary>
public static class PasswordPolicy
{
    private const string CuratedSpecials = "!$%^&*()-_=+[]{}@#~;:,.?/";

    /// <summary>
    /// Preset aligned with the OWASP Application Security Verification Standard
    /// (ASVS) 4.0.3, Section V2.1. Generates 16-character passwords requiring
    /// uppercase, lowercase, digit, and special character categories with a
    /// minimum of 2 characters from each.
    /// </summary>
    public static PasswordGeneratorOptions Owasp => new()
    {
        Length = 16,
        CharacterSets =
        [
            new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 2 },
            new CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz", Min = 2 },
            new CharacterSet { Characters = "0123456789",                 Min = 2 },
            new CharacterSet { Characters = CuratedSpecials,              Min = 2 }
        ],
        ExcludeAmbiguous = true,
        MaxRepetition = 1,
        AllowSequences = false,
        AllowUpperLowerSequences = false,
    };

    /// <summary>
    /// Preset aligned with NIST Special Publication 800-63B (Digital Identity
    /// Guidelines, June 2017 / 2020 revision), §5.1.1.1. Emphasises length
    /// over composition rules, permits all printable ASCII characters, and
    /// does not restrict character repetition or sequential runs.
    /// </summary>
    public static PasswordGeneratorOptions Nist80063B => new()
    {
        Length = 15,
        CharacterSets =
        [
            new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",                       Min = 1 },
            new CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz",                       Min = 1 },
            new CharacterSet { Characters = "0123456789",                                        Min = 1 },
            new CharacterSet { Characters = PasswordGeneratorOptions.AsciiOnlySpecialCharacters, Min = 1 }
        ],
        AsciiOnly = true,
        MaxRepetition = -1,
        AllowSequences = true,
        AllowUpperLowerSequences = true,
    };

    /// <summary>
    /// Preset aligned with PCI Data Security Standard (DSS) v4.0,
    /// Requirement 8.3.6 (March 2022). Generates 14-character passwords
    /// requiring both numeric and alphabetic characters; special characters
    /// are also included as a best practice.
    /// </summary>
    public static PasswordGeneratorOptions Pci => new()
    {
        Length = 14,
        CharacterSets =
        [
            new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 2 },
            new CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz", Min = 2 },
            new CharacterSet { Characters = "0123456789",                 Min = 2 },
            new CharacterSet { Characters = CuratedSpecials,              Min = 2 }
        ],
        ExcludeAmbiguous = true,
        MaxRepetition = 1,
        AllowSequences = false,
        AllowUpperLowerSequences = false,
    };
}
