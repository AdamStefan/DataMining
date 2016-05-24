using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataMining.DecisionTrees
{
    public class Forest : IEnumerable<DecisionTree>
    {
        #region Fields

        private readonly List<DecisionTree> _decisionTrees;
        private readonly int _classes;

        #endregion

        #region Instance

        public Forest(int classes)
        {
            _classes = classes;
            _decisionTrees = new List<DecisionTree>();
        }

        #endregion

        public double[] Compute(IDataRow dataRow, bool discreteVote = true)
        {
            double[] responses = new double[_classes];                        
            
            object sync = new object();
            Parallel.For((long) 0, _decisionTrees.Count, i =>
            {
                var results = _decisionTrees[(int) i].Compute(dataRow);
                
                lock (sync)
                {
                    if (!discreteVote)
                    {
                        for (int index = 0; index < responses.Length; index++)
                        {
                            responses[index] += results[index];
                        }
                    }
                    else
                    {
                        var maxIndex = results.IndexOfMax();
                        responses[maxIndex]++;
                    }
                }
            });
            

            for (int i = 0; i < responses.Length; i++)
            {
                responses[i] = responses[i]/_decisionTrees.Count;
            }
            return responses;
        }

        public double[][] Compute(IDataRow[] dataRows)
        {
            var ret = new double[dataRows.Length][];

            var index = 0;
            foreach (var row in dataRows)
            {
                ret[index] = Compute(row);
            }

            return ret;
        }

        public int GetClass(IDataRow dataRow)
        {
            var estimates = Compute(dataRow);
            var maxEstimate = 0.0;
            var ret = 0;
            for (int i = 0; i < estimates.Length; i++)
            {
                if (estimates[i] > maxEstimate)
                {
                    maxEstimate = estimates[i];
                    ret = i;
                }
            }

            return ret;
        }

        #region Methods

        public void Add(DecisionTree decisionTree)
        {
            _decisionTrees.Add(decisionTree);
        }

        #region IEnumerable

        public IEnumerator<DecisionTree> GetEnumerator()
        {
            return _decisionTrees.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _decisionTrees.GetEnumerator();
        }

        #endregion

        #endregion

    }
}