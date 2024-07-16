using System;
using System.Globalization;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="bool"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
/// <remarks>
/// Creates a new instance with the given <paramref name="culture"/>
/// </remarks>
/// <param name="culture"></param>
public sealed class BoolTypeDecider(CultureInfo culture):DecideTypesForStrings<bool>(culture,TypeCompatibilityGroup.Numerical,typeof(bool))
{
    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo newCulture)
    {
        return new BoolTypeDecider(newCulture);
    }

    /// <inheritdoc />
    protected override object? ParseImpl(ReadOnlySpan<char> candidateString)
    {
        if (bool.TryParse(candidateString, out var sysResult)) return sysResult;

        candidateString = StripWhitespace(candidateString);

        bool? b = candidateString.Length switch
        {
            1 => "1tTyYjJ0fFnN".IndexOf(candidateString[0]) != -1 ? "0fFnN".IndexOf(candidateString[0]) == -1 : null,
            2 => candidateString.Equals("ja",StringComparison.OrdinalIgnoreCase) ? true :
            (
                candidateString.Equals("no",StringComparison.OrdinalIgnoreCase) ||
                candidateString.Equals("-1",StringComparison.OrdinalIgnoreCase)
            ) ? false : null,
            3 => candidateString.Equals("yes",StringComparison.OrdinalIgnoreCase) ||
                candidateString.Equals(".t.",StringComparison.OrdinalIgnoreCase) ? true :
                candidateString.Equals(".f.",StringComparison.OrdinalIgnoreCase) ? false : null,
            4 => candidateString.Equals("true",StringComparison.OrdinalIgnoreCase) ? true :
                candidateString.Equals("nein",StringComparison.OrdinalIgnoreCase) ? false : null,
            5 => candidateString.Equals("false",StringComparison.OrdinalIgnoreCase) ? false : null,
            _ => null
        };
        return b ?? throw new Exception("Invalid bool");
    }

    private static ReadOnlySpan<char> StripWhitespace(ReadOnlySpan<char> candidateString)
    {
        while (candidateString.Length > 0 && char.IsWhiteSpace(candidateString[0]))
            candidateString = candidateString[1..];
        while (candidateString.Length > 0 && char.IsWhiteSpace(candidateString[^1]))
            candidateString = candidateString[..^1];
        return candidateString;
    }

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(ReadOnlySpan<char> candidateString,IDataTypeSize? size)
    {
        var strippedString = StripWhitespace(candidateString);

        // "Y" / "N" is boolean unless the settings say it can't
        if (!Settings.CharCanBeBoolean && strippedString.Length == 1 && char.IsAsciiLetter(strippedString[0]))
            return false;

        return bool.TryParse(candidateString, out _) || candidateString.Length switch
        {
            1 => "1tTyYjJ0fFnN".IndexOf(candidateString[0]) != -1,
            2 => candidateString.Equals("ja",StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals("no",StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals("-1",StringComparison.OrdinalIgnoreCase),
            3 => candidateString.Equals("yes",StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals(".t.",StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals(".f.",StringComparison.OrdinalIgnoreCase),
            4 => candidateString.Equals("true",StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals("nein",StringComparison.OrdinalIgnoreCase),
            5 => candidateString.Equals("false",StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}