﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Kangal.Attributes;

namespace Kangal
{
    public static class DataReaderExtensions
    {
        /// <summary>
        /// In the records IDataReader object returns back as generic list 
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="reader">IDataReader</param>
        /// <returns>IEnumerable<T/></returns>
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
                    var columnAttribute =
                        (ColumnAliasAttribute)propertyCustomAttributes.FirstOrDefault(e => e.GetType() == typeof(ColumnAliasAttribute));
                    var columnName = !string.IsNullOrEmpty(columnAttribute?.Alias) ? columnAttribute.Alias : property.Name;
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

        /// <summary>
        /// In the records IDataReader object returns back as XDocument
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <param name="rootName">Root name</param>
        /// <param name="nodeName">Node name</param>
        /// <returns>ToXDocument</returns>
        public static XDocument ToXDocument(this IDataReader reader,string rootName,string nodeName)
        {
            if (string.IsNullOrEmpty(rootName)) throw new ArgumentNullException(nameof(rootName));
            if (string.IsNullOrEmpty(nodeName)) throw new ArgumentNullException(nameof(nodeName));

            var xDocument = new XDocument(new XElement(rootName));
            while (reader.Read())
            {
                var xElement = new XElement(nodeName);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if(reader.IsDBNull(i)) continue;
                    xElement.Add(new XElement(reader.GetName(i), reader[i]));
                }
                xDocument.Root?.Add(xElement);
            }
            return xDocument;
        }
        /// <summary>
        /// In the records IDataReader object returns back as IEnumerable&lt;DataTable&gt; 
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <returns>IEnumerable&lt;DataTable&gt;</returns>
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
