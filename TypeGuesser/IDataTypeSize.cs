namespace TypeGuesser
{
    /// <summary>
    /// Interface for a classes which track Type information needed by a DBMS to create columns e.g. if you
    /// are defining a <see cref="string"/> column you need to indicate to the DBMS how long it is and if it
    /// should support <see cref="Unicode"/>
    /// </summary>
    public interface IDataTypeSize
    {
        /// <summary>
        /// When creating a <see cref="decimal"/> column, describes the required the Scale/Precision
        /// </summary>
        DecimalSize Size { get; set; }

        /// <summary>
        /// When creating a <see cref="string"/> column, describes the maximum length e.g. "varchar(50)"
        /// </summary>
        int? Width { get; set; }

        /// <summary>
        /// When creating a <see cref="string"/> column, describes whether it must support holding unicode characters (e.g. nvarchar vs varchar)
        /// </summary>
        bool Unicode { get;set;}
    }
}