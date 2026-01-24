using System;
using System.Collections.Generic;
using System.Data;

namespace Kangal;

/// <summary>
/// Provides extension methods for working with <see cref="IDataReader"/> objects.
/// </summary>
/// <remarks>
/// These extensions simplify the process of mapping data from an <see cref="IDataReader"/>
/// to strongly typed objects, supporting features like property reflection, mappings,
/// and handling of nullable database values. The provided methods are particularly useful
/// for working with database results when a lightweight alternative to ORMs is desired.
/// </remarks>
public static partial class DataReaderExtensions
{
    /// <summary>
    /// Converts IDataReader records to a generic list.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="reader">IDataReader to read from</param>
    /// <returns>IEnumerable of entities</returns>
    /// <exception cref="ArgumentException">Thrown when the column name is invalid or type casting fails</exception>
    /// <remarks>
    /// Performance: Property reflection is cached outside the row loop to minimize overhead.
    /// This method reads all records and materializes them immediately, allowing the reader to be disposed of.
    /// </remarks>
    public static IEnumerable<T> AsIEnumerable<T>(this IDataReader reader) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        
        if (reader.FieldCount == 0)
        {
            yield break;
        }

        var properties = typeof(T).GetProperties();

        var propertyMappings = BuildPropertyMappings<T>(properties);

        while (reader.Read() && !reader.IsClosed)
        {
            var entity = new T();

            foreach (var mapping in propertyMappings)
            {
                var columnValue = reader[mapping.ColumnName];

                mapping.Property.SetValue(entity, columnValue == DBNull.Value ? null : columnValue, null);

                yield return entity;
            }
        }
    }
}