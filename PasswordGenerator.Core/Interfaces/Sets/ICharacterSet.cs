namespace PasswordGenerator.Core.Interfaces.Sets;

/// <summary>Represents a named group of characters used during password generation.</summary>
public interface ICharacterSet
{
    /// <summary>The resolved array of characters available for selection.</summary>
    char[] Set { get; set; }

    /// <summary>The raw character string from which <see cref="Set"/> is derived.</summary>
    string Characters { get; set; }

    /// <summary>The minimum number of characters that must be drawn from this set.</summary>
    int Min { get; set; }

    /// <summary>The relative weight used when distributing remaining characters across sets.</summary>
    int Weight { get; set; }
}