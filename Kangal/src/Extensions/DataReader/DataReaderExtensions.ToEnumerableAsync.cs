using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace Kangal;

/// <summary>
/// Provides extension methods for working with instances of <see cref="IDataReader"/> for efficient data access.
/// </summary>
/// <remarks>
/// This class contains methods to simplify the process of converting data read from a database
/// into entity objects, particularly by using <see cref="SqlDataReader"/> asynchronously.
/// Reflection is used to map database columns to entity properties.
/// </remarks>
public static partial class DataReaderExtensions
{
    /// <summary>
    /// Converts IDataReader records to a generic list asynchronously.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="reader">IDataReader to read from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IEnumerable of entities</returns>
    /// <exception cref="ArgumentException">Thrown when the column name is invalid or type casting fails</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
    /// <remarks>
    /// Performance: Property reflection is cached outside the row loop to minimize overhead.
    /// This method reads all records and materializes them immediately, allowing the reader to be disposed of.
    /// </remarks>
    public static async IAsyncEnumerable<T> AsIEnumerableAsync<T>(this IDataReader reader, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        
        if (reader.FieldCount == 0)
        {
            yield break;
        }

        if (reader is not SqlDataReader sqlReader)
        {
            yield break;
        }

        var properties = typeof(T).GetProperties();

        var propertyMappings = BuildPropertyMappings<T>(properties);

        while (await sqlReader.ReadAsync(cancellationToken).ConfigureAwait(false) && !sqlReader.IsClosed)
        {
            var entity = new T();

            foreach (var mapping in propertyMappings)
            {
                var columnValue = sqlReader[mapping.ColumnName];

                mapping.Property.SetValue(entity, columnValue == DBNull.Value ? null : columnValue, null);

                yield return entity;
            }
        }
    }
}