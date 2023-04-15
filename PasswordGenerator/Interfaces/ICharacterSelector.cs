namespace PasswordGenerator.Interfaces
{
    public interface ICharacterSelector
    {
        public char GetNextCharacter(char[]? characters);
    }
}