using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using com.sun.org.apache.bcel.@internal.generic;
using DataMining;
using DataMining.Distributions;
using edu.stanford.nlp.patterns;
using Data = javax.xml.crypto.Data;

namespace TestApplication
{
    public class ExpediaReader : IDataPointsReader
    {
        private int _count;
        private bool _loaded = false;

        private Dictionary<int, int>[] _symbols;
        private int[] _maximumValues = new int[24];
        private string _trainPath = @"C:\Working Projects\Kaggle\Expedia\train.csv";
        private string _testPath = @"C:\Working Projects\Kaggle\Expedia\test.csv";
        private int _numberOfLines = 0;
        //private double[,][] _probabilities;
        //private double[] _classesValues;

        private  IDistribution[,] _distribution;
        private  IDistribution _classesProbablityDistribution;
        private NaiveBayesClassifier _classifier;
        //private  List<int>[,] _nGramProbabilities;

        private void Init()
        {
            _maximumValues[0] = 365;
            _maximumValues[1] = 53;
            _maximumValues[2] = 4;
            _maximumValues[3] = 239;
            _maximumValues[4] = 1027;
            _maximumValues[5] = 56508;
            _maximumValues[6] = 12408;
            _maximumValues[7] = 1198785;
            _maximumValues[8] = 1;
            _maximumValues[9] = 1;
            _maximumValues[10] = 10;
            _maximumValues[11] = 365;
            _maximumValues[12] = 365;
            _maximumValues[13] = 9;
            _maximumValues[14] = 9;
            _maximumValues[15] = 8;
            _maximumValues[16] = 65107;
            _maximumValues[17] = 9;
            _maximumValues[18] = 1;
            _maximumValues[19] = 269;
            _maximumValues[20] = 7;
            _maximumValues[21] = 212;
            _maximumValues[22] = 2117;
            _maximumValues[23] = 99;

            _numberOfLines = 37670293;
            		        //2147483647	
                            //1198785

                              

            //if (!_loaded)
            //{
            //    using (var textString = new StreamReader(@"C:\Research\Kaggle\Expedia\trainData\train.csv",
            //        System.Text.Encoding.ASCII, false, 100000000))
            //    {
            //        var line = textString.ReadLine();
            //        while (!textString.EndOfStream)
            //        {
            //            _numberOfLines++;
            //            line = textString.ReadLine();
            //            var columms = line.Split(",".ToCharArray());
            //            for (var index = 0; index < 24; index++)
            //            {
            //                var value = GetDataFromLine(columms,index);
            //                if (value.HasValue && value.Value > _maximumValues[index])
            //                {
            //                    _maximumValues[index] = value.Value;
            //                }
                            
            //            }

            //            //var columms = line.Split(",".ToCharArray());
            //            //var dateTime = DateTime.Parse(columms[0]);
            //            //var siteName = int.Parse(columms[1]);
            //            //var posa_continent = int.Parse(columms[2]);
            //            //var user_location_country = int.Parse(columms[3]);
            //            //var user_location_region = int.Parse(columms[4]);
            //            //var user_location_city = int.Parse(columms[5]);
            //            //var orig_destination_distance = int.Parse(columms[6]);
            //            //var user_id = int.Parse(columms[7]);
            //            //var is_mobile = int.Parse(columms[8]);
            //            //var is_package = int.Parse(columms[9]);
            //            //var channel = int.Parse(columms[10]);
            //            //var srch_ci = int.Parse(columms[11]);
            //            //var srch_co = int.Parse(columms[12]);
            //            //var srch_adults_cnt = int.Parse(columms[13]);
            //            //var srch_children_cnt = int.Parse(columms[14]);
            //            //var srch_rm_cnt = int.Parse(columms[15]);
            //            //var srch_destination_id = int.Parse(columms[16]);
            //            //var srch_destination_type_id = int.Parse(columms[17]);
            //            //var hotel_continent = int.Parse(columms[18]);
            //            //var hotel_country = int.Parse(columms[19]);
            //            //var hotel_market = int.Parse(columms[20]);
            //            //var is_booking = int.Parse(columms[21]);
            //            //var cnt = int.Parse(columms[22]);
            //            //var classId = int.Parse(columms[23]);
            //            //if (!hashSet.Contains(classId))
            //            //{
            //            //    hashSet.Add(classId);
            //            //}
            //        }
            //    }

            //    _loaded = true;
            //}
            _loaded = true;
            Classes = _maximumValues[23] + 1;
        }

