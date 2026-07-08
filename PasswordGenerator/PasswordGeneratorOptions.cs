using PasswordGenerator.Sets;

namespace PasswordGenerator;

/// <summary>Configuration options for password generation.</summary>
public class PasswordGeneratorOptions
{
    /// <summary>Printable ASCII special characters (OWASP set).</summary>
    public const string AsciiOnlySpecialCharacters = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

    /// <summary>Visually ambiguous characters that can be excluded.</summary>
    public const string AmbiguousCharacters = "0O1lI|S5B8Z2";

    /// <summary>Allow adjacent identical characters in the output.</summary>
    public bool AllowSequences { get; set; } = false;

    /// <summary>Allow adjacent upper/lower-case variants of the same letter.</summary>
    public bool AllowUpperLowerSequences { get; set; } = false;

    /// <summary>Restrict special characters to the ASCII range only.</summary>
    public bool AsciiOnly { get; set; } = false;

    /// <summary>The character sets and their minimum counts used to compose a password.</summary>
    public List<CharacterSet> CharacterSets { get; set; } =
    [
        new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 5 },
        new CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz", Min = 5 },
        new CharacterSet { Characters = "0123456789",                 Min = 2 },
        new CharacterSet { Characters = "!$%^&*()-_=+[]{}@#~;:,.?/", Min = 2 }
    ];

    /// <summary>Exclude visually ambiguous characters from all sets.</summary>
    public bool ExcludeAmbiguous { get; set; } = false;

    /// <summary>Additional characters to exclude from all sets.</summary>
    public string ExcludedCharacters { get; set; } = string.Empty;

    /// <summary>Ensure the first and last characters are letters or digits.</summary>
    public bool ExcludeLeadingTrailingSymbols { get; set; } = false;

    /// <summary>Desired password length.</summary>
    public int Length { get; set; } = 22;

    /// <summary>Maximum times a single character may repeat (<c>-1</c> for unlimited).</summary>
    public int MaxRepetition { get; set; } = 2;

    /// <summary>Require the password to start with a letter.</summary>
    public bool MustStartWithLetter { get; set; } = false;

    internal IEnumerable<CharacterSet> GetEffectiveCharacterSets()
    {
        IEnumerable<CharacterSet> sets = CharacterSets;

        if (AsciiOnly)
        {
            sets = sets.Select(characterSet => characterSet.Characters.Any(character => character > 127)
                ? new CharacterSet { Characters = AsciiOnlySpecialCharacters, Min = characterSet.Min }
                : characterSet);
        }

        var allExcluded = ExcludeAmbiguous
            ? ExcludedCharacters + AmbiguousCharacters
            : ExcludedCharacters;

        if (allExcluded.Length > 0)
        {
            var excludedSet = new HashSet<char>(allExcluded);
            sets = sets.Select(characterSet => new CharacterSet
            {
                Characters = new string(characterSet.Characters.Where(c => !excludedSet.Contains(c)).ToArray()),
                Min = characterSet.Min
            });
        }

        return sets;
    }

    internal void CopyTo(PasswordGeneratorOptions target)
    {
        target.Length = Length;
        target.MaxRepetition = MaxRepetition;
        target.AllowSequences = AllowSequences;
        target.AllowUpperLowerSequences = AllowUpperLowerSequences;
        target.AsciiOnly = AsciiOnly;
        target.ExcludeAmbiguous = ExcludeAmbiguous;
        target.ExcludedCharacters = ExcludedCharacters;
        target.ExcludeLeadingTrailingSymbols = ExcludeLeadingTrailingSymbols;
        target.MustStartWithLetter = MustStartWithLetter;
        target.CharacterSets = CharacterSets
            .Select(cs => new CharacterSet { Characters = cs.Characters, Min = cs.Min })
            .ToList();
    }

    internal PasswordGeneratorOptions Snapshot()
    {
        var copy = new PasswordGeneratorOptions();
        CopyTo(copy);
        return copy;
    }
}
