using NHunspell;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SentimentAnalysis
{
    internal static class TextParser
    {
        
        private static readonly Lazy<StanfordLemmatizer>  Lemmatizer = new Lazy<StanfordLemmatizer>();
        private static Hunspell _spell;
        private  static Regex _multipleCharacterRegex = new Regex( "(.)\\1{1,}");

        public static IList<string> SplitToWords(string sentence, bool correct = false)
        {
            //if (lemmatize)
            //{
            //    return DoLemmatize(sentence);
            //}
            var words = SplitToWordsNoLemmatize(sentence);

            if (_spell == null)
            {
                _spell = new Hunspell("en_us.aff", "en_us.dic");
            }
            var stems = new List<string>();
            foreach (var word in words)
            {
                var tmpWord = _multipleCharacterRegex.Replace(word, "$1$1");
                if (correct)
                {
                    var correctlySpelled = _spell.Spell(word);
                    if (!correctlySpelled)
                    {
                        var tmp = _spell.Suggest(word);
                        if (tmp != null && tmp.Count == 1)
                        {
                            tmpWord = tmp[0];
                        }
                    }
                }

                var wordStems = _spell.Stem(tmpWord);
                if (wordStems.Count > 0)
                {
                    stems.AddRange(wordStems);
                }
                else
                {
                    stems.Add(word);
                }

            }

            return stems;
        }

        //private static IEnumerable<string> ProcessCustom(string word)
        //{
        //    if (word.ToLower == "im")
        //    {
        //        yield "i"
        //    }
        //}
        


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

        public static string ReplaceMoreThanTwoSameLetters(string text)
        {            
            return _multipleCharacterRegex.Replace(text, "$1$1");            
           
        }

    }
}
