using System;
using System.Collections.Generic;
using System.Data;

namespace Kangal;

/// <summary>
/// Provides extension methods for working with <see cref="IDataReader"/> instances.
/// </summary>
public static partial class DataReaderExtensions
{
    /// <summary>
    /// Converts IDataReader to a collection of DataTables.
    /// </summary>
    /// <param name="reader">IDataReader to read from</param>
    /// <returns>IEnumerable of DataTable objects</returns>
    /// <remarks>
    /// This method creates one DataTable per result set in the reader.
    /// Useful for handling multiple result sets from stored procedures.
    /// </remarks>
    public static IEnumerable<DataTable> ToDataTable(this IDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        if (reader.FieldCount == 0)
        {
            yield return new DataTable();

            yield break;
        }
        
        do
        {
            var dataTable = new DataTable();
            dataTable.Load(reader);
            yield return dataTable;
        } while (!reader.IsClosed && reader.NextResult());
    }
}