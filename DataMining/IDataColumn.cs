using System;
using System.Collections.Generic;

namespace DataMining
{
    public interface IDataColumn
    {
        IEnumerable<object> Values { get; }
        string Name { get; }
        Type DataType { get; }
        bool IsNumeric { get; }
    }
}