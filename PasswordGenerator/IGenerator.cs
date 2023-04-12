namespace PasswordGenerator
{
    public interface IGenerator
    {
        IGenerator Configure();
        string Generate();
    }
}