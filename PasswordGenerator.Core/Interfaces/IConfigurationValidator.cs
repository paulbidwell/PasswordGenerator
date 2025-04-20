using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Core.Interfaces
{
    public interface IConfigurationValidator
    {
        void Validate(IGeneratorConfig config);
    }
}