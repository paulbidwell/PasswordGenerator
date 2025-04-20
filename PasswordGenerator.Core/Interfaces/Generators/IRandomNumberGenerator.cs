namespace PasswordGenerator.Core.Interfaces.Generators
{
    public interface IRandomNumberGenerator
    {
        public int GetRandomIntInRange(int min, int max);
    }
}