using PasswordGenerator.Generators;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class SecureRngTests
    {
        [Fact]
        public void GetRandomIntInRange_MinGreaterThanMax_ThrowsArgumentException()
        {
            var rng = new SecureRng();
            Assert.Throws<ArgumentException>(() => rng.GetRandomIntInRange(5, 4));
        }

        [Fact]
        public void GetRandomIntInRange_MinEqualsMax_ReturnsMin()
        {
            var rng = new SecureRng();
            Assert.Equal(7, rng.GetRandomIntInRange(7, 7));
        }

        [Fact]
        public void GetRandomIntInRange_ValidRange_ReturnsValueWithinBounds()
        {
            var rng = new SecureRng();
            for (var i = 0; i < 100; i++)
            {
                var result = rng.GetRandomIntInRange(1, 10);
                Assert.InRange(result, 1, 10);
            }
        }

        [Fact]
        public void GetRandomIntInRange_MaxIsIntMaxValue_ReturnsValueWithinBounds()
        {
            var rng = new SecureRng();
            var result = rng.GetRandomIntInRange(int.MaxValue - 1, int.MaxValue);
            Assert.InRange(result, int.MaxValue - 1, int.MaxValue);
        }

        [Fact]
        public void GetRandomIntInRange_ZeroToZero_ReturnsZero()
        {
            var rng = new SecureRng();
            Assert.Equal(0, rng.GetRandomIntInRange(0, 0));
        }
    }
}
