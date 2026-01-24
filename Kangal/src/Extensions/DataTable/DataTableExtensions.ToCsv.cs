using System;
using System.Data;
using System.Text;

namespace Kangal;

/// <summary>
/// Provides extension methods for operating on DataTable instances.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Converts DataTable content to CSV formatted string.
    /// </summary>
    /// <param name="dataTable">DataTable to convert</param>
    /// <param name="comma">Delimiter character (default: ";")</param>
    /// <param name="ignoreNull">If true, null values are omitted; if false, they are represented as "NULL"</param>
    /// <returns>CSV formatted string</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataTable is null or empty</exception>
    public static string ToCsv(this DataTable dataTable, string? comma = null, bool ignoreNull = false)
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        comma = string.IsNullOrEmpty(comma) ? ";" : comma;
        var csvBuilder = new StringBuilder();

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            for (var j = 0; j < dataTable.Columns.Count; j++)
            {
                var value = dataTable.Rows[i][j];
                var stringValue = value == DBNull.Value ? "" : value.ToString() ?? "";

                if (string.IsNullOrEmpty(stringValue))
                {
                    if (!ignoreNull)
                    {
                        csvBuilder.Append("NULL");
                    }
                }
                else
                {
                    var needsQuotes = stringValue.Contains(comma) || stringValue.Contains('"') || stringValue.Contains('\n') || stringValue.Contains('\r');
                    if (needsQuotes)
                    {
                        csvBuilder.Append('"');
                        csvBuilder.Append(stringValue.Replace("\"", "\"\""));
                        csvBuilder.Append('"');
                    }
                    else
                    {
                        csvBuilder.Append(stringValue);
                    }
                }

                if (j < dataTable.Columns.Count - 1)
                {
                    csvBuilder.Append(comma);
                }
            }

            if (i < dataTable.Rows.Count - 1)
            {
                csvBuilder.AppendLine();
            }
        }

        return csvBuilder.ToString();
    }
}