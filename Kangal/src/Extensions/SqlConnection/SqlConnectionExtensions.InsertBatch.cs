using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Inserts multiple entities into a database using parameterized queries.
    /// </summary>
    /// <typeparam name="T">The type of entities to be inserted.</typeparam>
    /// <param name="connection">The SqlConnection to the database.</param>
    /// <param name="entities">The collection of entities to insert.</param>
    /// <param name="transaction">Optional SqlTransaction for managing the operation. The caller must handle commit/rollback.</param>
    /// <param name="tableName">An optional table name. If not specified, uses the entity type name or a TableName attribute if present.</param>
    /// <param name="batchSize">The maximum number of entities to process in a single batch (default: 1000).</param>
    /// <returns>The total number of rows affected by the insert operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the entities collection is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a database operation fails.</exception>
    /// <remarks>
    /// This method employs parameterized queries to mitigate the risk of SQL injection.
    /// Transaction handling: When a transaction is provided, the caller is responsible for managing its commit or rollback.
    /// Efficiency: Processes large collections in batches to avoid exceeding database parameter limits.
    /// </remarks>
    public static int InsertBatch<T>(this SqlConnection connection, IEnumerable<T> entities, SqlTransaction? transaction = null, string? tableName = null,
                                int batchSize = DefaultBatchSize) where T : class
    {
        var entityList = entities.ToArray();

        if (entityList.Length == 0)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var totalAffected = 0;
        var entityCount = entityList.Length;

        for (var batchStart = 0; batchStart < entityCount; batchStart += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, entityCount - batchStart);
            var batch = entityList.Skip(batchStart).Take(currentBatchSize).ToList();

            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            var (query, parameters, _) = BuildBatchInsertQuery(batch, tableName);
            command.CommandText = query;

            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            totalAffected += command.ExecuteNonQuery();
        }

        return totalAffected;
    }

    /// <summary>
    /// Save multiple entities to the database asynchronously using parameterized queries.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="connection">The SqlConnection to the database</param>
    /// <param name="entities">The collection of entities to save</param>
    /// <param name="transaction">Optional SqlTransaction. If provided, the caller is responsible for commit/rollback</param>
    /// <param name="tableName">Optional table name. If not provided, uses the entity type name or TableName attribute</param>
    /// <param name="batchSize">The number of entities to process per batch (default: 1000)</param>
    /// <param name="cancellationToken">A token to observe for cancellation of the operation</param>
    /// <returns>The total count of affected rows</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entities collection is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database operation fails</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    /// <remarks>
    /// This method uses parameterized queries to mitigate SQL injection risks.
    /// Transaction management: The caller must handle commit/rollback operations if a transaction is passed.
    /// Performance: Large collections of entities are processed in smaller batches to address parameter limit constraints.
    /// </remarks>
    public static async Task<int> InsertBatchAsync<T>(this SqlConnection connection, IEnumerable<T> entities, SqlTransaction? transaction = null,
                                                 string? tableName = null, int batchSize = DefaultBatchSize,
                                                 CancellationToken cancellationToken = default) where T : class
    {
        var entityList = entities.ToArray();

        if (entityList == null || entityList.Length == 0)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var totalAffected = 0;
        var entityCount = entityList.Length;

        for (var batchStart = 0; batchStart < entityCount; batchStart += batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentBatchSize = Math.Min(batchSize, entityCount - batchStart);
            var batch = entityList.Skip(batchStart).Take(currentBatchSize).ToList();

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;

            var (query, parameters, _) = BuildBatchInsertQuery(batch, tableName);
            command.CommandText = query;

            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            totalAffected += await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        return totalAffected;
    }

    /// <summary>
    /// Builds a batch insert SQL query for the specified list of entities and the target table name.
    /// </summary>
    /// <typeparam name="T">Type of the entities to be inserted.</typeparam>
    /// <param name="entities">List of entities to insert into the database.</param>
    /// <param name="tableName">
    /// Optional target table name. If not provided, the method will attempt to resolve the table name
    /// using the <see cref="TableNameAttribute"/> attribute or the entity type name.
    /// </param>
    /// <returns>A tuple containing the SQL query string, a list of associated <see cref="SqlParameter"/> objects, and the resolved table name.</returns>
    /// <exception cref="ArgumentException">Thrown if the list of entities is empty or the table name cannot be resolved.</exception>
    /// <remarks>
    /// This method dynamically generates column and value pairs using reflection based on the properties of the specified entity type.
    /// Properties marked with the <see cref="IgnoreAttribute"/> will be excluded from the query.
    /// The column names can be customized through the <see cref="ColumnAliasAttribute"/>; if not specified, the property names are used as column names.
    /// The method ensures the security of queries through parameterized values to prevent SQL injection and supports efficient bulk operations.
    /// </remarks>
    private static (string query, List<SqlParameter> parameters, string tableName) BuildBatchInsertQuery<T>(List<T> entities, string? tableName)
        where T : class
    {
        var parameters = new List<SqlParameter>();
        var valuesClauses = new List<string>();

        var entityType = typeof(T);

        var tableNameAttribute = entityType.GetCustomAttribute<TableNameAttribute>();
        var resolvedTableName = string.IsNullOrEmpty(tableName) ? tableNameAttribute?.TableName ?? entityType.Name : tableName;

        var properties = entityType.GetProperties().Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null).ToList();

        // SQL Injection Protection: Escape table name and column names
        var escapedTableName = SqlIdentifierValidator.ValidateAndEscape(resolvedTableName, nameof(tableName));
        var escapedColumnNames = properties.Select(p =>
        {
            var columnAlias = p.GetCustomAttribute<ColumnAliasAttribute>();
            var columnName = columnAlias?.Alias ?? p.Name;
            return SqlIdentifierValidator.ValidateAndEscape(columnName, nameof(p));
        }).ToList();

        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            var parameterNames = new List<string>();

            for (var j = 0; j < properties.Count; j++)
            {
                var property = properties[j];
                var paramName = $"@p{i}_{j}";
                parameterNames.Add(paramName);

                var value = property.GetValue(entity);
                parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
            }

            valuesClauses.Add($"({string.Join(",", parameterNames)})");
        }

        var query = $"INSERT INTO {escapedTableName} ({string.Join(",", escapedColumnNames)}) VALUES {string.Join(",", valuesClauses)}";
        return (query, parameters, resolvedTableName);
    }
}
