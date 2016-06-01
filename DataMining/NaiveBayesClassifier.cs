using System;
using System.Collections.Generic;
using System.Linq;
using DataMining.Distributions;

namespace DataMining
{
    [Serializable]
    public class NaiveBayesClassifier
    {

        private readonly IDistribution[,] _distribution;

        private readonly IDistribution _classesProbablityDistribution;
        private static ComparerLikelyhood _comparerLikelyhood = new ComparerLikelyhood();

        private readonly int _classes;

        public NaiveBayesClassifier(DataSample[] samples, int classes, ColumnDataType[] columnDataTypes)
        {
            _classes = classes;

            _distribution = new IDistribution[classes, columnDataTypes.Length];

            _classesProbablityDistribution = new CategoricalDistribution(
                samples.Select(item => item.ClassId).ToArray(), classes);
            var splitDataPerClass = SplitDataPerClass(samples, _classes, columnDataTypes.Length);

            var groups = GetClassGroups(samples, _classes);

            for (int index = 0; index < columnDataTypes.Length; index++)
            {
                //var values = GetDataPerClass(samples, _classes, index);
                Double[][] values = new double[classes][];
                for (int classIndex = 0; classIndex < classes; classIndex++)
                {
                    values[classIndex] = splitDataPerClass[index, classIndex];
                }
                //var values = splitDataPerClass[index,_]
                if (values.All(item => item == null))
                {
                    continue;
                }

                for (int classIndex = 0; classIndex < classes; classIndex++)
                {
                    var itemsOnClass = values[classIndex] ?? new double[0];

                    if (!columnDataTypes[index].IsDiscrete)
                    {
                        _distribution[classIndex, index] = new GaussianDistribution(itemsOnClass);
                    }
                    else
                    {

                        _distribution[classIndex, index] =
                            new CategoricalDistribution(itemsOnClass.Select(Convert.ToInt32).ToArray(),
                                columnDataTypes[index].NumberOfCategories, groups[classIndex]);
                    }
                }
            }
        }

        public NaiveBayesClassifier(IDistribution[,] distribution, IDistribution classDistribution)
        {
            _classes = distribution.GetLength(0);

            _distribution = distribution;
            _classesProbablityDistribution = classDistribution;

        }

        public Double[,][] SplitDataPerClass(DataSample[] samples, int classes, int columns)
        {
            var retLists = new List<Double>[columns, classes];
            var dataRet = new double[columns, classes][];


            for (int index = 0; index < samples.Length; index++)
            {
                var sample = samples[index];
                foreach (var datapoint in sample.DataPoints)
                {

                    if (retLists[datapoint.ColumnId, sample.ClassId] == null)
                    {
                        retLists[datapoint.ColumnId, sample.ClassId] = new List<double>();
                    }

                    retLists[datapoint.ColumnId, sample.ClassId].Add(datapoint.Value);

                }
            }
            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                for (int index = 0; index < classes; index++)
                {
                    if (retLists[columnIndex, index] != null)
                    {
                        dataRet[columnIndex, index] = retLists[columnIndex, index].ToArray();
                    }
                }
            }
            return dataRet;
        }

        public Double[][] GetDataPerClass(DataSample[] samples, int classes, int columnId)
        {
            var retLists = new List<double>[classes];
            var dataRet = new double[classes][];

            for (int index = 0; index < samples.Length; index++)
            {
                var sample = samples[index];
                foreach (var datapoint in sample.DataPoints)
                {
                    if (datapoint.ColumnId == columnId)
                    {
                        if (retLists[sample.ClassId] == null)
                        {
                            retLists[sample.ClassId] = new List<double>();
                        }

                        retLists[sample.ClassId].Add(datapoint.Value);
                    }
                }
            }

            for (int index = 0; index < classes; index++)
            {
                if (retLists[index] != null)
                {
                    dataRet[index] = retLists[index].ToArray();
                }
            }
            return dataRet;

        }

        public int[] GetClassGroups(DataSample[] samples, int classes)
        {
            var ret = new int[classes];

            for (int index = 0; index < samples.Length; index++)
            {
                ret[samples[index].ClassId] ++;
            }

            return ret;
        }

        public int Compute(DataSample sample)
        {
            var probabilities = GetLikelyhood(sample);

            var maxProbabilityIndex = 0;

            for (int index = 0; index < probabilities.Length; index++)
            {
                if (probabilities[maxProbabilityIndex] < probabilities[index])
                {
                    maxProbabilityIndex = index;
                }
            }

            return maxProbabilityIndex;

        }

        public Double[] GetLikelyhood(DataSample sample)
        {
            var probabilities = new double[_classes];

            //for (int index = 0; index < probabilities.Length; index++)
            //{
            //    probabilities[index] = _classesProbablityDistribution.GetLogProbability(index);

            //    foreach (var dataPoint in sample.DataPoints)
            //    {
            //        var value = Convert.ToDouble(dataPoint.Value);

            //        probabilities[index] = probabilities[index] +
            //                               _distribution[index, dataPoint.ColumnId].GetLogProbability(value);
            //    }
            //}
            GetLikelyhood(sample, probabilities);

            return probabilities;
        }

        public void GetLikelyhood(DataSample sample, double[] result)
        {
            var probabilities = result;

            for (int index = 0; index < probabilities.Length; index++)
            {
                probabilities[index] = _classesProbablityDistribution.GetLogProbability(index);

                foreach (var dataPoint in sample.DataPoints)
                {
                    var value = Convert.ToDouble(dataPoint.Value);

                    probabilities[index] = probabilities[index] +
                                           _distribution[index, dataPoint.ColumnId].GetLogProbability(value);
                }
            }
        }

        public struct ClassLikelyhood
        {
            public int ClassId { get; set; }
            public double Value { get; set; }
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

        public void GetLikelyhood(DataSample sample, ClassLikelyhood[] result)
        {
            for (int index = 0; index < _classes; index++)
            {
                //result[index].Value = _classesProbablityDistribution.GetLogProbability(index);
                result[index].ClassId = index;

                foreach (var dataPoint in sample.DataPoints)
                {
                    var value = Convert.ToDouble(dataPoint.Value);

                    result[index].Value = result[index].Value +
                                          _distribution[index, dataPoint.ColumnId].GetLogProbability(value);
                }
            }
            Array.Sort(result, _comparerLikelyhood);
        }

    }
}