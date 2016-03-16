using System;

namespace DataMining
{
    public class DoubleConverter : IValueConverter<double>
    {
        public Double Convert(object toConvert)
        {
            return System.Convert.ToDouble(toConvert);
        }
    }
}