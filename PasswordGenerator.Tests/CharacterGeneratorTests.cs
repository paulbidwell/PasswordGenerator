using PasswordGenerator.Generators;
using PasswordGenerator.Sets;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class CharacterGeneratorTests
    {
        private static CharacterGenerator CreateGenerator(FakeGeneratorConfig config)
        {
            var rng = new SecureRng();
            var selector = new CharacterSelector(rng);
            return new CharacterGenerator(config, selector, rng, new StubPasswordShuffler(), new StubPositionConstraintEnforcer());
        }

        [Fact]
        public void GeneratePassword_ReturnsArrayOfCorrectLength()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 12,
                MaxRepetition = -1,
                AllowSequences = true,
                CharacterSets = [new FakeCharacterSet { Set = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), Min = 0 }]
            };

            var result = CreateGenerator(config).GeneratePassword();

            Assert.Equal(12, result.Length);
        }

        [Fact]
        public void GeneratePassword_SatisfiesMinimumCharacterSetRequirements()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = -1,
                AllowSequences = true,
                CharacterSets =
                [
                    new FakeCharacterSet { Set = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), Min = 3 },
                    new FakeCharacterSet { Set = "0123456789".ToCharArray(), Min = 2 }
                ]
            };

            var result = CreateGenerator(config).GeneratePassword();

            Assert.True(result.Count(char.IsUpper) >= 3);
            Assert.True(result.Count(char.IsDigit) >= 2);
        }

        [Fact]
        public void GeneratePassword_WithMaxRepetitionOne_EachCharAppearsAtMostOnce()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = 1,
                AllowSequences = true,
                CharacterSets = [new FakeCharacterSet { Set = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), Min = 0 }]
            };

            var result = CreateGenerator(config).GeneratePassword();

            Assert.All(result.GroupBy(c => c), g => Assert.Equal(1, g.Count()));
        }

        [Fact]
        public void GeneratePassword_WithMaxRepetitionNegativeOne_DoesNotRestrictRepeats()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = -1,
                AllowSequences = true,
                CharacterSets = [new FakeCharacterSet { Set = ['X'], Min = 0 }]
            };

            var result = CreateGenerator(config).GeneratePassword();

            Assert.Equal(10, result.Length);
            Assert.All(result, c => Assert.Equal('X', c));
        }

        [Fact]
        public void GeneratePassword_ReturnsNonNullArray()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 8,
                MaxRepetition = -1,
                AllowSequences = true,
                CharacterSets = [new FakeCharacterSet { Set = "abcdefgh".ToCharArray(), Min = 0 }]
            };

            var result = CreateGenerator(config).GeneratePassword();

            Assert.NotNull(result);
        }

        [Fact]
        public void GeneratePassword_WeightedSelection_FavoursLargerSets()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 1000,
                MaxRepetition = -1,
                AllowSequences = true,
                CharacterSets =
                [
                    new FakeCharacterSet { Set = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), Min = 0 },
                    new FakeCharacterSet { Set = "0123456789".ToCharArray(), Min = 0 }
                ]
            };

            var result = CreateGenerator(config).GeneratePassword();

            var letterCount = result.Count(char.IsUpper);
            var digitCount = result.Count(char.IsDigit);

            Assert.True(letterCount > digitCount,
                $"Expected letters ({letterCount}) to significantly outnumber digits ({digitCount}) with weighted selection.");
            Assert.True(letterCount > 550,
                $"Expected letters ({letterCount}) to be well above 50% with a 26:10 weight ratio.");
        }

        [Fact]
        public void GeneratePassword_ExplicitWeight_OverridesSetSize()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 1000,
                MaxRepetition = -1,
                AllowSequences = true,
                CharacterSets =
                [
                    new FakeCharacterSet { Set = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), Min = 0, Weight = 1 },
                    new FakeCharacterSet { Set = "0123456789".ToCharArray(), Min = 0, Weight = 9 }
                ]
            };

            var result = CreateGenerator(config).GeneratePassword();

            var digitCount = result.Count(char.IsDigit);

            Assert.True(digitCount > 750,
                $"Expected digits ({digitCount}) to dominate with explicit 9:1 weight ratio.");
        }

        [Fact]
        public void GeneratePassword_RetryLimitExceeded_ThrowsInvalidOperationException()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 2,
                MaxRepetition = 0,
                AllowSequences = true,
                CharacterSets = [new FakeCharacterSet { Set = ['X'], Min = 0 }]
            };

            Assert.Throws<InvalidOperationException>(() => CreateGenerator(config).GeneratePassword());
        }
    }
}