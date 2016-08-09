using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Kangal
{
    public static class DataExtensions
    {
        public static int Save(this SqlConnection connection, DataTable dataTable, string tableName)
        {
            if (dataTable == null) throw new ArgumentException("dataTable is null");
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("tableName is null");
            if (dataTable.Rows.Count == 0) return 0;
            var query = makeMeSaveQuery(dataTable, tableName);
            if (string.IsNullOrEmpty(query)) return 0;

            var command = connection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteNonQuery();
        }
       

        public static IEnumerable<T> ToList<T>(this IDataReader reader) where T :class, new()
        {
            if (reader == null || reader.FieldCount == 0) return Enumerable.Empty<T>();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable.ToList<T>();
        }

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

        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : class 
        {
            var dataTable = new DataTable();
            var enumerable = list as IList<T> ?? list.ToList();
            if (!enumerable.Any()) return dataTable;

            var properties = enumerable.FirstOrDefault().GetType().GetProperties();
            foreach (var property in properties)
            {
                var column = new DataColumn(property.Name,
                    property.PropertyType.Name.Contains("Nullable") ? typeof(object) : property.PropertyType);
                dataTable.Columns.Add(column);
            }

            foreach (var item in enumerable)
            {
                var values = item.GetType().GetProperties().Select(property => property.GetValue(item,null)).ToArray();
                dataTable.Rows.Add(values);
                Array.Clear(values, 0, values.Length);
            }
            return dataTable;
        }

        public static IEnumerable<DataTable> ToDataTable(this IDataReader reader)
        {
            
            if (reader == null || reader.FieldCount == 0) return Enumerable.Empty<DataTable>();

            var tableList = new List<DataTable>();

            while (reader.Read())
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);
                tableList.Add(dataTable);
                if(reader.IsClosed) break;
            }

            return tableList;
        }



        #region PRIVATE
        private static string makeMeSaveQuery(DataTable dataTable, string tableName)
        {
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

                var query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName,
                    string.Join(",", columnWithValues.Keys), string.Join(",", columnWithValues.Values));

                queries.Add(query);
                columnWithValues.Clear();
            }
            return string.Join("\n", queries);
        }

        private static object setSqlValue(object value)
        {
            if (value == DBNull.Value) return "NULL";
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

        #endregion
    }
}
