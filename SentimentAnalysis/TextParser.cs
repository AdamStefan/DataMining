using System;
using System.Collections.Generic;

namespace SentimentAnalysis
{
    internal static class TextParser
    {
        
        private static readonly Lazy<StanfordLemmatizer>  Lemmatizer = new Lazy<StanfordLemmatizer>();

        public static IList<string> SplitToWords(string sentence, bool lemmatize = true)
        {
            if (lemmatize)
            {
                return DoLemmatize(sentence);
            }
            return SplitToWordsNoLemmatize(sentence);
        }


        public static List<List<string>> SplitToWords(IEnumerable<string> sentences, bool lemmatize = true)
        {
            if (lemmatize)
            {
                return DoLemmatize(sentences);
            }
            var ret = new List<List<string>>();
            foreach (var sentence in sentences)
            {
                ret.Add(SplitToWordsNoLemmatize(sentence));
            }
            return ret;
        }

        private static List<string> SplitToWordsNoLemmatize(string sentence)
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
                    if (index > 0 && index < sentence.Length - 1 && Char.IsLetter(sentence[index - 1]) &&
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

            ProcessPart(sentence, ref currentWordStartIndex, ref currentWordLength, words);
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


        private static IList<string> DoLemmatize(string sentence)
        {
            return Lemmatizer.Value.TokenizeAndLemmatize(sentence);
        }

        private static List<List<string>> DoLemmatize(IEnumerable<string> sentences)
        {
            return Lemmatizer.Value.TokenizeAndLemmatize(sentences);
        }

    }
}
