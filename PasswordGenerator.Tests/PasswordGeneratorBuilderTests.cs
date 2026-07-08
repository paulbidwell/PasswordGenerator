using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Sets;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class PasswordGeneratorBuilderTests
    {
        [Fact]
        public void Create_ReturnsNewBuilder()
        {
            var builder = PasswordGeneratorBuilder.Create();
            Assert.NotNull(builder);
        }

        [Fact]
        public void From_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => PasswordGeneratorBuilder.From(null!));
        }

        [Fact]
        public void From_CopiesOptions_DoesNotMutateOriginal()
        {
            var original = new PasswordGeneratorOptions { Length = 30 };
            var builder = PasswordGeneratorBuilder.From(original);
            builder.WithLength(10);

            Assert.Equal(30, original.Length);
        }

        [Fact]
        public void Build_DefaultOptions_GeneratesPasswordOfDefaultLength()
        {
            var generator = PasswordGeneratorBuilder.Create().Build();
            var password = generator.Generate();

            Assert.Equal(22, password.Length);
        }

        [Theory]
        [InlineData(14)]
        [InlineData(16)]
        [InlineData(32)]
        public void WithLength_ProducesPasswordOfExpectedLength(int length)
        {
            var password = PasswordGeneratorBuilder.Create()
                .WithLength(length)
                .Build()
                .Generate();

            Assert.Equal(length, password.Length);
        }

        [Fact]
        public void WithMaxRepetition_Unlimited_AllowsRepeats()
        {
            var generator = PasswordGeneratorBuilder.Create()
                .WithMaxRepetition(-1)
                .Build();

            var password = generator.Generate();
            Assert.NotNull(password);
        }

        [Fact]
        public void AllowSequences_Default_DoesNotThrow()
        {
            var password = PasswordGeneratorBuilder.Create()
                .AllowSequences()
                .Build()
                .Generate();

            Assert.NotNull(password);
        }

        [Fact]
        public void AsciiOnly_AllCharactersAreAscii()
        {
            var password = PasswordGeneratorBuilder.Create()
                .AsciiOnly()
                .WithLength(40)
                .WithMaxRepetition(-1)
                .AllowSequences()
                .Build()
                .Generate();

            Assert.All(password.ToCharArray(), c => Assert.True(c <= 127));
        }

        [Fact]
        public void ExcludeAmbiguousCharacters_RemovesAmbiguous()
        {
            var passwords = PasswordGeneratorBuilder.Create()
                .ExcludeAmbiguousCharacters()
                .WithLength(40)
                .WithMaxRepetition(-1)
                .AllowSequences()
                .Build()
                .GenerateBatch(20)
                .Items;

            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters.ToHashSet();
            foreach (var pw in passwords)
            {
                Assert.All(pw.ToCharArray(), c => Assert.DoesNotContain(c, ambiguous));
            }
        }

        [Fact]
        public void ExcludeCharacters_RemovesSpecifiedCharacters()
        {
            var passwords = PasswordGeneratorBuilder.Create()
                .ExcludeCharacters("abc")
                .WithLength(30)
                .WithMaxRepetition(-1)
                .AllowSequences()
                .Build()
                .GenerateBatch(20)
                .Items;

            foreach (var pw in passwords)
            {
                Assert.DoesNotContain('a', pw);
                Assert.DoesNotContain('b', pw);
                Assert.DoesNotContain('c', pw);
            }
        }

        [Fact]
        public void MustStartWithLetter_FirstCharIsLetter()
        {
            var passwords = PasswordGeneratorBuilder.Create()
                .MustStartWithLetter()
                .Build()
                .GenerateBatch(20)
                .Items;

            Assert.All(passwords, pw => Assert.True(char.IsLetter(pw[0])));
        }

        [Fact]
        public void ExcludeLeadingTrailingSymbols_BothEndsAreLetterOrDigit()
        {
            var passwords = PasswordGeneratorBuilder.Create()
                .ExcludeLeadingTrailingSymbols()
                .Build()
                .GenerateBatch(20)
                .Items;

            Assert.All(passwords, pw =>
            {
                Assert.True(char.IsLetterOrDigit(pw[0]));
                Assert.True(char.IsLetterOrDigit(pw[^1]));
            });
        }

        [Fact]
        public void ClearCharacterSets_ThenAddCustom_UsesOnlyCustom()
        {
            var password = PasswordGeneratorBuilder.Create()
                .ClearCharacterSets()
                .AddCharacterSet("ABCDEFGHIJKLMNOPQRSTUVWXYZ", min: 5)
                .AddCharacterSet("0123456789", min: 5)
                .WithLength(14)
                .WithMaxRepetition(-1)
                .AllowSequences()
                .Build()
                .Generate();

            Assert.Equal(14, password.Length);
            Assert.All(password.ToCharArray(), c =>
                Assert.True(char.IsUpper(c) || char.IsDigit(c)));
        }

        [Fact]
        public void AddCharacterSet_NullCharacters_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PasswordGeneratorBuilder.Create().AddCharacterSet(null!));
        }

        [Fact]
        public void Build_GenerateBatch_ReturnsRequestedCount()
        {
            var results = PasswordGeneratorBuilder.Create()
                .WithLength(24)
                .Build()
                .GenerateBatch(10)
                .Items;

            Assert.Equal(10, results.Count);
            Assert.All(results, pw => Assert.Equal(24, pw.Length));
        }

        [Fact]
        public void Build_ReturnsIndependentGenerators()
        {
            var builder = PasswordGeneratorBuilder.Create().WithLength(16);
            var gen1 = builder.Build();

            builder.WithLength(30);
            var gen2 = builder.Build();

            Assert.Equal(16, gen1.Generate().Length);
            Assert.Equal(30, gen2.Generate().Length);
        }

        [Fact]
        public void WithUnlimitedRepetition_AllowsRepeats()
        {
            var password = PasswordGeneratorBuilder.Create()
                .WithUnlimitedRepetition()
                .Build()
                .Generate();

            Assert.NotNull(password);
        }

        [Fact]
        public void From_OwaspPreset_ProducesCorrectLength()
        {
            var password = PasswordGeneratorBuilder.From(PasswordPolicy.Owasp)
                .Build()
                .Generate();

            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void From_Preset_CanBeOverridden()
        {
            var password = PasswordGeneratorBuilder.From(PasswordPolicy.Owasp)
                .WithLength(20)
                .Build()
                .Generate();

            Assert.Equal(20, password.Length);
        }
    }
}
