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

        public double[] Compute(IDataRow dataRow)
        {
            double[] responses = new double[_classes];
            object sync = new object();
            Parallel.For((long)0, _decisionTrees.Count, i =>
            {
                var results = _decisionTrees[(int)i].Compute(dataRow);
                lock (sync)
                {
                    for (int index = 0; index < responses.Length; index++)
                    {
                        responses[index] += results[index];
                    }
                }

            });


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