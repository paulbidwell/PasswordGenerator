namespace PasswordGenerator
{
    public class PasswordGeneratorOptions
    {
        public bool AllowSequences { get; set; }
        public bool AllowUpperLowerSequences { get; set; }
        public List<CharacterSetOptions> CharactersSets { get; set; }
        public int Length { get; set; }
        public int MaxRepetition { get; set; }
        public string OutputPath { get; set; }
        public bool OutputToConsole { get; set; }
        public bool OutputToFile { get; set; }
        public int PasswordsToGenerate { get; set; }
    }
}