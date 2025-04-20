using Microsoft.Extensions.Options;
using PasswordGenerator.Core;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Generators
{
    public class GeneratorConfig(IOptions<PasswordGeneratorOptions> options, ICharacterSetManager characterSetManager) : IGeneratorConfig
    {
        public List<ICharacterSet> CharacterSets { get; } = characterSetManager.CreateAndShuffleCharacterSets(options.Value.CharacterSets);
        public int MaxRepetition { get; } = options.Value.MaxRepetition;
        public int Length { get; } = options.Value.Length;
        public bool AllowSequences { get; } = options.Value.AllowSequences;
        public bool AllowUpperLowerSequences { get; } = options.Value.AllowUpperLowerSequences;
    }
}