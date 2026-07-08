using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Generators;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class GeneratorTests
    {
        private static Generator CreateGenerator(
            StubConfigurationValidator? validator = null,
            ICharacterGenerator? charGen = null)
        {
            var config = new FakeGeneratorConfig { Length = 5, MaxRepetition = -1, AllowSequences = true };
            return new Generator(
                config,
                validator ?? new StubConfigurationValidator(),
                charGen ?? new StubCharacterGenerator(['x', 'M', '3', 'k', 'P']));
        }

        [Fact]
        public void Generate_ReturnsStringFromCharacterGenerator()
        {
            var charGen = new StubCharacterGenerator(['H', 'e', 'l', 'l', 'o']);
            var generator = CreateGenerator(charGen: charGen);

            Assert.Equal("Hello", generator.Generate());
        }

        [Fact]
        public void Constructor_CallsValidate()
        {
            var validator = new StubConfigurationValidator();
            _ = CreateGenerator(validator: validator);

            Assert.True(validator.WasCalled);
        }

        [Fact]
        public void Constructor_WhenValidatorThrows_ExceptionPropagates()
        {
            var validator = new StubConfigurationValidator(new ArgumentException("bad config"));

            Assert.Throws<ArgumentException>(() => CreateGenerator(validator: validator));
        }

        [Fact]
        public void Generate_ReturnsNonNullString()
        {
            Assert.NotNull(CreateGenerator().Generate());
        }

        [Fact]
        public void GenerateBatch_ReturnsRequestedCount()
        {
            var charGen = new SequentialCharacterGenerator([
                ['x', 'M', '3', 'k', 'P'],
                ['R', 'v', '7', 'n', 'G'],
                ['L', 'w', '2', 'j', 'S']
            ]);
            var generator = CreateGenerator(charGen: charGen);

            var result = generator.GenerateBatch(3);

            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public void GenerateBatch_AllPasswordsAreUnique()
        {
            var charGen = new SequentialCharacterGenerator([
                ['x', 'M', '3', 'k', 'P'],
                ['R', 'v', '7', 'n', 'G'],
                ['L', 'w', '2', 'j', 'S'],
                ['T', 'a', '9', 'f', 'H'],
                ['Q', 'z', '5', 'c', 'N']
            ]);
            var generator = CreateGenerator(charGen: charGen);

            var result = generator.GenerateBatch(5);

            Assert.Equal(result.Items.Count, new HashSet<string>(result.Items, StringComparer.Ordinal).Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void GenerateBatch_ZeroOrNegative_ThrowsArgumentOutOfRangeException(int count)
        {
            var generator = CreateGenerator();
            Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateBatch(count));
        }

        [Fact]
        public void GenerateBatch_SingleItem_ReturnsSinglePassword()
        {
            var generator = CreateGenerator();

            var result = generator.GenerateBatch(1);

            Assert.Single(result.Items);
        }

        [Fact]
        public void GenerateBatch_WhenDuplicatesExhausted_ReturnsPartialResultWithWarning()
        {
            var charGen = new SequentialCharacterGenerator([
                ['x', 'M', '3', 'k', 'P'],
                ['x', 'M', '3', 'k', 'P']
            ]);
            var generator = CreateGenerator(charGen: charGen);

            var result = generator.GenerateBatch(2);

            Assert.True(result.Items.Count < 2);
            Assert.NotNull(result.Warning);
        }

        [Fact]
        public void Generate_WhenAllowSequencesFalse_RejectsSequentialPatterns()
        {
            var charGen = new SequentialCharacterGenerator([
                ['a', 'b', 'c', 'd', 'e'],
                ['x', 'M', '3', 'k', 'P']
            ]);
            var config = new FakeGeneratorConfig { Length = 5, MaxRepetition = -1, AllowSequences = false };
            var generator = new Generator(config, new StubConfigurationValidator(), charGen);

            var result = generator.Generate();

            Assert.Equal("xM3kP", result);
        }

        [Fact]
        public void Generate_WhenAllowSequencesFalse_RejectsKeyboardPatterns()
        {
            var charGen = new SequentialCharacterGenerator([
                ['q', 'w', 'e', 'r', 't'],
                ['T', 'a', '9', 'f', 'H']
            ]);
            var config = new FakeGeneratorConfig { Length = 5, MaxRepetition = -1, AllowSequences = false };
            var generator = new Generator(config, new StubConfigurationValidator(), charGen);

            var result = generator.Generate();

            Assert.Equal("Ta9fH", result);
        }

        [Fact]
        public void Generate_WhenAllowSequencesTrue_AcceptsSequentialPatterns()
        {
            var charGen = new StubCharacterGenerator(['a', 'b', 'c', 'd', 'e']);
            var config = new FakeGeneratorConfig { Length = 5, MaxRepetition = -1, AllowSequences = true };
            var generator = new Generator(config, new StubConfigurationValidator(), charGen);

            var result = generator.Generate();

            Assert.Equal("abcde", result);
        }

        [Fact]
        public void Generate_WhenAllowSequencesFalse_AllAttemptsUnwanted_ThrowsInvalidOperationException()
        {
            var charGen = new StubCharacterGenerator(['a', 'b', 'c', 'd', 'e']);
            var config = new FakeGeneratorConfig { Length = 5, MaxRepetition = -1, AllowSequences = false };
            var generator = new Generator(config, new StubConfigurationValidator(), charGen);

            Assert.Throws<InvalidOperationException>(() => generator.Generate());
        }
    }
}