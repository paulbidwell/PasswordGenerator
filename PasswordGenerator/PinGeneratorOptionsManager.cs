using Microsoft.Extensions.Options;

namespace PasswordGenerator;

internal sealed class PinGeneratorOptionsManager(IOptionsMonitor<PinGeneratorOptions> monitor) : IPinGeneratorOptionsManager
{
    private readonly Lock _lock = new();

    public PinGeneratorOptions Current => monitor.CurrentValue;

    public void Configure(Action<PinGeneratorOptions> configure)
    {
        lock (_lock)
        {
            configure(monitor.CurrentValue);
        }
    }
}
