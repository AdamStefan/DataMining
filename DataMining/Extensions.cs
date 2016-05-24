using System;
using System.Collections.Generic;

namespace DataMining
{
    public static class Extensions
    {
        public static bool TryConvertToNumeric(this object value, out double numericValue)
        {
            numericValue = -1;
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            if (type == typeof (string))
            {
                return double.TryParse((string) value, out numericValue);
            }

            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {                
                case TypeCode.Char:                    
                case TypeCode.SByte:                    
                case TypeCode.Byte:                    
                case TypeCode.Int16:                    
                case TypeCode.UInt16:                    
                case TypeCode.Int32:                    
                case TypeCode.UInt32:                    
                case TypeCode.Int64:                    
                case TypeCode.UInt64:                    
                case TypeCode.Single:                    
                case TypeCode.Double:                    
                case TypeCode.Decimal:
                    numericValue = Convert.ToDouble(value);
                    return true;                    
                case TypeCode.DateTime:
                    numericValue = Convert.ToDateTime(value).Ticks;
                    return true;                                    
            }            

            return false;
        }

        public static bool IsNumeric(this object value)
        {            
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            if (type == typeof(string))
            {
                double numericValue;
                return double.TryParse((string)value, out numericValue);
            }

            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:                    
                    return true;
                case TypeCode.DateTime:                    
                    return true;
            }

            return false;
        }        

        public static IEnumerable<T[]> Split<T>(this T[] collection, int index)
        {
            if (index < 0)
            {
                throw new ArgumentException("index");
            }

            var left = new T[index + 1];
            var right = new T[collection.Length - index - 1];

            Array.Copy(collection, left, index + 1);
            Array.Copy(collection, index + 1, right, 0, collection.Length - index - 1);

            return new[] { left, right };
        }

        public static int[] Sample(this int[] source, int numberOfItems)
        {
            var ret = new int[numberOfItems];
            Sample(source, ret);

            return ret;
        }

        public static void Sample(this int[] source, int[] dest)
        {
            Random random = new Random();
            Buffer.BlockCopy(source, 0, dest, 0, dest.Length*sizeof (int));

            for (int index = dest.Length; index < source.Length; index++)
            {
                var newIndex = random.Next(0, index + 1);
                
                if (newIndex < dest.Length)
                {
                    dest[newIndex] = source[index];
                }
            }
        }

        public static int IndexOfMax(this double[] source)
        {
            var maxEstimate = Double.MinValue;
            var ret = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] > maxEstimate)
                {
                    maxEstimate = source[i];
                    ret = i;
                }
            }

            return ret;
        }


    }
}
