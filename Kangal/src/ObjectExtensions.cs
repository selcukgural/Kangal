using System;

namespace Kangal
{
    internal static class ObjectExtensions
    {
        public static object ToSqlString(this object value)
        {
            if (value == DBNull.Value || value == null) return "NULL";
            var type = value.GetType();
            switch (type.Name)
            {
                case "Xml":
                case "Char":
                case "Guid":
                case "String":
                case "DateTime":
                case "TimeSpan":
                    return "'" + value + "'";
                case "Boolean":
                    return (bool) value ? 1 : 0;
                default:
                    return value;                  
            }
        }
    }
}