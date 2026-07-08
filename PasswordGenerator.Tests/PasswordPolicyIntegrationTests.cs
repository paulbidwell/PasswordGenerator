using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Core.Interfaces.Generators;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class PasswordPolicyIntegrationTests
    {
        private const int BatchSize = 50;

        private static IReadOnlyList<string> GenerateBatch(PasswordGeneratorOptions options)
        {
            using var provider = new ServiceCollection()
                .AddPasswordGenerator(options)
                .BuildServiceProvider();

            return provider.GetRequiredService<IGenerator>().GenerateBatch(BatchSize).Items;
        }

        [Fact]
        public void Owasp_AllPasswords_HaveCorrectLength()
        {
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p => Assert.Equal(16, p.Length));
        }

        [Fact]
        public void Owasp_AllPasswords_ContainUppercase()
        {
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p =>
                Assert.True(p.Count(char.IsAsciiLetterUpper) >= 2,
                    $"Password '{p}' has fewer than 2 uppercase characters"));
        }

        [Fact]
        public void Owasp_AllPasswords_ContainLowercase()
        {
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p =>
                Assert.True(p.Count(char.IsAsciiLetterLower) >= 2,
                    $"Password '{p}' has fewer than 2 lowercase characters"));
        }

        [Fact]
        public void Owasp_AllPasswords_ContainDigits()
        {
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p =>
                Assert.True(p.Count(char.IsAsciiDigit) >= 2,
                    $"Password '{p}' has fewer than 2 digit characters"));
        }

        [Fact]
        public void Owasp_AllPasswords_ContainSpecialCharacters()
        {
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p =>
                Assert.True(p.Count(c => !char.IsLetterOrDigit(c)) >= 2,
                    $"Password '{p}' has fewer than 2 special characters"));
        }

        [Fact]
        public void Owasp_AllPasswords_ExcludeAmbiguousCharacters()
        {
            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters.ToHashSet();
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p =>
                Assert.True(p.All(c => !ambiguous.Contains(c)),
                    $"Password '{p}' contains ambiguous characters"));
        }

        [Fact]
        public void Owasp_AllPasswords_HaveNoConsecutiveRepeats()
        {
            var passwords = GenerateBatch(PasswordPolicy.Owasp);
            Assert.All(passwords, p =>
            {
                for (var i = 1; i < p.Length; i++)
                    Assert.True(p[i] != p[i - 1],
                        $"Password '{p}' has consecutive repeat at position {i}");
            });
        }

        [Fact]
        public void Nist80063B_AllPasswords_HaveCorrectLength()
        {
            var passwords = GenerateBatch(PasswordPolicy.Nist80063B);
            Assert.All(passwords, p => Assert.Equal(15, p.Length));
        }

        [Fact]
        public void Nist80063B_AllPasswords_ContainUppercase()
        {
            var passwords = GenerateBatch(PasswordPolicy.Nist80063B);
            Assert.All(passwords, p =>
                Assert.True(p.Any(char.IsAsciiLetterUpper),
                    $"Password '{p}' has no uppercase characters"));
        }

        [Fact]
        public void Nist80063B_AllPasswords_ContainLowercase()
        {
            var passwords = GenerateBatch(PasswordPolicy.Nist80063B);
            Assert.All(passwords, p =>
                Assert.True(p.Any(char.IsAsciiLetterLower),
                    $"Password '{p}' has no lowercase characters"));
        }

        [Fact]
        public void Nist80063B_AllPasswords_ContainDigits()
        {
            var passwords = GenerateBatch(PasswordPolicy.Nist80063B);
            Assert.All(passwords, p =>
                Assert.True(p.Any(char.IsAsciiDigit),
                    $"Password '{p}' has no digit characters"));
        }

        [Fact]
        public void Nist80063B_AllPasswords_ContainSpecialCharacters()
        {
            var passwords = GenerateBatch(PasswordPolicy.Nist80063B);
            Assert.All(passwords, p =>
                Assert.True(p.Any(c => !char.IsLetterOrDigit(c)),
                    $"Password '{p}' has no special characters"));
        }

        [Fact]
        public void Nist80063B_AllPasswords_ContainOnlyAsciiCharacters()
        {
            var passwords = GenerateBatch(PasswordPolicy.Nist80063B);
            Assert.All(passwords, p =>
                Assert.True(p.All(c => c <= 127),
                    $"Password '{p}' contains non-ASCII characters"));
        }

        [Fact]
        public void Pci_AllPasswords_HaveCorrectLength()
        {
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p => Assert.Equal(14, p.Length));
        }

        [Fact]
        public void Pci_AllPasswords_ContainUppercase()
        {
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p =>
                Assert.True(p.Count(char.IsAsciiLetterUpper) >= 2,
                    $"Password '{p}' has fewer than 2 uppercase characters"));
        }

        [Fact]
        public void Pci_AllPasswords_ContainLowercase()
        {
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p =>
                Assert.True(p.Count(char.IsAsciiLetterLower) >= 2,
                    $"Password '{p}' has fewer than 2 lowercase characters"));
        }

        [Fact]
        public void Pci_AllPasswords_ContainDigits()
        {
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p =>
                Assert.True(p.Count(char.IsAsciiDigit) >= 2,
                    $"Password '{p}' has fewer than 2 digit characters"));
        }

        [Fact]
        public void Pci_AllPasswords_ContainSpecialCharacters()
        {
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p =>
                Assert.True(p.Count(c => !char.IsLetterOrDigit(c)) >= 2,
                    $"Password '{p}' has fewer than 2 special characters"));
        }

        [Fact]
        public void Pci_AllPasswords_ExcludeAmbiguousCharacters()
        {
            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters.ToHashSet();
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p =>
                Assert.True(p.All(c => !ambiguous.Contains(c)),
                    $"Password '{p}' contains ambiguous characters"));
        }

        [Fact]
        public void Pci_AllPasswords_HaveNoConsecutiveRepeats()
        {
            var passwords = GenerateBatch(PasswordPolicy.Pci);
            Assert.All(passwords, p =>
            {
                for (var i = 1; i < p.Length; i++)
                    Assert.True(p[i] != p[i - 1],
                        $"Password '{p}' has consecutive repeat at position {i}");
            });
        }
    }
}
