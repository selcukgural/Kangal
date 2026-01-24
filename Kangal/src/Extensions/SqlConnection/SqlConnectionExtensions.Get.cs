using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="SqlConnection"/> class.
/// </summary>
public static partial class SqlConnectionExtensions
{
    /// <summary>
    /// Execute a SELECT query and return results as strongly typed entities.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="connection">SqlConnection</param>
    /// <param name="query">SELECT query to execute</param>
    /// <returns>Enumerable of entities</returns>
    /// <exception cref="ArgumentNullException">Thrown when a query is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
    /// <remarks>
    /// This method properly disposes of the IDataReader and prevents connection leaks.
    /// The reader is materialized immediately to allow connection reuse.
    /// Connection state is automatically managed - if the connection is closed, it will be opened and closed after the operation.
    /// </remarks>
    public static IEnumerable<T> Get<T>(this SqlConnection connection, string query) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var shouldCloseConnection = EnsureConnectionOpen(connection);
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = command.ExecuteReader();
            return reader.ToList<T>();
        }
        finally
        {
            if (shouldCloseConnection && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }

    /// <summary>
    /// Execute a SELECT query asynchronously and return results as strongly typed entities.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="connection">SqlConnection</param>
    /// <param name="query">SELECT query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerable of entities</returns>
    /// <exception cref="ArgumentNullException">Thrown when a query is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database operation fails</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
    /// <remarks>
    /// This method properly disposes of the IDataReader and prevents connection leaks.
    /// The reader is materialized immediately to allow connection reuse.
    /// Connection state is automatically managed - if the connection is closed, it will be opened and closed after the operation.
    /// </remarks>
    public static async Task<IEnumerable<T>> GetAsync<T>(this SqlConnection connection, string query, CancellationToken cancellationToken = default) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var shouldCloseConnection = EnsureConnectionOpen(connection);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = query;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.ToListAsync<T>(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (shouldCloseConnection && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
