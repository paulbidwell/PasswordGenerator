using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class CharacterSet : ICharacterSet
    {
        public required char[] Set { get; set; }
        public required string Characters { get; set; }
        public int Min { get; set; }
    }
}