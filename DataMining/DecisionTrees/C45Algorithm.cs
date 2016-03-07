namespace DataMining.DecisionTrees
{
    public class C45Algorithm
    {
        public DecisionTree BuildConditionalTree(ITableData data, bool optimized = false)
        {
            if (optimized)
            {
                var ret = new C45AlgorithmDataOptimized(TableFixedData.FromTableData(data));
                return ret.BuildConditionalTree();
            }

            return new C45AlgorithmData().BuildConditionalTree(data);
        }

        public DecisionTree BuildConditionalTreeOptimized(TableFixedData data)
        {
            var ret = new C45AlgorithmDataOptimized(data);
            return ret.BuildConditionalTree();
        }
    }
}
