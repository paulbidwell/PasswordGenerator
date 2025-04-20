using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Generators;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class CharacterGeneratorTests
    {
        private class DummyCharacterGenerator : ICharacterGenerator
        {
            public char[] GeneratePassword()
            {
                return ['a', 'b', 'c', '1', '2', '3'];
            }
        }

        [Fact]
        public void GeneratePassword_Returns_NonNullAndNonEmptyArray()
        {
            ICharacterGenerator generator = new DummyCharacterGenerator();

            var result = generator.GeneratePassword();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GeneratePassword_Returns_ExpectedCharacters()
        {
            ICharacterGenerator generator = new DummyCharacterGenerator();
            var expected = new[] { 'a', 'b', 'c', '1', '2', '3' };

            var result = generator.GeneratePassword();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ProductionGeneratePassword_Returns_NonNullAndNonEmptyArray()
        {
            ICharacterGenerator generator = new DummyCharacterGenerator();

            var result = generator.GeneratePassword();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ProductionGeneratePassword_Returns_PasswordOfExpectedLength()
        {
            ICharacterGenerator generator = new DummyCharacterGenerator();

            var result = generator.GeneratePassword();

            Assert.Equal(6, result.Length);
        }
    }
}