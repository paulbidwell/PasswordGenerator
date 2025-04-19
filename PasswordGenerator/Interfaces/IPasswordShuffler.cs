namespace PasswordGenerator.Interfaces
{
    public interface IPasswordShuffler
    {
        void Shuffle(char[] buffer, IGeneratorConfig config);
    }
}