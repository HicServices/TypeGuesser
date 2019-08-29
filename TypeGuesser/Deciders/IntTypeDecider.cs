using System;
using System.Globalization;
using TB.ComponentModel;

namespace TypeGuesser.Deciders
{
    public class IntTypeDecider : DecideTypesForStrings<int>
    {
        public IntTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical, typeof(Int16) , typeof(Int32), typeof(Int64),typeof(byte))
        {
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