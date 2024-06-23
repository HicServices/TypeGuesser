using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="decimal"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
public sealed class DecimalTypeDecider : DecideTypesForStrings<decimal>
{
    /// <summary>
    /// The culture specific symbol for decimal point e.g. '.' (in uk english)
    /// </summary>
    private readonly char _decimalIndicator;

    /// <summary>
    /// Creates new instance that recognizes strings with a decimal point representation
    /// </summary>
    /// <param name="culture"></param>
    public DecimalTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical,typeof(decimal), typeof(float) , typeof(double),typeof(long))
    {
        _decimalIndicator = Culture.NumberFormat.NumberDecimalSeparator.Last();
    }

    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
    {
        return new DecimalTypeDecider(culture);
    }

    /// <inheritdoc />
    protected override object? ParseImpl(ReadOnlySpan<char> value) => decimal.TryParse(value, Culture.NumberFormat, out var d) ? d : null;

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(ReadOnlySpan<char> candidateString, IDataTypeSize? sizeRecord)
    {
        candidateString = TrimTrailingZeros(candidateString);

        if(IsExplicitDate(candidateString))
            return false;

        if (!decimal.TryParse(candidateString, NumberStyles.Any, Culture, out var t))
            return false;

        var dec = (SqlDecimal) t;
        sizeRecord?.Size.IncreaseTo(dec.Precision - dec.Scale,dec.Scale);

        return true;
    }

    private ReadOnlySpan<char> TrimTrailingZeros(ReadOnlySpan<char> s)
    {
        //don't trim 0 unless there's a decimal point e.g. don't trim from 1,000
        if (!s.Contains(_decimalIndicator))
            return s;


        var trim = 0;
        var foundOnlyZeros = true;

        //step back from the end of the string
        for (var i = s.Length - 1; i >= 0; i--)
            if (s[i] == '0')
            {
                //if we have only found zeros up till now
                if (foundOnlyZeros)
                    trim++; //we can trim
            }
            else
            if (s[i] == _decimalIndicator) //we have reached the decimal point, break out and do the trim
                break;
            else if (!(s[i] > '0' && s[i] <= '9')) //we found something odd before the decimal point e.g. the exponent character
                return s;
            else
                foundOnlyZeros = false; //we found a non zero so we should stop trimming (but don't break in case we find an exponent or something else later on)

        return trim > 0 ? s[..^trim] : s;
    }
}