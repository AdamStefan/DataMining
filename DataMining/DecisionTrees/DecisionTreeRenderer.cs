using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
            Dictionary<DecisionTree.DecisionNode, GridCoordinate> points = new Dictionary<DecisionTree.DecisionNode, GridCoordinate>();
            var descendands = root.Descendents.ToList();
            descendands.Add(root);
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

            Func<DecisionTree.DecisionNode, GridCoordinate?> getPointFromChildren = (node) =>
            {
                if (node.Children == null || !node.Children.Any())
                {
                    return null;
                }
                var xValue = (int)node.Children.Average(item =>
                {
                    var point = points[item];
                    return point.Column;
                });

                return new GridCoordinate { Column = xValue, Row = node.Depth };
            };


            for (int index = maxDepth; index >= 0; index--)
            {
                var nextLeftIndex = new GridCoordinate() {Row = index, Column = 0};
                var items = getRows(index);
                foreach (var item in items)
                {
                    var point = getPointFromChildren(item);
                    if (point.HasValue)
                    {
                        points.Add(item, point.Value);
                        if (point.Value.Column > nextLeftIndex.Column)
                        {
                            nextLeftIndex.Column = point.Value.Column + 2;
                        }
                    }
                    else
                    {
                        points.Add(item, nextLeftIndex);
                        nextLeftIndex.Column = nextLeftIndex.Column + 2;
                    }
                }
            }

            return points;
        }

        private class NodeRenderer
        {
            public Size ItemSize { get; set; }
            private readonly int _xOffSet;
            private readonly int _yOffSet;

            public NodeRenderer(Size blockSize, Size offSet)
            {
                ItemSize = blockSize;
                _xOffSet = offSet.Width;
                _yOffSet = offSet.Height;
            }

            public Point GetBitmapLocation(GridCoordinate coordinate)
            {
                return new Point(_xOffSet + (coordinate.Column*ItemSize.Width),
                    _yOffSet + (coordinate.Row*ItemSize.Height));
            }

            public void RenderNode(Graphics graphics, GridCoordinate coordinate, DecisionTree.DecisionNode node)
            {
                var location = GetBitmapLocation(coordinate);
                graphics.DrawRectangle(Pens.Black, location.X, location.Y, ItemSize.Width, ItemSize.Height - 100);
                string message = String.Empty;

                 var font = new Font("Arial", 9);

                if (node.IsLeaf)
                {
                    message = node.Class;
                }
                else
                {
                    message = node.Children.First().Attribute;
                }

                var size = graphics.MeasureString(message, font);
                var textWidth = size.Width;
                var textHeight = size.Height;

                var stringLocationX = ((location.X + location.X + ItemSize.Width) / (float)2) - (textWidth / 2);
                var stringLocationY = ((location.Y + location.Y + ItemSize.Height - 100) / (float)2) - (textHeight / 2);

                graphics.DrawString(message, font, Brushes.Black, stringLocationX, stringLocationY);

            }

            public void RenderEdges(Graphics graphics, GridCoordinate source, GridCoordinate destination , string edgeText)
            {
                var locationSource = GetBitmapLocation(source);
                var locationDestination = GetBitmapLocation(destination);
                var locationSourceX = locationSource.X + (ItemSize.Width/2);
                var locationSourceY = locationSource.Y + ItemSize.Height - 100;

                var locationDestinationX = locationDestination.X + (ItemSize.Width / 2);
                var locationDestinationY = locationDestination.Y;

                graphics.DrawLine(Pens.Black, locationSourceX, locationSourceY, locationDestinationX,
                    locationDestinationY);

                var font = new Font("Arial", 8);

                var textWidth = graphics.MeasureString(edgeText, font).Width;

                var stringLocationY = (locationDestinationY + locationSourceY)/(float)2;                                
                var stringLocationX = ((locationDestinationX + locationSourceX) / (float)2)- textWidth/2;

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
