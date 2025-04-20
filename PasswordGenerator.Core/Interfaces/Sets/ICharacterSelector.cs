namespace PasswordGenerator.Core.Interfaces.Sets
{
    public interface ICharacterSelector
    {
        public char GetNextCharacter(char[]? characters);
    }
}