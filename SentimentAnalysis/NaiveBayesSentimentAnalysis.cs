using System;
using System.Collections.Generic;
using System.Linq;
using DataMining;

namespace SentimentAnalysis
{
    public class NaiveBayesSentimentAnalysis
    {
        #region Fields

        private NaiveBayesClassifier _naiveBayesClassifier;
        private Dictionary<string, int> _wordDictionary = new Dictionary<string, int>();
        private readonly HashSet<string> _negationWords = new HashSet<string> {"not", "nor", "no"};
        private ColumnDataType[] _columnsDataTypes;
        private Dictionary<string,int > _classes;

        #endregion

        #region Methods

        public void Train(IEnumerable<Tuple<string, string>> trainingSet, int count)
        {
            _wordDictionary = new Dictionary<string, int>();
            _classes = new Dictionary<string, int>();

            DataSample[] samples = new DataSample[count];
            int wordId = 0;
            int classId = 0;
            var trainingItemIndex = 0;
            trainingSet = trainingSet.Take(count);
            foreach (var trainingItem in trainingSet)
            {                
                
                string[] sentences = { trainingItem.Item1 };
                var classValue = trainingItem.Item2;
                if (!_classes.ContainsKey(classValue))
                {
                    _classes.Add(classValue, classId);
                    classId++;
                }

                var dataSample = new DataSample
                {
                    ClassId = _classes[classValue]
                };

                var sampleDataPoints = new List<DataPoint>();

                foreach (var sentence in sentences)
                {                                     
                    var sentenceWords = TextParser.SplitToWords(sentence);
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

                            sampleDataPoints.Add(new DataPoint {ColumnId = _wordDictionary[currentWord], Value = 1});
                        }
                    }
                }
                dataSample.DataPoints = sampleDataPoints.ToArray();
                samples[trainingItemIndex] = dataSample;

                trainingItemIndex++;
            }
            _columnsDataTypes = new ColumnDataType[wordId];
            for (var index = 0; index < wordId; index++)
            {
                _columnsDataTypes[index] = new ColumnDataType {IsDiscrete = true, NumberOfCategories = 2};
            }

            _naiveBayesClassifier = new NaiveBayesClassifier(samples, 2, _columnsDataTypes);
        }


        


        public int Compute(string sentence)
        {
            var sample = new DataSample();
            var sampleDataPoints = new List<DataPoint>();

            var sentenceWords = TextParser.SplitToWords(sentence).ToArray();
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


                    if (_wordDictionary.ContainsKey(currentWord))
                    {
                        sampleDataPoints.Add(new DataPoint {ColumnId = _wordDictionary[currentWord],Value = 1});
                    }

                }
            }
            sample.DataPoints = sampleDataPoints.ToArray();

            return _naiveBayesClassifier.Compute(sample);
        }

        #endregion
    }
}
