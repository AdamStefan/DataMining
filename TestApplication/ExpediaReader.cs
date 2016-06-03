using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DataMining;
using DataMining.Distributions;

namespace TestApplication
{
    public class ExpediaReader
    {
        private int _count;
        private bool _loaded = false;

        private Dictionary<int, int>[] _symbols;

        #region Fields

        private int date_time = 0;
        private int site_name = 1;
        private int posa_continent = 2;
        private int user_location_country = 3;
        private int user_location_region = 4;
        private int user_location_city = 5;
        private int orig_destination_distance = 6;
        private int user_id = 7;
        private int is_mobile = 8;
        private int is_package = 9;
        private int channel = 10;
        private int srch_ci = 11;
        private int srch_co = 12;
        private int srch_adults_cnt = 13;
        private int srch_children_cnt = 14;
        private int srch_rm_cnt = 15;
        private int srch_destination_id = 16;
        private int srch_destination_type_id = 17;
        private int is_booking = 18;
        private int cnt = 19;
        private int hotel_continent = 20;
        private int hotel_country = 21;
        private int hotel_market = 22;

        private int hotel_cluster = 23;
        private int[] testMappings = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, -1, -1, 19, 20, 21 };



        private int[] _maximumValues = new int[24];

        #endregion

        private string _trainPath = @"C:\Working Projects\Kaggle\Expedia\train.csv";
        private string _testPath = @"C:\Working Projects\Kaggle\Expedia\test.csv";
        private int _numberOfLines = 0;


        private static ComparerLikelyhood _comparerLikelyhood = new ComparerLikelyhood();

        private IDistribution[,] _distribution;
        private IDistribution _classesProbablityDistribution;

        private void Init()
        {
            _maximumValues[0] = 12;
            _maximumValues[1] = 53;
            _maximumValues[2] = 4;
            _maximumValues[3] = 239;
            _maximumValues[4] = 1027;
            _maximumValues[5] = 56508;
            //_maximumValues[6] = 12408/100;
            _maximumValues[6] = 12408;
            _maximumValues[7] = 1198785;
            _maximumValues[8] = 1;
            _maximumValues[9] = 1;
            _maximumValues[10] = 10;
            _maximumValues[11] = 12;
            _maximumValues[12] = 12;
            _maximumValues[13] = 9;
            _maximumValues[14] = 9;
            _maximumValues[15] = 8;
            _maximumValues[16] = 65107;
            _maximumValues[17] = 9;
            _maximumValues[18] = 10;
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

            public IDistribution[,] Distribution { get; set; }
            public IDistribution ClassesProbablityDistribution { get; set; }

        }

        private void LoadProbabilities(List<NGram> nGrams = null)
        {
            if (nGrams == null)
            {
                nGrams = new List<NGram>();
            }
            var totalColumns = 23 + nGrams.Count();
            var bookedItems = 0;
            var notbooked = 0;

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
            var buffer = 5000000;
            var dataBuffer = new int?[23];

            using (
                var textString = new StreamReader(_trainPath,
                    Encoding.ASCII, false, buffer))
            {
                var line = textString.ReadLine();
                while (!textString.EndOfStream)
                {

                    line = textString.ReadLine();
                    var columms = line.Split(',');
                    var classVal = GetDataFromLine(columms, 23).Value;
                    var isbooked = int.Parse(columms[is_booking]) == 1;

                    //if (isbooked)
                    //{
                    classesValues[classVal] += 1;
                    //}
                    //else
                    //{
                    //    classesValues[classVal] += 0.05;
                    //}

                    for (var index = 0; index < 21; index++)
                    {
                        dataBuffer[index] = GetDataFromLine(columms, index);
                        //if (dataBuffer[index].HasValue )
                        //{
                        //    var value = dataBuffer[index].Value;
                        //    if (isbooked)
                        //    {
                        //        probabilities[classVal, index][value] += 1;
                        //    }
                        //    else
                        //    {
                        //        probabilities[classVal, index][value] += 0.05;
                        //    }                           
                        //}
                    }
                    var newColumndIndex = 0;

                    foreach (var nGram in nGrams)
                    {
                        if (!isbooked && nGram.IsBookingOnly)
                        {
                            newColumndIndex++;
                            continue;
                        }
                        var code = GetDataFromLine(dataBuffer, nGram.Columns);
                        if (!code.HasValue)
                        {
                            newColumndIndex++;
                            continue;
                        }

                        var dict = _symbols[newColumndIndex];

                        int value;
                        if (!dict.TryGetValue(code.Value, out value))
                        {
                            dict.Add(code.Value, dict.Count);

                            nGramProbabilities[classVal, newColumndIndex].Add(isbooked
                                ? (dict.Count)
                                : -(dict.Count));
                        }
                        else
                        {
                            nGramProbabilities[classVal, newColumndIndex].Add(isbooked ? value + 1 : -(value + 1));
                        }


                        newColumndIndex++;
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
                        var absValue = Math.Abs(value) - 1;
                        probabilities[index, 23 + jindex][absValue] += value >= 0 ? 1 : 0.05;
                    }
                }
            }

            for (int index = 0; index < probabilities.GetLength(0); index++)
            {
                for (int jindex = 23; jindex < probabilities.GetLength(1); jindex++)
                {
                    _distribution[index, jindex] = new CategoricalDistribution(probabilities[index, jindex]);
                }
            }

            _classesProbablityDistribution = new CategoricalDistribution(classesValues);

            Serialize();
        }



