using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using DataMining;
using DataMining.DecisionTrees;
using TestApplication.SentimentAnalysis;

namespace TestApplication
{
    class Program
    {
        private static void Main(string[] args)
        {

            Tools.Test();

            TestSentimentAnalysis();
            var data = LoadDataFromfCSV("Data.csv");

            TestNaiveBayes();
            var algorithm = new C45Algorithm();
            var fixedData = TableFixedData.FromTableData(data);


            // var val = algorithm.ComputeGain(data, "Attribute1");
            double splitValue;
            var delta = 0.0;

            var watch1 = new Stopwatch();
            var watch2 = new Stopwatch();


            var decisionalTree = algorithm.BuildConditionalTreeOptimized(fixedData);

            for (int index = 0; index < 50; index++)
            {
                //var className1 = ret1.GetClass(data.ToList()[8]);
                var row = data.ToList()[index];
                var estimatedClassName = decisionalTree.GetClass(row);
                if (estimatedClassName != row.Class)
                {
                   // missed++;
                }
            }




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


        private static void TestNaiveBayes()
        {
            var data = LoadDataFromfCSV("Data.csv");

            var fixedData = TableFixedData.FromTableData(data);
            var samples = TableFixedData.ToSample(fixedData);
            var columnsTypes = fixedData.ColumnDataTypes;

            var algorithm = new NaiveBayesClassifierOld(fixedData);

            var algorithm1 = new NaiveBayesClassifier(samples,fixedData.ClassesValue.Length,columnsTypes);

            var dataRow = data.ToList()[2];

            var className = algorithm.Compute(dataRow);
            var classId = algorithm1.Compute(fixedData.GetSample(dataRow));
            
            var className1 = fixedData.ClassesValue[classId];

            int missed = 0;

            for (int index = 0; index < 50; index++)
            {
                var row = data.ToList()[index];
                var estimatedClassName = algorithm.Compute(row);
                if (estimatedClassName != row.Class)
                {
                    missed++;
                }

            }
        }

        private static void TestSentimentAnalysis()
        {

            //var messageValue =
            //   "@switchfoot http://twitpic.com/2y1zl - Awww, that's a bummer.  You shoulda got David Carr of Third Day to do it. ;D";


           // var message = NaiveBayesSentimentAnalysis.SplitToWords(messageValue);

            //string[] sentences = Regex.Split(messageValue, @"(?<=[.!?])\s+(?=\p{Lt})");

            //var data1 = Regex.Split(sentences[0], @"\W+");

            var data =
                LoadDataFromfCSV(
                    @"C:\Users\IBM_ADMIN\Downloads\trainingandtestdata\training.1600000.processed.noemoticon.csv", ",",
                    false, false, new[] {0, 5}, 0);

            var count = data.Count;
            var itemsToTrain = data.Select(item => new Tuple<string, string>((string)item["Column1"], item.Class));

            var naiveBayesSentimentAnalysis = new NaiveBayesSentimentAnalysis();
            naiveBayesSentimentAnalysis.Train(itemsToTrain, count - 1);
            var ret = naiveBayesSentimentAnalysis.Compute("I hate you");
            var ret1 = naiveBayesSentimentAnalysis.Compute("I love you");

            var ret21 = naiveBayesSentimentAnalysis.Compute("But have been tied up in a customer issue, so need further discussions on what next");
            var ret3 = naiveBayesSentimentAnalysis.Compute("Only thing at the moment is we are going to end your teams involvement on Cafe project ");

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

        public static TableData LoadDataFromfCSV(string fileName, string delimeter = null, bool hasHeaderRecord = true,
            bool ignoreQuotes = true, int[] columnIndexes = null, int classIndex=-1)
        {
            var configuration = new CsvConfiguration();
            configuration.Delimiter = delimeter ?? "\t";
            configuration.HasExcelSeparator = false;
            configuration.IgnoreQuotes = ignoreQuotes;
            configuration.HasHeaderRecord = hasHeaderRecord;
            configuration.QuoteNoFields = true;
            using (var reader = new CsvReader(new StreamReader(fileName), configuration))
            {

                var data = new TableData();
                var index = 0;

                while (reader.Read())
                {

                    if (index == 0)
                    {
                        var noOfAttributes = hasHeaderRecord ? reader.FieldHeaders.Length : reader.CurrentRecord.Length;

                        if (columnIndexes == null)
                        {
                            columnIndexes = new int[noOfAttributes];
                            for (var j = 0; j < columnIndexes.Length; j++)
                            {
                                columnIndexes[j] = j;
                            }
                        }


                        for (int column = 0; column < columnIndexes.Length; column++)
                        {
                            var columnName = column == classIndex
                                ? "Class"
                                : hasHeaderRecord
                                    ? reader.FieldHeaders[columnIndexes[column]]
                                    : "Column" + column;
                            data.AddAttribute(columnName);
                        }

                        index++;
                    }


                    var row = data.NewRow();
                    var attributes = data.Attributes.ToArray();
                    for (var columnIndex = 0; columnIndex < columnIndexes.Length; columnIndex++)
                    {
                        var columnName = attributes[columnIndex];
                        row[columnName] = reader.GetField(columnIndexes[columnIndex]);
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
