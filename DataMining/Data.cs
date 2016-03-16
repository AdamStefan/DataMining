using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataMining
{
    public class TableData : ITableData
    {
        #region Fields

        private readonly Dictionary<string, BaseDataColumn> _data = new Dictionary<string, BaseDataColumn>();
        public const string ClassAttributeName = "Class";

        #endregion

        #region Properties

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public int Count { get; private set; }
      

        public IEnumerable<string> Attributes
        {
            get { return _data.Keys; }
        }

        public IDataRow this[int rowIndex]
        {
            get { return new DataRow(this, rowIndex); }
            set
            {
                var row = this[rowIndex];
                foreach (var attribute in value.Attributes)
                {
                    row[attribute] = value[attribute];
                }
            }
        }

        public IDataColumn this[string attributeName]
        {
            get { return _data[attributeName]; }
        }

        #endregion

        #region Instance

        public TableData()
        {
            var column = new Attribute(ClassAttributeName, typeof (string));
            var dataColumnn = new IndexedDataColumn(column);
            _data[ClassAttributeName] = dataColumnn;
        }

        #endregion

        #region Methods

        public IEnumerable<string> GetClasses()
        {
            var classColumn = ((IndexedDataColumn) _data[ClassAttributeName]);
            var ret = classColumn.IndexedValues.Select(item => item.Key.ToString());
            return ret;
        }

        public bool AddAttribute(string attributeName)
        {
            return AddAttribute(attributeName, typeof (object));
        }

        private bool AddAttribute(string attributeName, Type attributeType)
        {
            if (_data.ContainsKey(attributeName))
            {
                return false;
            }

            var column = new Attribute(attributeName, attributeType);
            var dataColumnn = new DataColumn(column);
            if (Count > 0)
            {
                var defaultValue = GetDefaultValue(attributeType);

                for (int i = 0; i < Count; i++)
                {
                    dataColumnn.Add(defaultValue);
                }
            }
            _data[attributeName] = dataColumnn;

            return true;
        }

        public int IndexOf(object value)
        {
            return IndexOf(value as IDataRow);
        }

        public int IndexOf(IDataRow row)
        {
            var dataRow = row as DataRow;
            if (dataRow == null || dataRow.Table != this)
            {
                return -1;
            }

            return dataRow.RowIndex;
        }

        public bool Contains(object item)
        {
            return Contains(item as IDataRow);
        }

        public bool Contains(IDataRow dataRow)
        {
            return IndexOf(dataRow) > 0;
        }

        public void CopyTo(IDataRow[] array, int arrayIndex)
        {
            var currentIndex = 0;
            for (int index = arrayIndex; index < array.Length; index++)
            {
                if (currentIndex >= Count)
                {
                    break;
                }
                array[index] = this[currentIndex];
                currentIndex++;
            }
        }

        public void Clear()
        {
            _data.Clear();
            var column = new Attribute(ClassAttributeName, typeof (string));
            var dataColumnn = new IndexedDataColumn(column);
            _data[ClassAttributeName] = dataColumnn;
        }

        public void Remove(object value)
        {
            Remove(value as IDataRow);
        }

        public bool Remove(IDataRow row)
        {
            var index = IndexOf(row);
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0)
            {
                foreach (var attribute in Attributes)
                {
                    var column = this[attribute] as BaseDataColumn;
                    if (column != null)
                    {
                        column.RemoveAtIndex(index);
                    }
                }

                Count--;
            }
        }

        public void Insert(int index, IDataRow value)
        {
            var newDataRow = value as NewDataRow;
            if (newDataRow == null)
            {
                throw new ArgumentException("value");
            }

            foreach (var column in _data.Keys)
            {
                _data[column].Insert(index, value[column]);
            }

            Count++;
        }

        public void Insert(int index, object value)
        {
            Insert(index, value as IDataRow);
        }

        public bool AddRow(IDataRow dataRow)
        {
            var newDataRow = dataRow as NewDataRow;
            if (newDataRow == null)
            {
                return false;
            }

            foreach (var column in _data.Keys)
            {
                _data[column].Add(dataRow[column]);
            }

            Count ++;
            return true;
        }

        public void Add(IDataRow dataRow)
        {
            AddRow(dataRow);
        }

        public IDataRow NewRow()
        {
            return new NewDataRow(this);
        }

        #endregion

        #region Private

        private class DataRow : IDataRow
        {
            #region Fields

            internal readonly TableData Table;
            private readonly int _rowIndex;

            public int RowIndex
            {
                get { return _rowIndex; }
            }

            #endregion

            internal DataRow(TableData table, int rowIndex)
            {
                Table = table;
                _rowIndex = rowIndex;
            }


            public object this[string attribute]
            {
                get { return Table._data[attribute][RowIndex]; }
                set { Table._data[attribute][RowIndex] = value; }
            }

            public string Class
            {
                get { return (string) this[ClassAttributeName]; }
                set { this[ClassAttributeName] = value; }
            }

            public IEnumerable<string> Attributes
            {
                get { return Table.Attributes; }
            }

            public bool IsNumeric(string attribute)
            {
                return Table[attribute].IsNumeric;
            }
        }

        private class NewDataRow : IDataRow
        {

            #region Fields

            private readonly TableData _table;

            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();


            #endregion

            internal NewDataRow(TableData table)
            {
                _table = table;

            }

            public object this[string attribute]
            {
                get { return _values.ContainsKey(attribute) ? _values[attribute] : null; }
                set
                {
                    var type = _table[attribute].DataType;
                    if (value != null && type != typeof (object) && !value.GetType().IsAssignableFrom(type))
                    {
                        throw new InvalidCastException();
                    }

                    _values[attribute] = value;
                }
            }

            public string Class
            {
                get { return (string) this[ClassAttributeName]; }
                set { this[ClassAttributeName] = value; }
            }

            public IEnumerable<string> Attributes
            {
                get { return _table.Attributes; }
            }

            public bool IsNumeric(string attribute)
            {
                return _table[attribute].IsNumeric;
            }
        }

        private class DataColumn : BaseDataColumn
        {
            #region Fields

            private readonly IList<object> _data = new List<object>();
            private readonly Attribute _attribute;
            private bool? _isNumeric;

            #endregion

            public DataColumn(Attribute attribute)
            {
                _attribute = attribute;
            }

            public override IEnumerable<object> Values
            {
                get { return _data; }
            }

            public override string Name
            {
                get { return _attribute.AttributeName; }
            }

            public override Type DataType
            {
                get { return _attribute.DataType; }
            }

            public override bool IsNumeric
            {
                get
                {
                    if (!_isNumeric.HasValue)
                    {
                        _isNumeric = ComputeIsNumeric();
                    }
                    return _isNumeric.Value;
                }
            }

            public override object this[int index]
            {
                get { return _data[index]; }
                set
                {
                    var type = _attribute.DataType;
                    if (value != null && type != typeof (object) && !value.GetType().IsAssignableFrom(type))
                    {
                        throw new InvalidCastException();
                    }

                    _data[index] = value;

                    if (value != null)
                    {
                        if (_isNumeric.HasValue && value.IsNumeric() != _isNumeric.Value)
                        {
                            _isNumeric = null;
                        }
                    }
                }
            }

            public override bool Add(object value)
            {
                OnBeforeAddData(value);

                _data.Add(value);

                OnAfterAddData(value);

                return true;
            }

            public override bool RemoveAtIndex(int index)
            {
                var valueToRemove = _data[index];
                _data.RemoveAt(index);

                if (!_isNumeric.HasValue)
                {
                    _isNumeric = ComputeIsNumeric();
                }
                else if (valueToRemove != null && _isNumeric.Value != valueToRemove.IsNumeric())
                {
                    _isNumeric = null;
                }
                return true;
            }

            private void OnBeforeAddData(object value)
            {
                var type = _attribute.DataType;
                if (value != null && type != typeof (object) && !value.GetType().IsAssignableFrom(type))
                {
                    throw new InvalidCastException();
                }
            }

            private void OnAfterAddData(object value)
            {
                if (!_isNumeric.HasValue)
                {
                    _isNumeric = ComputeIsNumeric();
                }
                else if (value != null)
                {
                    _isNumeric = value.IsNumeric();
                }
            }

            public override bool Insert(int index, object data)
            {
                OnBeforeAddData(data);
                _data.Insert(index, data);
                OnAfterAddData(data);
                return true;
            }
        }

        private class IndexedDataColumn : BaseDataColumn
        {
            #region Fields

            private readonly IList<object> _data = new List<object>();
            private readonly Dictionary<object, List<int>> _indexedData = new Dictionary<object, List<int>>();
            private readonly Attribute _attribute;
            private bool? _isNumeric;

            #endregion

            public IndexedDataColumn(Attribute attribute)
            {
                _attribute = attribute;
            }

            public override IEnumerable<object> Values
            {
                get { return _data; }
            }

            public override string Name
            {
                get { return _attribute.AttributeName; }
            }

            public override Type DataType
            {
                get { return _attribute.DataType; }
            }

            public override object this[int index]
            {
                get { return _data[index]; }
                set
                {
                    var type = _attribute.DataType;
                    if (value != null && type != typeof (object) && !value.GetType().IsAssignableFrom(type))
                    {
                        throw new InvalidCastException();
                    }
                    _data[index] = value;
                    var valueToAdd = value ?? DBNull.Value;
                    if (!_indexedData.ContainsKey(valueToAdd))
                    {
                        _indexedData[valueToAdd] = new List<int>();
                    }
                    _indexedData[valueToAdd].Add(index);

                    if (value != null)
                    {
                        if (_isNumeric.HasValue && value.IsNumeric() != _isNumeric.Value)
                        {
                            _isNumeric = null;
                        }
                    }
                }
            }

            public override bool IsNumeric
            {
                get
                {
                    if (!_isNumeric.HasValue)
                    {
                        _isNumeric = ComputeIsNumeric();
                    }
                    return _isNumeric.Value;
                }
            }

            public IEnumerable<KeyValuePair<object, List<int>>> IndexedValues
            {
                get { return _indexedData; }
            }

            public override bool Add(object value)
            {
                OnBeforeAddData(value);
                _data.Add(value);
                OnAfterAddData(value);
                return true;
            }

            private void OnBeforeAddData(object value)
            {
                var type = _attribute.DataType;
                if (value != null && type != typeof (object) && !value.GetType().IsAssignableFrom(type))
                {
                    throw new InvalidCastException();
                }
            }

            private void OnAfterAddData(object value)
            {
                var valueToAdd = value ?? DBNull.Value;
                if (!_indexedData.ContainsKey(valueToAdd))
                {
                    _indexedData[valueToAdd] = new List<int>();
                }
                _indexedData[valueToAdd].Add(_data.Count);

                if (!_isNumeric.HasValue)
                {
                    _isNumeric = ComputeIsNumeric();
                }
                else if (value != null)
                {
                    _isNumeric = value.IsNumeric();
                }
            }

            public override bool RemoveAtIndex(int index)
            {
                var valueToRemove = _data[index];
                _data.RemoveAt(index);

                if (!_isNumeric.HasValue)
                {
                    _isNumeric = ComputeIsNumeric();
                }
                else if (valueToRemove != null && _isNumeric.Value != valueToRemove.IsNumeric())
                {
                    _isNumeric = null;
                }

                return true;
            }

            public override bool Insert(int index, object data)
            {
                OnBeforeAddData(data);
                _data.Insert(index, data);
                OnAfterAddData(data);
                return true;
            }
        }

        private class Attribute
        {

            #region Instance

            public Attribute(string attribute, Type type)
            {
                AttributeName = attribute;
                DataType = type;
            }

            #endregion

            public string AttributeName { get; private set; }

            public Type DataType { get; private set; }

        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        #endregion

        public IEnumerator<IDataRow> GetEnumerator()
        {
            return new DataEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataEnumerator(this);
        }

        private class DataEnumerator : IEnumerator<IDataRow>
        {
            #region Fields

            private TableData _tableData;
            private int _currentIndex = -1;

            #endregion


            public DataEnumerator(TableData tableData)
            {
                _tableData = tableData;
            }

            public void Dispose()
            {
                _tableData = null;
            }

            public bool MoveNext()
            {
                if (_currentIndex >= _tableData.Count - 1)
                {
                    return false;
                }

                _currentIndex++;
                return true;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public IDataRow Current
            {
                get { return _tableData[_currentIndex]; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}