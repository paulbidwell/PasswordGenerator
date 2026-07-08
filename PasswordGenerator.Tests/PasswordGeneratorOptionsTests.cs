using PasswordGenerator.Sets;
using Xunit;

namespace PasswordGenerator.Tests
{
    public class PasswordGeneratorOptionsTests
    {
        private static PasswordGeneratorOptions CreateOptions(
            Action<PasswordGeneratorOptions>? configure = null)
        {
            var options = new PasswordGeneratorOptions();
            configure?.Invoke(options);
            return options;
        }

        [Fact]
        public void Defaults_ExcludedCharactersIsEmpty()
        {
            var options = CreateOptions();
            Assert.Equal(string.Empty, options.ExcludedCharacters);
        }

        [Fact]
        public void Defaults_ExcludeAmbiguousIsFalse()
        {
            var options = CreateOptions();
            Assert.False(options.ExcludeAmbiguous);
        }

        [Fact]
        public void Defaults_GetEffectiveCharacterSets_ReturnsOriginalSets()
        {
            var options = CreateOptions();
            var effective = options.GetEffectiveCharacterSets().ToList();

            Assert.Equal(options.CharacterSets.Count, effective.Count);
            for (var i = 0; i < effective.Count; i++)
            {
                Assert.Equal(options.CharacterSets[i].Characters, effective[i].Characters);
            }
        }

