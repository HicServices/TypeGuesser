using System;
using System.Globalization;

namespace TypeGuesser.Deciders
{
    /// <summary>
    /// DecideTypesForStrings for types that we should never assign to strings but need to support for CurrentEstimate
    /// </summary>
    public class NeverGuessTheseTypeDecider : DecideTypesForStrings<object>
    {
        /// <summary>
        /// Creates a guesser which always returns false initialized with the default Types that should not be guessed (byte arrays / Guid etc)
        /// </summary>
        /// <param name="culture"></param>
        public NeverGuessTheseTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Exclusive, typeof(byte[]), typeof(Guid))
        {
        }

        /// <inheritdoc/>
        protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
        {
            return new NeverGuessTheseTypeDecider(culture);
        }

        /// <inheritdoc/>
        protected override object ParseImpl(string value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord)
        {
            //strings should never be interpreted as byte arrays
            return false;
        }
    }
}