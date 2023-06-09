﻿using Microsoft.Extensions.Options;
using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class GeneratorConfig : Interfaces.IGeneratorConfig
    {
        public List<ICharacterSet> CharacterSets { get; }
        public int MaxRepetition { get; }
        public int Length { get; }
        public bool AllowSequences { get; }
        public bool AllowUpperLowerSequences { get; }

        public GeneratorConfig(IOptions<PasswordGeneratorOptions> options, ICharacterSetManager characterSetManager)
        {
            MaxRepetition = options.Value.MaxRepetition;
            Length = options.Value.Length;
            AllowSequences = options.Value.AllowSequences;
            AllowUpperLowerSequences = options.Value.AllowUpperLowerSequences;
            CharacterSets = characterSetManager.CreateAndShuffleCharacterSets(options.Value.CharactersSets);
        }
    }
}