using CsvHelper;
using DataMining;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class Program
    {
        private static void Main(string[] args)
        {

            // Tools.Test();
            var data = LoadDataFromfCSV("Data.csv");
            var algorithm = new C45Algorithm();
            var fixedData = TableFixedData.FromTableData(data);


            // var val = algorithm.ComputeGain(data, "Attribute1");
            double splitValue;
            var delta = 0.0;

            var watch1 = new Stopwatch();
            var watch2 = new Stopwatch();

            for (int index = 0; index < 100; index++)
            {

                watch1.Start();
                var ret = algorithm.BuildConditionalTree(data);
                watch1.Stop();


                watch2.Start();
                var ret1 = algorithm.BuildConditionalTreeOptimized(fixedData);
                watch2.Stop();

                //var className = ret.GetClass(data.ToList()[8]);
                var className1 = ret1.GetClass(data.ToList()[8]);

            }
            delta = watch1.Elapsed.Subtract(watch2.Elapsed).TotalMilliseconds;

        }


        private static void Test()
        {
            var data = new TableData();

            Random random = new Random(321);
            const int rows = 50;
            const int columns = 3;
            var matrix = new int[rows, columns];

            for (int currentColumn = 0; currentColumn < columns; currentColumn++)
            {
                data.AddAttribute("attribute" + (currentColumn + 1));
            }

            for (var currentRow = 0; currentRow < rows; currentRow++)
            {
                var row = data.NewRow();
                for (var currentColumn = 0; currentColumn < columns; currentColumn++)
                {
                    var attribute = "attribute" + (currentColumn + 1);
                    matrix[currentRow, currentColumn] = random.Next();
                    row[attribute] = matrix[currentRow, currentColumn];
                }

                data.AddRow(row);
            }

            for (var currentRow = 0; currentRow < rows; currentRow++)
            {
                for (var currentColumn = 0; currentColumn < columns; currentColumn++)
                {
                    var attribute = "attribute" + (currentColumn + 1);
                    //Convert.ChangeType()
                    //var matrix[currentRow, currentColumn]
                    //data[currentRow][attribute]

                    if (matrix[currentRow, currentColumn] != (int)data[currentRow][attribute])
                    {
                        throw new Exception("InvalidData");
                    }
                }
            }
        }

        public static TableData LoadDataFromfCSV(string fileName)
        {
            var configuration = new CsvHelper.Configuration.CsvConfiguration();
            configuration.Delimiter = "\t";
            configuration.HasExcelSeparator = false;
            configuration.IgnoreQuotes = true;
            configuration.QuoteNoFields = true;
            using (var reader = new CsvReader(new StreamReader(fileName), configuration))
            {

                var data = new TableData();
                var index = 0;
                string[] headers = null;

                while (reader.Read())
                {

                    if (index == 0)
                    {
                        headers = reader.FieldHeaders;

                        for (var columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                        {
                            var columnName = headers[columnIndex];
                            data.AddAttribute(columnName);
                        }
                        index++;
                        continue;
                    }


                    var row = data.NewRow();
                    for (var columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                    {
                        var columnName = headers[columnIndex];
                        row[columnName] = reader.GetField(columnIndex);
                    }
                    data.AddRow(row);

                }
                return data;
            }

        }


        public static TableData LoadDataFromExcelFile(string fileName)
        {
            //var workbook = new Workbook(fileName);
            //var sheet = workbook.Worksheets[0];

            //var maxDataRow = sheet.Cells.MaxDataRow;
            //var maxDataColumn = sheet.Cells.MaxDataColumn;

            //var minDataRow = sheet.Cells.MinDataRow;
            //var minDataColumn = sheet.Cells.MinDataColumn;

            //var dataRows = maxDataRow - minDataRow;
            //var columnRows = maxDataColumn - minDataColumn;


            //if (dataRows > 1 && columnRows > 1)
            //{
            //    var data = new TableData();                

            //    for (var columnIndex = minDataColumn; columnIndex <= maxDataColumn; columnIndex++)
            //    {
            //        var columnName = sheet.Cells[minDataRow, columnIndex].StringValue;
            //        data.AddAttribute(columnName);
            //    }

            //    for (int rowIndex = minDataRow + 1; rowIndex <= maxDataRow; rowIndex++)
            //    {
            //        var row = data.NewRow();
            //        for (var columnIndex = minDataColumn; columnIndex <= maxDataColumn; columnIndex++)
            //        {
            //            var columnName = sheet.Cells[minDataRow, columnIndex].StringValue;                                                
            //            row[columnName] = sheet.Cells[rowIndex, columnIndex].Value;
            //        }
            //        data.AddRow(row);
            //    }

            //    return data;
            //}
            return null;
        }
    }
}
