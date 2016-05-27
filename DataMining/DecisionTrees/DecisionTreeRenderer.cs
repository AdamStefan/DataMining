using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DataMining.DecisionTrees
{
    public class DecisionTreeRenderer
    {
        public Bitmap RenderTree(DecisionTree tree, Size blockSize)
        {
            var descendents = tree.Root.Descendents;
            var numberOfLeaves = descendents.Count(item => item.IsLeaf);
            var depth = tree.Root.Descendents.Max(item => item.Depth);
            var offset = new Size(50, 50);

            if (numberOfLeaves > 20)
            {
                tree.Options.MaxNumberOfTerminalNodes = 20;
                tree.Prune();
                descendents = tree.Root.Descendents;
                numberOfLeaves = descendents.Count(item => item.IsLeaf);
                depth = tree.Root.Descendents.Max(item => item.Depth);
            }

            var width = ((2*blockSize.Width)*numberOfLeaves) + 2*(offset.Width);
            var height = ((blockSize.Height + 100)*(depth + 1)) + 2*(offset.Height);

            blockSize.Height = blockSize.Height + 100;

            NodeRenderer nr = new NodeRenderer(blockSize, offset);
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.White, 0, 0, width, height);

            var coordinates = GetCoordinates(tree.Root);

            foreach (var coordinate in coordinates)
            {
                var node = coordinate.Key;
                nr.RenderNode(graphics, coordinate.Value, node);
                if (!node.IsLeaf)
                {
                    foreach (var child in node.Children)
                    {
                        nr.RenderEdges(graphics, coordinate.Value, coordinates[child],
                            string.Format("{0} {1}", DecisionTree.GetStringCondition(child.Condition),
                                child.ThreshHold));
                    }
                }
            }

            return bitmap;
        }

        private Dictionary<DecisionTree.DecisionNode, GridCoordinate> GetCoordinates(DecisionTree.DecisionNode root)
        {
            Dictionary<DecisionTree.DecisionNode, GridCoordinate> points =
                new Dictionary<DecisionTree.DecisionNode, GridCoordinate>();                        
            var leavesNodes = new List<DecisionTree.DecisionNode>();
            DecisionTree.LoadLeaves(root, leavesNodes);
                              

            Func<DecisionTree.DecisionNode, GridCoordinate?> getPointFromChildren = node =>
            {
                if (node.Children == null || !node.Children.Any())
                {
                    return null;
                }
                var xValue = (int) node.Children.Average(item =>
                {
                    var point = points[item];
                    return point.Column;
                });

                return new GridCoordinate {Column = xValue, Row = node.Depth};
            };


            var nextColumnIndex = 0;
            var nodesToProcess = leavesNodes;
            while (nodesToProcess.Count>0)
            {
                var listNewNodesToProcess = new List<DecisionTree.DecisionNode>();
                foreach (var node in nodesToProcess)
                {
                    if (points.ContainsKey(node))
                    {
                        continue;
                    }
                    if (node.IsLeaf)
                    {
                        points.Add(node, new GridCoordinate { Column = nextColumnIndex, Row = node.Depth });
                        nextColumnIndex = nextColumnIndex + 2;
                    }
                    else
                    {
                        var childrenResolved = node.Children.All(child => points.ContainsKey(child));
                        if (!childrenResolved)
                        {
                            continue;
                        }
                        var point = getPointFromChildren(node);
                        if (point.HasValue)
                        {
                            points.Add(node, point.Value);
                        }
                    }

                    if (node.Parent != null)
                    {
                        listNewNodesToProcess.Add(node.Parent);
                    }
                }

                nodesToProcess = listNewNodesToProcess;
            }
           

            return points;
        }

        private class NodeRenderer
        {
            private Size ItemSize { get; set; }
            private readonly int _xOffSet;
            private readonly int _yOffSet;

            public NodeRenderer(Size blockSize, Size offSet)
            {
                ItemSize = blockSize;
                _xOffSet = offSet.Width;
                _yOffSet = offSet.Height;
            }

            private Point GetBitmapLocation(GridCoordinate coordinate)
            {
                return new Point(_xOffSet + (coordinate.Column*ItemSize.Width),
                    _yOffSet + (coordinate.Row*ItemSize.Height));
            }

            public void RenderNode(Graphics graphics, GridCoordinate coordinate, DecisionTree.DecisionNode node)
            {
                var location = GetBitmapLocation(coordinate);
                graphics.DrawRectangle(Pens.Black, location.X, location.Y, ItemSize.Width, ItemSize.Height - 100);

                var font = new Font("Arial", 9);

                var message = node.IsLeaf
                    ? node.Class
                    : node.Children.First().Attribute + Environment.NewLine + "(" + node.Class + ")";

                var size = graphics.MeasureString(message, font);
                var textWidth = size.Width;
                var textHeight = size.Height;

                var stringLocationX = ((location.X + location.X + ItemSize.Width)/(float) 2) - (textWidth/2);
                var stringLocationY = ((location.Y + location.Y + ItemSize.Height - 100)/(float) 2) - (textHeight/2);

                graphics.DrawString(message, font, Brushes.Black, stringLocationX, stringLocationY);

            }

            public void RenderEdges(Graphics graphics, GridCoordinate source, GridCoordinate destination,
                string edgeText)
            {
                var locationSource = GetBitmapLocation(source);
                var locationDestination = GetBitmapLocation(destination);
                var locationSourceX = locationSource.X + (ItemSize.Width/2);
                var locationSourceY = locationSource.Y + ItemSize.Height - 100;

                var locationDestinationX = locationDestination.X + (ItemSize.Width/2);
                var locationDestinationY = locationDestination.Y;

                graphics.DrawLine(Pens.Black, locationSourceX, locationSourceY, locationDestinationX,
                    locationDestinationY);

                var font = new Font("Arial", 8);

                var textWidth = graphics.MeasureString(edgeText, font).Width;

                var stringLocationY = (locationDestinationY + locationSourceY)/(float) 2;
                var stringLocationX = ((locationDestinationX + locationSourceX)/(float) 2) - textWidth/2;

                graphics.DrawString(edgeText, font, Brushes.Black, stringLocationX, stringLocationY);

            }
        }

        private struct GridCoordinate
        {
            public int Row { get; set; }
            public int Column { get; set; }

        }
    }
}
