using Xunit;

namespace PasswordGenerator.Tests
{
    public class PinGeneratorBuilderTests
    {
        [Fact]
        public void Create_ReturnsNewBuilder()
        {
            var builder = PinGeneratorBuilder.Create();
            Assert.NotNull(builder);
        }

        [Fact]
        public void Build_DefaultOptions_GeneratesPinOfDefaultLength()
        {
            var pin = PinGeneratorBuilder.Create().Build().Generate();
            Assert.Equal(6, pin.Length);
        }

        [Fact]
        public void Build_DefaultOptions_AllDigits()
        {
            var pin = PinGeneratorBuilder.Create().Build().Generate();
            Assert.All(pin.ToCharArray(), c => Assert.True(char.IsDigit(c)));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(12)]
        public void WithLength_ProducesPinOfExpectedLength(int length)
        {
            var pin = PinGeneratorBuilder.Create()
                .WithLength(length)
                .Build()
                .Generate();

            Assert.Equal(length, pin.Length);
        }

        [Fact]
        public void RejectTrivialPatterns_False_DoesNotThrow()
        {
            var pin = PinGeneratorBuilder.Create()
                .RejectTrivialPatterns(false)
                .Build()
                .Generate();

            Assert.NotNull(pin);
        }

        [Fact]
        public void WithMaxRetries_CustomValue_DoesNotThrow()
        {
            var pin = PinGeneratorBuilder.Create()
                .WithMaxRetries(50)
                .Build()
                .Generate();

            Assert.NotNull(pin);
        }

        [Fact]
        public void Build_GenerateBatch_ReturnsRequestedCount()
        {
            var results = PinGeneratorBuilder.Create()
                .WithLength(8)
                .Build()
                .GenerateBatch(10)
                .Items;

            Assert.Equal(10, results.Count);
            Assert.All(results, pin =>
            {
                Assert.Equal(8, pin.Length);
                Assert.All(pin.ToCharArray(), c => Assert.True(char.IsDigit(c)));
            });
        }

        [Fact]
        public void Build_ReturnsIndependentGenerators()
        {
            var builder = PinGeneratorBuilder.Create().WithLength(4);
            var gen1 = builder.Build();

            builder.WithLength(8);
            var gen2 = builder.Build();

            Assert.Equal(4, gen1.Generate().Length);
            Assert.Equal(8, gen2.Generate().Length);
        }

        [Fact]
        public void Build_LengthTooShort_ThrowsOnBuild()
        {
            var builder = PinGeneratorBuilder.Create()
                .WithLength(2);

            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void Build_LengthTooLong_ThrowsOnBuild()
        {
            var builder = PinGeneratorBuilder.Create()
                .WithLength(20);

            Assert.Throws<ArgumentException>(() => builder.Build());
        }
    }
}
