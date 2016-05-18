namespace DataMining.DecisionTrees
{
    public class C45Algorithm
    {
        public DecisionTree BuildConditionalTree(ITableData data, TreeOptions options, bool optimized = false)
        {
            if (optimized)
            {
                var ret = new C45AlgorithmDataOptimized(TableFixedData.FromTableData(data), options);
                return ret.BuildConditionalTree();
            }

            return new C45AlgorithmData().BuildConditionalTree(data);
        }

        public DecisionTree BuildConditionalTreeOptimized(TableFixedData data, TreeOptions options)
        {
            var ret = new C45AlgorithmDataOptimized(data, options);
            return ret.BuildConditionalTree();
        }
    }
}
