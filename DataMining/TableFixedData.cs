using System.Collections.Generic;
using System.Linq;

namespace DataMining
{
    public class TableFixedData
    {
        #region Fields

        private int _classIndex = 0;

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

            return tableFixedData;
        }


    }
}