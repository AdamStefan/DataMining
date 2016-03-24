using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using ikvm.@internal;
using java.util;

namespace SentimentAnalysis
{
    public class StanfordLemmatizer
    {

        private readonly StanfordCoreNLP _pipeline;
        private readonly string _separator;
        

        public StanfordLemmatizer()
        {
            // Path to the folder with models extracted from `stanford-corenlp-3.6.0-models.jar`
            var jarRoot = @"C:\Work\NLP\Stanford\stanford-corenlp-full-2015-12-09\stanford-corenlp-3.6.0-models";
            _separator = Guid.NewGuid().ToString();

            // Text for processing            
            // Annotation pipeline configuration
            var props = new Properties();
            //props.setProperty("annotators", "tokenize, ssplit, pos, lemma, parse, ner,dcoref");
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, parse, ner");
            props.setProperty("ner.useSUTime", "0");

            // We should change current directory, so StanfordCoreNLP could find all the model files automatically
            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(jarRoot);
            _pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);
        }


        private string GetSeparator()
        {
            return _separator;
        }

        public List<string> TokenizeAndLemmatize(string documentText)
        {
            var annotation = new Annotation(documentText);
            _pipeline.annotate(annotation);

            var ret = new List<string>();

            var tokenKey = ClassLiteral<CoreAnnotations.TokensAnnotation>.Value;
            var lemmaKey = ClassLiteral<CoreAnnotations.LemmaAnnotation>.Value;

            var tokenItems = annotation.get(tokenKey) as ArrayList;

            if (tokenItems == null)
            {
                return ret;
            }

            ret.AddRange(tokenItems.OfType<CoreLabel>().Select(tmp => (string)tmp.get(lemmaKey)));

            return ret;
        }
        

        public List<List<string>> TokenizeAndLemmatize(IEnumerable<string> documentTexts)
        {
            var sb = new StringBuilder();
            var separator = GetSeparator();
            var ret = new List<List<string>>();

            documentTexts.ToList().ForEach(item =>
            {
                var value = item.Replace(separator, string.Empty);
                sb.Append(separator);
                sb.Append(value);
                
            });


            var annotation = new Annotation(sb.ToString());
            _pipeline.annotate(annotation);

            //var ret = new List<string>();

            var tokenKey = ClassLiteral<CoreAnnotations.TokensAnnotation>.Value;
            var lemmaKey = ClassLiteral<CoreAnnotations.LemmaAnnotation>.Value;

            var tokenItems = annotation.get(tokenKey) as ArrayList;

            if (tokenItems == null)
            {
                return null;
            }

            foreach (var coreLabel in tokenItems.OfType<CoreLabel>())
            {
                var token = (string) coreLabel.get(lemmaKey);
                if (token == separator)
                {
                    ret.Add(new List<string>());
                }
                else
                {
                    ret[ret.Count-1].Add(token);
                }
            }
            

            return ret;
        }
    }
}
