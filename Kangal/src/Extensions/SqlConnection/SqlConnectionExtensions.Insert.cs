using System;
using System.Collections.Generic;
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
    /// Inserts a single entity into a database using parameterized queries.
    /// </summary>
    /// <typeparam name="T">The type of the entity to insert.</typeparam>
    /// <param name="connection">The SqlConnection to be used for the database operation.</param>
    /// <param name="entity">The entity to insert into the database.</param>
    /// <param name="transaction">An optional SqlTransaction. If provided, the caller is responsible for committing or rolling back the transaction.</param>
    /// <param name="tableName">An optional table name. If not specified, the entity type name or a TableName attribute will be used.</param>
    /// <returns>The number of rows affected (1 if the operation is successful).</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database operation fails.</exception>
    public static int Insert<T>(this SqlConnection connection, T entity, SqlTransaction? transaction = null, string? tableName = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entities = new List<T> { entity };
        return connection.InsertBatch(entities, transaction, tableName, DefaultBatchSize);
    }

    /// <summary>
    /// Inserts a single entity into a database asynchronously using parameterized queries.
    /// </summary>
    /// <typeparam name="T">The type of the entity to insert.</typeparam>
    /// <param name="connection">The SqlConnection to be used for the database operation.</param>
    /// <param name="entity">The entity to insert into the database.</param>
    /// <param name="transaction">An optional SqlTransaction. If provided, the caller is responsible for committing or rolling back the transaction.</param>
    /// <param name="tableName">An optional table name. If not specified, the entity type name or a TableName attribute will be used.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation, with an integer result representing the number of rows affected (1 if the operation is successful).</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database operation fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
    public static Task<int> InsertAsync<T>(this SqlConnection connection, T entity, SqlTransaction? transaction = null, string? tableName = null,
                                           CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entities = new List<T> { entity };
        return connection.InsertBatchAsync(entities, transaction, tableName, DefaultBatchSize, cancellationToken);
    }
}
