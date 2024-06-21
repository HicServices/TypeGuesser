using System;
using System.Globalization;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="TimeSpan"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
/// <remarks>
/// Creates a new instance for recognizing time values (without date) in strings
/// </remarks>
/// <param name="culture"></param>
public sealed class TimeSpanTypeDecider(CultureInfo culture) : DecideTypesForStrings<TimeSpan>(culture, TypeCompatibilityGroup.Exclusive, typeof(TimeSpan))
{
    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo newCulture) => new TimeSpanTypeDecider(newCulture);

    /// <inheritdoc/>
    protected override object ParseImpl(string value) => DateTime.Parse(value).TimeOfDay;

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(ReadOnlySpan<char> candidateString, IDataTypeSize? sizeRecord)
    {
        try
        {
            //if it parses as a date and has the default date portion of 1/1/1
            return DateTime.TryParse(candidateString, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault,
                       out var t) &&
                   t is (1, 1, 1); //without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }
}