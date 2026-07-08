using PasswordGenerator.Generators;
using PasswordGenerator.Shufflers;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class PasswordShufflerTests
    {
        [Fact]
        public void Shuffle_PassesAllowSequencesFromConfig_DoesNotThrow()
        {
            var shuffler = new PasswordShuffler(new CollectionShuffler(new SecureRng()));
            var buffer = "ABCDE".ToCharArray();
            var config = new FakeGeneratorConfig { AllowSequences = true, AllowUpperLowerSequences = true, Length = 5 };

            shuffler.Shuffle(buffer, config);

            Assert.Equal(5, buffer.Length);
        }

        [Fact]
        public void Shuffle_DisallowSequences_UnresolvableBuffer_ThrowsInvalidOperationException()
        {
            var shuffler = new PasswordShuffler(new CollectionShuffler(new FakeRandomNumberGenerator(0)));
            var buffer = new[] { 'a', 'a', 'a' };
            var config = new FakeGeneratorConfig { AllowSequences = false, AllowUpperLowerSequences = false, Length = 3 };

            Assert.Throws<InvalidOperationException>(() => shuffler.Shuffle(buffer, config));
        }
    }
}
