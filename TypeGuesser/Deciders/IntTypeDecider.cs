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
        public IntTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical, typeof(Int16) , typeof(Int32), typeof(Int64),typeof(byte))
        {
        }

        protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
        {
            return new IntTypeDecider(culture);
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord)
        {
            if (!candidateString.IsConvertibleTo(out int i, Culture))
                return false;

            sizeRecord.Size.IncreaseTo(i.ToString().Trim('-').Length,0);
            return true;
        }
    }
}