namespace PasswordGenerator.Core.Interfaces.Generators;

/// <summary>
/// Holds the results of a batch generation, including any warning when
/// fewer than the requested number of unique items could be produced.
/// </summary>
public sealed record BatchResult(IReadOnlyList<string> Items, string? Warning = null);
