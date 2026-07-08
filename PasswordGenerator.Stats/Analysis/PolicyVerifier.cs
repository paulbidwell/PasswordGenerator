using PasswordGenerator.Stats.Models;

namespace PasswordGenerator.Stats.Analysis;

public static class PolicyVerifier
{
    public static PolicyVerificationStats Verify(
        IReadOnlyList<string> passwords,
        string mode,
        PasswordGeneratorOptions? passwordOptions,
        PinGeneratorOptions? pinOptions)
    {
        var violations = new List<PolicyViolation>();
        var ruleCounts = new Dictionary<string, int>();

        for (var i = 0; i < passwords.Count; i++)
        {
            var rules = mode == "pin"
                ? VerifyPin(passwords[i], pinOptions ?? new PinGeneratorOptions())
                : VerifyPassword(passwords[i], passwordOptions ?? new PasswordGeneratorOptions());

            if (rules.Count > 0)
            {
                violations.Add(new PolicyViolation(i, passwords[i], rules));
                foreach (var rule in rules)
                    ruleCounts[rule] = ruleCounts.GetValueOrDefault(rule) + 1;
            }
        }

        return new PolicyVerificationStats(
            passwords.Count,
            passwords.Count - violations.Count,
            violations.Count,
            ruleCounts,
            violations);
    }

    private static List<string> VerifyPassword(string password, PasswordGeneratorOptions options)
    {
        var failures = new List<string>();

        if (password.Length != options.Length)
            failures.Add($"Length: expected {options.Length}, got {password.Length}");

        var effectiveSets = GetEffectiveCharacterSets(options);
        for (var i = 0; i < effectiveSets.Count; i++)
        {
            var chars = effectiveSets[i].Characters;
            var min = effectiveSets[i].Min;
            var charSet = chars.ToHashSet();
            var count = password.Count(c => charSet.Contains(c));
            if (count < min)
                failures.Add($"CharSet[{i + 1}] ({InferLabel(chars)}): min {min}, got {count}");
        }

        if (options.ExcludeAmbiguous)
        {
            var ambiguous = PasswordGeneratorOptions.AmbiguousCharacters.ToHashSet();
            var found = password.Where(c => ambiguous.Contains(c)).Distinct().ToList();
            if (found.Count > 0)
                failures.Add($"AmbiguousChars: found '{string.Join("", found)}'");
        }

        if (options.ExcludedCharacters.Length > 0)
        {
            var excluded = options.ExcludedCharacters.ToHashSet();
            var found = password.Where(c => excluded.Contains(c)).Distinct().ToList();
            if (found.Count > 0)
                failures.Add($"ExcludedChars: found '{string.Join("", found)}'");
        }

        if (options.AsciiOnly && password.Any(c => c > 127))
            failures.Add("AsciiOnly: non-ASCII character found");

        if (options.MaxRepetition >= 0)
        {
            var maxRun = GetMaxConsecutiveRun(password);
            if (maxRun > options.MaxRepetition + 1)
                failures.Add($"MaxRepetition: max allowed {options.MaxRepetition}, found run of {maxRun}");
        }

        if (options.MustStartWithLetter && password.Length > 0 && !char.IsLetter(password[0]))
            failures.Add($"MustStartWithLetter: starts with '{password[0]}'");

        if (options.ExcludeLeadingTrailingSymbols && password.Length > 0)
        {
            if (!char.IsLetterOrDigit(password[0]))
                failures.Add($"LeadingSymbol: starts with '{password[0]}'");
            if (!char.IsLetterOrDigit(password[^1]))
                failures.Add($"TrailingSymbol: ends with '{password[^1]}'");
        }

        return failures;
    }

    private static List<string> VerifyPin(string pin, PinGeneratorOptions options)
    {
        var failures = new List<string>();

        if (pin.Length != options.Length)
            failures.Add($"Length: expected {options.Length}, got {pin.Length}");

        if (!pin.All(char.IsAsciiDigit))
            failures.Add("DigitsOnly: non-digit character found");

        return failures;
    }

    private static int GetMaxConsecutiveRun(string text)
    {
        if (text.Length == 0) return 0;
        var max = 1;
        var run = 1;
        for (var i = 1; i < text.Length; i++)
        {
            if (text[i] == text[i - 1])
                run++;
            else
                run = 1;
            if (run > max) max = run;
        }
        return max;
    }

    private static string InferLabel(string characters)
    {
        if (characters.Length == 0) return "Empty";
        if (characters.All(char.IsAsciiLetterUpper)) return "Uppercase";
        if (characters.All(char.IsAsciiLetterLower)) return "Lowercase";
        if (characters.All(char.IsAsciiDigit)) return "Digits";
        return "Symbols";
    }

    private static List<(string Characters, int Min)> GetEffectiveCharacterSets(PasswordGeneratorOptions options)
    {
        var sets = options.CharacterSets
            .Select(cs => (Characters: cs.Characters, cs.Min))
            .ToList();

        if (options.AsciiOnly)
        {
            sets = sets.Select(s => s.Characters.Any(c => c > 127)
                ? (PasswordGeneratorOptions.AsciiOnlySpecialCharacters, s.Min)
                : s).ToList();
        }

        var allExcluded = options.ExcludeAmbiguous
            ? options.ExcludedCharacters + PasswordGeneratorOptions.AmbiguousCharacters
            : options.ExcludedCharacters;

        if (allExcluded.Length > 0)
        {
            var excludedSet = new HashSet<char>(allExcluded);
            sets = sets.Select(s =>
                (new string(s.Characters.Where(c => !excludedSet.Contains(c)).ToArray()), s.Min)).ToList();
        }

        return sets;
    }
}
