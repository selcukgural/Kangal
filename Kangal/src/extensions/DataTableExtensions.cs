using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Kangal.Attributes;

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
                    if (string.IsNullOrEmpty(columnName)) continue;
                    var value = dataTable.Rows[i].ItemArray[j];

                    foreach (var property in generic.GetType().GetProperties())
                    {
                        var columnAliasAttribute =
                            property.GetCustomAttributes(typeof(ColumnAliasAttribute), false)
                                .Cast<ColumnAliasAttribute>()
                                .FirstOrDefault(e => e.Alias.Equals(columnName));

                        if (columnAliasAttribute != null)
                        {
                            var propetyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            property.SetValue(generic, (value == null || value == DBNull.Value) ? null : Convert.ChangeType(value, propetyType),null);
                            break;
                        }
                        var prop =
                            generic.GetType()
                                .GetProperties()
                                .FirstOrDefault(e => e.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                        if (prop == null || !prop.Name.Equals(columnName)) continue;
                        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        prop.SetValue(generic, (value == null || value == DBNull.Value) ? null : Convert.ChangeType(value, propType),null);
                        break;
                    }
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

        public static XDocument ToXDocument(this DataTable dataTable,XmlWriteMode xmlWriteMode = XmlWriteMode.IgnoreSchema,string nodeName = null,bool writeHierarchy = true)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) throw new ArgumentNullException(nameof(dataTable), "DataTable is null");
            dataTable.TableName  = string.IsNullOrEmpty(nodeName) ? "main" : nodeName;
            using (var stringWriter = new StringWriter())
            {
                dataTable.WriteXml(stringWriter, xmlWriteMode, writeHierarchy);
                return XDocument.Parse(stringWriter.ToString());
            }
        }

        public static string ToJson(this DataTable dataTable,JsonFormat jsonFormat = JsonFormat.Simple,JsonFormatSettings jsonFormatSettings = null)
        {
            var builder = new StringBuilder();
            builder.Append("[");

            var columnCounter = 0;

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                if (jsonFormat == JsonFormat.Showy) builder.AppendLine().Append("  ");
                builder.Append("{");
                if (jsonFormat == JsonFormat.Showy) builder.AppendLine().Append("    ");
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    builder.Append($@"""{dataTable.Columns[j]}"":{setJsonValueWithFormat(dataTable.Rows[i][j],jsonFormatSettings,jsonFormat)},");
                    columnCounter++;
                    if (columnCounter == dataTable.Columns.Count) builder.Remove(builder.Length - 1, 1);
                    if (jsonFormat == JsonFormat.Showy) builder.AppendLine().Append("    ");
                }
                columnCounter = 0;
                if (jsonFormat == JsonFormat.Showy) builder.Remove(builder.Length - 2, 2);
                builder.Append("},");
            }
            builder.Remove(builder.Length - 1, 1);
            if (jsonFormat == JsonFormat.Showy) builder.AppendLine();
            builder.Append("]");
            return builder.ToString();
        }

        private static string setJsonValueWithFormat(object value,JsonFormatSettings jsonFormatSettings,JsonFormat jsonFormat)
        {

            if (value == null || value == DBNull.Value) return "null";

            var typeName = value.GetType().Name;
            switch (typeName)
            {
                case "Boolean":
                    return value.ToString().ToLower();
                case "DateTime":
                    return string.IsNullOrEmpty(jsonFormatSettings?.DateTimeFormat) ? $@"""{DateTime.Parse(value.ToString()).ToLocalTime()}""" : $@"""{DateTime.Parse(value.ToString()).ToString(jsonFormatSettings.DateTimeFormat)}""";
                case "Decimal":
                    return string.IsNullOrEmpty(jsonFormatSettings?.DecimalFormat)
                        ? value.ToString().Replace(",", ".")
                        : decimal.Parse(value.ToString()).ToString(jsonFormatSettings.DecimalFormat);
                case "Double":
                    return string.IsNullOrEmpty(jsonFormatSettings?.DoubleFormat)
                        ? value.ToString().Replace(",",".")
                        : double.Parse(value.ToString()).ToString(jsonFormatSettings.DoubleFormat);
                case "SqlHierarchyId":
                    var sqlHierarchyId = value.ToString();
                    var builder = new StringBuilder();
                    if (jsonFormat == JsonFormat.Showy)
                    {
                        return sqlHierarchyId.Equals("NULL")
                            ? builder.AppendLine("{")
                                .Append("      ")
                                .AppendLine(@"""IsNull"":true")
                                .Append("  }")
                                .ToString()
                            : builder.AppendLine("{")
                                .Append("      ")
                                .AppendLine(@"""IsNull"":false")
                                .Append("  }")
                                .ToString();
                    }
                    return sqlHierarchyId.Equals("NULL")
                        ? builder.Append(@"{""IsNull"":true}").ToString()
                        : builder.Append(@"{""IsNull"":false}").ToString();
                case "Int32":
                case "Int64":
                case "Int16":
                case "Byte":
                case "Single":
                    return value.ToString();
                case "String":
                case "Char":
                case "Date":
                case "Guid":
                case "TimeSpan":
                case "DateTime2":
                case "DateTimeOffset":
                case "Xml":
                    return $@"""{value
                        .ToString()
                        .Replace("\\", "\\\\")
                        .Replace(@"""", @"\""")
                        .Replace("\r", "\\r")
                        .Replace("\n", "\\n")
                        .Replace(@"\d", "\\d")
                        .Replace("\t","\\t")
                        .Replace("\f","\\f")
                        .Replace(@"\u","\\u")}""";
                default:
                     throw new ArgumentException($"{typeName} data type not support yet");

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