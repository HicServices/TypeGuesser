using System.Globalization;
using TB.ComponentModel;

namespace TypeGuesser.Deciders
{
    public class BoolTypeDecider: DecideTypesForStrings<bool>
    {
        public BoolTypeDecider(CultureInfo culture): base(culture,TypeCompatibilityGroup.Numerical,typeof(bool))
        {
        }
    }
}