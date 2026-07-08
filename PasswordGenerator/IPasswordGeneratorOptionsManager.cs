namespace PasswordGenerator;

/// <summary>Provides access to and runtime mutation of <see cref="PasswordGeneratorOptions"/>.</summary>
public interface IPasswordGeneratorOptionsManager
{
    /// <summary>Gets the current effective options.</summary>
    PasswordGeneratorOptions Current { get; }

    /// <summary>Applies <paramref name="configure"/> to the current options.</summary>
    /// <param name="configure">A delegate that mutates the options.</param>
    void Configure(Action<PasswordGeneratorOptions> configure);
}
