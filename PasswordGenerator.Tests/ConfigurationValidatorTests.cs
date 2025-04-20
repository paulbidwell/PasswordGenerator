using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class FakeCharacterSet : ICharacterSet
    {
        public char[] Set { get; set; } = [];
        public string Characters { get; set; } = "";
        public int Min { get; set; }
    }

    public class FakeGeneratorConfig : IGeneratorConfig
    {
        public List<ICharacterSet> CharacterSets { get; init; } = [];
        public int MaxRepetition { get; init; }
        public int Length { get; init; }
        public bool AllowSequences { get; init; }
        public bool AllowUpperLowerSequences { get; init; }
    }

    public class ConfigurationValidatorTests
    {
        [Fact]
        public void Validate_ValidConfiguration_DoesNotThrow()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 20,
                MaxRepetition = 1,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets =
                [
                    new FakeCharacterSet { Set = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), Min = 5 },
                    new FakeCharacterSet { Set = "abcdefghijklmnopqrstuvwxyz".ToCharArray(), Min = 5 },
                    new FakeCharacterSet { Set = "0123456789".ToCharArray(), Min = 2 },
                    new FakeCharacterSet { Set = "!£$%^&*()-_=+[]{}@#~".ToCharArray(), Min = 2 }
                ]
            };

            var validator = new ConfigurationValidator();
            validator.Validate(config);
        }

        [Fact]
        public void Validate_EmptyCharacterSet_ThrowsArgumentException()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = 1,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets = [new FakeCharacterSet { Set = "".ToCharArray(), Min = 1 }]
            };

            var validator = new ConfigurationValidator();
            Assert.Throws<ArgumentException>(() => validator.Validate(config));
        }

        [Fact]
        public void Validate_MinUsageExceedsLength_ThrowsArgumentException()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 5,
                MaxRepetition = 1,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets =
                [
                    new FakeCharacterSet { Set = "ABCD".ToCharArray(), Min = 3 },
                    new FakeCharacterSet { Set = "EFGH".ToCharArray(), Min = 3 }
                ]
            };

            var validator = new ConfigurationValidator();
            Assert.Throws<ArgumentException>(() => validator.Validate(config));
        }

        [Fact]
        public void Validate_InvalidMinUsage_ThrowsArgumentException()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = 1,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets =
                [
                    new FakeCharacterSet { Set = "ABCDEFG".ToCharArray(), Min = -1 },
                    new FakeCharacterSet { Set = "abcdefg".ToCharArray(), Min = 11 }
                ]
            };

            var validator = new ConfigurationValidator();
            Assert.Throws<ArgumentException>(() => validator.Validate(config));
        }

        [Fact]
        public void Validate_MaxRepetitionZeroAndNotEnoughUniqueCharacters_ThrowsInvalidOperationException()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = 0,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets = [new FakeCharacterSet { Set = "AAA".ToCharArray(), Min = 1 }]
            };

            var validator = new ConfigurationValidator();
            Assert.Throws<InvalidOperationException>(() => validator.Validate(config));
        }

        [Fact]
        public void Validate_MaxRepetitionLessThanNegativeOne_ThrowsArgumentException()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = -2,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets = [new FakeCharacterSet { Set = "ABCDEFGHIJ".ToCharArray(), Min = 1 }]
            };

            var validator = new ConfigurationValidator();
            Assert.Throws<ArgumentException>(() => validator.Validate(config));
        }

        [Fact]
        public void Validate_MaxRepetitionZeroWithEnoughUniqueCharacters_DoesNotThrow()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 5,
                MaxRepetition = 0,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets = [new FakeCharacterSet { Set = "ABCDE".ToCharArray(), Min = 0 }]
            };

            var validator = new ConfigurationValidator();
            validator.Validate(config);
        }

        [Fact]
        public void Validate_MaxRepetitionNegativeOne_DoesNotThrow()
        {
            var config = new FakeGeneratorConfig
            {
                Length = 10,
                MaxRepetition = -1,
                AllowSequences = false,
                AllowUpperLowerSequences = false,
                CharacterSets = [new FakeCharacterSet { Set = "ABCDEFGHIJ".ToCharArray(), Min = 0 }]
            };

            var validator = new ConfigurationValidator();
            validator.Validate(config);
        }
    }
}