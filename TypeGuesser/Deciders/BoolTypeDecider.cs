using System.Globalization;
using System.Text.RegularExpressions;
using TB.ComponentModel;

namespace TypeGuesser.Deciders
{
    /// <summary>
    /// Guesses whether strings are <see cref="bool"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
    /// </summary>
    public class BoolTypeDecider: DecideTypesForStrings<bool>
    {
        Regex singleCharacter = new Regex(@"^\s*[A-Za-z]\s*$");

        /// <summary>
        /// Creates a new instance with the given <paramref name="culture"/>
        /// </summary>
        /// <param name="culture"></param>
        public BoolTypeDecider(CultureInfo culture): base(culture,TypeCompatibilityGroup.Numerical,typeof(bool))
        {
        }

        /// <inheritdoc/>
        protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
        {
            return  new BoolTypeDecider(culture);
        }

        /// <inheritdoc/>
        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize size)
        {
            // "Y" / "N" is boolean unless the settings say it can't
            if (!Settings.CharCanBeBoolean && singleCharacter.IsMatch(candidateString))
                return false;

            return base.IsAcceptableAsTypeImpl(candidateString, size);
        }
    }
}