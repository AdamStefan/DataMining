using System;
using System.Collections.Generic;
using System.Linq;
using DataMining.Distributions;

namespace DataMining
{    
    public class NaiveBayesClassifier
    {
        private readonly IDistribution[,] _distribution;
        private readonly IDistribution _classesProbablityDistribution;        
        private readonly int _classes;        

        public NaiveBayesClassifier(DataSample[] samples, int classes, ColumnDataType[] columnDataTypes)
        {                                 
            _classes = classes;

            _distribution = new IDistribution[classes, columnDataTypes.Length];
            _classesProbablityDistribution = new CategoricalDistribution(samples.Select(item => item.ClassId).ToArray(), classes);

            for (int index = 0; index < columnDataTypes.Length; index++)
            {               
                var values = GetDataPerClass(samples, _classes, index);
                if (values.All(item => item == null))
                {
                    continue;
                }

                for (int classIndex = 0; classIndex < classes; classIndex++)
                {
                    if (columnDataTypes[index] == ColumnDataType.Continuous)
                    {
                        _distribution[classIndex, index] = new GaussianDistribution(values[classIndex]);
                    }
                    else
                    {
                        _distribution[classIndex, index] =
                            new CategoricalDistribution(values[classIndex].Select(Convert.ToInt32).ToArray());
                    }
                }

            }

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

            return probabilities;
        }

    }
}