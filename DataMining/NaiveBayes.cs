using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMining.Distributions;

namespace DataMining
{
    public class NaiveBayesClassifier
    {
        #region Fields

        private TableFixedData _data;
        private IDistribution[,] _distribution;
        private IDistribution _classesProbablityDistribution;
        private bool _laplacianSmoothing;

        #endregion

        #region Instance

        public NaiveBayesClassifier(TableFixedData data)
        {
            _data = data;                        
            var doubleConverter = new DoubleConverter();

            _distribution = new IDistribution[data.ClassesValue.Length, data.Attributes.Length];

            for (int index = 0; index < data.Attributes.Length; index++)
            {
                if (data.Attributes[index] == TableData.ClassAttributeName)
                {
                    var column = data.GetColumn<int>(index);
                    _classesProbablityDistribution = new CategoricalDistribution(column, data.ClassesValue.Length);

                    continue;
                }
                else
                {
                    var isColumnNumeric = data[0, index].IsNumeric();
                    if (isColumnNumeric)
                    {

                        var values = GetDataPerClass<double>(data, index, doubleConverter);
                        for (int classIndex = 0; classIndex < data.ClassesValue.Length; classIndex++)
                        {
                            _distribution[classIndex, index] = new GaussianDistribution(values[classIndex]);
                        }
                    }
                    else
                    {
                        var values = GetDataPerClass<string>(data, index);

                        for (int classIndex = 0; classIndex < data.ClassesValue.Length; classIndex++)
                        {
                            var categoryData = values[classIndex].Select(item => data.GetSymbol(item, index)).ToArray();
                            _distribution[classIndex, index] = new CategoricalDistribution(categoryData);
                        }
                    }
                }

               
            }
        }       

        #endregion


        #region Methods

        private T[][] GetDataPerClass<T>(TableFixedData data, int columnIndex, IValueConverter<T> converter = null)
        {
            var retLists = new List<T>[data.ClassesValue.Length];
            var rows = data.Count;
            T[][] ret = new T[data.ClassesValue.Length][];

            for (int index = 0; index < rows; index++)
            {
                var classValue = data.Class(index);
                if (retLists[classValue] == null)
                {
                    retLists[classValue] = new List<T>();
                }

                if (converter != null)
                {
                    retLists[classValue].Add(converter.Convert(data[index, columnIndex]));
                }
                else
                {
                    retLists[classValue].Add((T)data[index, columnIndex]);
                }

            }

            for (int index = 0; index < data.ClassesValue.Length; index++)
            {
                ret[index] = retLists[index].ToArray();
            }

            return ret;
        }


        #endregion

        public string Compute(IDataRow datarow)
        {
            var probabilities = new double[_data.ClassesValue.Length];
            var attributes = datarow.Attributes.ToArray();
            var doubleConverter = new DoubleConverter();
            var maxProbabilityIndex = 0;

            for (int index = 0; index < probabilities.Length; index++)
            {
                probabilities[index] = 1;

                for (int columnIndex = 0; columnIndex < _data.Attributes.Length; columnIndex++)
                {
                    if (attributes[columnIndex] == TableData.ClassAttributeName)
                    {
                        probabilities[index] = _classesProbablityDistribution.GetLogProbability(index);
                        continue;
                    }

                    var value = datarow[attributes[columnIndex]];
                    if (!value.IsNumeric())
                    {
                        probabilities[index] = probabilities[index] + _distribution[index, columnIndex].GetLogProbability(_data.GetSymbol((string)value, columnIndex));
                    }
                    else
                    {
                        probabilities[index] = probabilities[index] + _distribution[index, columnIndex].GetLogProbability(doubleConverter.Convert(value));
                    }
                }
                if (probabilities[maxProbabilityIndex] < probabilities[index])
                {
                    maxProbabilityIndex = index;
                }
            }
          
            return _data.ClassesValue[maxProbabilityIndex];
            
        }

    }



    //public class DiscreteNaiveBayesClasifier

    public struct DataPoint
    {
        public int ColumnId { get; set; }
        public double Value { get; set; }
    }

    public class Sample
    {
        public DataPoint[] DataPoints { get; set; }
        public int ClassId { get; set; }
    }
     

    public class NaiveBayesClassifierExtended
    {
        private IDistribution[,] _distribution;
        private IDistribution _classesProbablityDistribution;
        private bool _laplacianSmoothing;
        private int _classes;
        private bool[] _isColumnNumeric;

        public NaiveBayesClassifierExtended(Sample[] samples, int classes, bool[] isColumnNumeric)
        {
            //_data = data;
            var attributes = new List<int>();
            var doubleConverter = new DoubleConverter();
            _isColumnNumeric = isColumnNumeric;
            _classes = classes;

            _distribution = new IDistribution[classes, isColumnNumeric.Length];
            _classesProbablityDistribution = new CategoricalDistribution(samples.Select(item => item.ClassId).ToArray(), classes);

            for (int index = 0; index < isColumnNumeric.Length; index++)
            {
                var values = GetDataPerClass(samples, _classes, index);
                if (isColumnNumeric[index])
                {

                    for (int classIndex = 0; classIndex < classes; classIndex++)
                    {
                        if (isColumnNumeric[index])
                        {
                            _distribution[classIndex, index] = new GaussianDistribution(values[classIndex]);
                        }
                        else
                        {
                            _distribution[classIndex, index] = new CategoricalDistribution(values[classIndex].Select(item => Convert.ToInt32(item)).ToArray());
                        }
                    }
                }

            }

        }

        public Double[][] GetDataPerClass(Sample[] samples, int classes, int columnIndex)
        {
            var retLists = new List<double>[classes];
            var dataRet = new double[classes][];

            for (int index = 0; index < samples.Length; index++)
            {
                var sample = samples[index];
                foreach (var datapoint in sample.DataPoints)
                {
                    if (datapoint.ColumnId == columnIndex)
                    {
                        retLists[sample.ClassId].Add(datapoint.Value);
                    }
                }

            }

            for (int index = 0; index < classes; index++)
            {
                dataRet[index] = retLists[index].ToArray();
            }
            return dataRet;

        }

        public int Compute(Sample sample)
        {
            var probabilities = new double[_classes];                        
            var maxProbabilityIndex = 0;

            for (int index = 0; index < probabilities.Length; index++)
            {
                probabilities[index] = _classesProbablityDistribution.GetLogProbability(index);

                foreach (var dataPoint in sample.DataPoints)
                {

                    var value = Convert.ToDouble(dataPoint.Value);

                    probabilities[index] = probabilities[index] + _distribution[index, dataPoint.ColumnId].GetLogProbability(value);

                }
                if (probabilities[maxProbabilityIndex] < probabilities[index])
                {
                    maxProbabilityIndex = index;
                }
            }

            return maxProbabilityIndex;

        }

    }

}

public interface IValueConverter<T>
{
     T Convert(object toConvert);
}

public class DoubleConverter : IValueConverter<double>
{
    public Double Convert(object toConvert)
    {
        return System.Convert.ToDouble(toConvert);
    }
}
