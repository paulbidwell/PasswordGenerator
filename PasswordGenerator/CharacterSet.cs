using PasswordGenerator.Interfaces;

namespace PasswordGenerator
{
    public class CharacterSet : ICharacterSet
    {
        public char[] Set { get; set; }
        public string Characters { get; set; }
        public int Min { get; set; }
    }
}