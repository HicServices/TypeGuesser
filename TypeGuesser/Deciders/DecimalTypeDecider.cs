﻿using System;
using System.Globalization;

namespace TypeGuesser.Deciders
{
    public class DecimalTypeDecider : DecideTypesForStrings
    {
        public DecimalTypeDecider(CultureInfo culture) : base(culture,TypeCompatibilityGroup.Numerical,typeof(decimal), typeof(float) , typeof(double))
        {
        }

        protected override object ParseImpl(string value)
        {
            return TryParseVague(value);
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString,IDataTypeSize sizeRecord)
        {
            decimal? t = TryParseVague(candidateString);

            if (t == null)
                return false;

            int before;
            int after;

            GetDecimalPlaces(t.Value, out before, out after);

            sizeRecord.Size.IncreaseTo(before,after);

            //could be whole number with no decimal
            return true;
        } 

        private decimal? TryParseVague(string candidate)
        {
            decimal x;

            if (decimal.TryParse(candidate, out x))
                return x;

            if (decimal.TryParse(candidate, NumberStyles.Float,CultureInfo.CurrentCulture, out x))
                return x;

            return null;
        }

        private void GetDecimalPlaces(decimal value, out int before, out int after)
        {
            decimal destructive = Math.Abs(value);

            if (value == 0)
            {
                before = 1;
                after = 0;
                return;
            }

            before = 0;
            while (destructive >= 1)
            {
                destructive = destructive / 10; //divide by 10
                destructive = decimal.Floor(destructive);//get rid of any overflowing decimal places 
                before++;
            }

            //always leave at least 1 before so that we can store 0s
            before = Math.Max(before, 1);

            //as if by magic... apparently
            after = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        }
    }
}