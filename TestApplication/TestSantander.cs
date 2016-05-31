using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using DataMining;
using DataMining.DecisionTrees;

namespace TestApplication
{
    public class TestSantander
    {

        public static void TestData()
        {
            var data = Program.LoadDataFromfCSV(@"C:\Work\NLP\train.csv", ",");

            var fixedData = TableFixedData.FromTableData(data);
            var dt = DateTime.Now;
            var attributes =
                fixedData.Attributes.Select((item, index) => index)
                    .Where(item => !fixedData.IsClassAttribute(item) && fixedData.Attributes[item].ToLower() != "id")
                    .ToArray();

            RandomForestAlgorithm rfa = new RandomForestAlgorithm(70);
            var forest = rfa.BuildForest(fixedData, new TreeOptions {MaxTreeDepth = 10}, attributes);

            //var ret4 = forest.Compute(data[0]);
            //var ret3 = forest.GetClass(data[0]);


            var ts = DateTime.Now.Subtract(dt);
            Console.WriteLine(ts.TotalMilliseconds);
            GC.Collect();
            var testData = Program.LoadDataFromfCSV(@"C:\Work\NLP\test.csv", ",");
            

            var configuration = new CsvConfiguration();
            configuration.Delimiter = "," ;
            configuration.HasExcelSeparator = false;
            configuration.IgnoreQuotes = true;
            configuration.HasHeaderRecord = true;
            configuration.QuoteNoFields = true;


            using (var writer = new StreamWriter("test.csv"))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    foreach (var testItem in testData)
                    {
                        var classId = forest.Compute(testItem);
                        csvWriter.WriteField(classId[0]);
                        csvWriter.NextRecord();                        
                    }
                }
            }
        }

        public static TableData LoadDataFromfCSV(string fileName, string delimeter = null, bool hasHeaderRecord = true,
         bool ignoreQuotes = true, int[] columnIndexes = null, int classIndex = -1)
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
    }
}
