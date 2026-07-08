namespace PasswordGenerator;

internal sealed class DirectPasswordOptionsManager(PasswordGeneratorOptions options) : IPasswordGeneratorOptionsManager
{
    public PasswordGeneratorOptions Current => options;

    public void Configure(Action<PasswordGeneratorOptions> configure)
    {
        configure(options);
    }
}
