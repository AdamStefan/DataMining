using System.Collections.Generic;

namespace DataMining
{
    public interface ITableData : IEnumerable<IDataRow>
    {
        IDataRow this[int rowIndex] { get; set; }
        IEnumerable<string> Attributes { get; }
        int IndexOf(IDataRow row);
        void Insert(int index, IDataRow value);
        void Add(IDataRow dataRow);
        IDataRow NewRow();
        void Clear();
        bool Contains(IDataRow dataRow);
        bool Remove(IDataRow row);
        void RemoveAt(int index);
        int Count { get; }        
    }
}