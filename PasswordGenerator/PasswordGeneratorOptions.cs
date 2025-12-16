using PasswordGenerator.Sets;

namespace PasswordGenerator;

/// <summary>
/// Configuration options for the password generator application.
/// </summary>
public class PasswordGeneratorOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether consecutive duplicate characters are allowed in passwords.
    /// </summary>
    public bool AllowSequences { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether case-insensitive sequences are allowed when AllowSequences is false.
    /// </summary>
    public bool AllowUpperLowerSequences { get; set; }

    /// <summary>
    /// Gets or sets the character sets to use for password generation.
    /// </summary>
    public required List<CharacterSet> CharacterSets { get; set; }

    /// <summary>
    /// Gets or sets the length of generated passwords.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times any character can repeat in a password.
    /// Set to -1 for unlimited repetition.
    /// </summary>
    public int MaxRepetition { get; set; }

    /// <summary>
    /// Gets or sets the file path where passwords will be written when OutputToFile is true.
    /// </summary>
    public required string OutputPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether passwords should be written to the console.
    /// </summary>
    public bool OutputToConsole { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether passwords should be written to a file.
    /// </summary>
    public bool OutputToFile { get; set; }

    /// <summary>
    /// Gets or sets the number of passwords to generate.
    /// </summary>
    public int PasswordsToGenerate { get; set; }
}