using DataMining;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestApplication.SentimentAnalysis
{
    public class SentimentAnalysis
    {

        #region Instance

        public SentimentAnalysis()
        {

        }

        #endregion

        #region Methods

        public double AnalyzeBasedOnLexicon(string sentence)
        {

            Dictionary<string, bool> words = new Dictionary<string, bool>();
            HashSet<string> negationWords = new HashSet<string>() { "not", "nor", "no" };

            var textFiles = new Dictionary<string, bool>();

            textFiles.Add("Lexicon\\NegativeWords.txt", false);
            textFiles.Add("Lexicon\\PositiveWords.txt", true);

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

        public double AnalyzedWithNaiveBayes(string testSentense, IEnumerable<Tuple<string, Boolean>> trainingSet)
        {
            HashSet<string> negationWords = new HashSet<string>() { "not", "nor", "no" };
            HashSet<string> words = new HashSet<string>();
            List<Sample> samples = new List<Sample>();


            foreach (var sample in trainingSet)
            {
                string[] sentences = Regex.Split(sample.Item1, @"(?<=[.!?])\s+(?=\p{Lt})");
                var dataSample = new Sample();
                foreach (var sentence in sentences)
                {
                    var sentenceWords = Regex.Split(sentence, @"\W+");
                    var isNegated = false;
                    for (int index = 0; index < sentenceWords.Length; index++)
                    {
                        var currentWord = sentenceWords[index].ToLower();
                        if (negationWords.Contains(currentWord))
                        {
                            isNegated = !isNegated;
                        }
                        else
                        {
                            if (currentWord.EndsWith("n't"))
                            {
                                isNegated = !isNegated;
                            }
                            else
                            {
                                currentWord = isNegated ? "not_" + currentWord : currentWord;
                            }


                            if (!words.Contains(currentWord))
                            {
                                words.Add(currentWord);
                            }
                        }

                    }
                }
            }


            


            return -1;
        }

        #endregion
    }
}
