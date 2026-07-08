using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Strength;
using PasswordGenerator.Stats.Models;
using System.Diagnostics;

namespace PasswordGenerator.Stats.Analysis;

public sealed class StatisticsRunner(
    IGenerator generator,
    IPinGenerator pinGenerator,
    IPasswordStrengthAnalyzer strengthAnalyzer,
    IGeneratorConfig generatorConfig)
{
    public GenerationReport Run(
        int count,
        string mode,
        PasswordGeneratorOptions? passwordOptions,
        PinGeneratorOptions? pinOptions,
        IProgress<string>? progress = null)
    {
        progress?.Report("Generating passwords…");
        var sw = Stopwatch.StartNew();
        var batch = mode == "pin"
            ? pinGenerator.GenerateBatch(count)
            : generator.GenerateBatch(count);
        sw.Stop();

        var passwords = batch.Items;
        var charSets = BuildCharacterSetInfo(mode, passwordOptions, pinOptions);
        var maxRepetition = mode == "pin" ? -1 : (passwordOptions?.MaxRepetition ?? 1);

        progress?.Report("Analyzing character frequency…");
        var frequencyStats = CharacterFrequencyAnalyzer.Analyze(passwords, charSets);

        progress?.Report("Analyzing repetition patterns…");
        var repetitionStats = RepetitionAnalyzer.Analyze(passwords, maxRepetition);

        progress?.Report("Computing entropy & strength…");
        var entropyStats = EntropyAnalyzer.Analyze(passwords, strengthAnalyzer, mode == "pin" ? null : generatorConfig);

        progress?.Report("Detecting duplicates…");
        var duplicateStats = DuplicateAnalyzer.Analyze(passwords);

        progress?.Report("Verifying policy compliance…");
        var policyVerification = PolicyVerifier.Verify(passwords, mode, passwordOptions, pinOptions);

        progress?.Report("Building report…");
        var configSummary = mode == "pin"
            ? BuildPinConfigSummary(pinOptions)
            : BuildPasswordConfigSummary(passwordOptions);

        return new GenerationReport(
            mode == "pin" ? "PIN" : "Password",
            count,
            sw.ElapsedMilliseconds,
            configSummary,
            frequencyStats,
            repetitionStats,
            entropyStats,
            duplicateStats,
            policyVerification);
    }

    private static List<(int Index, string Label, string Characters)> BuildCharacterSetInfo(
        string mode,
        PasswordGeneratorOptions? passwordOptions,
        PinGeneratorOptions? pinOptions)
    {
        if (mode == "pin")
        {
            return [(0, "Digits", "0123456789")];
        }

        var options = passwordOptions ?? new PasswordGeneratorOptions();
        var allExcluded = options.ExcludeAmbiguous
            ? options.ExcludedCharacters + PasswordGeneratorOptions.AmbiguousCharacters
            : options.ExcludedCharacters;
        var excludedSet = allExcluded.Length > 0 ? new HashSet<char>(allExcluded) : null;

        var sets = new List<(int, string, string)>();
        for (var i = 0; i < options.CharacterSets.Count; i++)
        {
            var cs = options.CharacterSets[i];
            var chars = cs.Characters;

            if (options.AsciiOnly && chars.Any(c => c > 127))
                chars = PasswordGeneratorOptions.AsciiOnlySpecialCharacters;

            if (excludedSet is not null)
                chars = new string(chars.Where(c => !excludedSet.Contains(c)).ToArray());

            var label = InferLabel(chars);
            sets.Add((i, label, chars));
        }

        return sets;
    }

    private static string InferLabel(string characters)
    {
        if (characters.Length == 0)
            return "Empty";
        if (characters.All(char.IsAsciiLetterUpper))
            return "Uppercase";
        if (characters.All(char.IsAsciiLetterLower))
            return "Lowercase";
        if (characters.All(char.IsAsciiDigit))
            return "Digits";

        return "Symbols";
    }

    private static string BuildPasswordConfigSummary(PasswordGeneratorOptions? options)
    {
        if (options is null)
            return "Default configuration";

        var parts = new List<string>
        {
            $"Length={options.Length}",
            $"MaxRepetition={options.MaxRepetition}",
            $"AllowSequences={options.AllowSequences}",
            $"AllowUpperLowerSequences={options.AllowUpperLowerSequences}"
        };

        for (var i = 0; i < options.CharacterSets.Count; i++)
        {
            var cs = options.CharacterSets[i];
            var label = InferLabel(cs.Characters);
            parts.Add($"Set{i + 1}={label}(min {cs.Min}, {cs.Characters.Length} chars)");
        }

        if (options.ExcludeAmbiguous) parts.Add("ExcludeAmbiguous");
        if (options.MustStartWithLetter) parts.Add("MustStartWithLetter");
        if (options.ExcludeLeadingTrailingSymbols) parts.Add("ExcludeLeadingTrailingSymbols");

        return string.Join("\n", parts);
    }

    private static string BuildPinConfigSummary(PinGeneratorOptions? options)
    {
        if (options is null)
            return "Default PIN configuration";

        return $"Length={options.Length}, RejectTrivialPatterns={options.RejectTrivialPatterns}, MaxRetries={options.MaxRetries}";
    }
}
