namespace TypeGuesser
{
    public interface IDataTypeSize
    {
        DecimalSize Size { get; set; }
        int? Width { get; set; }
        bool Unicode { get;set;}
    }
}