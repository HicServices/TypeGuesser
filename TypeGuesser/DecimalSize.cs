using System;

namespace TypeGuesser
{
    /// <summary>
    /// Records the number of decimal places required to represent a given decimal.  This can class can represent a int (in which case <see cref="NumbersAfterDecimalPlace"/> will be 0) or
    /// decimal.  If the origin of the class is not numerical then a <see cref="DecimalSize"/> might still exist but it should be <see cref="IsEmpty"/>.
    /// </summary>
    public class DecimalSize
    {
        public int NumbersBeforeDecimalPlace;
        public int NumbersAfterDecimalPlace;

        /// <summary>
        /// Creates a new empty instance
        /// </summary>
        public DecimalSize()
        {
            
        }

        /// <summary>
        /// Creates a new instance initialized to the sizes provided
        /// </summary>
        /// <param name="numbersBeforeDecimalPlace"></param>
        /// <param name="numbersAfterDecimalPlace"></param>
        public DecimalSize(int numbersBeforeDecimalPlace, int numbersAfterDecimalPlace)
        {
            NumbersBeforeDecimalPlace = Math.Max(0,numbersBeforeDecimalPlace);
            NumbersAfterDecimalPlace = Math.Max(0,numbersAfterDecimalPlace);
        }

        /// <summary>
        /// Returns true if both <see cref="NumbersAfterDecimalPlace"/> and <see cref="NumbersBeforeDecimalPlace"/> are null/zero
        /// </summary>
        public bool IsEmpty => Precision == 0;

        /// <summary>
        /// Returns the sum of <see cref="NumbersBeforeDecimalPlace"/> and <see cref="NumbersAfterDecimalPlace"/>
        /// </summary>
        public int Precision => (NumbersBeforeDecimalPlace) + (NumbersAfterDecimalPlace);

        /// <summary>
        /// Returns the <see cref="NumbersAfterDecimalPlace"/>
        /// </summary>
        public int Scale => NumbersAfterDecimalPlace;

        /// <summary>
        /// Expands the instance to accomodate the new size (if expansion is required)
        /// </summary>
        /// <param name="numbersBeforeDecimalPlace"></param>
        public void IncreaseTo(int numbersBeforeDecimalPlace)
        {
            NumbersBeforeDecimalPlace = Math.Max(NumbersBeforeDecimalPlace, numbersBeforeDecimalPlace);
        }

        /// <summary>
        /// Expands the instance to accomodate the new size (if expansion is required)
        /// </summary>
        /// <param name="numbersBeforeDecimalPlace"></param>
        /// <param name="numbersAfterDecimalPlace"></param>
        public void IncreaseTo(int numbersBeforeDecimalPlace, int numbersAfterDecimalPlace)
        {
            NumbersBeforeDecimalPlace = Math.Max(NumbersBeforeDecimalPlace, numbersBeforeDecimalPlace);
            NumbersAfterDecimalPlace = Math.Max(NumbersAfterDecimalPlace, numbersAfterDecimalPlace);
        }

        /// <summary>
        /// Expands the instance to accomodate the new size (if expansion is required)
        /// </summary>
        /// <param name="other"></param>
        private void IncreaseTo(DecimalSize other)
        {
            NumbersBeforeDecimalPlace = Math.Max(NumbersBeforeDecimalPlace, other.NumbersBeforeDecimalPlace);
            NumbersAfterDecimalPlace = Math.Max(NumbersAfterDecimalPlace, other.NumbersAfterDecimalPlace);
        }


        /// <summary>
        /// Returns the number of characters required to represent the currently computed decimal size e.g. 1.2 requries length of 3.
        /// </summary>
        /// <returns></returns>
        public int ToStringLength()
        {
            int lengthRequired = 0;

            lengthRequired += NumbersAfterDecimalPlace;
            lengthRequired += NumbersBeforeDecimalPlace;

            //if it has things after the decimal point
            if (Scale != 0)
                lengthRequired++;

            return lengthRequired;
        }

        /// <summary>
        /// Returns a new <see cref="DecimalSize"/> which is big enough to accomodate decimals of <paramref name="first"/> size and those of <paramref name="second"/>.  
        /// For example if the first is decimal(3,0) and the second is decimal(5,4) then the returned result would be decimal(7,4).
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static DecimalSize Combine(DecimalSize first, DecimalSize second)
        {
            if (first == null)
                return second;

            if (second == null)
                return first;

            var newSize = new DecimalSize();
            newSize.IncreaseTo(first);
            newSize.IncreaseTo(second);
            
            return newSize;
        }

        #region Equality
        protected bool Equals(DecimalSize other)
        {
            return NumbersBeforeDecimalPlace == other.NumbersBeforeDecimalPlace && NumbersAfterDecimalPlace == other.NumbersAfterDecimalPlace;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DecimalSize)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NumbersBeforeDecimalPlace.GetHashCode() * 397) ^ NumbersAfterDecimalPlace.GetHashCode();
            }
        }
        #endregion
    }
}
