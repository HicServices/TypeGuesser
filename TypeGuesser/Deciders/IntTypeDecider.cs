using System.Globalization;
using TB.ComponentModel;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="int"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
/// <remarks>
/// Creates a new instance for recognizing whole numbers in string values
/// </remarks>
/// <param name="culture"></param>
public sealed class IntTypeDecider(CultureInfo culture) : DecideTypesForStrings<int>(culture,TypeCompatibilityGroup.Numerical, typeof(short) , typeof(int), typeof(byte))
{

    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo newCulture)
    {
        return new IntTypeDecider(newCulture);
    }

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize? sizeRecord)
    {
        if(IsExplicitDate(candidateString))
            return false;

        if (!candidateString.IsConvertibleTo(out int i, Culture))
            return false;

        sizeRecord?.Size.IncreaseTo(i.ToString().Trim('-').Length,0);
        return true;
    }
}