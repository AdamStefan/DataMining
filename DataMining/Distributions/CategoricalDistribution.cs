using System;
using System.Linq;

namespace DataMining.Distributions
{
    /// <summary>
    /// Also known as Multinomial distribution
    /// </summary>
    public class CategoricalDistribution : IDistribution
    {
        #region Fields

        private readonly double[] _probabilities;

        #endregion

        #region Properties

        public double Expectation { get; private set; }

        #endregion

        #region Instance

        public CategoricalDistribution(double[] weights)
        {
            var sum = weights.Sum();
            _probabilities = weights.Select(item => item/sum).ToArray();

            for (int i = 0; i < _probabilities.Length; i++)
            {
                Expectation += (i + 1)*_probabilities[i];
            }
        }


        public CategoricalDistribution(int[] values)
            : this(values, values.Max() + 1)
        {
        }

        public CategoricalDistribution(int[] values, int categories)
        {
            var freq = new int[categories];
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
            var count = (double) values.Length;
            _probabilities = freq.Select(item => item/count).ToArray();
            for (int i = 0; i < _probabilities.Length; i++)
            {
                Expectation += (i+1)*_probabilities[i];
            }
        }
        
        #endregion

        #region Methods

        public double GetLogProbability(double value)
        {
            if (value >= _probabilities.Length)
            {
                return  Double.MinValue;
            }
            return Math.Log( _probabilities[(int) value]);
        }

        public double GetExpectation()
        {
            return Expectation;
        }

        #endregion
    }
}