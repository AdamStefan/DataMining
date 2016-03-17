using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentAnalysis
{
    internal static class TextParser
    {
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
    }
}
