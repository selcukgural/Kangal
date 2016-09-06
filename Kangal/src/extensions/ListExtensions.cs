using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kangal.Attributes;

namespace Kangal
{
    public static class ListExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : class
        {
            var dataTable = new DataTable();
            var enumerable = list as IList<T> ?? list.ToList();
            if (!enumerable.Any()) return dataTable;

            var firstOrDefault = enumerable.FirstOrDefault();
            if (firstOrDefault == null) throw new ArgumentNullException(nameof(list));

            var properties = firstOrDefault.GetType().GetProperties();
            foreach (var property in properties)
            {
                var columnAlias = (ColumnAliasAttribute)property.GetCustomAttributes(typeof(ColumnAliasAttribute), false).FirstOrDefault();
                var propertyName = columnAlias?.Alias ?? property.Name;
                var column = new DataColumn(propertyName,
                    property.PropertyType.Name.Contains("Nullable") ? typeof(object) : property.PropertyType);
                dataTable.Columns.Add(column);
            }
            foreach (var item in enumerable)
            {
                var values = item.GetType().GetProperties().Select(property => property.GetValue(item, null)).ToArray();
                dataTable.Rows.Add(values);
                Array.Clear(values, 0, values.Length);
            }
            return dataTable;
        }
        internal static string MakeMeSaveQuery<T>(this IEnumerable<T> entities, string tableName = null) where T : class
        {
            var columnWithValues = new Dictionary<string, object>();
            var queries = new List<string>();
            foreach (var entity in entities)
            {
                var tableAtt = (TableNameAttribute)entity.GetType().GetCustomAttributes(typeof(TableNameAttribute), false).FirstOrDefault();
                tableName = string.IsNullOrEmpty(tableName) ? Helpers.TableNameHelper.GetTableName(entity, tableAtt) : tableName;

                foreach (var property in entity.GetType().GetProperties())
                {
                    var ignoreAtt = (IgnoreAttribute)property.GetCustomAttributes(typeof(IgnoreAttribute), false).FirstOrDefault();
                    if(ignoreAtt != null) continue;
                    var columnAtt = (ColumnAliasAttribute)property.GetCustomAttributes(typeof(ColumnAliasAttribute), false).FirstOrDefault();
                    var columnName = string.IsNullOrEmpty(columnAtt?.Alias) ? property.Name : columnAtt.Alias;
                    if (columnWithValues.ContainsKey(columnName))
                    {
                        throw new ArgumentException($"This column name already exists: {columnName}");
                    }
                    columnWithValues.Add(columnName, property.GetValue(entity, null).ToSqlString());
                }
                var query =
                    $"INSERT INTO {tableName} ({string.Join(",", columnWithValues.Keys)}) VALUES ({string.Join(",", columnWithValues.Values)});";
                columnWithValues.Clear();
                queries.Add(query);
            }
            return string.Join("\n", queries);
        }
    }
}