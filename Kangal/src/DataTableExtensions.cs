using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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

        public static void ChangeColumnName(this DataTable dataTable, string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName)) throw new ArgumentNullException(nameof(oldName));
            if (string.IsNullOrEmpty(newName)) throw new ArgumentNullException(nameof(newName));

            if(oldName.Equals(newName)) throw new ArgumentException("old and new column name the same");

            var colIndex = dataTable.Columns.IndexOf(oldName);
            if (colIndex.Equals(-1)) throw new ArgumentNullException(nameof(oldName), "column not found");
            dataTable.Columns[colIndex].ColumnName = newName;
            dataTable.AcceptChanges();
        }

        public static void RemoveColumn(this DataTable dataTable, string columnName)
        {
            if(string.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

            var colIndex = dataTable.Columns.IndexOf(columnName);
            if (colIndex.Equals(-1)) throw new ArgumentNullException(nameof(columnName),"column not found");
            dataTable.Columns.RemoveAt(colIndex);
            dataTable.AcceptChanges();
        }

        public static string ToCsv(this DataTable dataTable, string comma = null,bool ignoreNull = false)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) throw new ArgumentNullException(nameof(dataTable), "DataTable is null");

            comma = string.IsNullOrEmpty(comma) ? ";" : comma;
            var csv = string.Empty;
            var cvsList = new List<string>();

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var value = $"{dataTable.Rows[i][j]}";
                    if (string.IsNullOrEmpty(value) && ignoreNull) { continue; }
                    if (string.IsNullOrEmpty(value) && !ignoreNull){csv += $"NULL{comma}";}
                    if (!string.IsNullOrEmpty(value)) { csv += value + comma;}
                }
                cvsList.Add(csv.Remove(csv.Length - 1));
                csv = string.Empty;
            }
            return string.Join("\n", cvsList);
        }

        public static XDocument ToXml(this DataTable dataTable,XmlWriteMode xmlWriteMode = XmlWriteMode.IgnoreSchema,string nodeName = null,bool writeHierarchy = true)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) throw new ArgumentNullException(nameof(dataTable), "DataTable is null");
            dataTable.TableName  = string.IsNullOrEmpty(nodeName) ? "main" : nodeName;
            using (var stringWriter = new StringWriter())
            {
                dataTable.WriteXml(stringWriter, xmlWriteMode, writeHierarchy);
                return XDocument.Parse(stringWriter.ToString());
            }
        }
        internal static string MakeMeSaveQuery(this DataTable dataTable, string tableName)
        {
            if (dataTable == null || dataTable.Rows.Count.Equals(0)) return string.Empty;
            if(string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName),"cannot be null table name");

            var columnWithValues = new Dictionary<string, object>();
            var queries = new List<string>();
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var columnName = dataTable.Columns[j].ColumnName;
                    var value = dataTable.Rows[i].ItemArray[j];
                    columnWithValues.Add(columnName, value.ToSqlString());
                }

                var query =
                    $"INSERT INTO {tableName} ({string.Join(",", columnWithValues.Keys)}) VALUES ({string.Join(",", columnWithValues.Values)});";

                queries.Add(query);
                columnWithValues.Clear();
            }
            return string.Join("\n", queries);
        }
    }
}