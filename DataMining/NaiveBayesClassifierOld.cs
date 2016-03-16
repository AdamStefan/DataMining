using System.Collections.Generic;
using System.Linq;
using DataMining.Distributions;

namespace DataMining
{
    public class NaiveBayesClassifierOld
    {
        #region Fields

        private TableFixedData _data;
        private IDistribution[,] _distribution;
        private IDistribution _classesProbablityDistribution;
        private bool _laplacianSmoothing;

        #endregion

        #region Instance

        public NaiveBayesClassifierOld(TableFixedData data)
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

        #endregion
    }
}