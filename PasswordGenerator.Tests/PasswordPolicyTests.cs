using PasswordGenerator.Sets;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class PasswordPolicyTests
    {
        [Fact]
        public void Owasp_Length_Is16()
        {
            var options = PasswordPolicy.Owasp;
            Assert.Equal(16, options.Length);
        }

        [Fact]
        public void Owasp_HasFourCharacterSets_WithMinTwo()
        {
            var options = PasswordPolicy.Owasp;
            Assert.Equal(4, options.CharacterSets.Count);
            Assert.All(options.CharacterSets, cs => Assert.Equal(2, cs.Min));
        }

        [Fact]
        public void Owasp_CharacterSets_ContainExpectedCategories()
        {
            var options = PasswordPolicy.Owasp;
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "abcdefghijklmnopqrstuvwxyz");
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "0123456789");
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "!$%^&*()-_=+[]{}@#~;:,.?/");
        }

        [Fact]
        public void Owasp_ExcludeAmbiguous_IsTrue()
        {
            Assert.True(PasswordPolicy.Owasp.ExcludeAmbiguous);
        }

        [Fact]
        public void Owasp_MaxRepetition_IsOne()
        {
            Assert.Equal(1, PasswordPolicy.Owasp.MaxRepetition);
        }

        [Fact]
        public void Owasp_AllowSequences_IsFalse()
        {
            Assert.False(PasswordPolicy.Owasp.AllowSequences);
        }

        [Fact]
        public void Owasp_AllowUpperLowerSequences_IsFalse()
        {
            Assert.False(PasswordPolicy.Owasp.AllowUpperLowerSequences);
        }

        [Fact]
        public void Owasp_ReturnsDefensiveCopy()
        {
            var a = PasswordPolicy.Owasp;
            var b = PasswordPolicy.Owasp;
            Assert.NotSame(a, b);
            Assert.NotSame(a.CharacterSets, b.CharacterSets);
        }

        [Fact]
        public void Nist80063B_Length_Is15()
        {
            Assert.Equal(15, PasswordPolicy.Nist80063B.Length);
        }

        [Fact]
        public void Nist80063B_HasFourCharacterSets_WithMinOne()
        {
            var options = PasswordPolicy.Nist80063B;
            Assert.Equal(4, options.CharacterSets.Count);
            Assert.All(options.CharacterSets, cs => Assert.Equal(1, cs.Min));
        }

        [Fact]
        public void Nist80063B_SpecialCharacters_AreFullAsciiSet()
        {
            var options = PasswordPolicy.Nist80063B;
            var specialSet = Assert.Single(options.CharacterSets,
                cs => !cs.Characters.Any(char.IsLetterOrDigit));
            Assert.Equal(PasswordGeneratorOptions.AsciiOnlySpecialCharacters, specialSet.Characters);
        }

        [Fact]
        public void Nist80063B_AsciiOnly_IsTrue()
        {
            Assert.True(PasswordPolicy.Nist80063B.AsciiOnly);
        }

        [Fact]
        public void Nist80063B_MaxRepetition_IsUnrestricted()
        {
            Assert.Equal(-1, PasswordPolicy.Nist80063B.MaxRepetition);
        }

        [Fact]
        public void Nist80063B_AllowSequences_IsTrue()
        {
            Assert.True(PasswordPolicy.Nist80063B.AllowSequences);
        }

        [Fact]
        public void Nist80063B_AllowUpperLowerSequences_IsTrue()
        {
            Assert.True(PasswordPolicy.Nist80063B.AllowUpperLowerSequences);
        }

        [Fact]
        public void Nist80063B_ReturnsDefensiveCopy()
        {
            var a = PasswordPolicy.Nist80063B;
            var b = PasswordPolicy.Nist80063B;
            Assert.NotSame(a, b);
            Assert.NotSame(a.CharacterSets, b.CharacterSets);
        }

        [Fact]
        public void Pci_Length_Is14()
        {
            Assert.Equal(14, PasswordPolicy.Pci.Length);
        }

        [Fact]
        public void Pci_HasFourCharacterSets_WithMinTwo()
        {
            var options = PasswordPolicy.Pci;
            Assert.Equal(4, options.CharacterSets.Count);
            Assert.All(options.CharacterSets, cs => Assert.Equal(2, cs.Min));
        }

        [Fact]
        public void Pci_CharacterSets_ContainExpectedCategories()
        {
            var options = PasswordPolicy.Pci;
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "abcdefghijklmnopqrstuvwxyz");
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "0123456789");
            Assert.Contains(options.CharacterSets, cs => cs.Characters == "!$%^&*()-_=+[]{}@#~;:,.?/");
        }

        [Fact]
        public void Pci_ExcludeAmbiguous_IsTrue()
        {
            Assert.True(PasswordPolicy.Pci.ExcludeAmbiguous);
        }

        [Fact]
        public void Pci_MaxRepetition_IsOne()
        {
            Assert.Equal(1, PasswordPolicy.Pci.MaxRepetition);
        }

        [Fact]
        public void Pci_AllowSequences_IsFalse()
        {
            Assert.False(PasswordPolicy.Pci.AllowSequences);
        }

        [Fact]
        public void Pci_AllowUpperLowerSequences_IsFalse()
        {
            Assert.False(PasswordPolicy.Pci.AllowUpperLowerSequences);
        }

        [Fact]
        public void Pci_ReturnsDefensiveCopy()
        {
            var a = PasswordPolicy.Pci;
            var b = PasswordPolicy.Pci;
            Assert.NotSame(a, b);
            Assert.NotSame(a.CharacterSets, b.CharacterSets);
        }
    }
}
