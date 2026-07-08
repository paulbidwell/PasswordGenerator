using PasswordGenerator.Core.Interfaces.Generators;
using PasswordGenerator.Generators;
using PasswordGenerator.Sets;
using PasswordGenerator.Shufflers;

namespace PasswordGenerator;

/// <summary>
/// Fluent builder that configures and creates an <see cref="IGenerator"/>
/// without requiring a dependency-injection container.
/// </summary>
public sealed class PasswordGeneratorBuilder
{
    private readonly PasswordGeneratorOptions _options = new();

    /// <summary>
    /// Creates a new builder initialised with the default
    /// <see cref="PasswordGeneratorOptions"/> values.
    /// </summary>
    public static PasswordGeneratorBuilder Create() => new();

    /// <summary>
    /// Creates a new builder initialised from a
    /// <see cref="PasswordGeneratorOptions"/> snapshot (e.g. a
    /// <see cref="PasswordPolicy"/> preset). The supplied instance is
    /// copied so later mutations on the builder do not affect the original.
    /// </summary>
    public static PasswordGeneratorBuilder From(PasswordGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var builder = new PasswordGeneratorBuilder();
        options.CopyTo(builder._options);
        return builder;
    }

    public PasswordGeneratorBuilder WithLength(int length)
    {
        _options.Length = length;
        return this;
    }

    public PasswordGeneratorBuilder WithMaxRepetition(int maxRepetition)
    {
        _options.MaxRepetition = maxRepetition;
        return this;
    }

    public PasswordGeneratorBuilder WithUnlimitedRepetition()
    {
        _options.MaxRepetition = -1;
        return this;
    }

    public PasswordGeneratorBuilder AllowSequences(bool allow = true)
    {
        _options.AllowSequences = allow;
        return this;
    }

    public PasswordGeneratorBuilder AllowUpperLowerSequences(bool allow = true)
    {
        _options.AllowUpperLowerSequences = allow;
        return this;
    }

    public PasswordGeneratorBuilder AsciiOnly(bool asciiOnly = true)
    {
        _options.AsciiOnly = asciiOnly;
        return this;
    }

    public PasswordGeneratorBuilder ExcludeAmbiguousCharacters(bool exclude = true)
    {
        _options.ExcludeAmbiguous = exclude;
        return this;
    }

    public PasswordGeneratorBuilder ExcludeCharacters(string characters)
    {
        ArgumentNullException.ThrowIfNull(characters);
        _options.ExcludedCharacters = characters;
        return this;
    }

    public PasswordGeneratorBuilder MustStartWithLetter(bool must = true)
    {
        _options.MustStartWithLetter = must;
        return this;
    }

    public PasswordGeneratorBuilder ExcludeLeadingTrailingSymbols(bool exclude = true)
    {
        _options.ExcludeLeadingTrailingSymbols = exclude;
        return this;
    }

    public PasswordGeneratorBuilder ClearCharacterSets()
    {
        _options.CharacterSets.Clear();
        return this;
    }

    public PasswordGeneratorBuilder AddCharacterSet(string characters, int min = 1)
    {
        ArgumentNullException.ThrowIfNull(characters);
        _options.CharacterSets.Add(new CharacterSet { Characters = characters, Min = min });
        return this;
    }

    /// <summary>
    /// Creates a fully wired <see cref="IGenerator"/> using the current
    /// builder state. The returned generator is independent — further
    /// mutations on this builder do not affect it.
    /// </summary>
    public IGenerator Build()
    {
        var snapshot = SnapshotOptions();

        var optionsManager = new DirectPasswordOptionsManager(snapshot);
        var rng = new SecureRng();
        var collectionShuffler = new CollectionShuffler(rng);
        var characterSetShuffler = new CharacterSetShuffler(collectionShuffler);
        var characterSetManager = new CharacterSetManager(characterSetShuffler, collectionShuffler);
        var config = new GeneratorConfig(optionsManager, characterSetManager);
        var validator = new ConfigurationValidator();
        var characterSelector = new CharacterSelector(rng);
        var passwordShuffler = new PasswordShuffler(collectionShuffler);
        var positionConstraintEnforcer = new PositionConstraintEnforcer();
        var characterGenerator = new CharacterGenerator(config, characterSelector, rng, passwordShuffler, positionConstraintEnforcer);

        return new Generator(config, validator, characterGenerator);
    }

    private PasswordGeneratorOptions SnapshotOptions() => _options.Snapshot();
}
