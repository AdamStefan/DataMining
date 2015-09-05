using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMining
{
    public class ConditionalTree
    {
        public enum PredicateCondition
        {
            Equal,
            LessThanOrEqual,
            GreaterThan,
        }

        public class ConditionalNode
        {
            public IEnumerable<ConditionalNode> Children { get; set; }

            public string Attribute { get; set; }

            public object ThreshHold { get; set; }

            public PredicateCondition Condition { get; set; }

            public string Class { get; set; }
        }


        public ConditionalNode Root { get; set; }


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

        private bool GetClass(IDataRow row, ConditionalNode node, out string className)
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

    }
}