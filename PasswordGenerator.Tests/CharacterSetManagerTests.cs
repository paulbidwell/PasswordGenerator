using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Generators;
using PasswordGenerator.Sets;
using PasswordGenerator.Shufflers;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class CharacterSetManagerTests
    {
        private static CharacterSetManager CreateManager()
        {
            var rng = new SecureRng();
            return new CharacterSetManager(
                new CharacterSetShuffler(new CollectionShuffler(rng)),
                new CollectionShuffler(rng));
        }

        [Fact]
        public void CreateAndShuffleCharacterSets_ReturnsCorrectCount()
        {
            var input = new List<ICharacterSet>
            {
                new FakeCharacterSet { Characters = "ABC", Set = "ABC".ToCharArray(), Min = 1 },
                new FakeCharacterSet { Characters = "DEF", Set = "DEF".ToCharArray(), Min = 2 }
            };

            Assert.Equal(2, CreateManager().CreateAndShuffleCharacterSets(input).Count);
        }

        [Fact]
        public void CreateAndShuffleCharacterSets_ClonesInputNotSameReference()
        {
            var original = new FakeCharacterSet { Characters = "ABCDE", Set = "ABCDE".ToCharArray(), Min = 1 };
            var input = new List<ICharacterSet> { original };

            var result = CreateManager().CreateAndShuffleCharacterSets(input);

            Assert.NotSame(original, result[0]);
        }

        [Fact]
        public void CreateAndShuffleCharacterSets_PreservesMinValues()
        {
            var input = new List<ICharacterSet>
            {
                new FakeCharacterSet { Characters = "ABCDE", Set = "ABCDE".ToCharArray(), Min = 3 }
            };

            Assert.Equal(3, CreateManager().CreateAndShuffleCharacterSets(input)[0].Min);
        }

        [Fact]
        public void CreateAndShuffleCharacterSets_SetContainsSameCharactersAsInput()
        {
            var input = new List<ICharacterSet>
            {
                new FakeCharacterSet { Characters = "ABCDE", Set = "ABCDE".ToCharArray(), Min = 0 }
            };

            var result = CreateManager().CreateAndShuffleCharacterSets(input);

            Assert.Equal("ABCDE".OrderBy(c => c), result[0].Set.OrderBy(c => c));
        }
    }
}
