using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TB.ComponentModel;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="bool"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
/// </summary>
/// <remarks>
/// Creates a new instance with the given <paramref name="culture"/>
/// </remarks>
/// <param name="culture"></param>
public sealed partial class BoolTypeDecider(CultureInfo culture) : DecideTypesForStrings<bool>(culture,TypeCompatibilityGroup.Numerical,typeof(bool))
{
    private static readonly Regex SingleCharacter = SingleCharacterRegex();

    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo newCulture)
    {
        return new BoolTypeDecider(newCulture);
    }

    /// <inheritdoc />
    protected override object? ParseImpl(ReadOnlySpan<char> value)
    {
        return value.Length > 1 && "1tTyYjJ".IndexOf(value[0]) != -1;
    }

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(ReadOnlySpan<char> candidateString, IDataTypeSize? size)
    {
        // "Y" / "N" is boolean unless the settings say it can't
        if (!Settings.CharCanBeBoolean && SingleCharacter.IsMatch(candidateString))
            return false;

        return candidateString.Length switch
        {
            1 => "1tTyYjJ0fFnN".IndexOf(candidateString[0]) != -1,
            2 => candidateString.Equals("ja", StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals("-1", StringComparison.OrdinalIgnoreCase),
            3 => candidateString.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals(".t.", StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals(".f.", StringComparison.OrdinalIgnoreCase),
            4 => candidateString.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                 candidateString.Equals("nein", StringComparison.OrdinalIgnoreCase),
            5 => candidateString.Equals("false", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    [GeneratedRegex(@"^\s*[A-Za-z]\s*$",RegexOptions.CultureInvariant)]
    private static partial Regex SingleCharacterRegex();
}