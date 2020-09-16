using System;
using System.Globalization;
using TB.ComponentModel;

namespace TypeGuesser.Deciders
{
    /// <summary>
    /// Guesses whether strings are <see cref="int"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
    /// </summary>
    public class IntTypeDecider : DecideTypesForStrings<int>
    {
        /// <summary>
        /// Creates a new instance for recognizing whole numbers in string values
        /// </summary>
        /// <param name="culture"></param>
        public IntTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical, typeof(Int16) , typeof(Int32), typeof(byte))
        {
        }

        /// <inheritdoc/>
        protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
        {
            return new IntTypeDecider(culture);
        }

        /// <inheritdoc/>
        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord)
        {
            if(IsExplicitDate(candidateString))
                return false;

            if (!candidateString.IsConvertibleTo(out int i, Culture))
                return false;

            sizeRecord.Size.IncreaseTo(i.ToString().Trim('-').Length,0);
            return true;
        }
    }
}