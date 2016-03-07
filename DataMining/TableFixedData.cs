using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMining
{
    public class TableFixedData
    {
        #region Fields

        private int _classIndex;
        private Dictionary<string, int>[] _labelsDictionary;

        #endregion

        #region Properties

        public object this[int rowIndex, int columnIndex]
        {
            get { return _data[rowIndex, columnIndex]; }
        }

        public int Count
        {
            get { return _data.GetLength(0); }
        }

        public int Class(int rowIndex)
        {
            return (int)_data[rowIndex, _classIndex];
        }

        #endregion

        #region Fields

        public string[] ClassesValue;
        public string[] Attributes;        

        private object[,] _data;

        #endregion

        public static TableFixedData FromTableData(ITableData tableData)
        {
            var tableFixedData = new TableFixedData();

            var attributesNo = tableData.Attributes.Count();
            var rowsNumber = tableData.Count;
            tableFixedData._data = new object[rowsNumber, attributesNo];
            var index = 0;
            var columns = new Dictionary<string, int>();
            foreach (var attribute in tableData.Attributes)
            {
                columns[attribute] = index;
                if (attribute == TableData.ClassAttributeName)
                {

                    tableFixedData._classIndex = index;
                }
                index++;
            }

            tableFixedData.Attributes = new string[columns.Count];
            foreach (var item in columns)
            {
                tableFixedData.Attributes[item.Value] = item.Key;
            }

            var classes = new Dictionary<string, int>();
            var currentClassesIndex = 0;

            for (index = 0; index < rowsNumber; index++)
            {
                for (int columnIndex = 0; columnIndex < tableFixedData.Attributes.Length; columnIndex++)
                {

                    var currentValue = tableData[index][tableFixedData.Attributes[columnIndex]];
                    var attribute = tableFixedData.Attributes[columnIndex];

                    if (attribute == TableData.ClassAttributeName)
                    {
                        if (!classes.ContainsKey((string)currentValue))
                        {
                            classes.Add((string)currentValue, currentClassesIndex);
                            currentClassesIndex++;
                        }

                        currentValue = classes[(string)currentValue];
                    }

                    tableFixedData._data[index, columnIndex] = currentValue;
                }
            }

            tableFixedData.ClassesValue = new string[classes.Count];
            foreach (var item in classes)
            {
                tableFixedData.ClassesValue[item.Value] = item.Key;
            }
            tableFixedData._labelsDictionary = new Dictionary<string, int>[tableFixedData.Attributes.Length];

            return tableFixedData;
        }

        public T[] GetColumn<T>(int columnIndex,IValueConverter<T> converter=null)
        {
            var column = new T[_data.GetLength(0)];
            for (int index = 0; index < column.Length; index++)
            {
                if (converter != null)
                {
                    column[index] = converter.Convert(_data[index, columnIndex]);
                }
                else
                {
                    column[index] = (T)_data[index, columnIndex];
                }
            }

            return column;
        }
        

        public int[] GetSymbols(int columnIndex)
        {
            if (columnIndex >= Attributes.Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (_data[0, columnIndex].IsNumeric())
            {
                throw new Exception("Selected column is numeric");
            }

            var column = new int[_data.GetLength(0)];
            if (_labelsDictionary[columnIndex] == null)
            {
                _labelsDictionary[columnIndex] = new Dictionary<string, int>();
            }
            var symbols = _labelsDictionary[columnIndex];
            var newSymbol = symbols.Keys.Count;

            for (int rowIndex = 0; rowIndex < column.Length; rowIndex++)
            {
                var currentValue = _data[rowIndex, columnIndex].ToString();
                if (!symbols.ContainsKey(currentValue))
                {
                    symbols.Add(currentValue, newSymbol);
                    newSymbol++;
                }
                column[rowIndex] = symbols[currentValue];              
            }

            return column;
        }

        public int GetSymbol(string value, int columnIndex)
        {
            if (columnIndex >= Attributes.Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (_labelsDictionary[columnIndex] == null)
            {
                GetSymbols(columnIndex);
            }
            return _labelsDictionary[columnIndex][value];
        }

    }
}