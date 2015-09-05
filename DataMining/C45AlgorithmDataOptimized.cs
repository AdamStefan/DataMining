using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMining
{
    public class C45AlgorithmDataOptimized
    {
        #region Fields


        private readonly TableFixedData _data;

        #endregion

        #region Instance

        public C45AlgorithmDataOptimized(TableFixedData data)
        {
            _data = data;
        }

        #endregion
        

        public double ComputeEntropy(IList<int> dataRowsIndexes, out bool sameClass)
        {
            var freq = new int[_data.ClassesValue.Length];
            var listVal = new List<int>();

            var itemCount = dataRowsIndexes.Count;

            foreach (var rowIndex in dataRowsIndexes)
            {
                var className = _data.Class(rowIndex);
                if (freq[className] == 0)
                {
                    listVal.Add(className);
                }
                freq[className]++;
            }

            sameClass = listVal.Count == 1;
            var sum = 0.0;
            for (var index = 0; index < listVal.Count; index++)
            {
                var val = ((double)freq[listVal[index]]) / itemCount;
                sum += val * Math.Log(val, 2);
            }

            return -sum;
        }

        private bool IsUnknown(object value)
        {
            return value == null || value == DBNull.Value || value == Type.Missing;
        }

        private ComputeAttributeEntropyResult ComputeAttributeEntropy(IList<int> dataRowsIndexes, int attributeIndex)
        {
            if (dataRowsIndexes.Count == 0)
            {
                return null;
            }

            return _data[0, attributeIndex].IsNumeric()
                ? ComputeAttributeEntropyNumericBinary(dataRowsIndexes, attributeIndex)
                : ComputeAttributeEntropyNotNumeric(dataRowsIndexes, attributeIndex);
        }


        private ComputeAttributeEntropyResult ComputeAttributeEntropyNumericBinary(IList<int> dataRowsIndexes,
            int attributeIndex)
        {
            var rows = dataRowsIndexes.Where(item => !IsUnknown(_data[item, attributeIndex])).OrderBy(item =>
            {
                double value;
                if (!_data[item, attributeIndex].TryConvertToNumeric(out value))
                {
                    value = Double.NaN;
                }
                return value;
            }).ToArray();

            var ret = new ComputeAttributeEntropyResult
            {
                IsNumeric = true,
                AttributeIndex = attributeIndex,
                KnownValues = rows.Length
            };


            var rowsCount = rows.Length;
            var minimumAttributeValue = Double.MaxValue;
            int minsplitIndex = 0;

            var freqLeft = new int[_data.ClassesValue.Length];
            var freqRight = new int[_data.ClassesValue.Length];
            var rightListVal = new List<int>();
            var leftListVal = new List<int>();

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
                var nextItemClass = _data.Class(rows[index + 1]);

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

                if (currentItemClass == nextItemClass) //breakpoints between values of the same class cannot be optimal
                {
                    continue;
                }

                double leftItemValue;
                if (!currentItemValue.TryConvertToNumeric(out leftItemValue))
                {
                    leftItemValue = Double.NaN;
                }

                double rightItemValue;
                if (!nextItemValue.TryConvertToNumeric(out rightItemValue))
                {
                    rightItemValue = Double.NaN;
                }

                if (leftItemValue == rightItemValue)
                {
                    continue;
                }

                var sumLeft = 0.0;
                var sumRight = 0.0;

                foreach (var item in leftListVal)
                {
                    var val = ((double)freqLeft[item]) / leftCount;
                    sumLeft += val * Math.Log(val, 2);
                }

                foreach (var item in rightListVal)
                {
                    if (freqRight[item] == 0)
                    {
                        continue;
                    }
                    var val = ((double)freqRight[item]) / rightCount;
                    sumRight += val * Math.Log(val, 2);
                }


                var leftValue = (((double)leftCount) / rowsCount) * (-sumLeft);
                var rightValue = (((double)rightCount) / rowsCount) * (-sumRight);

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
            ret.Subsets =
                rows.Split(minsplitIndex)
                    .Select(item => new ComputeAttributeEntropyResult.Subset { Rows = item, Value = splitValue });


            return ret;
        }

        private ComputeAttributeEntropyResult ComputeAttributeEntropyNotNumeric(IList<int> dataRowsIndexes,
            int attributeIndex)
        {
            var freq = new Dictionary<object, IList<int>>();
            var itemCount = 0;

            var ret = new ComputeAttributeEntropyResult { AttributeIndex = attributeIndex, IsNumeric = false };

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
                bool sameClass;
                sum += (((double)list.Count) / itemCount) * ComputeEntropy(list, out sameClass);
            }

            ret.Subsets =
                freq.Select(item => new ComputeAttributeEntropyResult.Subset { Rows = item.Value, Value = item.Key });
            ret.EntropyValue = sum;

            return ret;
        }

        private Tuple<double, ComputeAttributeEntropyResult> ComputeBestGain(IList<int> dataRowsIndexes, IList<int> attributesIndexes, out bool sameClass)
        {
            var entropy = ComputeEntropy(dataRowsIndexes, out sameClass);
            if (sameClass)
            {
                return new Tuple<double, ComputeAttributeEntropyResult>(double.MaxValue, null);
            }

            var maxGain = Double.MinValue;
            ComputeAttributeEntropyResult minAttributeEntropyResult = null;

            var locker = new object();
            Parallel.ForEach(attributesIndexes, attribute =>
            {
                var attributeEntropy = ComputeAttributeEntropy(dataRowsIndexes, attribute);
                var gain = (entropy - attributeEntropy.EntropyValue) * attributeEntropy.KnownValues / dataRowsIndexes.Count;

                lock (locker)
                {
                    if (gain > maxGain)
                    {
                        maxGain = gain;
                        minAttributeEntropyResult = attributeEntropy;
                    }
                }
            });

            return new Tuple<double, ComputeAttributeEntropyResult>(maxGain, minAttributeEntropyResult);
        }

        private class ComputeAttributeEntropyResult
        {
            public double EntropyValue { get; set; }
            public int AttributeIndex { get; set; }
            public bool IsNumeric { get; set; }
            public IEnumerable<Subset> Subsets { get; set; }
            public int KnownValues { get; set; }

            public class Subset
            {
                public object Value { get; set; }
                public IList<int> Rows { get; set; }
            }
        }

        public ConditionalTree BuildConditionalTree()
        {
            var listRows = new List<int>();

            for (int index = 0; index < _data.Count; index++)
            {
                listRows.Add(index);
            }

            if (listRows.Count == 0)
            {
                return null;
            }
            var conditionalTree = new ConditionalTree
            {
                Root = new ConditionalTree.ConditionalNode()
            };

            var attributes = new List<int>();

            for (int index = 0; index < _data.Attributes.Length; index++)
            {
                if (_data.Attributes[index] == TableData.ClassAttributeName)
                {
                    continue;
                }
                attributes.Add(index);
            }
            //BuildConditionalNodesRecursive(listRows, attributes, conditionalTree.Root);
            BuildConditionalNodes(listRows, attributes, conditionalTree.Root);
            return conditionalTree;
        }

        private void BuildConditionalNodesRecursive(IList<int> rows, IList<int> attributesIndexes,
            ConditionalTree.ConditionalNode parentNode)
        {
            var attributesList = attributesIndexes.ToList();
            if (attributesList.Count == 0)
            {
                return;
            }
            bool sameClass;
            var result = ComputeBestGain(rows, attributesIndexes, out sameClass);


            if (sameClass)
            {
                parentNode.Class = _data.ClassesValue[_data.Class(rows[0])];
                return;
            }
            var attributeResult = result.Item2;



            var first = true;
            var children = new List<ConditionalTree.ConditionalNode>();
            attributesList.Remove(attributeResult.AttributeIndex);

            foreach (var subset in attributeResult.Subsets)
            {
                var node = new ConditionalTree.ConditionalNode
                {
                    Condition = attributeResult.IsNumeric
                        ? first
                            ? ConditionalTree.PredicateCondition.LessThanOrEqual
                            : ConditionalTree.PredicateCondition.GreaterThan
                        : ConditionalTree.PredicateCondition.Equal,
                    ThreshHold = subset.Value,
                    Attribute = _data.Attributes[attributeResult.AttributeIndex]
                };

                BuildConditionalNodesRecursive(subset.Rows, attributesList, node);
                first = false;

                children.Add(node);
            }
            parentNode.Children = children;
        }

        private class Iteration
        {
            public IList<int> Rows;
            public IList<int> Attributes;            
            public ConditionalTree.ConditionalNode ParentNode;
        }

        private void BuildConditionalNodes(IList<int> rows, IList<int> attributesIndexes,
            ConditionalTree.ConditionalNode parentNode)
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

                bool sameClass;
                var result = ComputeBestGain(currentIteration.Rows, currentIteration.Attributes,
                    out sameClass);

                if (sameClass)
                {
                    currentIteration.ParentNode.Class = _data.ClassesValue[_data.Class(currentIteration.Rows[0])];
                    currentIteration = stack.Count > 0 ? stack.Pop() : null;
                    continue;
                }


                var attributeResult = result.Item2;
                var first = true;
                var children = new List<ConditionalTree.ConditionalNode>();

                attributesList.Remove(attributeResult.AttributeIndex);

                foreach (var subset in attributeResult.Subsets)
                {
                    var node = new ConditionalTree.ConditionalNode
                    {
                        Condition = attributeResult.IsNumeric
                            ? first
                                ? ConditionalTree.PredicateCondition.LessThanOrEqual
                                : ConditionalTree.PredicateCondition.GreaterThan
                            : ConditionalTree.PredicateCondition.Equal,
                        ThreshHold = subset.Value,
                        Attribute = _data.Attributes[attributeResult.AttributeIndex]
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