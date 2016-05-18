namespace DataMining.DecisionTrees
{
    public class RandomForestAlgorithm
    {
        #region Fields

        private int _trees;
        private int _classes;
        private double? _coverageRatio;
        private double _sampleRatio;

        #endregion


        #region Instance

        public RandomForestAlgorithm(int trees, int classes, double? sampleRatio = null, double? coverageRatio = null)
        {
            _trees = trees;
            _classes = classes;

            if (sampleRatio != null)
            {
                _sampleRatio = sampleRatio.Value;
            }

            if (coverageRatio != null)
            {
                _coverageRatio = coverageRatio.Value;
            }
        }

        #endregion

        #region Methods

        public Forest BuildForest(TableFixedData data, TreeOptions options = null)
        {
            return null;

        }

        #endregion
    }
}