        public void Serialize()
        {
            var context = new Context() { Symbols = _symbols, ClassesProbablityDistribution = _classesProbablityDistribution, Distribution = _distribution };
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
                this._distribution = context.Distribution;
                this._classesProbablityDistribution = context.ClassesProbablityDistribution;
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

            var ngrams = new List<NGram>();

            ngrams.Add(new NGram(new[] { user_location_city, orig_destination_distance, srch_children_cnt }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { user_location_city, orig_destination_distance }) { IsBookingOnly = true });
            // is booking only
            ngrams.Add(new NGram(new[] { user_id, user_location_city, srch_destination_id, hotel_country, hotel_market }) { IsBookingOnly = true }); // is booking only
            ngrams.Add(new NGram(new[] { user_id, srch_destination_id, hotel_country, hotel_market })
            {
                IsBookingOnly = true
            }); // is booking only

            ngrams.Add(new NGram(new[] { user_location_city, srch_destination_id, srch_children_cnt }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { srch_destination_id, hotel_country, hotel_market, is_package }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { hotel_market }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { srch_destination_id }));

            LoadProbabilities(ngrams);
            Estimate();

        }

        private class NGram
        {
            public int[] Columns { get; set; }
            public bool IsBookingOnly { get; set; }

            public NGram(int[] columns)
            {
                Columns = columns;

            }
        }

        public void Estimate()
        {
            var ngrams = new List<NGram>();


            ngrams.Add(new NGram(new[] { user_location_city, orig_destination_distance, srch_children_cnt }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { user_location_city, orig_destination_distance }) { IsBookingOnly = true });
            // is booking only
            ngrams.Add(new NGram(new[] { user_id, user_location_city, srch_destination_id, hotel_country, hotel_market }) { IsBookingOnly = true }); // is booking only
            ngrams.Add(new NGram(new[] { user_id, srch_destination_id, hotel_country, hotel_market })
            {
                IsBookingOnly = true
            }); // is booking only

            ngrams.Add(new NGram(new[] { user_location_city, srch_destination_id, srch_children_cnt }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { srch_destination_id, hotel_country, hotel_market, is_package }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { hotel_market }) { IsBookingOnly = true });
            ngrams.Add(new NGram(new[] { srch_destination_id }));


            //var dataSamples = GetDataSamples(Enumerable.Range(0, 20), ngrams);
            var dataSamples = GetDataSamples(Enumerable.Empty<int>(), ngrams);
            var index = 0;


            var collectionPartitioner = Partitioner.Create(0, dataSamples.Count);

            Parallel.ForEach(collectionPartitioner, (range, loopState) =>
            {
                ClassLikelyhood[] resultData = new ClassLikelyhood[2 * Classes];
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    GetLikelyhood(dataSamples[i], resultData);
                    dataSamples[i].Tag = resultData[0].ClassId + " " + resultData[1].ClassId + " " +
                                         resultData[2].ClassId + " " +
                                         resultData[3].ClassId + " " + resultData[4].ClassId;
                }
            });


