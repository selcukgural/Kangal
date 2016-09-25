using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Kangal.Attributes;

namespace Kangal
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<T> ToList<T>(this IDataReader reader) where T :class, new()
        {
            if (reader == null || reader.FieldCount == 0) return Enumerable.Empty<T>();
            var entities = new List<T>();
            while (reader.Read() && !reader.IsClosed)
            {
                var entity = new T();
                foreach (var property in entity.GetType().GetProperties())
                {
                    var propertyCustomAttributes = property.GetCustomAttributes().ToList();
                    var ignoreAttribute = propertyCustomAttributes.FirstOrDefault(e => e.GetType() == typeof(IgnoreAttribute));
                    if (ignoreAttribute != null) continue;
                    var colomnAttribute =
                        (ColumnAliasAttribute)propertyCustomAttributes.FirstOrDefault(e => e.GetType() == typeof(ColumnAliasAttribute));
                    var columnName = !string.IsNullOrEmpty(colomnAttribute?.Alias) ? colomnAttribute.Alias : property.Name;
                    var columnValue = new object();
                    try
                    {
                        columnValue = reader[columnName];
                        property.SetValue(entity, columnValue, null);
                    }
                    catch (Exception ex) when (ex is IndexOutOfRangeException)
                    {
                        throw new ArgumentException($"Invalid column name {columnName}");
                    }
                    catch
                    {
                        throw new ArgumentException($"{columnValue.GetType().FullName} cannot cast {property.PropertyType.FullName}");
                    }
                }
                entities.Add(entity);
            }
            return entities;
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
                if (!reader.IsClosed) continue;
                reader.Close();
                break;
            }
            return tableList;
        }
    }
}
