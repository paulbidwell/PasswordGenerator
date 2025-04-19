namespace PasswordGenerator.Interfaces
{
    public interface IConfigurationValidator
    {
        void Validate(IGeneratorConfig config);
    }
}