            ClassLikelyhood[] resData = new ClassLikelyhood[2 * Classes];
            GetLikelyhood(dataSamples[0], resData);
            using (var sw = new StreamWriter(string.Format("Submission_{0}", DateTime.Now.ToString("dd-MM-yy hh-mm"))))
            {
                sw.WriteLine("id,hotel_cluster");
                foreach (var sample in dataSamples)
                {
                    var data = index + "," + sample.Tag;
                    sw.WriteLine(data);
                    index++;
                }
            }

        }


        private int? GetDataFromLine(string[] line, int columnIndex, bool isTest = false)
        {
            if (columnIndex <= 23)
            {
                var data = !isTest ? line[columnIndex] : line[testMappings[columnIndex]];

                if (columnIndex == date_time || columnIndex == srch_ci || columnIndex == srch_co)
                {
                    DateTime date;
                    if (DateTime.TryParse(data, out date))
                    {
                        return date.Month;
                    }
                }
                else if (columnIndex == orig_destination_distance)
                {
                    double value;
                    if (Double.TryParse(data, out value))
                    {
                        return Convert.ToInt32(value);
                        //return value.GetHashCode();
                    }
                }
                else if (columnIndex == srch_children_cnt)
                {
                    int value;
                    if (int.TryParse(data, out value))
                    {
                        if (value > 0)
                        {
                            return 1;
                        }
                        return value;
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

        private int? GetDataFromLine(string[] line, int[] columnIndexex, bool isTest = false)
        {
            var maxValue = 2000000;
            var codeToRet = 0;
            foreach (var column in columnIndexex)
            {
                var ret = GetDataFromLine(line, column, isTest);

                if (ret.HasValue)
                {
                    codeToRet += maxValue + ret.Value;
                }
                else
                {
                    return null;
                }
                maxValue = maxValue << 1;
            }
            return codeToRet;
        }

        private int? GetDataFromLine(int?[] values, int[] columnIndexex, bool isTest = false)
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
                else
                {
                    return null;
                }
                maxValue = maxValue << 1;
            }
            return codeToRet;
        }

        public void GetLikelyhood(DataSample sample, ClassLikelyhood[] result)
        {
            var matched = new int[Classes];
            var currentIindex = 0;


            foreach (var dataPoint in sample.DataPoints)
            {
                var itemsFound = 0;
                for (int index = 0; index < Classes; index++)
                {
                    if (matched[index] != 0)
                    {
                        continue;
                    }

                    var value = Convert.ToDouble(dataPoint.Value);
                    var prob = _distribution[index, dataPoint.ColumnId].GetProbability(value) * _classesProbablityDistribution.GetProbability(index);

                    if (prob > Double.Epsilon)
                    {
                        result[currentIindex + itemsFound].ClassId = index;
                        result[currentIindex + itemsFound].Value = prob;
                        itemsFound++;
                        matched[index]++;
                    }
                }
                if (itemsFound > 0)
                {
                    Array.Sort(result, currentIindex, itemsFound, _comparerLikelyhood);
                    currentIindex = currentIindex + itemsFound;
                }

                if (currentIindex >= 5)
                {
                    return;
                }
            }

            if (currentIindex < 5)
            {
                for (int index = 0; index < Classes; index++)
                {
                    if (matched[index] != 0)
                    {
                        continue;
                    }


                    var prob = _classesProbablityDistribution.GetProbability(index);

                    result[currentIindex + index].ClassId = index;
                    result[currentIindex + index].Value = prob;

                }

                Array.Sort(result, currentIindex, Classes, _comparerLikelyhood);
            }
        }

        public int Classes { get; set; }


        private IList<DataSample> GetDataSamples(IEnumerable<int> fields, List<NGram> nGrams = null)
        {
            var listSamples = new List<DataSample>();
            using (
                var textString = new StreamReader(_testPath,
                    Encoding.ASCII, false, 100000000))
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
                        var data = GetDataFromLine(columns, field, true);
                        if (data.HasValue)
                        {
                            dataPoints.Add(new DataPoint { ColumnId = field, Value = data.Value });
                        }
                    }
                    var columnIndex = 0;
                    foreach (var field in nGrams)
                    {
                        var data = GetDataFromLine(columns, field.Columns, true);
                        if (data.HasValue)
                        {
                            var dict = _symbols[columnIndex];
                            if (dict.ContainsKey(data.Value))
                            {
                                dataPoints.Add(new DataPoint { ColumnId = columnIndex + 23, Value = dict[data.Value] });
                            }
                        }
                        columnIndex++;
                    }
                    dataSample.DataPoints = dataPoints.ToArray();
                    listSamples.Add(dataSample);

                }
            }

            return listSamples;
        }

        private class ComparerLikelyhood : IComparer<ClassLikelyhood>
        {
            public int Compare(ClassLikelyhood x, ClassLikelyhood y)
            {
                if (x.Value > y.Value)
                {
                    return -1;
                }
                else if (x.Value < y.Value)
                {
                    return 1;
                }
                return 0;
            }
        }

        public struct ClassLikelyhood
        {
            public int ClassId { get; set; }
            public double Value { get; set; }
        }

    }
}


