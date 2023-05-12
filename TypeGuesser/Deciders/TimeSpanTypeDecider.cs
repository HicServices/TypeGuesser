using System;
using System.Globalization;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="TimeSpan"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
public class TimeSpanTypeDecider : DecideTypesForStrings<TimeSpan>
{
    /// <summary>
    /// Creates a new instance for recognizing time values (without date) in strings
    /// </summary>
    /// <param name="culture"></param>
    public TimeSpanTypeDecider(CultureInfo culture): base(culture,TypeCompatibilityGroup.Exclusive, typeof(TimeSpan))
    {
    }

    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
    {
        return new TimeSpanTypeDecider(culture);
    }

    /// <inheritdoc/>
    protected override object ParseImpl(string value)
    {
        var dt = DateTime.Parse(value);

        return dt.TimeOfDay;
    }

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(string candidateString,IDataTypeSize sizeRecord)
    {
        try
        {
            //if it parses as a date 
            if (DateTime.TryParse(candidateString, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out var t))
            {
                return t is { Year: 1, Month: 1, Day: 1 };//without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too 
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}