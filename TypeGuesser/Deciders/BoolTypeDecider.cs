using System.Globalization;
using System.Text.RegularExpressions;
using TB.ComponentModel;

namespace TypeGuesser.Deciders
{
    public class BoolTypeDecider: DecideTypesForStrings<bool>
    {
        Regex singleCharacter = new Regex(@"^\s*[A-Za-z]\s*$");

        public BoolTypeDecider(CultureInfo culture): base(culture,TypeCompatibilityGroup.Numerical,typeof(bool))
        {
        }

        protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
        {
            return  new BoolTypeDecider(culture);
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize size)
        {
            // "Y" / "N" is boolean unless the settings say it can't
            if (!Settings.CharCanBeBoolean && singleCharacter.IsMatch(candidateString))
                return false;

            return base.IsAcceptableAsTypeImpl(candidateString, size);
        }
    }
}