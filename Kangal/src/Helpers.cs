using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Kangal
{
    internal static class Helpers
    {
        public static class SqlParameterHelper
        {
            public static SqlParameter[] CreateSqlParameters(IEnumerable<PropertyInfo> propertyInfos, object item)
            {
                var properties = propertyInfos as IList<PropertyInfo> ?? propertyInfos.ToList();
                if (properties == null || !properties.Any()) throw new ArgumentException("propertyInfos");

                return properties.Select(property => new SqlParameter
                {
                    DbType = property.PropertyType.GetDbType(),
                    Value = property.GetValue(item, null),
                    ParameterName = $@"{property.Name}"
                }).ToArray();
            }
        }

        public static class QueryHelper
        {
            public enum QueryType : byte
            {
                INSERT = 0,
                DELETE = 1,
                UPDATE = 2,
                SELECT = 3
            }
        }
    }
}