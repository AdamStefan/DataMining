using System;
using System.Linq;

namespace DataMining.Distributions
{
    [Serializable]
    public class GaussianDistribution : IDistribution
    {

        #region Properties

        public double Expectation { get; private set; }

        public double StandardDeviation { get; private set; }        

        #endregion


        #region Instance
        public GaussianDistribution(double expectation, double standardDeviation)
        {
            Expectation = expectation;
            StandardDeviation = standardDeviation;
        }

        public GaussianDistribution() : this(0, 1)
        {
        }

        public GaussianDistribution(double[] dataSet)
        {                       
            Expectation = dataSet.Average();
            var sum = 0d;

            foreach (var value in dataSet)
            {
                var item = (value - Expectation);
                sum += (item*item);
            }

            StandardDeviation = Math.Sqrt(sum/dataSet.Length);
        }

        public GaussianDistribution(int[] dataSet)
        {
            Expectation = dataSet.Average();
            var sum = 0d;

            foreach (var value in dataSet)
            {
                var item = (value - Expectation);
                sum += (item * item);
            }

            StandardDeviation = Math.Sqrt(sum / dataSet.Length);
        }

        #endregion

        #region Methods

        public double GetLogProbability(double value)
        {
            var coeff = StandardDeviation*Math.Sqrt(2*Math.PI);
            var exponentialValue = -Math.Pow(value - Expectation,2)/2*StandardDeviation*StandardDeviation;
            var retNotLog = (1 / coeff) * Math.Exp(exponentialValue);
            
            var ret = - Math.Log(coeff) + exponentialValue;
            return ret;
        }

        public double GetExpectation()
        {
            return Expectation;
        }

        #endregion


        public double GetProbability(double value)
        {
            var coeff = StandardDeviation * Math.Sqrt(2 * Math.PI);
            var exponentialValue = -Math.Pow(value - Expectation, 2) / 2 * StandardDeviation * StandardDeviation;
            var retNotLog = (1 / coeff) * Math.Exp(exponentialValue);
            return retNotLog;
        }
    }
}
