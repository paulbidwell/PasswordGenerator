using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Core.Interfaces.Generators;

public interface IGeneratorConfig
{
    public List<ICharacterSet> CharacterSets { get; }
    public int MaxRepetition { get; }
    public int Length { get; }
    public bool AllowSequences { get; }
    public bool AllowUpperLowerSequences { get; }
}