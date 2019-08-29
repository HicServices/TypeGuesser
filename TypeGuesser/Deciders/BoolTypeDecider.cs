using System.Globalization;

namespace TypeGuesser.Deciders
{
    public class BoolTypeDecider: DecideTypesForStrings
    {
        public BoolTypeDecider(CultureInfo culture): base(culture,TypeCompatibilityGroup.Numerical,typeof(bool))
        {
        }

        protected override object ParseImpl(string value)
        {
            return bool.Parse(value);
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString,DecimalSize sizeRecord)
        {
            bool result;

            return bool.TryParse(candidateString, out result);
        }
    }
}