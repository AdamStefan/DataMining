using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining
{
    public class Tools
    {

        private static double ComputeEntropy(IList<int> data, out bool sameClass)
        {
            var freq = new Dictionary<int, int>();

            var itemCount = data.Count;

            foreach (var item in data)
            {
                var className = item;
                if (!freq.ContainsKey(className))
                {
                    freq.Add(className, 0);
                }
                freq[className]++;
            }

            sameClass = freq.Count == 1;
            var sum = 0.0;

            foreach (var item in freq)
            {
                var val = ((double)item.Value) / itemCount;
                sum += val * Math.Log(val, 2);
            }

            return -sum;
        }


        public static double ComputeEntropy2(IList<int> data, out bool sameClass, int classNo)
        {
            var freq = new int[classNo];
            var listVal = new List<int>();

            var itemCount = data.Count;

            foreach (var item in data)
            {
                var className = item;
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

        public static IList<int> GenerateInt(int classNo, int length)
        {
            var random = new Random();
            var values = new int[length];
            for (int index = 0; index < length; index++)
            {
                values[index] = random.Next(classNo);
            }

            return new List<int>(values);
        }

        public static void Test()
        {

            var stopWatch1 = new Stopwatch();
            var stopWatch2 = new Stopwatch();

            for (int index = 0; index < 1000; index++)
            {
                var list = GenerateInt(10, 20000);
                bool  sameClass;
                stopWatch1.Start();
                var val1 = ComputeEntropy(list, out sameClass);
                stopWatch1.Stop();

                stopWatch2.Start();
                var val2 = ComputeEntropy2(list, out sameClass,10);
                stopWatch2.Stop();

                if (val1 != val2)
                {
                    throw new Exception();
                }
            }
            var delta = stopWatch1.Elapsed.Subtract(stopWatch2.Elapsed);
        }

    }
}
