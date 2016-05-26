using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using DataMining;

namespace SentimentAnalysis.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private NaiveBayesSentimentAnalysis _naiveBayesSentimentAnalysis = new NaiveBayesSentimentAnalysis();
        private DispatcherTimer _dispatcherTimer;
        private string _message;
        private object _sync = new object();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(2000)};
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            
        }

        void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentMessage = _message;
            if (String.IsNullOrWhiteSpace(currentMessage))
            {
                return;
            }
            var compute = _naiveBayesSentimentAnalysis.Compute(currentMessage);
            var brush = compute == 0 ? Brushes.Red : Brushes.Green;           

            lock (_sync)
            {
                txtMessage.Foreground = brush;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // training set was loaded from http://cs.stanford.edu/people/alecmgo/trainingandtestdata.zip

            //C:\Users\IBM_ADMIN\Downloads\trainingandtestdata
            this.txtNumberOfItems.Text = string.Empty;
            pnlTextBox.IsEnabled = false;
            var data =
                LoadDataFromfCSV(
                    @"C:\Users\StefanAlexandru\Downloads\trainingandtestdata\training.1600000.processed.noemoticon.csv", ",",
                    false, false, new[] { 0, 5 }, 0);

            //var data =
            //    LoadDataFromfCSV(
            //        @"C:\Users\IBM_ADMIN\Downloads\trainingandtestdata\training.1600000.processed.noemoticon.csv", ",",
            //        false, false, new[] { 0, 5 }, 0);

            var count = data.Count;
            var itemsToTrain = data.Select(item => new Tuple<string, string>((string)item["Column1"], item.Class));
        
            _naiveBayesSentimentAnalysis.Train(itemsToTrain, count - 1);

            this.txtNumberOfItems.Text = string.Format("Trained finished on a {0} items", count);

            

            pnlTextBox.IsEnabled = true;
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

        private void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Start();
            _message = txtMessage.Text;
            lock (_sync)
            {
                txtMessage.Foreground = Brushes.Black;
            }
        }

        private double DataTest()
        {
            var dataTest =
                LoadDataFromfCSV(
                    @"C:\Users\StefanAlexandru\Downloads\trainingandtestdata\testdata.manual.2009.06.14.csv", ",",
                    false, false, new[] { 0, 5 }, 0);
            var items = 0;
            var missedItems = 0;
            foreach (var item in dataTest)
            {
                if (item.Class == "4" || item.Class == "0")
                {
                    var value = item.Class == "4" ? 1 : 0;
                    var estimatedValue = _naiveBayesSentimentAnalysis.Compute(item["Column1"].ToString());
                    if (estimatedValue != value)
                    {
                        missedItems++;
                    }
                    items++;
                }

            }
            var percentageResults = (1 - (missedItems / ((double)items))); ;
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Total tests :{0}",items));
            sb.AppendLine(string.Format("Passed :{0}", items - missedItems));
            sb.AppendLine(string.Format("Failed :{0}", missedItems));
            sb.AppendLine(string.Format("Accurracy  :{0}", percentageResults.ToString("P", CultureInfo.InvariantCulture)));
            efficientyPercentage.Text = sb.ToString();

            return percentageResults;
            
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var percentageResults = DataTest();
            
        }

    }
}
