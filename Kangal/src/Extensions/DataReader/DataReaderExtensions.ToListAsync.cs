using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Kangal;

/// <summary>
/// Provides extension methods for working with <see cref="IDataReader"/> objects,
/// enabling functionalities such as materializing data into entity collections asynchronously or synchronously.
/// </summary>
/// <remarks>
/// This class is split into multiple partial files to handle asynchronous and synchronous operations separately.
/// It aims to streamline data access patterns by mapping <see cref="IDataReader"/> fields to entity properties efficiently.
/// </remarks>
/// <threadsafety>
/// The methods in this class are thread-safe as long as the <see cref="IDataReader"/> object is not shared across threads.
/// </threadsafety>
public static partial class DataReaderExtensions
{
    /// <summary>
    /// Converts IDataReader records to a generic list asynchronously.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="reader">IDataReader to read from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities</returns>
    /// <exception cref="ArgumentException">Thrown when the column name is invalid or type casting fails</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
    /// <remarks>
    /// Performance: Property reflection is cached outside the row loop to minimize overhead.
    /// This method reads all records and materializes them immediately, allowing the reader to be disposed of.
    /// </remarks>
    public static async Task<List<T>> ToListAsync<T>(this IDataReader reader, CancellationToken cancellationToken = default) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        
        if (reader.FieldCount == 0)
        {
            return [];
        }

        if (reader is not SqlDataReader sqlReader)
        {
            return reader.ToList<T>();
        }

        var entities = new List<T>();
        var properties = typeof(T).GetProperties();

        var propertyMappings = BuildPropertyMappings<T>(properties);

        while (await sqlReader.ReadAsync(cancellationToken).ConfigureAwait(false) && !sqlReader.IsClosed)
        {
            var entity = new T();

            foreach (var mapping in propertyMappings)
            {
                try
                {
                    var columnValue = sqlReader[mapping.ColumnName];

                    mapping.Property.SetValue(entity, columnValue == DBNull.Value ? null : columnValue, null);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException($"Invalid column name: '{mapping.ColumnName}'. Column does not exist in the result set.");
                }
                catch (Exception ex) when (ex is not ArgumentException)
                {
                    var columnValue = sqlReader[mapping.ColumnName];
                    var valueTypeName = columnValue.GetType().FullName;
                    throw new ArgumentException($"Cannot cast value of type '{valueTypeName}' to property type '{mapping.Property.PropertyType.FullName}' for column '{mapping.ColumnName}'.", ex);
                }
            }

            entities.Add(entity);
        }

        return entities;
    }
}
