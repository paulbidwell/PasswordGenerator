using PasswordGenerator.Generators;
using PasswordGenerator.Shufflers;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class CharacterSetShufflerTests
    {
        private static CharacterSetShuffler CreateShuffler() =>
            new(new CollectionShuffler(new SecureRng()));

        [Fact]
        public void ShuffleCharacterSet_EmptySet_ThrowsArgumentException()
        {
            var set = new FakeCharacterSet { Set = [], Characters = "" };
            Assert.Throws<ArgumentException>(() => CreateShuffler().ShuffleCharacterSet(set));
        }

        [Fact]
        public void ShuffleCharacterSet_ValidSet_DoesNotThrow()
        {
            var set = new FakeCharacterSet { Set = "ABC".ToCharArray(), Characters = "ABC" };
            CreateShuffler().ShuffleCharacterSet(set);
        }

        [Fact]
        public void ShuffleCharacterSet_ValidSet_ContainsSameCharactersAfterShuffle()
        {
            var set = new FakeCharacterSet { Set = "ABCDE".ToCharArray(), Characters = "ABCDE" };
            CreateShuffler().ShuffleCharacterSet(set);
            Assert.Equal("ABCDE".OrderBy(c => c), set.Set.OrderBy(c => c));
        }
    }
}
