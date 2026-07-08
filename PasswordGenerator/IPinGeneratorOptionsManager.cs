namespace PasswordGenerator;

/// <summary>Provides access to and runtime mutation of <see cref="PinGeneratorOptions"/>.</summary>
public interface IPinGeneratorOptionsManager
{
    /// <summary>Gets the current effective PIN options.</summary>
    PinGeneratorOptions Current { get; }

    /// <summary>Applies <paramref name="configure"/> to the current PIN options.</summary>
    /// <param name="configure">A delegate that mutates the options.</param>
    void Configure(Action<PinGeneratorOptions> configure);
}
