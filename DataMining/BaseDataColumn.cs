using System;
using System.Collections.Generic;

namespace DataMining
{
    public abstract class BaseDataColumn : IDataColumn
    {
        public abstract IEnumerable<object> Values { get; }
        public abstract string Name { get; }
        public abstract Type DataType { get; }
        public abstract bool IsNumeric { get; }
        public abstract object this[int index] { get; set; }
        public abstract bool Add(object value);
        public abstract bool RemoveAtIndex(int index);
        public abstract bool Insert(int index, object data);

        protected bool ComputeIsNumeric()
        {
            foreach (var item in Values)
            {
                Double numericValue;
                if (item != null && item != DBNull.Value && item != System.Type.Missing &&
                    !item.TryConvertToNumeric(out numericValue))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
