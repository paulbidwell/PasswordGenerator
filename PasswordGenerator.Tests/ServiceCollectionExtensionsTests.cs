using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Core.Interfaces.Generators;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class ServiceCollectionExtensionsTests : IDisposable
    {
        private readonly ServiceProvider _provider;

        public ServiceCollectionExtensionsTests()
        {
            _provider = new ServiceCollection()
                .AddPasswordGenerator()
                .BuildServiceProvider();
        }

        public void Dispose() => _provider.Dispose();

        [Fact]
        public void AddPasswordGenerator_ResolvesGenerator()
        {
            var generator = _provider.GetRequiredService<IGenerator>();
            Assert.NotNull(generator);
        }

        [Fact]
        public void AddPasswordGenerator_ResolvesOptionsManager()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.NotNull(manager);
        }

        [Fact]
        public void AddPasswordGenerator_DefaultOptions_GeneratesPasswordOfDefaultLength()
        {
            var generator = _provider.GetRequiredService<IGenerator>();
            var password = generator.Generate();
            Assert.Equal(22, password.Length);
        }

        [Fact]
        public void AddPasswordGenerator_WithConfigure_AppliesLengthAtRegistration()
        {
            using var provider = new ServiceCollection()
                .AddPasswordGenerator(o => o.Length = 16)
                .BuildServiceProvider();

            var password = provider.GetRequiredService<IGenerator>().Generate();
            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void OptionsManager_Current_ReflectsDefaultLength()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.Equal(22, manager.Current.Length);
        }

        [Fact]
        public void OptionsManager_Current_ReflectsDefaultMaxRepetition()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.Equal(2, manager.Current.MaxRepetition);
        }

        [Fact]
        public void OptionsManager_Configure_UpdatesCurrentLength()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            manager.Configure(o => o.Length = 18);
            Assert.Equal(18, manager.Current.Length);
        }

        [Fact]
        public void OptionsManager_Configure_ChangesReflectedInNextGenerate()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            manager.Configure(o => o.Length = 18);

            var password = _provider.GetRequiredService<IGenerator>().Generate();
            Assert.Equal(18, password.Length);
        }

        [Fact]
        public void OptionsManager_Configure_MultipleChangesAppliedTogether()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();

            manager.Configure(o =>
            {
                o.Length = 16;
                o.AllowSequences = true;
                o.MaxRepetition = -1;
            });

            Assert.Equal(16, manager.Current.Length);
            Assert.True(manager.Current.AllowSequences);
            Assert.Equal(-1, manager.Current.MaxRepetition);
        }

        [Fact]
        public void OptionsManager_Configure_CanBeCalledMultipleTimes()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();

            manager.Configure(o => o.Length = 16);
            manager.Configure(o => o.Length = 20);

            var password = _provider.GetRequiredService<IGenerator>().Generate();
            Assert.Equal(20, password.Length);
        }

        [Fact]
        public void OptionsManager_IsSingleton_SameInstanceResolvedTwice()
        {
            var manager1 = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            var manager2 = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.Same(manager1, manager2);
        }

        [Fact]
        public void AsciiOnly_DefaultIsFalse()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.False(manager.Current.AsciiOnly);
        }

        [Fact]
        public void AsciiOnly_WhenEnabled_GeneratedPasswordContainsOnlyAsciiCharacters()
        {
            using var provider = new ServiceCollection()
                .AddPasswordGenerator(o =>
                {
                    o.AsciiOnly = true;
                    o.CharacterSets[3].Characters = "!£$€¥©®°±×÷";
                })
                .BuildServiceProvider();

            var generator = provider.GetRequiredService<IGenerator>();

            for (var i = 0; i < 20; i++)
            {
                var password = generator.Generate();
                Assert.All(password, c => Assert.True(c <= 127, $"Non-ASCII character '{c}' (U+{(int)c:X4}) found in password"));
            }
        }

        [Fact]
        public void AsciiOnly_WhenDisabled_NonAsciiCharactersCanAppear()
        {
            using var provider = new ServiceCollection()
                .AddPasswordGenerator(o =>
                {
                    o.AsciiOnly = false;
                    o.MaxRepetition = -1;
                    o.CharacterSets[3] = new PasswordGenerator.Sets.CharacterSet { Characters = "£", Min = 1 };
                })
                .BuildServiceProvider();

            var generator = provider.GetRequiredService<IGenerator>();
            var password = generator.Generate();

            Assert.Contains(password, c => c > 127);
        }

        [Fact]
        public void AsciiOnly_AsciiOnlySpecialCharacters_ContainsOnlyAsciiChars()
        {
            Assert.All(PasswordGenerator.PasswordGeneratorOptions.AsciiOnlySpecialCharacters,
                c => Assert.True(c <= 127, $"Non-ASCII character '{c}' found in AsciiOnlySpecialCharacters"));
        }

        [Fact]
        public void AsciiOnly_AsciiOnlySpecialCharacters_ContainsNoPrintableAlphanumerics()
        {
            Assert.All(PasswordGenerator.PasswordGeneratorOptions.AsciiOnlySpecialCharacters,
                c => Assert.False(char.IsLetterOrDigit(c), $"Alphanumeric character '{c}' found in AsciiOnlySpecialCharacters"));
        }

        [Fact]
        public void ExcludedCharacters_DefaultIsEmpty()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.Equal(string.Empty, manager.Current.ExcludedCharacters);
        }

        [Fact]
        public void ExcludeAmbiguous_DefaultIsFalse()
        {
            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.False(manager.Current.ExcludeAmbiguous);
        }

        [Fact]
        public void ExcludedCharacters_GeneratedPasswordDoesNotContainExcludedChars()
        {
            const string excluded = "aeiou";

            using var provider = new ServiceCollection()
                .AddPasswordGenerator(o =>
                {
                    o.ExcludedCharacters = excluded;
                    o.MaxRepetition = -1;
                })
                .BuildServiceProvider();

            var generator = provider.GetRequiredService<IGenerator>();

            for (var i = 0; i < 20; i++)
            {
                var password = generator.Generate();
                Assert.All(password, c =>
                    Assert.DoesNotContain(c, excluded));
            }
        }

        [Fact]
        public void ExcludeAmbiguous_GeneratedPasswordDoesNotContainAmbiguousChars()
        {
            using var provider = new ServiceCollection()
                .AddPasswordGenerator(o =>
                {
                    o.ExcludeAmbiguous = true;
                    o.MaxRepetition = -1;
                })
                .BuildServiceProvider();

            var generator = provider.GetRequiredService<IGenerator>();
            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters.ToHashSet();

            for (var i = 0; i < 20; i++)
            {
                var password = generator.Generate();
                Assert.All(password, c =>
                    Assert.DoesNotContain(c, ambiguous));
            }
        }

        [Fact]
        public void ExcludeAmbiguous_And_ExcludedCharacters_CombinedExclusion()
        {
            const string customExcluded = "xyz";

            using var provider = new ServiceCollection()
                .AddPasswordGenerator(o =>
                {
                    o.ExcludeAmbiguous = true;
                    o.ExcludedCharacters = customExcluded;
                    o.MaxRepetition = -1;
                })
                .BuildServiceProvider();

            var generator = provider.GetRequiredService<IGenerator>();
            var allExcluded = (customExcluded + PasswordGeneratorOptions.AmbiguousCharacters).ToHashSet();

            for (var i = 0; i < 20; i++)
            {
                var password = generator.Generate();
                Assert.All(password, c =>
                    Assert.DoesNotContain(c, allExcluded));
            }
        }

        [Fact]
        public void AddPasswordGenerator_WithOptionsInstance_AppliesAllProperties()
        {
            var preset = PasswordPolicy.Owasp;

            using var provider = new ServiceCollection()
                .AddPasswordGenerator(preset)
                .BuildServiceProvider();

            var manager = provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.Equal(preset.Length, manager.Current.Length);
            Assert.Equal(preset.MaxRepetition, manager.Current.MaxRepetition);
            Assert.Equal(preset.AllowSequences, manager.Current.AllowSequences);
            Assert.Equal(preset.ExcludeAmbiguous, manager.Current.ExcludeAmbiguous);
            Assert.Equal(preset.CharacterSets.Count, manager.Current.CharacterSets.Count);
        }

        [Fact]
        public void AddPasswordGenerator_WithOptionsInstance_GeneratesCorrectLengthPassword()
        {
            using var provider = new ServiceCollection()
                .AddPasswordGenerator(PasswordPolicy.Owasp)
                .BuildServiceProvider();

            var password = provider.GetRequiredService<IGenerator>().Generate();
            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void AddPasswordGenerator_WithOptionsInstance_DefensivelyCopies()
        {
            var options = new PasswordGeneratorOptions { Length = 22 };

            using var provider = new ServiceCollection()
                .AddPasswordGenerator(options)
                .BuildServiceProvider();

            options.Length = 99;

            var manager = provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            Assert.Equal(22, manager.Current.Length);
        }

        [Fact]
        public void AddPasswordGenerator_WithNullOptionsInstance_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ServiceCollection().AddPasswordGenerator((PasswordGeneratorOptions)null!));
        }

        [Fact]
        public void OptionsManager_Configure_ExcludedCharacters_ReflectedInNextGenerate()
        {
            const string excluded = "!$%^&*";

            var manager = _provider.GetRequiredService<IPasswordGeneratorOptionsManager>();
            manager.Configure(o =>
            {
                o.ExcludedCharacters = excluded;
                o.MaxRepetition = -1;
            });

            var generator = _provider.GetRequiredService<IGenerator>();
            var password = generator.Generate();

            Assert.All(password, c =>
                Assert.DoesNotContain(c, excluded));
        }
    }
}
