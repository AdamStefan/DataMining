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

        public string[] ClassesValue;

        public string[] Attributes;

        private object[,] _data;

        #endregion

        #region Properties

        public bool IsDiscreteColumn(int column)
        {
            return ColumnDataTypes[column].IsDiscrete;
        }

        public bool IsClassAttribute(int column)
        {
            return column == _classIndex;
        }

        public ColumnDataType[] ColumnDataTypes { get; private set; }

        public object this[int rowIndex, int columnIndex]
        {
            get { return _data[columnIndex, rowIndex]; }
        }       

        public int Count
        {
            get { return _data.GetLength(1); }
        }

        public int Class(int rowIndex)
        {
            return (int) _data[_classIndex, rowIndex];
        }

        #endregion

        public static TableFixedData FromTableData(ITableData tableData)
        {
            var tableFixedData = new TableFixedData();

            var attributesNo = tableData.Attributes.Count();
            var rowsNumber = tableData.Count;
            tableFixedData._data = new object[attributesNo, rowsNumber];
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



            for (int columnIndex = 0; columnIndex < tableFixedData.Attributes.Length; columnIndex++)
            {
                var attribute = tableFixedData.Attributes[columnIndex];
                var isColumnNumeric = tableData[attribute].IsNumeric;

                for (index = 0; index < rowsNumber; index++)
                {
                    var currentValue = tableData[index][tableFixedData.Attributes[columnIndex]];
                    
                    if (attribute == TableData.ClassAttributeName)
                    {
                        if (!classes.ContainsKey((string) currentValue))
                        {
                            classes.Add((string) currentValue, currentClassesIndex);
                            currentClassesIndex++;
                        }

                        currentValue = classes[(string) currentValue];
                    }
                    else if (isColumnNumeric)
                    {
                        double numericValue;
                        if (currentValue.TryConvertToNumeric(out numericValue))
                        {
                            currentValue = numericValue;
                        }
                    }


                    tableFixedData._data[columnIndex, index] = currentValue;
                }
            }

            tableFixedData.ClassesValue = new string[classes.Count];
            foreach (var item in classes)
            {
                tableFixedData.ClassesValue[item.Value] = item.Key;
            }
            tableFixedData.BuildSymbols();

            return tableFixedData;
        }

        private void BuildSymbols()
        {
            _labelsDictionary = new Dictionary<string, int>[Attributes.Length];

            ColumnDataTypes = new ColumnDataType[Attributes.Length];

            var noOfRows = Count;

            for (int index = 0; index < Attributes.Length; index++)
            {
                if (_labelsDictionary[index] == null)
                {
                    _labelsDictionary[index] = new Dictionary<string, int>();
                }

                if (this[0, index].IsNumeric())
                {
                    ColumnDataTypes[index] = new ColumnDataType();
                    continue;
                }

                var symbols = _labelsDictionary[index];
                var newSymbol = 0;

                for (int rowIndex = 0; rowIndex < noOfRows; rowIndex++)
                {
                    var currentValue = this[rowIndex, index].ToString();
                    if (!symbols.ContainsKey(currentValue))
                    {
                        symbols.Add(currentValue, newSymbol);
                        newSymbol++;
                    }
                }
                ColumnDataTypes[index] = new ColumnDataType {IsDiscrete = true, NumberOfCategories = newSymbol};
            }
        }

        public static DataSample[] ToSample(TableFixedData tableFixedData)
        {
            var samples = new DataSample[tableFixedData.Count];

            for (int rowIndex = 0; rowIndex < tableFixedData.Count; rowIndex++)
            {
                var currentSample = new DataSample
                {
                    DataPoints = new DataPoint[tableFixedData.Attributes.Length - 1],
                    ClassId = tableFixedData.Class(rowIndex)
                };
                int dataPointIndex = 0;
                for (int columnIndex = 0; columnIndex < tableFixedData.Attributes.Length; columnIndex++)
                {
                    if (tableFixedData.Attributes[columnIndex] != TableData.ClassAttributeName)
                    {
                        var value = tableFixedData[rowIndex, columnIndex];
                        var dataPoint = new DataPoint
                        {
                            ColumnId = columnIndex,
                            Value =
                                tableFixedData.IsDiscreteColumn(columnIndex)
                                    ? Convert.ToDouble(tableFixedData.GetSymbol(value.ToString(), columnIndex))
                                    : Convert.ToDouble(value)
                        };

                        currentSample.DataPoints[dataPointIndex] = dataPoint;

                        dataPointIndex++;
                    }

                }

                samples[rowIndex] = currentSample;
            }

            return samples;
        }

        public DataSample GetSample(IDataRow dataRow)
        {
            var sample = new DataSample {DataPoints = new DataPoint[Attributes.Length - 1]};
            var dataPointIndex = 0;

            for (var columnIndex = 0; columnIndex < Attributes.Length; columnIndex++)
            {
                if (Attributes[columnIndex] != TableData.ClassAttributeName)
                {
                    var value = dataRow[Attributes[columnIndex]];

                    var dataPoint = new DataPoint
                    {
                        ColumnId = columnIndex,
                        Value =
                            IsDiscreteColumn(columnIndex)
                                ? Convert.ToDouble(GetSymbol(value.ToString(), columnIndex))
                                : Convert.ToDouble(value)
                    };

                    sample.DataPoints[dataPointIndex] = dataPoint;
                    dataPointIndex++;

                }                
            }

            return sample;
        }

        public T[] GetColumn<T>(int columnIndex, IValueConverter<T> converter = null)
        {
            var column = new T[Count];
            for (int index = 0; index < column.Length; index++)
            {
                if (converter != null)
                {
                    column[index] = converter.Convert(this[index, columnIndex]);
                }
                else
                {
                    column[index] = (T)this[index, columnIndex];
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

            if (this[0, columnIndex].IsNumeric())
            {
                throw new Exception("Selected column is numeric");
            }

            var column = new int[Count];
            if (_labelsDictionary[columnIndex] == null)
            {
                _labelsDictionary[columnIndex] = new Dictionary<string, int>();
            }
            var symbols = _labelsDictionary[columnIndex];
            var newSymbol = symbols.Keys.Count;

            for (int rowIndex = 0; rowIndex < column.Length; rowIndex++)
            {
                var currentValue = this[rowIndex, columnIndex].ToString();
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