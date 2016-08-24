using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Kangal.Attributes;

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

        public static class TableNameHelper
        {
            internal static string GetTableName(object entity,TableNameAttribute tableNameAttribute = null)
            {
                return string.IsNullOrEmpty(tableNameAttribute?.Name)
                    ? entity.GetType().Name
                    : tableNameAttribute.Name;
            }
        }
    }
}