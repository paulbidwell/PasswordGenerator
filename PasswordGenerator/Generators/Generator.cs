using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Interfaces.Generators;

namespace PasswordGenerator.Generators
{
    public class Generator(IGeneratorConfig config, IConfigurationValidator validator, ICharacterGenerator characterGenerator) : IGenerator
    {
        public string Generate()
        {
            validator.Validate(config);
            var generated = characterGenerator.GeneratePassword();
            return new string(generated);
        }
    }
}