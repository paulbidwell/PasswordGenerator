using PasswordGenerator.Generators;
using PasswordGenerator.Shufflers;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class CollectionShufflerTests
    {
        [Fact]
        public void Shuffle_EmptyCollection_DoesNotThrow()
        {
            var shuffler = new CollectionShuffler(new SecureRng());
            shuffler.Shuffle(new List<char>(), allowSequences: true, allowUpperLower: true);
        }

        [Fact]
        public void Shuffle_SingleElement_DoesNotThrow()
        {
            var shuffler = new CollectionShuffler(new SecureRng());
            var list = new List<char> { 'a' };
            shuffler.Shuffle(list, allowSequences: false, allowUpperLower: false);
            Assert.Equal('a', list[0]);
        }

        [Fact]
        public void Shuffle_AllowSequences_DoesNotThrowEvenWithAllDuplicates()
        {
            var shuffler = new CollectionShuffler(new SecureRng());
            shuffler.Shuffle(new List<char> { 'a', 'a', 'a' }, allowSequences: true, allowUpperLower: true);
        }

        [Fact]
        public void Shuffle_DisallowSequences_UnresolvableSequences_ThrowsInvalidOperationException()
        {
            var shuffler = new CollectionShuffler(new FakeRandomNumberGenerator(0));
            Assert.Throws<InvalidOperationException>(
                () => shuffler.Shuffle(new List<char> { 'a', 'a', 'a' }, allowSequences: false, allowUpperLower: false));
        }

        [Fact]
        public void Shuffle_AllowUpperLower_TreatsDifferentCaseAsDistinct_DoesNotThrow()
        {
            var shuffler = new CollectionShuffler(new SecureRng());
            shuffler.Shuffle(new List<char> { 'a', 'A' }, allowSequences: false, allowUpperLower: true);
        }

        [Fact]
        public void Shuffle_NoAllowUpperLower_TreatsDifferentCaseAsSame_ThrowsInvalidOperationException()
        {
            var shuffler = new CollectionShuffler(new FakeRandomNumberGenerator(0));
            Assert.Throws<InvalidOperationException>(
                () => shuffler.Shuffle(new List<char> { 'a', 'A' }, allowSequences: false, allowUpperLower: false));
        }

        [Fact]
        public void Shuffle_NonCharList_DisallowSequences_DetectsDuplicatesViaToString()
        {
            var shuffler = new CollectionShuffler(new FakeRandomNumberGenerator(0));
            Assert.Throws<InvalidOperationException>(
                () => shuffler.Shuffle(new List<string> { "x", "x", "x" }, allowSequences: false, allowUpperLower: true));
        }

        [Fact]
        public void Shuffle_NonCharList_AllowSequences_DoesNotThrow()
        {
            var shuffler = new CollectionShuffler(new SecureRng());
            var list = new List<string> { "alpha", "beta", "gamma" };

            shuffler.Shuffle(list, allowSequences: true, allowUpperLower: true);

            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void Shuffle_NonCharList_DisallowSequences_CaseInsensitive_DetectsDuplicates()
        {
            var shuffler = new CollectionShuffler(new FakeRandomNumberGenerator(0));
            Assert.Throws<InvalidOperationException>(
                () => shuffler.Shuffle(new List<string> { "a", "A" }, allowSequences: false, allowUpperLower: false));
        }
    }
}