        [Serializable]
        public class Context
        {
            
            public Dictionary<int, int>[] Symbols { get; set; }
            
            public NaiveBayesClassifier NaiveBayesClassifier { get; set; }

        }

        private void LoadProbabilities(List<int[]> nGrams = null)
        {
            if (nGrams == null)
            {
                nGrams = new List<int[]>();
            }
            var totalColumns = 23 + nGrams.Count();

            var probabilities = new double[Classes, totalColumns][];
            var nGramProbabilities = new List<int>[Classes, nGrams.Count()];
            var classesValues = new double[Classes];
            _symbols = new Dictionary<int, int>[nGrams.Count];

            _distribution = new IDistribution[Classes, totalColumns];

            for (int index = 0; index < nGrams.Count; index++)
            {

                _symbols[index] = new Dictionary<int, int>();
                for (int jindex = 0; jindex < Classes; jindex++)
                {
                    nGramProbabilities[jindex, index] = new List<int>();
                }

            }
           

            DateTime dt = DateTime.Now;
            //var buffer = 100000000;
            var buffer =    5000000;
            var dataBuffer = new int?[23];

            using (
                var textString = new StreamReader(_trainPath,
                    System.Text.Encoding.ASCII, false, buffer))
            {
                var line = textString.ReadLine();
                while (!textString.EndOfStream)
                {

                    line = textString.ReadLine();
                    var columms = line.Split(",".ToCharArray());
                    var classVal = GetDataFromLine(columms, 23).Value;
                    classesValues[classVal]++;
                    for (var index = 0; index < 23; index++)
                    {
                        if (probabilities[classVal, index] == null)
                        {
                            if (index != 6)
                            {
                                probabilities[classVal, index] = new double[_maximumValues[index] + 1];
                            }
                            else
                            {
                                probabilities[classVal, index] = new double[3];
                            }
                        }

                        dataBuffer[index] = GetDataFromLine(columms, index);
                        if (dataBuffer[index].HasValue)
                        {
                            var value = dataBuffer[index].Value;
                            if (index == 6)
                            {
                                var n = probabilities[classVal, index][0];
                                var oldMean = probabilities[classVal, index][1];
                                var newMean = (probabilities[classVal, index][1] * (n / (n + 1))) + (value / (n + 1));
                                var oldVar = probabilities[classVal, index][2];
                                var newVar = ((n / (n + 1)) * (oldVar + (oldMean * oldMean)) +
                                              ((value * value) / (n + 1))) - (newMean * newMean);

                                probabilities[classVal, index][0] = n + 1;
                                probabilities[classVal, index][1] = newMean;
                                probabilities[classVal, index][2] = newVar;
                            }
                            else
                            {
                                probabilities[classVal, index][value]++;
                            }
                        }
                    }
                    var newColumndIndex = 0;

                    foreach (var nGram in nGrams)
                    {
                        var code = GetDataFromLine(dataBuffer, nGram);
                       
                        var dict = _symbols[newColumndIndex];                       

                        int value;
                        if (!dict.TryGetValue(code, out value))
                        {
                            dict.Add(code, dict.Count);
                            
                            nGramProbabilities[classVal, newColumndIndex].Add(dict.Count - 1);
                        }
                        else
                        {
                            nGramProbabilities[classVal, newColumndIndex].Add(value);
                        }


                        newColumndIndex ++;
                    }

                }
            }

            var ts = DateTime.Now.Subtract(dt);
            Console.WriteLine(ts.TotalMinutes);
            GC.Collect();
            for (int index = 0; index < nGramProbabilities.GetLength(0); index++)
            {
                for (int jindex = 0; jindex < nGramProbabilities.GetLength(1); jindex++)
                {
                    var dict = _symbols[jindex];
                    probabilities[index, 23 + jindex] = new double[dict.Count + 1];
                    foreach (var value in nGramProbabilities[index, jindex])
                    {
                        probabilities[index, 23 + jindex][value]++;
                    }
                }
            }

            for (int index = 0; index < probabilities.GetLength(0); index++)
            {
                for (int jindex = 0; jindex < probabilities.GetLength(1); jindex++)
                {
                    if (jindex != 6)
                    {
                        _distribution[index, jindex] = new CategoricalDistribution(probabilities[index, jindex]);
                    }
                    else
                    {
                        var n = probabilities[index, jindex][0];
                        var mean = probabilities[index, jindex][1];
                        var var = probabilities[index, jindex][2];
                        var std = Math.Sqrt(var);
                        _distribution[index, jindex] = new GaussianDistribution(mean, std);
                    }
                }
            }

            _classesProbablityDistribution = new CategoricalDistribution(classesValues);

            _classifier = new NaiveBayesClassifier(_distribution, _classesProbablityDistribution);
            Serialize();
        }



