using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining
{
    public struct Statistics
    {
        public double Entropy { get; set; }
        public bool SameClass { get; set; }
        public double Confidence { get; set; }
        public int[] Frequencies { get; set; }
        public int DatasetLength { get; set; }
        public object MostFrequentClass { get; set; }
        public string[] Classes { get; set; }                
    }
}
