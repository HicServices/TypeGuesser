using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TypeGuesser.Deciders;

/// <summary>
/// Class responsible for deciding whether a given string representation is likely to be for a given C# Type e.g.
/// "2001-01-01" is likely to be a date.
/// 
/// <para>Each IDecideTypesForStrings should be for a single Type although different sizes is allowed e.g. Int16, Int32, Int64</para>
/// 
/// <para>Implementations should be as mutually exclusive with as possible.  Look also at <see cref="DatabaseTypeRequest.PreferenceOrder"/> and <see cref="Guesser"/></para>
/// </summary>
public partial interface IDecideTypesForStrings
{
    /// <summary>
    /// Matches any number which looks like a proper decimal but has leading zeroes e.g. 012837 including.  Also matches if there is a
    /// decimal point (optionally followed by other digits).  It must match at least 2 digits at the start e.g. 01.01 would be matched
    /// but 0.01 wouldn't be matched (that's a legit float).  This is used to preserve leading zeroes in input (desired because it could
    /// be a serial number or otherwise important leading 0).  In this case the <see cref="Guesser"/> will use varchar(x) to represent the
    /// column instead of decimal(x,y).
    /// 
    /// <para>Also allows for starting with a negative sign e.g. -01.01 would be matched as a string</para>
    /// <para>Also allows for leading / trailing white space</para>
    /// 
    /// </summary>
    protected static readonly Regex ZeroPrefixedNumber = ZeroPrefixedNumberRegex();

    /// <summary>
    /// Determines how the decider interacts with other deciders e.g. IntTypeDecider can fall
    /// back to DecimalTypeDecider because you can store ints in decimal columns.
    /// </summary>
    TypeCompatibilityGroup CompatibilityGroup { get; }

    /// <summary>
    /// List of input Types which the decider will evaluate true
    /// </summary>
    HashSet<Type> TypesSupported { get; }

    /// <summary>
    /// Controls ambiguous parse behaviour e.g. whether "Y" should be interpreted as True or not
    /// </summary>
    GuessSettings Settings { get; set; }

    /// <summary>
    /// The culture for parsing with
    /// </summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// Returns true if the <paramref name="candidateString"/> is convertible and safely modelled as one of the <see cref="TypesSupported"/>.
    /// </summary>
    /// <param name="candidateString">a string containing a value of unknown type (e.g. "fish" or "1")</param>
    /// <param name="size">The current size estimate of floating point numbers (or null if not appropriate).  This will be modified by the method
    /// if appropriate to the data passed</param>
    /// <returns>True if the <paramref name="candidateString"/> is a valid value for the <see cref="TypesSupported"/> by the decider</returns>
    bool IsAcceptableAsType(string candidateString,IDataTypeSize? size);

    /// <summary>
    /// Converts the provided <paramref name="value"/> to an object of the Type modelled by this <see cref="IDecideTypesForStrings"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    object? Parse(string value);

    /// <summary>
    /// Returns a new instance of this class with the same <see cref="Culture"/> and <see cref="Settings"/> etc
    /// </summary>
    /// <returns></returns>
    IDecideTypesForStrings Clone();

    [GeneratedRegex(@"^\s*-?0+[1-9]+\.?[0-9]*\s*$",RegexOptions.CultureInvariant)]
    private static partial Regex ZeroPrefixedNumberRegex();
}