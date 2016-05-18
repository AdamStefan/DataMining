using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
