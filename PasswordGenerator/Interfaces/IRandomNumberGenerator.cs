namespace PasswordGenerator.Interfaces
{
    public interface IRandomNumberGenerator
    {
        public int GetRandomIntInRange(int min, int max);
    }
}