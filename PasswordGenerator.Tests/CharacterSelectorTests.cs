using PasswordGenerator.Generators;
using PasswordGenerator.Sets;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class CharacterSelectorTests
    {
        [Fact]
        public void GetNextCharacter_NullArray_ThrowsArgumentNullException()
        {
            var selector = new CharacterSelector(new SecureRng());
            Assert.Throws<ArgumentNullException>(() => selector.GetNextCharacter(null));
        }

        [Fact]
        public void GetNextCharacter_EmptyArray_ThrowsArgumentException()
        {
            var selector = new CharacterSelector(new SecureRng());
            Assert.Throws<ArgumentException>(() => selector.GetNextCharacter([]));
        }

        [Fact]
        public void GetNextCharacter_ValidArray_ReturnsCharacterFromArray()
        {
            var selector = new CharacterSelector(new SecureRng());
            var chars = new[] { 'A', 'B', 'C' };

            for (var i = 0; i < 50; i++)
            {
                Assert.Contains(selector.GetNextCharacter(chars), chars);
            }
        }

        [Fact]
        public void GetNextCharacter_SingleElementArray_AlwaysReturnsThatElement()
        {
            var selector = new CharacterSelector(new SecureRng());
            var chars = new[] { 'Z' };

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal('Z', selector.GetNextCharacter(chars));
            }
        }
    }
}
