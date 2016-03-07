using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMining.DecisionTrees
{
    public class C45AlgorithmData
    {             

        private double ComputeEntropy(IEnumerable<IDataRow> dataRows, out bool sameClass)
        {
            var freq = new Dictionary<string, int>();

            var itemCount = 0;

            foreach (var row in dataRows)
            {
                if (!freq.ContainsKey(row.Class))
                {
                    freq.Add(row.Class, 0);
                }
                freq[row.Class]++;
                itemCount++;
            }
            sameClass = freq.Count == 1;
            var sum = 0.0;

            foreach (var item in freq)
            {
                var val = ((double) item.Value)/itemCount;
                sum += val*Math.Log(val, 2);
            }

            return -sum;
        }

        private bool IsUnknown(object value)
        {
            return value == null || value == DBNull.Value || value == Type.Missing;
        }

        private ComputeAttributeEntropyResult ComputeAttributeEntropy(IList<IDataRow> dataRows, string attribute)
        {
            if (dataRows.Count == 0)
            {
                return null;
            }
            var firstRow = dataRows[0];
            return firstRow[attribute].IsNumeric()
                ? ComputeAttributeEntropyNumericBinary(dataRows, attribute)
                : ComputeAttributeEntropyNotNumeric(dataRows, attribute);
        }

        #region Old Code

        //public double ComputeAttributeEntropyNumericBinary(IEnumerable<IDataRow> dataRows, string attribute,
        //    out double splitValue)
        //{
        //    var rows = dataRows.OrderBy(item =>
        //    {
        //        double value;
        //        if (!item[attribute].TryConvertToNumeric(out value))
        //        {
        //            value = Double.NaN;
        //        }
        //        return value;
        //    }).ToList();

        //    var left = new List<IDataRow>();
        //    var right = new List<IDataRow>(rows);

        //    var rowsCount = rows.Count;
        //    var minimumAttributeValue = Double.MaxValue;
        //    int minsplitIndex = 0;            


        //    for (int index = 0; index < rowsCount - 1; index++)
        //    {
        //        left.Add(rows[index]);
        //        right.RemoveAt(0);

        //        double leftItemValue;
        //        if (!rows[index][attribute].TryConvertToNumeric(out leftItemValue))
        //        {
        //            leftItemValue = Double.NaN;
        //        }

        //        double rightItemValue;
        //        if (!rows[index + 1][attribute].TryConvertToNumeric(out rightItemValue))
        //        {
        //            rightItemValue = Double.NaN;
        //        }

        //        if (leftItemValue == rightItemValue)
        //        {
        //            continue;
        //        }
        //        var leftValue = (((double) left.Count)/rowsCount)*ComputeEntropy(left);
        //        var rightValue = (((double) (right.Count))/rowsCount)*ComputeEntropy(right);
        //        var currentAttributeValue = leftValue + rightValue;
        //        if (currentAttributeValue < minimumAttributeValue)
        //        {
        //            minsplitIndex = index;
        //            minimumAttributeValue = currentAttributeValue;
        //        }

        //    }            
        //    left[minsplitIndex][attribute].TryConvertToNumeric(out splitValue);            

        //    return minimumAttributeValue;
        //}

        #endregion

        private ComputeAttributeEntropyResult ComputeAttributeEntropyNumericBinary(IList<IDataRow> dataRows,
            string attribute)
        {

            var rows = dataRows.Where(item => !IsUnknown(item[attribute])).OrderBy(item =>
            {
                double value;
                if (!item[attribute].TryConvertToNumeric(out value))
                {
                    value = Double.NaN;
                }
                return value;
            }).ToList();

            var ret = new ComputeAttributeEntropyResult
            {
                IsNumeric = true,
                Attribute = attribute,
                KnownValues = rows.Count
            };

            var left = new List<IDataRow>();
            var right = new List<IDataRow>(rows);

            var rowsCount = rows.Count;
            var minimumAttributeValue = Double.MaxValue;
            int minsplitIndex = 0;

            var freqLeft = new Dictionary<string, int>();
            var freqRight = new Dictionary<string, int>();

            foreach (var row in right)
            {
                if (!freqRight.ContainsKey(row.Class))
                {
                    freqRight.Add(row.Class, 0);
                }
                freqRight[row.Class]++;
            }


            for (int index = 0; index < rowsCount - 1; index++)
            {
                var currentItem = rows[index];
                var nextItem = rows[index + 1];
                left.Add(currentItem);
                right.RemoveAt(0);


                #region LeftPart calculation

                if (!freqLeft.ContainsKey(currentItem.Class))
                {
                    freqLeft.Add(currentItem.Class, 0);
                }

                freqLeft[currentItem.Class]++;

                #endregion

                #region RightPart calculation

                freqRight[currentItem.Class]--;

                #endregion

                //if (currentItem.Class == nextItem.Class)
                //    //breakpoints between values of the same class cannot be optimal
                //{
                //    continue;
                //}

                double leftItemValue;
                if (!currentItem[attribute].TryConvertToNumeric(out leftItemValue))
                {
                    leftItemValue = Double.NaN;
                }

                double rightItemValue;
                if (!nextItem[attribute].TryConvertToNumeric(out rightItemValue))
                {
                    rightItemValue = Double.NaN;
                }

                if (Math.Abs(leftItemValue - rightItemValue) < double.Epsilon)
                {
                    continue;
                }

                var sumLeft = 0.0;
                var sumRight = 0.0;

                foreach (var item in freqLeft)
                {
                    var val = ((double) item.Value)/left.Count;
                    sumLeft += val*Math.Log(val, 2);
                }

                foreach (var item in freqRight)
                {
                    if (item.Value == 0)
                    {
                        continue;
                    }
                    var val = ((double) item.Value)/right.Count;
                    sumRight += val*Math.Log(val, 2);
                }


                var leftValue = (((double) left.Count)/rowsCount)*(-sumLeft);
                var rightValue = (((double) right.Count)/rowsCount)*(-sumRight);

                var currentAttributeValue = leftValue + rightValue;
                if (currentAttributeValue < minimumAttributeValue)
                {
                    minsplitIndex = index;
                    minimumAttributeValue = currentAttributeValue;
                }
            }

            double splitValue;
            left[minsplitIndex][attribute].TryConvertToNumeric(out splitValue);

            ret.EntropyValue = minimumAttributeValue;
            ret.Subsets =
                rows.Split(minsplitIndex)
                    .Select(item => new ComputeAttributeEntropyResult.Subset {Rows = item, Value = splitValue});


            return ret;
        }

        private ComputeAttributeEntropyResult ComputeAttributeEntropyNotNumeric(IList<IDataRow> dataRows,
            string attribute)
        {
            var freq = new Dictionary<object, IList<IDataRow>>();
            var itemCount = 0;

            var ret = new ComputeAttributeEntropyResult {Attribute = attribute, IsNumeric = false};

            foreach (var row in dataRows)
            {
                var currentValue = row[attribute];
                if (IsUnknown(currentValue))
                {
                    continue;
                }
                if (!freq.ContainsKey(currentValue))
                {
                    freq.Add(currentValue, new List<IDataRow>());
                }
                freq[currentValue].Add(row);
                itemCount++;
            }
            ret.KnownValues = itemCount;

            var sum = 0.0;
            foreach (var item in freq)
            {
                var list = item.Value;
                bool sameClass;
                sum += (((double) list.Count)/itemCount)*ComputeEntropy(list, out sameClass);
            }

            ret.Subsets =
                freq.Select(item => new ComputeAttributeEntropyResult.Subset {Rows = item.Value, Value = item.Key});
            ret.EntropyValue = sum;

            return ret;
        }

        private Tuple<double, ComputeAttributeEntropyResult> ComputeBestGain(IList<IDataRow> dataRows,
            IList<string> attributes, out bool sameClass)
        {
            var entropy = ComputeEntropy(dataRows, out sameClass);
            if (sameClass)
            {
                return new Tuple<double, ComputeAttributeEntropyResult>(double.MaxValue, null);
            }
            var maxGain = Double.MinValue;
            ComputeAttributeEntropyResult minAttributeEntropyResult = null;


            foreach (var attribute in attributes)
            {
                var attributeEntropy = ComputeAttributeEntropy(dataRows, attribute);
                var gain = (entropy - attributeEntropy.EntropyValue)*attributeEntropy.KnownValues/dataRows.Count;

                if (gain > maxGain)
                {
                    maxGain = gain;
                    minAttributeEntropyResult = attributeEntropy;
                }
            }

            return new Tuple<double, ComputeAttributeEntropyResult>(maxGain, minAttributeEntropyResult);
        }

        private class ComputeAttributeEntropyResult
        {
            public double EntropyValue { get; set; }
            public string Attribute { get; set; }
            public bool IsNumeric { get; set; }
            public IEnumerable<Subset> Subsets { get; set; }
            public int KnownValues { get; set; }

            public class Subset
            {
                public object Value { get; set; }
                public IList<IDataRow> Rows { get; set; }
            }
        }

        public DecisionTree BuildConditionalTree(ITableData data)
        {
            var listRows = data.ToList();
            if (listRows.Count == 0)
            {
                return null;
            }
            var firstRow = listRows[0];
            var conditionaTree = new DecisionTree
            {
                Root = new DecisionTree.DecisionNode()
            };
                
            var attributes = firstRow.Attributes.ToList();
            attributes.Remove(TableData.ClassAttributeName);
            BuildConditionalNodesRecursive(listRows, attributes, conditionaTree.Root);
            return conditionaTree;
        }

        private void BuildConditionalNodesRecursive(IList<IDataRow> rows, IList<string> attributes,
            DecisionTree.DecisionNode parentNode)
        {
            var attributesList = attributes.ToList();
            if (attributesList.Count == 0)
            {
                return;
            }
            bool sameClass;
            var result = ComputeBestGain(rows, attributes, out sameClass);
            if (sameClass)
            {
                parentNode.Class = rows.First().Class;
                return;
            }
            var attributeResult = result.Item2;
            var first = true;
            var children = new List<DecisionTree.DecisionNode>();


            attributesList.Remove(attributeResult.Attribute);

            foreach (var subset in attributeResult.Subsets)
            {
                var node = new DecisionTree.DecisionNode
                {
                    Condition = attributeResult.IsNumeric
                        ? first
                            ? DecisionTree.PredicateCondition.LessThanOrEqual
                            : DecisionTree.PredicateCondition.GreaterThan
                        : DecisionTree.PredicateCondition.Equal,
                    ThreshHold = subset.Value,
                    Attribute = attributeResult.Attribute
                };

                BuildConditionalNodesRecursive(subset.Rows, attributesList, node);
                first = false;

                children.Add(node);
            }
            parentNode.Children = children;
        }
        
    }
}