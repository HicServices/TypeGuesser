using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Xml;
using TB.ComponentModel;

namespace TypeGuesser.Deciders
{
    public class DecimalTypeDecider : DecideTypesForStrings<decimal>
    {
        public DecimalTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical,typeof(decimal), typeof(float) , typeof(double))
        {
        }
        
        protected override bool IsAcceptableAsTypeImpl(string candidateString,IDataTypeSize sizeRecord)
        {
            candidateString = TrimTrailingZeros(candidateString);

            if (!decimal.TryParse(candidateString, NumberStyles.Any, Culture, out var t))
                return false;
            
            var dec = ((SqlDecimal) t);
            sizeRecord.Size.IncreaseTo(dec.Precision - dec.Scale,dec.Scale);

            return true;
        }

        private string TrimTrailingZeros(string s)
        {
            //don't trim 0 unless theres a decimal point e.g. don't trim from 1,000
            if (s.IndexOf(Culture.NumberFormat.NumberDecimalSeparator) == -1)
                return s;

            int trim = 0;

            //step back from the end of the string
            for (int i = s.Length - 1; i >= 0; i--)
                if (s[i] == '0')
                    trim++;
                else if (s[i] == 'E' || s[i] == 'e') //don't trim if there are exponents
                    return s;
                else
                    break;  //non zero digit

            if (trim > 0)
                return s.Substring(0, s.Length - trim);

            return s;
        }
    }
}