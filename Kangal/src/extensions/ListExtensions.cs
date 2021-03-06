﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Kangal.Attributes;

namespace Kangal
{
    public static class ListExtensions
    {
        /// <summary>
        /// The Generic List convert to DataTable
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="entityList">Entities</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> entityList) where T : class
        {
            var dataTable = new DataTable();
            var entities = entityList as IList<T> ?? entityList.ToList();
            if (!entities.Any()) return dataTable;

            var firstEntity = entities.FirstOrDefault();
            if (firstEntity == null) throw new ArgumentNullException(nameof(entityList));

            var properties = firstEntity.GetType().GetProperties();
            foreach (var property in properties)
            {
                var ignoreAttribute = property.GetCustomAttribute(typeof(IgnoreAttribute), false);
                if(ignoreAttribute != null) continue;
                var columnAlias = (ColumnAliasAttribute)property.GetCustomAttribute(typeof(ColumnAliasAttribute), false);
                var propertyName = columnAlias?.Alias ?? property.Name;
                var column = new DataColumn(propertyName,
                    property.PropertyType.Name.Contains("Nullable") ? typeof(object) : property.PropertyType);
                dataTable.Columns.Add(column);
            }
            foreach (var entity in entities)
            {
                var values = entity.GetType().GetProperties().Where(e => e.GetCustomAttribute(typeof(IgnoreAttribute),false) == null).Select(e=> e.GetValue(entity,null)).ToArray();
                dataTable.Rows.Add(values);
                Array.Clear(values, 0, values.Length);
            }
            return dataTable;
        }
        /// <summary>
        /// The Entities convert to XDocument.
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="entities">Entities</param>
        /// <param name="rootName">Root name</param>
        /// <returns>XDocument</returns>
        public static XDocument ToXDocument<T>(this IEnumerable<T> entities,string rootName) where T : class
        {
            if (string.IsNullOrEmpty(rootName)) throw new ArgumentNullException(nameof(rootName));

            var xDocument = new XDocument();
            var nodeName = typeof(T).Name;
            xDocument.Add(new XElement(rootName));
            foreach (var entity in entities)
            {
                var xElement = new XElement(nodeName);
                foreach (var property in entity.GetType().GetProperties())
                {
                    var ignoreAttribute = (IgnoreAttribute)property.GetCustomAttribute(typeof(IgnoreAttribute), false);
                    if (ignoreAttribute != null) continue;
                    var columnAliasAttribute = (ColumnAliasAttribute)property.GetCustomAttribute(typeof(ColumnAliasAttribute), false);
                    var columnName = string.IsNullOrEmpty(columnAliasAttribute?.Alias) ? property.Name : columnAliasAttribute.Alias;
                    xElement.Add(new XElement(columnName, property.GetValue(entity, null)));
                }
                xDocument.Root?.Add(xElement);
            }
            return xDocument;
        }
        internal static string MakeMeSaveQuery<T>(this IEnumerable<T> entities, string tableName = null) where T : class
        {
            var columnWithValues = new Dictionary<string, object>();
            var queries = new List<string>();
            foreach (var entity in entities)
            {
                var tableNameAttribute =
                    (TableNameAttribute) entity.GetType().GetCustomAttribute(typeof(TableNameAttribute), false);
                tableName = string.IsNullOrEmpty(tableName) ? Helpers.TableNameHelper.GetTableName(entity, tableNameAttribute) : tableName;

                foreach (var property in entity.GetType().GetProperties())
                {
                    var ignoreAttribute = (IgnoreAttribute)property.GetCustomAttribute(typeof(IgnoreAttribute), false);
                    if(ignoreAttribute != null) continue;
                    var columnAliasAttribute = (ColumnAliasAttribute)property.GetCustomAttribute(typeof(ColumnAliasAttribute), false);
                    var columnName = string.IsNullOrEmpty(columnAliasAttribute?.Alias) ? property.Name : columnAliasAttribute.Alias;
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