        [Fact]
        public void ExcludedCharacters_RemovesSpecifiedCharsFromAllSets()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEF", Min = 1 },
                    new CharacterSet { Characters = "123456", Min = 1 }
                ];
                o.ExcludedCharacters = "A1";
            });

            var effective = options.GetEffectiveCharacterSets().ToList();

            Assert.Equal("BCDEF", effective[0].Characters);
            Assert.Equal("23456", effective[1].Characters);
        }

        [Fact]
        public void ExcludedCharacters_PreservesMinValues()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEF", Min = 3 }
                ];
                o.ExcludedCharacters = "A";
            });

            var effective = options.GetEffectiveCharacterSets().ToList();
            Assert.Equal(3, effective[0].Min);
        }

        [Fact]
        public void ExcludedCharacters_CanExcludeAllCharsFromSet()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "AB", Min = 1 }
                ];
                o.ExcludedCharacters = "AB";
            });

            var effective = options.GetEffectiveCharacterSets().ToList();
            Assert.Equal("", effective[0].Characters);
        }

        [Fact]
        public void ExcludedCharacters_Empty_LeavesSetUnchanged()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEF", Min = 1 }
                ];
                o.ExcludedCharacters = "";
            });

            var effective = options.GetEffectiveCharacterSets().ToList();
            Assert.Equal("ABCDEF", effective[0].Characters);
        }

        [Fact]
        public void ExcludeAmbiguous_RemovesAmbiguousCharsFromSets()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 1 },
                    new CharacterSet { Characters = "abcdefghijklmnopqrstuvwxyz", Min = 1 },
                    new CharacterSet { Characters = "0123456789", Min = 1 }
                ];
                o.ExcludeAmbiguous = true;
            });

            var effective = options.GetEffectiveCharacterSets().ToList();
            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters.ToHashSet();

            foreach (var set in effective)
            {
                Assert.All(set.Characters, c =>
                    Assert.DoesNotContain(c, ambiguous));
            }
        }

        [Fact]
        public void ExcludeAmbiguous_False_RetainsAmbiguousChars()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 1 },
                    new CharacterSet { Characters = "0123456789", Min = 1 }
                ];
                o.ExcludeAmbiguous = false;
            });

            var effective = options.GetEffectiveCharacterSets().ToList();

            Assert.Contains('O', effective[0].Characters);
            Assert.Contains('0', effective[1].Characters);
        }

        [Fact]
        public void ExcludeAmbiguous_And_ExcludedCharacters_ComposeExclusions()
        {
            var options = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Min = 1 }
                ];
                o.ExcludeAmbiguous = true;
                o.ExcludedCharacters = "X";
            });

            var effective = options.GetEffectiveCharacterSets().ToList();
            var chars = effective[0].Characters;

            Assert.DoesNotContain('O', chars);
            Assert.DoesNotContain('B', chars);
            Assert.DoesNotContain('S', chars);
            Assert.DoesNotContain('Z', chars);
            Assert.DoesNotContain('I', chars);
            Assert.DoesNotContain('X', chars);
            Assert.Contains('A', chars);
        }

        [Fact]
        public void AsciiOnly_And_ExcludedCharacters_ApplyInOrder()
        {
            var options = CreateOptions(o =>
            {
                o.AsciiOnly = true;
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEF", Min = 1 },
                    new CharacterSet { Characters = "£€¥©", Min = 1 }
                ];
                o.ExcludedCharacters = "!A";
            });

            var effective = options.GetEffectiveCharacterSets().ToList();

            Assert.DoesNotContain('A', effective[0].Characters);
            Assert.DoesNotContain('!', effective[1].Characters);
            Assert.All(effective[1].Characters, c => Assert.True(c <= 127));
        }

        [Fact]
        public void AmbiguousCharacters_Constant_ContainsExpectedChars()
        {
            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters;

            Assert.Contains('0', ambiguous);
            Assert.Contains('O', ambiguous);
            Assert.Contains('1', ambiguous);
            Assert.Contains('l', ambiguous);
            Assert.Contains('I', ambiguous);
            Assert.Contains('|', ambiguous);
            Assert.Contains('S', ambiguous);
            Assert.Contains('5', ambiguous);
            Assert.Contains('B', ambiguous);
            Assert.Contains('8', ambiguous);
            Assert.Contains('Z', ambiguous);
            Assert.Contains('2', ambiguous);
        }

        [Fact]
        public void AsciiOnly_NonAsciiSet_ReplacedWithAsciiSpecialCharacters()
        {
            var options = CreateOptions(o =>
            {
                o.AsciiOnly = true;
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABC", Min = 1 },
                    new CharacterSet { Characters = "£€¥©®", Min = 2 }
                ];
            });

            var effective = options.GetEffectiveCharacterSets().ToList();

            Assert.Equal("ABC", effective[0].Characters);
            Assert.Equal(1, effective[0].Min);

            Assert.Equal(PasswordGeneratorOptions.AsciiOnlySpecialCharacters, effective[1].Characters);
            Assert.Equal(2, effective[1].Min);
        }

        [Fact]
        public void AsciiOnly_AllAsciiSets_ReturnedUnchanged()
        {
            var options = CreateOptions(o =>
            {
                o.AsciiOnly = true;
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABCDEFGHIJ", Min = 3 },
                    new CharacterSet { Characters = "0123456789", Min = 2 }
                ];
            });

            var effective = options.GetEffectiveCharacterSets().ToList();

            Assert.Equal("ABCDEFGHIJ", effective[0].Characters);
            Assert.Equal("0123456789", effective[1].Characters);
        }

        [Fact]
        public void Snapshot_CopiesAllScalarProperties()
        {
            var source = CreateOptions(o =>
            {
                o.Length = 30;
                o.MaxRepetition = 3;
                o.AllowSequences = true;
                o.AllowUpperLowerSequences = true;
                o.AsciiOnly = true;
                o.ExcludeAmbiguous = true;
                o.ExcludedCharacters = "XYZ";
                o.ExcludeLeadingTrailingSymbols = true;
                o.MustStartWithLetter = true;
            });

            var snapshot = source.Snapshot();

            Assert.Equal(30, snapshot.Length);
            Assert.Equal(3, snapshot.MaxRepetition);
            Assert.True(snapshot.AllowSequences);
            Assert.True(snapshot.AllowUpperLowerSequences);
            Assert.True(snapshot.AsciiOnly);
            Assert.True(snapshot.ExcludeAmbiguous);
            Assert.Equal("XYZ", snapshot.ExcludedCharacters);
            Assert.True(snapshot.ExcludeLeadingTrailingSymbols);
            Assert.True(snapshot.MustStartWithLetter);
        }

        [Fact]
        public void Snapshot_DeepClonesCharacterSets()
        {
            var source = CreateOptions(o =>
            {
                o.CharacterSets =
                [
                    new CharacterSet { Characters = "ABC", Min = 2 },
                    new CharacterSet { Characters = "123", Min = 1 }
                ];
            });

            var snapshot = source.Snapshot();

            Assert.Equal(source.CharacterSets.Count, snapshot.CharacterSets.Count);
            Assert.Equal("ABC", snapshot.CharacterSets[0].Characters);
            Assert.Equal(2, snapshot.CharacterSets[0].Min);

            source.CharacterSets[0] = new CharacterSet { Characters = "MUTATED", Min = 99 };
            Assert.Equal("ABC", snapshot.CharacterSets[0].Characters);
            Assert.Equal(2, snapshot.CharacterSets[0].Min);
        }

        [Fact]
        public void CopyTo_OverwritesTargetProperties()
        {
            var source = CreateOptions(o =>
            {
                o.Length = 50;
                o.MaxRepetition = 5;
                o.MustStartWithLetter = true;
                o.CharacterSets = [new CharacterSet { Characters = "XY", Min = 1 }];
            });

            var target = new PasswordGeneratorOptions();

            source.CopyTo(target);

            Assert.Equal(50, target.Length);
            Assert.Equal(5, target.MaxRepetition);
            Assert.True(target.MustStartWithLetter);
            Assert.Single(target.CharacterSets);
            Assert.Equal("XY", target.CharacterSets[0].Characters);
        }
    }
}
