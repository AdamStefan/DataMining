namespace DataMining.DecisionTrees
{
    public class C45Algorithm
    {
        public DecisionTree BuildConditionalTree(ITableData data, TreeOptions options)
        {
            var ret = new C45AlgorithmDataOptimized(TableFixedData.FromTableData(data), options);
            return ret.BuildConditionalTree();
        }

        public DecisionTree BuildConditionalTree(TableFixedData data, TreeOptions options,
            int[] attributes = null)
        {
            var ret = new C45AlgorithmDataOptimized(data, options);
            return ret.BuildConditionalTree(null, attributes);
        }
    }
}
