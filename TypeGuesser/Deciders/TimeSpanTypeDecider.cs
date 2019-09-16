using System;
using System.Globalization;

namespace TypeGuesser.Deciders
{
    /// <summary>
    /// Guesses whether strings are <see cref="TimeSpan"/> and handles parsing approved strings according to the <see cref="DecideTypesForStrings{T}.Culture"/>
    /// </summary>
    public class TimeSpanTypeDecider : DecideTypesForStrings<TimeSpan>
    {
        public TimeSpanTypeDecider(CultureInfo culture): base(culture,TypeCompatibilityGroup.Exclusive, typeof(TimeSpan))
        {
        }

        protected override IDecideTypesForStrings CloneImpl(CultureInfo culture)
        {
            return new TimeSpanTypeDecider(culture);
        }

        protected override object ParseImpl(string value)
        {
            var dt = DateTime.Parse(value);

            return dt.TimeOfDay;
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString,IDataTypeSize sizeRecord)
        {
            try
            {
                DateTime t;

                //if it parses as a date 
                if (DateTime.TryParse(candidateString, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out t))
                {
                    return t.Year == 1 && t.Month == 1 && t.Day == 1;//without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too 
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}