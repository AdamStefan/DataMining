using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CsvHelper;
using CsvHelper.Configuration;
using DataMining;
using DataMining.DecisionTrees;
using Microsoft.Win32;
using Size = System.Drawing.Size;

namespace Presentation.DecisionTrees
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //var img2 = new BitmapImage(new Uri(@"\Output\Data2.jpg", UriKind.Relative));
            //tree.Source = img2;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog

            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Text documents (.csv)|*.csv"
            };

            // Set filter for file extension and default file extension
            // Display OpenFileDialog by calling ShowDialog method

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox

            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                BuildTree(filename);

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


        public void BuildTree(string fileName, string outputDirectory = "Output")
        {
            var data = LoadDataFromfCSV(fileName);
            var decisionTreeName = Path.GetFileNameWithoutExtension(fileName);

            var algorithm = new C45Algorithm();
            var fixedData = TableFixedData.FromTableData(data);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var decisionalTree = algorithm.BuildConditionalTree(fixedData, new TreeOptions());

            var decisionTreeRenderer = new DecisionTreeRenderer();
            var bitmap = decisionTreeRenderer.RenderTree(decisionalTree, new Size(100, 50));
            var decisionTreeFileName = Path.Combine(outputDirectory, string.Format("{0}.jpg", decisionTreeName));
            bitmap.Save(decisionTreeFileName, ImageFormat.Jpeg);

            var img = new BitmapImage(new Uri(decisionTreeFileName, UriKind.RelativeOrAbsolute));

            img.Freeze();
            //img.EndInit();
            //mainGrid.Children.Remove(tree);
            tree.Source = img;
            tree.UpdateLayout();

            //tree = new Image {Source = img};
            //mainGrid.Children.Add(tree);
            //Grid.SetRow(tree,1);
            //Grid.SetColumn(tree, 1);
            txtMessage.Text = decisionalTree.ToPseudocode();
        }

    }
}
