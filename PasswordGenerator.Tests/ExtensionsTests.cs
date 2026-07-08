using PasswordGenerator.Core;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void TryAddCount_NewChar_BelowMax_ReturnsTrueAndAddsToDict()
        {
            var dict = new Dictionary<char, int>();
            var result = dict.TryAddCount('a', 2, out _);
            Assert.True(result);
            Assert.Equal(1, dict['a']);
        }

        [Fact]
        public void TryAddCount_ExistingChar_BelowMax_ReturnsTrueAndIncrements()
        {
            var dict = new Dictionary<char, int> { ['a'] = 1 };
            var result = dict.TryAddCount('a', 2, out _);
            Assert.True(result);
            Assert.Equal(2, dict['a']);
        }

        [Fact]
        public void TryAddCount_CharAtMaxCount_ReturnsFalseAndLeavesCountUnchanged()
        {
            var dict = new Dictionary<char, int> { ['a'] = 2 };
            var result = dict.TryAddCount('a', 2, out var exceeds);
            Assert.False(result);
            Assert.True(exceeds);
            Assert.Equal(2, dict['a']);
        }

        [Fact]
        public void TryAddCount_MaxZero_FirstOccurrence_ReturnsFalse()
        {
            var dict = new Dictionary<char, int>();
            var result = dict.TryAddCount('a', 0, out _);
            Assert.False(result);
            Assert.False(dict.ContainsKey('a'));
        }
    }
}
