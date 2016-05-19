using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMining.DecisionTrees
{
    public class C45AlgorithmDataOptimized
    {
        #region Fields

        private readonly TableFixedData _data;
        private readonly TreeOptions _options;

        #endregion

        #region Instance

        public C45AlgorithmDataOptimized(TableFixedData data, TreeOptions options)
        {
            _data = data;
            if (options == null)
            {
                options = new TreeOptions();
            }
            _options = options;
        }

        #endregion


        public Statistics ComputeStatistics(IList<int> dataRowsIndexes)
        {
            var listVal = new List<int>();
            var ret = new Statistics
            {
                Frequencies = new int[_data.ClassesValue.Length],
                DatasetLength = dataRowsIndexes.Count,
                MostFrequentClass = 0
            };

            foreach (var rowIndex in dataRowsIndexes)
            {
                var className = _data.Class(rowIndex);
                if (ret.Frequencies[className] == 0)
                {
                    listVal.Add(className);
                }
                ret.Frequencies[className]++;
            }

            ret.SameClass = listVal.Count == 1;
            var topFrequent = -1;
            var sum = 0.0;
            for (var index = 0; index < listVal.Count; index++)
            {
                if (topFrequent < ret.Frequencies[listVal[index]])
                {
                    ret.MostFrequentClass = listVal[index];
                    topFrequent = ret.Frequencies[listVal[index]];
                }
                var val = ret.Frequencies[listVal[index]]/(double) ret.DatasetLength;
                sum += val*Math.Log(val, 2);
            }

            ret.Confidence = ret.Frequencies[(int) ret.MostFrequentClass]/(double) ret.DatasetLength;
            ret.Entropy = -sum;

            return ret;
        }
        
        private bool IsUnknown(object value)
        {
            return value == null || value == DBNull.Value || value == Type.Missing;
        }

        private ComputeAttributeEntropyResult ComputeAttributeEntropy(int[] dataRowsIndexes, int attributeIndex)
        {
            if (dataRowsIndexes.Length == 0)
            {
                return null;
            }

            return _data[0, attributeIndex].IsNumeric()
                ? ComputeAttributeEntropyNumericBinary(dataRowsIndexes, attributeIndex)
                : ComputeAttributeEntropyNotNumeric(dataRowsIndexes, attributeIndex);
        }


        private ComputeAttributeEntropyResult ComputeAttributeEntropyNumericBinary(int[] dataRowsIndexes,
            int attributeIndex)
        {
            //var rows = dataRowsIndexes.Where(item => !IsUnknown(_data[item, attributeIndex])).OrderBy(item => (double) _data[item, attributeIndex])
            //        .ToArray();

            var rows = new int[dataRowsIndexes.Length];
            Buffer.BlockCopy(dataRowsIndexes, 0, rows, 0, dataRowsIndexes.Length*sizeof (int));
            Array.Sort(rows, new ComparerAttr(_data, attributeIndex));

            var ret = new ComputeAttributeEntropyResult
            {
                IsNumeric = true,
                AttributeIndex = attributeIndex,
                KnownValues = rows.Length
            };

            var minimumAttributeValue = Double.MaxValue;
            int minsplitIndex = 0;

            var freqLeft = new int[_data.ClassesValue.Length];
            var freqRight = new int[_data.ClassesValue.Length];
            var rightListVal = new List<int>();
            var leftListVal = new List<int>();

            var rowsCount = rows.Length;


            foreach (var rowIndex in rows)
            {
                var classVal = _data.Class(rowIndex);
                if (freqRight[classVal] == 0)
                {
                    rightListVal.Add(classVal);
                }
                freqRight[classVal]++;
            }

            var leftCount = 0;
            var rightCount = rows.Length;

            for (int index = 0; index < rowsCount - 1; index++)
            {
                var currentItemClass = _data.Class(rows[index]);

                var currentItemValue = _data[rows[index], attributeIndex];
                var nextItemValue = _data[rows[index + 1], attributeIndex];
                leftCount++;
                rightCount--;

                #region LeftPart calculation

                if (freqLeft[currentItemClass] == 0)
                {
                    leftListVal.Add(currentItemClass);
                }

                freqLeft[currentItemClass]++;

                #endregion

                #region RightPart calculation

                freqRight[currentItemClass]--;

                #endregion

                if (currentItemValue.Equals(nextItemValue))
                {
                    continue;
                }

                var sumLeft = 0.0;
                var sumRight = 0.0;

                foreach (var item in leftListVal)
                {
                    var val = ((double) freqLeft[item])/leftCount;
                    sumLeft += val*Math.Log(val, 2);
                }

                foreach (var item in rightListVal)
                {
                    if (freqRight[item] == 0)
                    {
                        continue;
                    }
                    var val = ((double) freqRight[item])/rightCount;
                    sumRight += val*Math.Log(val, 2);
                }


                var leftValue = (((double) leftCount)/rowsCount)*(-sumLeft);
                var rightValue = (((double) rightCount)/rowsCount)*(-sumRight);

                var currentAttributeValue = leftValue + rightValue;
                if (currentAttributeValue < minimumAttributeValue)
                {
                    minsplitIndex = index;
                    minimumAttributeValue = currentAttributeValue;
                }
            }

            double splitValue;
            _data[rows[minsplitIndex], attributeIndex].TryConvertToNumeric(out splitValue);

            ret.EntropyValue = minimumAttributeValue;
            ret.Subsets = new Lazy<IEnumerable<ComputeAttributeEntropyResult.Subset>>(
                () =>
                    rows.Split(minsplitIndex)
                        .Select(item => new ComputeAttributeEntropyResult.Subset {Rows = item, Value = splitValue}));


            return ret;
        }


        private ComputeAttributeEntropyResult ComputeAttributeEntropyNotNumeric(IList<int> dataRowsIndexes,
            int attributeIndex)
        {
            var freq = new Dictionary<object, IList<int>>();
            var itemCount = 0;

            var ret = new ComputeAttributeEntropyResult {AttributeIndex = attributeIndex, IsNumeric = false};

            foreach (var rowIndex in dataRowsIndexes)
            {
                var currentValue = _data[rowIndex, attributeIndex];
                if (IsUnknown(currentValue))
                {
                    continue;
                }
                if (!freq.ContainsKey(currentValue))
                {
                    freq.Add(currentValue, new List<int>());
                }
                freq[currentValue].Add(rowIndex);
                itemCount++;
            }
            ret.KnownValues = itemCount;

            var sum = 0.0;
            foreach (var item in freq)
            {
                var list = item.Value;
                var statistics = ComputeStatistics(list);
                sum += (((double) list.Count)/itemCount)*statistics.Entropy;
            }

            ret.Subsets =
                new Lazy<IEnumerable<ComputeAttributeEntropyResult.Subset>>(
                    () =>
                        freq.Select(
                            item => new ComputeAttributeEntropyResult.Subset {Rows = item.Value, Value = item.Key}));
            ret.EntropyValue = sum;

            return ret;
        }

        private class ComparerAttr : IComparer<int>
        {
            private readonly TableFixedData _data;
            private readonly int _attributeIndex;

            public ComparerAttr(TableFixedData data, int attributeIndex)
            {
                _data = data;
                _attributeIndex = attributeIndex;
            }

            public int Compare(int x, int y)
            {
                var oXVal = _data[x, _attributeIndex];
                var oYVal = _data[y, _attributeIndex];
                if (oXVal == null || oXVal == DBNull.Value || oXVal == Type.Missing)
                {
                    oXVal = Double.MaxValue;
                }
                if (oYVal == null || oYVal == DBNull.Value || oYVal == Type.Missing)
                {
                    oYVal = Double.MaxValue;
                }
                
                var xVal =  (double) oXVal;
                var yVal = (double) oYVal;
                if (xVal > yVal)
                {
                    return 1;
                }
                else if (xVal < yVal)
                {
                    return -1;
                }
                return 0;
            }
        }

        private Tuple<double, ComputeAttributeEntropyResult, Statistics> ComputeBestGain(int[] dataRowsIndexes,
            IList<int> attributesIndexes)
        {
            var statistics = ComputeStatistics(dataRowsIndexes);
            if (statistics.SameClass)
            {
                return new Tuple<double, ComputeAttributeEntropyResult, Statistics>(double.MaxValue, null, statistics);
            }

            var maxGain = Double.MinValue;
            ComputeAttributeEntropyResult minAttributeEntropyResult = null;
            var collectionPartitioner = Partitioner.Create(0, attributesIndexes.Count);
            
            var locker = new object();
            Parallel.ForEach(collectionPartitioner, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var attribute = attributesIndexes[i];

                    var attributeEntropy = ComputeAttributeEntropy(dataRowsIndexes, attribute);
                    var gain = (statistics.Entropy - attributeEntropy.EntropyValue)*attributeEntropy.KnownValues/
                               dataRowsIndexes.Length;

                    lock (locker)
                    {
                        if (minAttributeEntropyResult == null)
                        {
                            minAttributeEntropyResult = attributeEntropy;
                        }
                        if (gain > maxGain)
                        {
                            maxGain = gain;
                            minAttributeEntropyResult = attributeEntropy;
                        }
                        else if (minAttributeEntropyResult != attributeEntropy)
                        {
                            attributeEntropy.Subsets = null;
                        }
                        else if (Double.IsNegativeInfinity(gain))
                        {
                            minAttributeEntropyResult.InvalidAttributes.Add(attribute);
                        }
                    }
                }
            });
                        
            return new Tuple<double, ComputeAttributeEntropyResult, Statistics>(maxGain, minAttributeEntropyResult,
                statistics);
        }

        private class ComputeAttributeEntropyResult
        {
            public double EntropyValue { get; set; }
            public int AttributeIndex { get; set; }
            public bool IsNumeric { get; set; }

            public Lazy<IEnumerable<Subset>> Subsets { get; set; }
            public int KnownValues { get; set; }
            private readonly IList<int> _invalidAttributes = new List<int>();

            public class Subset
            {
                public object Value { get; set; }
                public IList<int> Rows { get; set; }
            }

            public IList<int> InvalidAttributes
            {
                get { return _invalidAttributes; }
            }

        }

        public DecisionTree BuildConditionalTree(int[] rows = null, IList<int> attributes = null)
        {
            if (rows == null)
            {
                rows = Enumerable.Range(0, _data.Count - 1).ToArray();
            }

            if (rows.Length == 0)
            {
                return null;
            }
            var conditionalTree = new DecisionTree
            {
                Root = new DecisionTree.DecisionNode(),
                Options = _options
            };
            if (attributes == null)
            {
                attributes = new List<int>();

                for (int index = 0; index < _data.Attributes.Length; index++)
                {
                    if (_data.Attributes[index] == TableData.ClassAttributeName)
                    {
                        continue;
                    }
                    attributes.Add(index);
                }
            }
            BuildConditionalNodesRecursive(rows, attributes, conditionalTree.Root);
            //BuildConditionalNodes(listRows, attributes, conditionalTree.Root);
            return conditionalTree;
        }


        private void BuildConditionalNodesRecursive(int[] rows, IList<int> attributesIndexes,
            DecisionTree.DecisionNode parentNode)
        {
            var attributesList = attributesIndexes.ToList();
            if (attributesList.Count == 0)
            {
                parentNode.Statistics = ComputeStatistics(rows);
                return;
            }

            var result = ComputeBestGain(rows, attributesIndexes);

            var statistics = result.Item3;

            parentNode.Statistics = statistics;
            parentNode.Class = _data.ClassesValue[(int) statistics.MostFrequentClass];

            if (statistics.SameClass || (parentNode.Depth == _options.MaxTreeDepth && _options.MaxTreeDepth > 0))
            {
                return;
            }

            var attributeResult = result.Item2;

            var first = true;
            var children = new List<DecisionTree.DecisionNode>();
            attributesList.Remove(attributeResult.AttributeIndex);
            if (attributeResult.InvalidAttributes.Any())
            {
                foreach (var item in attributeResult.InvalidAttributes)
                {
                    attributesList.Remove(item);
                }
            }

            foreach (var subset in attributeResult.Subsets.Value)
            {
                if (subset.Rows.Count < _options.MinItemsOnNode)
                {
                    continue;
                }
                var node = new DecisionTree.DecisionNode
                {
                    Condition = attributeResult.IsNumeric
                        ? first
                            ? DecisionTree.PredicateCondition.LessThanOrEqual
                            : DecisionTree.PredicateCondition.GreaterThan
                        : DecisionTree.PredicateCondition.Equal,
                    ThreshHold = subset.Value,
                    Attribute = _data.Attributes[attributeResult.AttributeIndex],
                    Depth = parentNode.Depth + 1,
                    Parent = parentNode
                };

                BuildConditionalNodesRecursive(subset.Rows.ToArray(), attributesList, node);
                first = false;

                children.Add(node);
            }
            parentNode.Children = children;
        }

        private class Iteration
        {
            public IList<int> Rows;
            public IList<int> Attributes;
            public DecisionTree.DecisionNode ParentNode;
        }

        private void BuildConditionalNodes(IList<int> rows, IList<int> attributesIndexes,
            DecisionTree.DecisionNode parentNode)
        {
            var stack = new Stack<Iteration>();

            stack.Push(new Iteration
            {
                Attributes = attributesIndexes,
                ParentNode = parentNode,
                Rows = rows,
            });

            Iteration currentIteration = stack.Pop();

            while (currentIteration != null)
            {
                var attributesList = currentIteration.Attributes.ToList();
                if (attributesList.Count == 0)
                {
                    currentIteration = stack.Count > 0 ? stack.Pop() : null;
                    continue;
                }


                var result = ComputeBestGain(currentIteration.Rows.ToArray(), currentIteration.Attributes);
                var statistics = result.Item3;

                currentIteration.ParentNode.Statistics = statistics;
                currentIteration.ParentNode.Class = _data.ClassesValue[(int) statistics.MostFrequentClass];
                if (statistics.SameClass || (currentIteration.ParentNode.Depth == _options.MaxTreeDepth && _options.MaxTreeDepth>0))
                {
                    currentIteration = stack.Count > 0 ? stack.Pop() : null;
                    continue;
                }
                var attributeResult = result.Item2;

                var first = true;
                var children = new List<DecisionTree.DecisionNode>();

                attributesList.Remove(attributeResult.AttributeIndex);
                if (attributeResult.InvalidAttributes.Any())
                {
                    foreach (var item in attributeResult.InvalidAttributes)
                    {
                        attributesList.Remove(item);
                    }
                }

                foreach (var subset in attributeResult.Subsets.Value)
                {
                    var node = new DecisionTree.DecisionNode
                    {
                        Condition = attributeResult.IsNumeric
                            ? first
                                ? DecisionTree.PredicateCondition.LessThanOrEqual
                                : DecisionTree.PredicateCondition.GreaterThan
                            : DecisionTree.PredicateCondition.Equal,
                        ThreshHold = subset.Value,
                        Attribute = _data.Attributes[attributeResult.AttributeIndex],
                        Depth = currentIteration.ParentNode.Depth + 1,
                        Parent = currentIteration.ParentNode
                    };

                    stack.Push(new Iteration
                    {
                        Rows = subset.Rows,
                        Attributes = attributesList,
                        ParentNode = node,

                    });
                    first = false;

                    children.Add(node);
                }
                currentIteration.ParentNode.Children = children;
                currentIteration = stack.Count > 0 ? stack.Pop() : null;
            }
        }
    }

}