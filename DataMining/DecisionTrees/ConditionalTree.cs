using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataMining.DecisionTrees
{
    public class DecisionTree
    {
        public enum PredicateCondition
        {
            Equal,
            LessThanOrEqual,
            GreaterThan,
        }

        public class DecisionNode
        {
            public IEnumerable<DecisionNode> Children { get; set; }

            public string Attribute { get; set; }

            public object ThreshHold { get; set; }

            public PredicateCondition Condition { get; set; }

            public string Class { get; set; }

            public object Tag { get; set; }

            public DecisionNode Parent { get; set; }

            public bool IsLeaf
            {
                get { return (Children == null) || !Children.Any(); }
            }            
        }

        public DecisionNode Root { get; set; }

        public string GetClass(IDataRow row)
        {
            if (Root == null)
            {
                return null;
            }
            string className = null;
            foreach (var child in Root.Children)
            {
                if (GetClass(row, child, out className))
                {
                    break;
                }
            }
            return className;
        }

        private static bool Navigate(IDataRow row, DecisionNode node, DecisionTreeVisitor visitor)
        {
            bool isValid = false;

            var rowValue = row[node.Attribute];
            switch (node.Condition)
            {
                case PredicateCondition.Equal:
                    if (row[node.Attribute].Equals(node.ThreshHold))
                    {
                        return false;
                    }
                    break;
                case PredicateCondition.LessThanOrEqual:
                    if (rowValue == null)
                    {
                        return false;
                    }
                    if (Convert.ToDouble(rowValue) <= Convert.ToDouble(node.ThreshHold))
                    {
                        isValid = true;
                    }
                    break;
                case PredicateCondition.GreaterThan:
                    if (rowValue == null)
                    {
                        return false;
                    }
                    if (Convert.ToDouble(rowValue) > Convert.ToDouble(node.ThreshHold))
                    {
                        isValid = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isValid)
            {
                visitor.VisitNode(node, row);
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        isValid = Navigate(row, child, visitor);
                        if (isValid)
                        {
                            break;
                        }
                    }
                }
            }

            return isValid;
        }

        private static bool GetClass(IDataRow row, DecisionNode node, out string className)
        {
            className = node.Class;
            bool isValid = false;

            var rowValue = row[node.Attribute];
            switch (node.Condition)
            {
                case PredicateCondition.Equal:
                    if (row[node.Attribute].Equals(node.ThreshHold))
                    {
                        isValid = true;
                    }
                    break;
                case PredicateCondition.LessThanOrEqual:
                    if (rowValue == null)
                    {
                        return false;
                    }
                    if (Convert.ToDouble(rowValue) <= Convert.ToDouble(node.ThreshHold))
                    {
                        isValid = true;
                    }
                    break;
                case PredicateCondition.GreaterThan:
                    if (rowValue == null)
                    {
                        return false;
                    }
                    if (Convert.ToDouble(rowValue) > Convert.ToDouble(node.ThreshHold))
                    {
                        isValid = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isValid && node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (GetClass(row, child, out className))
                    {
                        break;
                    }
                }
            }

            return isValid;
        }

        public static void GeneratePseudoCode(DecisionNode node, StringBuilder sb, bool first, int level = 0)
        {
            if (node == null)
            {
                return;
            }
            if (node.Attribute != null)
            {
                var firstParam = first ? "if " : "else if ";
                var attributeParam = node.Attribute;
                string operatorParam;
                var sentence = "{0} {1} {2}" + " \"{3}\" then ";
                switch (node.Condition)
                {
                    case PredicateCondition.Equal:
                        operatorParam = "=";
                        break;
                    case PredicateCondition.LessThanOrEqual:
                        operatorParam = "<=";
                        break;
                    case PredicateCondition.GreaterThan:
                        operatorParam = ">";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                AppendNewLine(sb, string.Format(sentence, firstParam, attributeParam, operatorParam, node.ThreshHold),
                    level);
                level++;
            }
            if (node.Class != null)
            {
                var ret = "class = " + node.Class;
                AppendNewLine(sb, ret, level);
            }
            else
            {
                if (node.Children != null)
                {
                    var children = node.Children.ToList();

                    for (int index = 0; index < children.Count; index++)
                    {
                        GeneratePseudoCode(children[index], sb, index == 0, level);
                    }
                }
            }
        }

        public string ToPseudocode()
        {
            var sb = new StringBuilder();
            GeneratePseudoCode(Root, sb, true);
            return sb.ToString();
        }

        private static void AppendNewLine(StringBuilder sb, string value, int noOfTabs)
        {
            for (int index = 0; index < noOfTabs; index++)
            {
                value = "\t" + value;
            }
            sb.AppendLine(value);
        }


        private void Prune(IEnumerable<IDataRow> rows)
        {
            if (Root == null)
            {
                return;
            }
            Dictionary<DecisionNode,NodeStatistics>  statistics = new Dictionary<DecisionNode, NodeStatistics>();
            Action<DecisionNode, IDataRow> action = (node, row) =>
            {
                if (!statistics.ContainsKey(node))
                {
                    statistics.Add(node, new NodeStatistics());
                }
                statistics[node].NumberOfItemsInNode ++;
                statistics[node].Frequencies[row.Class]++;
                statistics[node].Node = node;
            };

            var visitor = new DecisionTreeVisitor(action);
            foreach (var row in rows)
            {
                foreach (var child in Root.Children)
                {
                    if (Navigate(row, child, visitor))
                    {
                        break;
                    }
                }
            }            
        }

        private class NodeStatistics
        {
            public int NumberOfItemsInNode { get; set; }
            public Dictionary<string,int> Frequencies { get; set; }
            public int NumberOfMissedValues { get; set; }
            public DecisionNode Node { get; set; }

            public NodeStatistics()
            {
                Frequencies = new Dictionary<string, int>();
            }
        }


    }

    public class DecisionTreeVisitor
    {
        private Action<DecisionTree.DecisionNode, IDataRow> _onVisitAction;

        public DecisionTreeVisitor(Action<DecisionTree.DecisionNode,IDataRow> action)
        {
            _onVisitAction = action;
        }

        public void VisitNode(DecisionTree.DecisionNode node, IDataRow row)
        {
            _onVisitAction(node, row);
        }
    }
}