using PasswordGenerator.Interfaces;

namespace PasswordGenerator
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