using System;
using System.Linq;

namespace DataMining.Distributions
{
    /// <summary>
    /// Also known as Multinomial distribution
    /// </summary>
    [Serializable]
    public class CategoricalDistribution : IDistribution
    {
        #region Fields

        private readonly double[] _probabilities;
        private double? _expectation;

        #endregion

        #region Properties

        public double Expectation
        {
            get
            {
                if (!_expectation.HasValue)
                {
                    var exp = 0.0;
                    for (int i = 0; i < _probabilities.Length; i++)
                    {
                        exp += (i + 1) * _probabilities[i];
                    }
                    _expectation = exp;
                }
                return _expectation.Value;
            }

        }

        #endregion

        #region Instance

        public CategoricalDistribution(double[] weights)
        {
            var sum = weights.Sum();
            _probabilities = weights;
            for (int index = 0; index < weights.Length; index++)
            {
                _probabilities[index] = _probabilities[index]/sum;
            }            

            //for (int i = 0; i < _probabilities.Length; i++)
            //{
            //    Expectation += (i + 1)*_probabilities[i];
            //}
        }
        

        public CategoricalDistribution(int[] values, int totalNumberOfItems)
            : this(values, values.Any() ? values.Max() + 1 : 0, totalNumberOfItems)
        {
        }

        public CategoricalDistribution(int[] values, int categories, int totalNumberOfItems)
        {
            var freq = new int[categories + 1];            

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] >= categories)
                {
                    throw new ArgumentException("Categories number should be higher then item values");
                }
                if (values[i] < 0)
                {
                    throw new ArgumentException("Item values should be non-negative");
                }
                freq[values[i]]++;
            }

            freq[categories] = totalNumberOfItems - values.Length;

            var count = (double) totalNumberOfItems;

            _probabilities = freq.Select(item => item/count).ToArray();
            //for (int i = 0; i < _probabilities.Length; i++)
            //{
            //    Expectation += (i + 1)*_probabilities[i];
            //}
        }

        #endregion

        #region Methods

        public double GetLogProbability(double value)
        {
            if (value >= _probabilities.Length)
            {
                //return Math.Log(1.0/3767020);
                return Double.MinValue;
            }
            //var probability = Math.Abs(_probabilities[(int) value]) > 2*Double.Epsilon
            //    ? _probabilities[(int) value]
            //    : (1.0/3767020);
            //return Math.Log(probability);
            return Math.Log(_probabilities[(int)value]);
        }

        public double GetExpectation()
        {
            return Expectation;
        }

        #endregion


        public double GetProbability(double value)
        {
            if (value >= _probabilities.Length)
            {
                //return Math.Log(1.0/3767020);
                return 0;
            }
            //var probability = Math.Abs(_probabilities[(int) value]) > 2*Double.Epsilon
            //    ? _probabilities[(int) value]
            //    : (1.0/3767020);
            //return Math.Log(probability);
            return _probabilities[(int)value];
        }
    }
}