        public void Serialize()
        {
            var context = new Context() {Symbols = _symbols, NaiveBayesClassifier = _classifier};
            FileStream fs = new FileStream("DataFile.dat", FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, context);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public void Derialize()
        {
            
            // Open the file containing the data that you want to deserialize.
            FileStream fs = new FileStream("DataFile.dat", FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Deserialize the hashtable from the file and 
                // assign the reference to the local variable.
                var context = (Context)formatter.Deserialize(fs);
                this._classifier = context.NaiveBayesClassifier;
                this._symbols = context.Symbols;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }


        public ExpediaReader()
        {
            Init();

            var ngrams = new List<int[]>();
            ngrams.Add(new[] {3, 16, 18, 19});
            ngrams.Add(new[] {3, 16, 18, 19, 20});
            ngrams.Add(new[] {4, 16, 18, 19});
            ngrams.Add(new[] {5, 16, 18, 19});
            ngrams.Add(new[] {7, 16});
            ngrams.Add(new[] {7, 20});
            LoadProbabilities(ngrams);
            Estimate();
          
        }

        public void Estimate()
        {
            var ngrams = new List<int[]>();
            ngrams.Add(new[] {3, 16, 18, 19});
            ngrams.Add(new[] {3, 16, 18, 19, 20});
            ngrams.Add(new[] {4, 16, 18, 19});
            ngrams.Add(new[] {5, 16, 18, 19});
            ngrams.Add(new[] {7, 16});
            ngrams.Add(new[] {7, 20});
            var dataSamples = GetDataSamples(Enumerable.Range(0, 23), ngrams);
            var index =0;
            using (var sw = new StreamWriter(string.Format("Submission_{0}", DateTime.Now.ToString())))
            {
                sw.WriteLine("id,hotel_cluster");
                foreach (var sample in dataSamples)
                {
                    var result = _classifier.GetLikelyhood(sample);
                    result.OrderBy(item => item).Take(5).ToArray();
                    var data = index + "," + result[0] + " " + result[1] + " " + result[2] + " " + result[3] + " " + result[4];
                    sw.WriteLine(data);                    
                }
            }
            
        }


        private int[] ReadTrain(int columnId)
        {
            var list = new List<int>();

            using (var textString = new StreamReader(_trainPath,
                    System.Text.Encoding.ASCII, false, 1024 * 1024 * 300))
            {
                textString.ReadLine();
                while (!textString.EndOfStream)
                {

                    var line = textString.ReadLine();
                    var columms = line.Split(",".ToCharArray());
                    var ret = GetDataFromLine(columms, columnId);
                    if (ret.HasValue)
                    {
                        list.Add(ret.Value);
                    }
                }
            }

            return list.ToArray();
        }

        private int[][] ReadTrainPerClass(int composedColumn)
        {                       
            var list = new List<int>[Classes];

            using (var textString = new StreamReader(_trainPath,
                    System.Text.Encoding.ASCII, false, 1024 * 1024 * 300))
            {
                var line = textString.ReadLine();
                while (!textString.EndOfStream)
                {
                    line = textString.ReadLine();
                    var columms = line.Split(",".ToCharArray());
                    int classValue = int.Parse(columms[23]);
                    if (list[classValue] == null)
                    {
                        list[classValue] = new List<int>();
                    }

                    var code = GetDataFromLine(columms, composedColumn);
                    if (code.HasValue)
                    {
                        list[classValue].Add(code.Value);
                    }

                }
            }
            var ret = new int[Classes][];

            for (int i = 0; i < Classes; i++)
            {
                ret[i] = list[i].ToArray();
                list[i] = null;
            }
            return ret;
        }

        //private int[][] ReadTrainPerClass(int[] composedColumn)
        //{
        //    string tmp = composedColumn.Aggregate(string.Empty, (current, value) => current + ("_" + value));
        //    var columnIdentifier = tmp.GetHashCode();
        //    if (_symbols.ContainsKey(columnIdentifier))
        //    {
        //        _symbols.Add(columnIdentifier, new Dictionary<int, int>());
        //    }

        //    var dict = _symbols[columnIdentifier];
        //    var lastItemValue = dict.Count;

        //    var list = new List<int>[Classes];

        //    using (var textString = new StreamReader(@"C:\Research\Kaggle\Expedia\trainData\train.csv",
        //            System.Text.Encoding.ASCII, false, 1024 * 1024 * 300))
        //    {
        //        var line = textString.ReadLine();
        //        while (!textString.EndOfStream)
        //        {
        //            line = textString.ReadLine();
        //            var columms = line.Split(",".ToCharArray());
        //            int classValue = int.Parse(columms[23]);
        //            if (list[classValue] == null)
        //            {
        //                list[classValue] = new List<int>();
        //            }

        //            var code = GetDataFromLine(columms, composedColumn);
        //            int value;
        //            if (!dict.TryGetValue(code, out value))
        //            {
        //                dict.Add(code, lastItemValue);
        //                list[classValue].Add(lastItemValue);
        //            }
        //            else
        //            {
        //                list[classValue].Add(value);
        //            }
        //        }
        //    }
        //    var ret = new int[Classes][];

        //    for (int i = 0; i < Classes; i++)
        //    {
        //        ret[i] = list[i].ToArray();
        //        list[i] = null;
        //    }
        //    return ret;
        //}

        private int? GetDataFromLine(string[] line, int columnIndex)
        {
            if (columnIndex <= 23)
            {
                var data = line[columnIndex];

                if (columnIndex == 0 || columnIndex == 11 || columnIndex == 12)
                {
                    DateTime date;
                    if (DateTime.TryParse(data, out date))
                    {
                        return date.DayOfYear;
                    }
                }
                else if (columnIndex == 6)
                {
                    double value;
                    if (Double.TryParse(data, out value))
                    {
                        return Convert.ToInt32(value);
                    }
                }
                else
                {
                    int value;
                    if (int.TryParse(data, out value))
                    {
                        return value;
                    }
                }
            }
            return null;
        }

        private int GetDataFromLine(string[] line, int[] columnIndexex)
        {            
            var maxValue = 2000000;
            var codeToRet = 0;               
            foreach (var column in columnIndexex)
            {
                var ret = GetDataFromLine(line, column);
                                
                if (ret.HasValue)
                {
                    codeToRet += maxValue + ret.Value;                    
                }
                maxValue = maxValue << 1;
            }
            return codeToRet;
        }

        private int GetDataFromLine(int?[] values, int[] columnIndexex)
        {
            var maxValue = 2000000;
            var codeToRet = 0;
            foreach (var column in columnIndexex)
            {
                var ret = values[column];

                if (ret.HasValue)
                {
                    codeToRet += maxValue + ret.Value;
                }
                maxValue = maxValue << 1;
            }
            return codeToRet;
        }

        

        public int Classes { get; set; }
        public int[] GetClassses()
        {
            return ReadTrain(23);
        }

        public int[][] GetDataPerClass(int columnId)
        {
            return ReadTrainPerClass(columnId);
        }

        public IEnumerable<DataSample> GetDataSamples(IEnumerable<int> fields, List<int[]> nGrams = null)
        {
            var listSamples = new List<DataSample>();
            using (
                var textString = new StreamReader(_testPath,
                    System.Text.Encoding.ASCII, false, 100000000))
            {
                var line = textString.ReadLine();
                while (!textString.EndOfStream)
                {
                    line = textString.ReadLine();
                    var columns = line.Split(",".ToCharArray());
                    var dataSample = new DataSample();
                    var dataPoints = new List<DataPoint>();
                    foreach (var field in fields)
                    {
                        var data = GetDataFromLine(columns, field + 1);
                        if (data.HasValue)
                        {
                            dataPoints.Add(new DataPoint() { ColumnId = field, Value = data.Value });
                        }
                    }
                    var columnIndex = 0;
                    foreach (var field in nGrams)
                    {
                        var data = GetDataFromLine(columns, field);
                        var dict = _symbols[columnIndex];
                        if (dict.ContainsKey(data))
                        {
                            dataPoints.Add(new DataPoint() { ColumnId = columnIndex + 23, Value = dict[data] });
                        }
                        columnIndex++;
                    }
                    dataSample.DataPoints = dataPoints.ToArray();
                    listSamples.Add(dataSample);

                }
            }

            return listSamples;
        }

        //public int[][] GetDataPerClass(int[] columns)
        //{
        //    return ReadTrainPerClass(columns);
        //}
        
    }
}
