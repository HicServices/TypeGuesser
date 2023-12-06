using System.Globalization;
using System.Text.RegularExpressions;

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
        return  new BoolTypeDecider(newCulture);
    }

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize size)
    {
        // "Y" / "N" is boolean unless the settings say it can't
        if (!Settings.CharCanBeBoolean && SingleCharacter.IsMatch(candidateString))
            return false;

        return base.IsAcceptableAsTypeImpl(candidateString, size);
    }

    [GeneratedRegex(@"^\s*[A-Za-z]\s*$",RegexOptions.CultureInvariant)]
    private static partial Regex SingleCharacterRegex();
}