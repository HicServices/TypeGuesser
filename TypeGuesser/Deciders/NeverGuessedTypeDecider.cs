using System;
using System.Globalization;

namespace TypeGuesser.Deciders;

/// <summary>
/// DecideTypesForStrings for types that we should never assign to strings but need to support for CurrentEstimate
/// </summary>
/// <remarks>
/// Creates a guesser which always returns false initialized with the default Types that should not be guessed (byte arrays / Guid etc)
/// </remarks>
/// <param name="culture"></param>
public sealed class NeverGuessTheseTypeDecider(CultureInfo culture) : DecideTypesForStrings<object>(culture,TypeCompatibilityGroup.Exclusive, typeof(byte[]), typeof(Guid))
{

    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo newCulture) => new NeverGuessTheseTypeDecider(newCulture);

    /// <inheritdoc/>
    protected override object ParseImpl(string value) => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord) =>
        //strings should never be interpreted as byte arrays
        false;
}