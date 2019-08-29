using System;
using System.Globalization;

namespace TypeGuesser.Deciders
{
    public class IntTypeDecider : DecideTypesForStrings
    {
        public IntTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical, typeof(Int16) , typeof(Int32), typeof(Int64),typeof(byte))
        {
        }

        protected override object ParseImpl(string value)
        {
            return System.Convert.ToInt32(value);
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord)
        {
            try
            {
                var t = System.Convert.ToInt32(candidateString);
                
                sizeRecord.Size.IncreaseTo(t.ToString().Length);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}