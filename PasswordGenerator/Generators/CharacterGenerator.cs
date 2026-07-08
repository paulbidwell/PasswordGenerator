using PasswordGenerator.Core;
using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Core.Interfaces.Sets;
using PasswordGenerator.Core.Interfaces.Shufflers;

namespace PasswordGenerator.Generators;

/// <summary>Assembles a password character buffer from configured character sets.</summary>
public class CharacterGenerator(
    IGeneratorConfig config,
    ICharacterSelector characterSelector,
    IRandomNumberGenerator randomNumberGenerator,
    IPasswordShuffler passwordShuffler,
    IPositionConstraintEnforcer positionConstraintEnforcer) : ICharacterGenerator
{
    private const int RetryCapMultiplier = 100;

    private readonly Lazy<(int[] Weights, int Total)> _fillWeights =
        new(() => BuildFillWeights(config.CharacterSets, config.Length));

    /// <inheritdoc />
    public char[] GeneratePassword()
    {
        var buffer = new char[config.Length];
        var index = 0;
        var characterCount = new Dictionary<char, int>();

        GenerateRequiredCharacters(buffer, ref index, characterCount);
        GenerateRandomCharacters(buffer, ref index, characterCount);

        passwordShuffler.Shuffle(buffer, config);
        positionConstraintEnforcer.Enforce(buffer, config);
        return buffer;
    }

    private void GenerateRequiredCharacters(char[] buffer, ref int index, IDictionary<char, int> characterCount)
    {
        var requiredSets = config.CharacterSets.Where(set => set.Min > 0).ToArray();
        foreach (var set in requiredSets)
        {
            AddRequiredCharactersFromSet(buffer, ref index, characterCount, set);
        }
    }

    private void AddRequiredCharactersFromSet(char[] buffer, ref int index, IDictionary<char, int> characterCount, ICharacterSet set)
    {
        var added = 0;

        while (added < set.Min)
        {
            char? next = characterSelector.GetNextCharacter(set.Set);

            if (config.MaxRepetition >= 0)
            {
                next = HandleRepetition(next.Value, characterCount);
            }

            if (next.HasValue)
            {
                buffer[index++] = next.Value;
                added++;
            }
        }
    }

    private void GenerateRandomCharacters(char[] buffer, ref int index, IDictionary<char, int> characterCount)
    {
        var globalRetries = 0;
        var maxGlobalRetries = config.Length * RetryCapMultiplier;

        while (index < config.Length)
        {
            if (++globalRetries > maxGlobalRetries)
            {
                throw new InvalidOperationException(
                    "Could not fill remaining password characters within retry limit. " +
                    "Consider increasing password length or expanding character sets.");
            }

            var characterSet = PickRandomCharacterSet();

            for (var retry = characterSet.Set.Length; retry > 0; retry--)
            {
                char? next = characterSelector.GetNextCharacter(characterSet.Set);

                if (config.MaxRepetition >= 0)
                {
                    next = HandleRepetition(next.Value, characterCount);
                }

                if (next.HasValue)
                {
                    buffer[index++] = next.Value;
                    break;
                }
            }
        }
    }

    private ICharacterSet PickRandomCharacterSet()
    {
        var sets = config.CharacterSets;
        var (weights, total) = _fillWeights.Value;

        var target = randomNumberGenerator.GetRandomIntInRange(0, total - 1);
        var cumulative = 0;

        for (var i = 0; i < sets.Count; i++)
        {
            cumulative += weights[i];

            if (target < cumulative)
                return sets[i];
        }

        return sets[^1];
    }

    private static (int[] Weights, int Total) BuildFillWeights(List<ICharacterSet> sets, int length)
    {
        var weights = new int[sets.Count];

        if (sets.Any(s => s.Weight > 0))
        {
            for (var i = 0; i < sets.Count; i++)
                weights[i] = sets[i].Weight > 0 ? sets[i].Weight : sets[i].Set.Length;
        }
        else
        {
            var totalPoolSize = sets.Sum(s => s.Set.Length);

            for (var i = 0; i < sets.Count; i++)
            {
                var targetCount = (double)sets[i].Set.Length / totalPoolSize * length;
                var compensated = targetCount - sets[i].Min;
                weights[i] = Math.Max(1, (int)Math.Round(compensated * 10000));
            }
        }

        return (weights, weights.Sum());
    }

    private char? HandleRepetition(char character, IDictionary<char, int> characterCount)
    {
        if (!characterCount.TryAddCount(character, config.MaxRepetition, out _))
        {
            return null;
        }
        return character;
    }
}