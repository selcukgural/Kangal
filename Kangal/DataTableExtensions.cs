using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kangal
{
    public static class DataTableExtensions
    {
        public static IEnumerable<T> ToList<T>(this DataTable dataTable) where T : new()
        {
            if (dataTable?.Rows == null || dataTable.Rows.Count == 0) return Enumerable.Empty<T>();

            var rowsCount = dataTable.Rows.Count;
            var genericList = new List<T>(rowsCount);

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var generic = new T();
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var columnName = dataTable.Columns[j].ColumnName;
                    var value = dataTable.Rows[i].ItemArray[j];
                    var property =
                        generic.GetType()
                            .GetProperties()
                            .FirstOrDefault(e => e.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    property?.SetValue(generic, value == DBNull.Value ? null : value, null);
                }
                genericList.Add(generic);
            }
            return genericList;
        }
        internal static string MakeMeSaveQuery(this DataTable dataTable, string tableName)
        {
            if (dataTable == null || dataTable.Rows.Count.Equals(0)) return string.Empty;
            if(string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            var columnWithValues = new Dictionary<string, object>();
            var queries = new List<string>();
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var columnName = dataTable.Columns[j].ColumnName;
                    var value = dataTable.Rows[i].ItemArray[j];
                    columnWithValues.Add(columnName, setSqlValue(value));
                }

                var query =
                    $"INSERT INTO {tableName} ({string.Join(",", columnWithValues.Keys)}) VALUES ({string.Join(",", columnWithValues.Values)});";

                queries.Add(query);
                columnWithValues.Clear();
            }
            return string.Join("\n", queries);
        }

        private static object setSqlValue(object value)
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
                default:
                    return value;
            }
        }
    }
}