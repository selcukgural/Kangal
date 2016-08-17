using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
                var column = new DataColumn(property.Name,
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
    }
}