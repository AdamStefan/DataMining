using System;
using System.Linq;

namespace DataMining.DecisionTrees
{
    public class RandomForestAlgorithm
    {
        #region Fields

        private readonly int _trees;        
        private double? _coverageRatio;
        private readonly double _sampleRatio = 0.63;

        #endregion


        #region Instance

        public RandomForestAlgorithm(int trees, double? sampleRatio = null, double? coverageRatio = null)
        {
            _trees = trees;            

            if (sampleRatio != null)
            {
                _sampleRatio = sampleRatio.Value;
            }

            if (coverageRatio != null)
            {
                _coverageRatio = coverageRatio.Value;
            }
        }

        #endregion

        #region Methods

        public Forest BuildForest(TableFixedData data, TreeOptions options = null, int[] attributes = null)
        {
            if (options == null)
            {
                options = new TreeOptions();
            }
            var countOfItemsInTree = Convert.ToInt32(_sampleRatio*data.Count);
            var countOfVariablesInTree =
                Convert.ToInt32(_coverageRatio.HasValue
                    ? Math.Floor(_coverageRatio.Value * (data.Attributes.Length-1))
                    : Math.Ceiling(Math.Sqrt(data.Attributes.Length - 1)));

            var rows = Enumerable.Range(0, data.Count).ToArray();

            if (attributes == null)
            {
                attributes = new int[data.Attributes.Length - 1];
                var currentIndex = 0;
                for (int index = 0; index < data.Attributes.Length; index++)
                {
                    if (data.IsClassAttribute(index))
                    {
                        continue;
                    }
                    attributes[currentIndex++] = index;
                }
            }

            var sampleRows = new int[countOfItemsInTree];
            var sampleAttributes = new int[countOfVariablesInTree];
            var c45Algorithm = new C45AlgorithmDataOptimized(data, options);
            var ret = new Forest(data.ClassesValue.Length);


            //var renderer = new DecisionTreeRenderer();
            //var sampleAttributes1 = new int[2];
            //sampleAttributes1[0] = 0;
            //sampleAttributes1[1] = 2;
            //var sampleRows1 = Enumerable.Range(0, 14).ToArray();

            //var tree1 = c45Algorithm.BuildConditionalTree(sampleRows1, sampleAttributes1);

            //var image1 = renderer.RenderTree(tree1, new System.Drawing.Size(100, 50));
            //var imageName1 = "tttestForest.jpg";
            //image1.Save(imageName1, System.Drawing.Imaging.ImageFormat.Jpeg);



            for (int i = 0; i < _trees; i++)
            {
                rows.Sample(sampleRows);
                attributes.Sample(sampleAttributes);

                var tree = c45Algorithm.BuildConditionalTree(sampleRows, sampleAttributes);
                //var image = renderer.RenderTree(tree,new System.Drawing.Size(100, 50));
                //var imageName = string.Format("testForest{0}.jpg", i);
                //image.Save(imageName, System.Drawing.Imaging.ImageFormat.Jpeg);
                
                ret.Add(tree);
            }

            return ret;
        }

        #endregion
    }
}
