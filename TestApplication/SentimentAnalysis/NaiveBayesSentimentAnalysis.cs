using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DataMining;

namespace TestApplication.SentimentAnalysis
{
    public class NaiveBayesSentimentAnalysis
    {
        #region Fields

        private NaiveBayesClassifier _naiveBayesClassifier;
        private Dictionary<string, int> _wordDictionary = new Dictionary<string, int>();
        private readonly HashSet<string> _negationWords = new HashSet<string> {"not", "nor", "no"};
        private ColumnDataType[] _columnsDataTypes;

        #endregion

        #region Methods

        public void Train(Tuple<string, Boolean>[] trainingSet)
        {
            _wordDictionary = new Dictionary<string, int>();

            DataSample[] samples = new DataSample[trainingSet.Length];
            int wordId = 0;

            for (var trainingItemIndex = 0; trainingItemIndex < trainingSet.Length; trainingItemIndex++)
            {
                var trainingItem = trainingSet[trainingItemIndex];

                //string[] sentences = Regex.Split(trainingItem.Item1, @"(?<=[.!?])\s+(?=\p{Lt})");
                string[] sentences = {trainingItem.Item1};

                var dataSample = new DataSample
                {
                    ClassId = !trainingItem.Item2 ? 0 : 1
                };

                var sampleDataPoints = new List<DataPoint>();

                foreach (var sentence in sentences)
                {
                    //var sentenceWords = Regex.Split(sentence, @"\W+");
                    var sentenceWords = SplitToWords(sentence);
                    var isNegated = false;
                    for (int index = 0; index < sentenceWords.Count; index++)
                    {
                        var currentWord = sentenceWords[index].ToLower();
                        if (currentWord.StartsWith("@"))
                        {
                            continue;
                        }
                        if (_negationWords.Contains(currentWord))
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


                            if (!_wordDictionary.ContainsKey(currentWord))
                            {
                                _wordDictionary.Add(currentWord, wordId);
                                wordId++;
                            }

                            sampleDataPoints.Add(new DataPoint {ColumnId = _wordDictionary[currentWord]});
                        }
                    }
                }
                dataSample.DataPoints = sampleDataPoints.ToArray();
                samples[trainingItemIndex] = dataSample;
            }
            _columnsDataTypes = new ColumnDataType[wordId];
            for (var index = 0; index < wordId; index++)
            {
                _columnsDataTypes[index] = ColumnDataType.Discrete;
            }

            _naiveBayesClassifier = new NaiveBayesClassifier(samples, 2, _columnsDataTypes);
        }


        public static IList<string> SplitToWords(string sentence)
        {
            var words = new List<string>();
            int currentWordStartIndex = 0;
            int currentWordLength = 0;

            for (int index = 0; index < sentence.Length; index++)
            {
                var currentChar = sentence[index];
                if (Char.IsLetter(currentChar) || currentChar == '@')
                {
                    if (currentWordStartIndex < 0)
                    {
                        currentWordStartIndex = index;
                    }
                    currentWordLength++;
                }
                else if (currentChar == '\'')
                {
                    if (index > 0 && index < sentence.Length && Char.IsLetter(sentence[index - 1]) &&
                        Char.IsLetter(sentence[index + 1]))
                    {
                        if (currentWordStartIndex < 0)
                        {
                            currentWordStartIndex = index;
                        }
                        currentWordLength++;
                    }
                    else
                    {
                        ProcessPart(sentence, ref currentWordStartIndex, ref currentWordLength, words);
                    }
                }
                else if (currentChar == ' ' || currentChar == '!' || currentChar == '?' || currentChar == ',')
                {
                    ProcessPart(sentence, ref currentWordStartIndex, ref currentWordLength, words);
                }
                else if (currentChar == '.' || currentChar == ':')
                {
                    if (currentWordLength > 0)
                    {
                        var token = sentence.Substring(currentWordStartIndex, currentWordLength).ToLower();
                        if (token.StartsWith("http") || token.StartsWith("www"))
                        {
                            currentWordLength++;
                        }
                        else
                        {
                            ProcessPart(sentence, ref currentWordStartIndex, ref currentWordLength, words);
                        }
                    }
                    else
                    {
                        currentWordStartIndex = -1;
                    }

                }
                else if (currentWordLength > 0)
                {
                    var token = sentence.Substring(currentWordStartIndex, currentWordLength).ToLower();
                    if (token.StartsWith("http") || token.StartsWith("www"))
                    {
                        currentWordLength++;
                    }
                    else
                    {
                        ProcessPart(sentence, ref currentWordStartIndex, ref currentWordLength, words);
                    }
                }
                else
                {
                    currentWordLength = 0;
                    currentWordStartIndex = -1;
                }
            }

            return words;
        }

        private static void ProcessPart(string sentence, ref int currentWordStartIndex, ref int currentWordLength,
            List<string> words)
        {
            if (currentWordStartIndex < 0 || currentWordLength == 0)
            {
                return;
            }
            var token = sentence.Substring(currentWordStartIndex, currentWordLength).ToLower();
            if (token.StartsWith("http") || token.StartsWith("www") || token.StartsWith("@"))
            {                
            }            
            else if (currentWordLength > 1)
            {
                words.Add(token);
                
            }
            else if (currentWordLength == 1 && token == "i")
            {
                words.Add(token);                
            }

            currentWordStartIndex = -1;
            currentWordLength = 0;
        }


        public int Compute(string sentence)
        {
            var sample = new DataSample();
            var sampleDataPoints = new List<DataPoint>();

            var sentenceWords = Regex.Split(sentence, @"\W+");
            var isNegated = false;

            for (int index = 0; index < sentenceWords.Length; index++)
            {
                var currentWord = sentenceWords[index].ToLower();
                if (_negationWords.Contains(currentWord))
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


                    if (!_wordDictionary.ContainsKey(currentWord))
                    {
                        sampleDataPoints.Add(new DataPoint {ColumnId = _wordDictionary[currentWord]});
                    }

                }
            }
            sample.DataPoints = sampleDataPoints.ToArray();

            return _naiveBayesClassifier.Compute(sample);
        }

        #endregion
    }
}
