namespace PasswordGenerator;

/// <summary>Configuration options for PIN generation.</summary>
public class PinGeneratorOptions
{
    /// <summary>Number of digits in the generated PIN.</summary>
    public int Length { get; set; } = 6;

    /// <summary>Reject trivial patterns such as repeated digits or sequential runs.</summary>
    public bool RejectTrivialPatterns { get; set; } = true;

    /// <summary>Maximum generation retries when a trivial pattern is detected.</summary>
    public int MaxRetries { get; set; } = 10;
}
