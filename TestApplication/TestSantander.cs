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
            var data = Program.LoadDataFromfCSV(@"C:\Work\NLP\train.csv",",");

            //var data =
            //    LoadDataFromfCSV(
            //        @"C:\Users\IBM_ADMIN\Downloads\trainingandtestdata\training.1600000.processed.noemoticon.csv", ",",
            //        false, false, new[] { 0, 5 }, 0);
            var algorithm = new C45Algorithm();
            var fixedData = TableFixedData.FromTableData(data);
            var decisionalTree = algorithm.BuildConditionalTreeOptimized(fixedData);
            var pseudocode = decisionalTree.ToPseudocode();


            var testData = Program.LoadDataFromfCSV(@"C:\Work\NLP\test.csv", ",");
            foreach (var testItem in testData)
            {
             var classD =   decisionalTree.GetClass(testItem);
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
