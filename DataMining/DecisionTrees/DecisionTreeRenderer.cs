using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining.DecisionTrees
{
    public class DecisionTreeRenderer
    {
        //public Bitmap RenderTree(DecisionTree tree, Rectangle blockSize)
        //{
        //    var descendents = tree.Root.Descendents;
        //    var numberOfLeaves = descendents.Count(item => item.IsLeaf);
        //    var depth = tree.Root.Descendents.Max(item => item.Depth);
        //    var width = (blockSize.Width + 2 * 40) * numberOfLeaves;
        //    var height = (blockSize.Height + 100) * depth;
        //    Bitmap bitmap = new Bitmap(width, height);

        //}

        private class Grid
        {
            public Rectangle ItemSize { get; set; }
            public int Columns { get; set; }
            public int Rows { get; set; }
            //public double ComputeWidth { get; }
            //public double ComputeHeight { get; }
            public void BuildGrid(DecisionTree.DecisionNode root)
            {
            }

            private void GetPoints(DecisionTree.DecisionNode root )
            {
                Dictionary<DecisionTree.DecisionNode, Point> points = new Dictionary<DecisionTree.DecisionNode,Point>();
                var descendands = root.Descendents;
                var maxDepth = descendands.Max(item => item.Depth);
                Func<int, IEnumerable<DecisionTree.DecisionNode>> getRows = (index) =>
                {
                    return descendands.Where(item => item.Depth == index).OrderBy(item =>
                    {
                        if (item.IsLeaf)
                        {
                            return 1;
                        }
                        return 0;
                    });
                };

                Func<DecisionTree.DecisionNode,Point?> getPointFromChildren = (node) =>{
                    if (node.Children== null || !node.Children.Any())
                    {
                    return null;
                    }
                    var xValue = (int) node.Children.Average(item=>{
                        var point = points[item];
                        return point.X;
                    });

                   return new Point(){X = xValue,Y= maxDepth - node.Depth};
                };

                
                for (int index = maxDepth; index >= 0; index--)
                {
                    var nextLeftIndex = new Point(0, index);
                    var items = getRows(index);
                    foreach (var item in items)
                    {
                        var point = getPointFromChildren(item);
                        if (point.HasValue)
                        {
                            points.Add(item, point.Value);
                            if (point.Value.X > nextLeftIndex.X)
                            {
                                nextLeftIndex.X = point.Value.X + 2;
                            }
                        }
                        else
                        {
                            points.Add(item, nextLeftIndex);
                            nextLeftIndex.X = nextLeftIndex.X + 2;
                        }
                    }                    
                }
            }
            


        }

       


    }
}
