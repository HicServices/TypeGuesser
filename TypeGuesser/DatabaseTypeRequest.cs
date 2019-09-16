using System;
using System.Collections.ObjectModel;

namespace TypeGuesser
{
    /// <summary>
    /// Describes a cross platform database field type you want created including maximum width for string based columns and precision/scale for
    /// decimals.
    /// </summary>
    public class DatabaseTypeRequest : IDataTypeSize
    {
        /// <summary>
        /// Any input string of unknown Type will be assignable to one of the following C# data types.  The order denotes system wide which
        /// data types to try  converting the string into in order of preference.  For the implementation of this see <see cref="Guesser"/>.
        /// </summary>
        public static readonly ReadOnlyCollection<Type> PreferenceOrder = new ReadOnlyCollection<Type>(new Type[]
        {
            typeof(bool),
            typeof(int),
            typeof(decimal),

            typeof(TimeSpan),
            typeof(DateTime), //ironically Convert.ToDateTime likes int and floats as valid dates -- nuts
            
            typeof(string)
        });

        private int? _maxWidthForStrings;

        /// <summary>
        /// The <see cref="System.Type"/> which this metadata describes.
        /// </summary>
        public Type CSharpType { get; set; }

        /// <summary>
        /// The <see cref="DecimalSize"/> of the largest scale / precision you want to be able to represent.  This is valid even if <see cref="CSharpType"/>
        /// is not a decimal (e.g. int).
        /// </summary>
        public DecimalSize Size { get; set; }

        /// <summary>
        /// The width in characters of the longest string representation of data you want to support.  This is valid even if <see cref="CSharpType"/>
        /// is not a string (E.g. a decimal).
        /// </summary>
        public int? Width
        {
            get => _maxWidthForStrings.HasValue ? Math.Max(_maxWidthForStrings.Value, Size.ToStringLength()): (int?)null;
            set => _maxWidthForStrings = value;
        }
        
        /// <summary>
        /// Only applies when <see cref="CSharpType"/> is <see cref="string"/>.  True indicates that the column should be
        /// nvarchar instead of varchar.
        /// </summary>
        public bool Unicode { get;set;}

        /// <summary>
        /// Creates a new instance with the given initial type description
        /// </summary>
        /// <param name="cSharpType"></param>
        /// <param name="maxWidthForStrings"></param>
        /// <param name="decimalPlacesBeforeAndAfter"></param>
        public DatabaseTypeRequest(Type cSharpType, int? maxWidthForStrings = null,
            DecimalSize decimalPlacesBeforeAndAfter = null)
        {
            CSharpType = cSharpType;
            Width = maxWidthForStrings;
            Size = decimalPlacesBeforeAndAfter?? new DecimalSize();
        }

        #region Equality
        protected bool Equals(DatabaseTypeRequest other)
        {
            return Equals(CSharpType, other.CSharpType) && Width == other.Width && Equals(Size, other.Size) && Unicode == other.Unicode;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DatabaseTypeRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (CSharpType != null ? CSharpType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Width.GetHashCode();
                hashCode = (hashCode*397) ^ (Size != null ? Size.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DatabaseTypeRequest left, DatabaseTypeRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DatabaseTypeRequest left, DatabaseTypeRequest right)
        {
            return !Equals(left, right);
        }
        #endregion

        public static DatabaseTypeRequest Max(DatabaseTypeRequest first, DatabaseTypeRequest second)
        {
            //if types differ
            if (PreferenceOrder.IndexOf(first.CSharpType) < PreferenceOrder.IndexOf(second.CSharpType))
            {
                second.Unicode = first.Unicode || second.Unicode;
                return second;
            }
            
            if (PreferenceOrder.IndexOf(first.CSharpType) > PreferenceOrder.IndexOf(second.CSharpType))
            {
                first.Unicode = first.Unicode || second.Unicode;
                return first;
            }
            
            if(!(first.CSharpType == second.CSharpType))
                throw new NotSupportedException(string.Format(SR.DatabaseTypeRequest_Max_Could_not_combine_Types___0___and___1___because_they_were_of_differing_Types_and_neither_Type_appeared_in_the_PreferenceOrder, first.CSharpType, second.CSharpType));

            //Types are the same, so max the sub elements (width, DecimalSize etc)
            int? newMaxWidthIfStrings = first.Width;

            //if first doesn't have a max string width
            if (newMaxWidthIfStrings == null)
                newMaxWidthIfStrings = second.Width; //use the second
            else if (second.Width != null)
                newMaxWidthIfStrings = Math.Max(newMaxWidthIfStrings.Value, second.Width.Value); //else use the max of the two

            //types are the same
            return new DatabaseTypeRequest(
                first.CSharpType,
                newMaxWidthIfStrings,
                DecimalSize.Combine(first.Size, second.Size)
                )
                {Unicode = first.Unicode || second.Unicode};

        }
    }
}
