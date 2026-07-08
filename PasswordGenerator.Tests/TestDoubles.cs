using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Tests;

internal class StubPositionConstraintEnforcer : IPositionConstraintEnforcer
{
    public void Enforce(char[] buffer, IGeneratorConfig config) { }
}

internal class FakeRandomNumberGenerator(int fixedValue = 0) : IRandomNumberGenerator
{
    public int GetRandomIntInRange(int min, int max) => fixedValue;
}

internal class StubPasswordShuffler : IPasswordShuffler
{
    public void Shuffle(char[] buffer, IGeneratorConfig config) { }
}

internal class StubConfigurationValidator : IConfigurationValidator
{
    public bool WasCalled { get; private set; }
    private readonly Exception? _toThrow;

    public StubConfigurationValidator(Exception? toThrow = null) => _toThrow = toThrow;

    public void Validate(IGeneratorConfig config)
    {
        WasCalled = true;
        if (_toThrow is not null) throw _toThrow;
    }
}

internal class StubCharacterGenerator : ICharacterGenerator
{
    private readonly char[] _result;
    public StubCharacterGenerator(char[] result) => _result = result;
    public char[] GeneratePassword() => (char[])_result.Clone();
}

internal class SequentialCharacterGenerator(char[][] sequence) : ICharacterGenerator
{
    private int _index;
    public char[] GeneratePassword() => (char[])sequence[_index++ % sequence.Length].Clone();
}
