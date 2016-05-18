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

            public int Depth { get; set; }
            
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

            public double ComputeErrorRate(double z)
            {
                var f = Statistics.Confidence;
                var N = Statistics.DatasetLength;
                var ret = (f + ((z*z)/2*N) + z*(Math.Sqrt((f/N) - ((f*f)/N) + ((z*z)/(4*N)))))/(1+ ((z*z)/N));
                return ret;
            }

            public double MissedItems
            {
                get
                {
                    if (Children != null && Children.Any())
                    {
                        return Children.Sum(item => item.MissedItems);
                    }

                    return (Statistics.DatasetLength * (1 - Statistics.Confidence));
                }
            }            

            public Statistics Statistics { get; set; }

            public IEnumerable<DecisionNode> Descendents
            {
                get
                {
                    if (Children == null)
                    {
                        return Enumerable.Empty<DecisionNode>();
                    }
                    var ret = Children.ToList();
                    foreach (var child in Children)
                    {
                        ret.AddRange(child.Descendents);
                    }

                    return ret;
                }
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
            if (node.Class != null && (node.Children == null || !node.Children.Any()))
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

        public TreeOptions Options { get; set; }

        private class NodeLossRate
        {
            public DecisionNode Node {get;set;}
            public double LossRate { get; set; }
        }

        public void Prune()
        {
            var listTerminalNodes = new List<DecisionNode>();
            Prune(Root, listTerminalNodes);
            if (Options.MaxNumberOfTerminalNodes <= 0 || Options.MaxNumberOfTerminalNodes >= listTerminalNodes.Count)
            {
                return;
            }

            var parentNodes = listTerminalNodes.Select(item => item.Parent).Distinct().Select(node => new NodeLossRate { Node = node, LossRate = 0 }).ToList();

            for (int index = 0; index < parentNodes.Count; index++)
            {
                var node = parentNodes[index];
                
                node.LossRate = node.Node.MissedItems - (node.Node.Statistics.DatasetLength * (1 - node.Node.Statistics.Confidence));
            }
                        
            while (listTerminalNodes.Count > Options.MaxNumberOfTerminalNodes && parentNodes.Count > 0)
            {
                parentNodes = parentNodes.OrderByDescending(item => item.LossRate).ToList();

                foreach (var child in parentNodes[0].Node.Descendents)
                {
                    listTerminalNodes.Remove(child);
                }
                
                var nodeToRemove = parentNodes[0].Node;
                
                nodeToRemove.Children = new DecisionNode[] { };
                parentNodes.RemoveAt(0);
                listTerminalNodes.Add(nodeToRemove);

                var node = new NodeLossRate { Node = nodeToRemove.Parent };
                node.LossRate = node.Node.MissedItems - (node.Node.Statistics.DatasetLength * (1 - node.Node.Statistics.Confidence));
                if (parentNodes.All(item => item.Node != node.Node))
                {
                    parentNodes.Add(node);
                }                
            }

        }

        private void Prune(DecisionNode node, List<DecisionNode> terminalNodes)
        {            
            var children = node.Children.ToList();
            for (int index = children.Count - 1; index >= 0; index--)
            {
                var child = children[index];
                if (child.Statistics.DatasetLength < Options.MinItemsOnNode ||
                    (child.Depth > Options.MaxTreeDepth && Options.MaxTreeDepth > 0))
                {
                    children.Remove(child);
                }
                else
                {
                    Prune(child, terminalNodes);
                }
            }
            node.Children = children.ToArray();
            if (!node.Children.Any())
            {
                terminalNodes.Add(node);                
            }            
        }

    }
}