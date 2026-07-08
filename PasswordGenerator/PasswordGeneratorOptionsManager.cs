using Microsoft.Extensions.Options;

namespace PasswordGenerator;

internal sealed class PasswordGeneratorOptionsManager(IOptionsMonitor<PasswordGeneratorOptions> monitor) : IPasswordGeneratorOptionsManager
{
    private readonly Lock _lock = new();

    public PasswordGeneratorOptions Current => monitor.CurrentValue;

    public void Configure(Action<PasswordGeneratorOptions> configure)
    {
        lock (_lock)
        {
            configure(monitor.CurrentValue);
        }
    }
}
