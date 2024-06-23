using System;
using System.Collections.Generic;
using System.Globalization;
using TB.ComponentModel;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="DateTime"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DecideTypesForStrings<T> :IDecideTypesForStrings
{
    private CultureInfo _culture;

    /// <inheritdoc/>
    public GuessSettings Settings { get; set; }

    /// <inheritdoc/>
    public virtual CultureInfo Culture
    {
        get => _culture;
        set => _culture = value;
    }

    /// <inheritdoc/>
    public TypeCompatibilityGroup CompatibilityGroup { get; }

    /// <inheritdoc/>
    public HashSet<Type> TypesSupported { get; }

    /// <summary>
    /// Determines behaviour of abstract base.  Call from derived classes to indicate what inter compatible Types you support parsing into / guessing
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="compatibilityGroup">How your Type interacts with other Guessers, e.g. can you fallback from one to another</param>
    /// <param name="typesSupported">All the Types your guesser supports e.g. multiple sizes of int (int32, int16 etc).  These should not overlap with other guessers in the app domain</param>
    protected DecideTypesForStrings(CultureInfo culture, TypeCompatibilityGroup compatibilityGroup,params Type[] typesSupported)
    {
        _culture = culture;

        Settings = GuessSettingsFactory.Create();

        CompatibilityGroup = compatibilityGroup;

        if(typesSupported.Length == 0)
            throw new ArgumentException(SR.DecideTypesForStrings_DecideTypesForStrings_DecideTypesForStrings_abstract_base_was_not_passed_any_typesSupported_by_implementing_derived_class);

        TypesSupported = [..typesSupported];
    }

    /// <inheritdoc/>
    public virtual bool IsAcceptableAsType(ReadOnlySpan<char> candidateString, IDataTypeSize? size)
    {
        //we must preserve leading zeroes if it's not actually 0 -- if they have 010101 then we have to use string but if they have just 0 we can use decimal
        return !IDecideTypesForStrings.ZeroPrefixedNumber.IsMatch(candidateString) && IsAcceptableAsTypeImpl(candidateString, size);
    }

    /// <summary>
    /// Returns true if <see cref="Settings"/> contains an <see cref="GuessSettings.ExplicitDateFormats"/> and one of them matches the <paramref name="candidateString"/>
    /// </summary>
    /// <param name="candidateString"></param>
    /// <returns></returns>
    protected bool IsExplicitDate(ReadOnlySpan<char> candidateString)
    {
        //if user has an explicit type format in mind and the candidate string is not null (which should hopefully be handled sensibly elsewhere)
        if(Settings.ExplicitDateFormats != null && !candidateString.IsEmpty && !candidateString.IsWhiteSpace())
            return DateTime.TryParseExact(candidateString,Settings.ExplicitDateFormats,Culture,DateTimeStyles.None,out _);

        return false;
    }

    /// <inheritdoc/>
    public object? Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return ParseImpl(value);
        }catch(Exception ex)
        {
            throw new FormatException(string.Format(SR.DecideTypesForStrings_Parse_Could_not_parse_string_value___0___with_Decider_Type__1_, value, GetType().Name),ex);
        }
    }

    /// <inheritdoc/>
    public IDecideTypesForStrings Clone()
    {
        var clone = CloneImpl(Culture);
        clone.Settings = Settings.Clone();
        return clone;
    }

    /// <summary>
    /// Create a new instance of Type {T}
    /// </summary>
    /// <param name="culture"></param>
    /// <returns></returns>
    protected abstract IDecideTypesForStrings CloneImpl(CultureInfo culture);

    /// <summary>
    /// Parses <paramref name="value"/> into Type T (of this decider).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract object? ParseImpl(ReadOnlySpan<char> value);

    /// <summary>
    /// Returns true if the given <paramref name="candidateString"/> is compatible with the T Type of this decider.  This is the preferred method of overriding IsAcceptable.
    /// </summary>
    /// <param name="candidateString"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    protected abstract bool IsAcceptableAsTypeImpl(ReadOnlySpan<char> candidateString, IDataTypeSize? size);
}