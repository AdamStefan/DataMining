using System.Collections.Generic;
using System.Linq;

namespace SentimentAnalysis
{
    public static class Utils
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var toReturn = new List<T>(batchSize);
            foreach (var item in source)
            {
                toReturn.Add(item);
                if (toReturn.Count == batchSize)
                {
                    yield return toReturn;
                    toReturn = new List<T>(batchSize);
                }
            }
            if (toReturn.Any())
            {
                yield return toReturn;
            }
        }
    }
}
