using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Generators
{
    /// <summary>Snapshot of generator configuration resolved from the current <see cref="IPasswordGeneratorOptionsManager"/>.</summary>
    public class GeneratorConfig(IPasswordGeneratorOptionsManager optionsManager, ICharacterSetManager characterSetManager) : IGeneratorConfig
    {
        public List<ICharacterSet> CharacterSets { get; } = characterSetManager.CreateAndShuffleCharacterSets(optionsManager.Current.GetEffectiveCharacterSets());
        public int MaxRepetition { get; } = optionsManager.Current.MaxRepetition;
        public int Length { get; } = optionsManager.Current.Length;
        public bool AllowSequences { get; } = optionsManager.Current.AllowSequences;
        public bool AllowUpperLowerSequences { get; } = optionsManager.Current.AllowUpperLowerSequences;
        public bool MustStartWithLetter { get; } = optionsManager.Current.MustStartWithLetter;
        public bool ExcludeLeadingTrailingSymbols { get; } = optionsManager.Current.ExcludeLeadingTrailingSymbols;
    }
}