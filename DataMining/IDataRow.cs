using System.Collections.Generic;

namespace DataMining
{
    public interface IDataRow
    {
        object this[string attribute] { get; set; }
        string Class { get; set; }
        IEnumerable<string> Attributes { get; }
        bool IsNumeric(string attribute);        
    }
}