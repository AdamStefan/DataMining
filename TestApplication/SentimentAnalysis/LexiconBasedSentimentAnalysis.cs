using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TestApplication.SentimentAnalysis
{
    public class LexiconBasedSentimentAnalysis
    {        

        #region Methods

        public double Compute(string sentence)
        {

            Dictionary<string, bool> words = new Dictionary<string, bool>();
            HashSet<string> negationWords = new HashSet<string>() { "not", "nor", "no" };

            var textFiles = new Dictionary<string, bool>
            {
                {"Lexicon\\NegativeWords.txt", false},
                {"Lexicon\\PositiveWords.txt", true}
            };


            foreach (var item in textFiles)
            {
                using (StreamReader sr = new StreamReader(item.Key))
                {
                    while (!sr.EndOfStream)
                    {
                        var wrd = sr.ReadLine();
                        words[wrd] = item.Value;
                    }
                }
            }

            var sentenceWords = Regex.Split(sentence, @"\W+");

            double positiveRatio = 0.0;
            double totalSum = 0.0;
            double negationFactor = -1;

            foreach (var word in sentenceWords)
            {
                var lowerWord = word.ToLower();

                if (negationWords.Contains(lowerWord))
                {
                    negationFactor = -negationFactor;
                }

                bool isPositive;
                if (!words.TryGetValue(word, out isPositive))
                {
                    continue;
                }
                positiveRatio += negationFactor * (isPositive ? 1 : -1);
                totalSum += 1;
            }

            if (totalSum > 0)
            {
                return positiveRatio / totalSum;
            }

            return 0;

        }        

        #endregion
    }
}
