using System;
using System.Globalization;

namespace TypeGuesser.Deciders
{
    /// <summary>
    /// DecideTypesForStrings for types that we should never assign to strings but need to support for CurrentEstimate
    /// </summary>
    public class NeverGuessTheseTypeDecider : DecideTypesForStrings
    {
        public NeverGuessTheseTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Exclusive, typeof(byte[]), typeof(Guid))
        {
        }

        protected override object ParseImpl(string value)
        {
            throw new NotSupportedException();
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord)
        {
            //strings should never be interpreted as byte arrays
            return false;
        }
    }
}