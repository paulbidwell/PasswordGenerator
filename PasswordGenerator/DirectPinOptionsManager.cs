namespace PasswordGenerator;

internal sealed class DirectPinOptionsManager(PinGeneratorOptions options) : IPinGeneratorOptionsManager
{
    public PinGeneratorOptions Current => options;

    public void Configure(Action<PinGeneratorOptions> configure)
    {
        configure(options);
    }
}
