using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="SqlConnection"/> class.
/// </summary>
public static partial class SqlConnectionExtensions
{
    private const int DefaultBatchSize = 1000;

    /// <summary>
    /// Ensures the connection is open, and returns whether it was originally closed.
    /// If the connection was closed, the caller should close it after the operation.
    /// </summary>
    /// <param name="connection">The SQL connection to check and open if necessary</param>
    /// <returns>True if the connection was originally closed; false if it was already open</returns>
    /// <exception cref="ArgumentNullException">Thrown when connection is null</exception>
    private static bool EnsureConnectionOpen(SqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (connection.State == ConnectionState.Open)
        {
            return false;
        }

        if (connection.State == ConnectionState.Broken)
        {
            connection.Close();
        }

        connection.Open();
        return true;
    }
    
    /// <summary>
    /// Save DataTable to database using parameterized queries.
    /// </summary>
    /// <param name="connection">SqlConnection</param>
    /// <param name="dataTable">DataTable to save</param>
    /// <param name="tableName">Target table name</param>
    /// <param name="transaction">Optional SqlTransaction. If provided, the caller is responsible for commit/rollback.</param>
    /// <param name="batchSize">Number of rows per batch (default: 1000)</param>
    /// <returns>Total affected rows count</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataTable or tableName is null/empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when database operation fails</exception>
    /// <remarks>
    /// This method uses parameterized queries to prevent SQL injection.
    /// Transaction management: If you pass a transaction, you must handle commit/rollback yourself.
    /// If no transaction is provided, each batch executes independently without transaction.
    /// Connection state is automatically managed - if the connection is closed, it will be opened and closed after the operation.
    /// </remarks>
    public static int Insert(this SqlConnection connection, DataTable dataTable, string tableName,
                           SqlTransaction? transaction = null, int batchSize = DefaultBatchSize)
    {
        ArgumentNullException.ThrowIfNull(dataTable);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        if (dataTable.Rows.Count == 0)
        {
            return 0;
        }

        var shouldCloseConnection = EnsureConnectionOpen(connection);
        try
        {
            var totalAffected = 0;
            var rowCount = dataTable.Rows.Count;

            for (var batchStart = 0; batchStart < rowCount; batchStart += batchSize)
            {
                var currentBatchSize = Math.Min(batchSize, rowCount - batchStart);
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                var (query, parameters) = BuildBatchInsertQuery(dataTable, tableName, batchStart, currentBatchSize);
                command.CommandText = query;

                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                totalAffected += command.ExecuteNonQuery();
            }

            return totalAffected;
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
    /// Save DataTable to a database asynchronously using parameterized queries.
    /// </summary>
    /// <param name="connection">SqlConnection</param>
    /// <param name="dataTable">DataTable to save</param>
    /// <param name="tableName">Target table name</param>
    /// <param name="transaction">Optional SqlTransaction. If provided, the caller is responsible for commit/rollback.</param>
    /// <param name="batchSize">Number of rows per batch (default: 1000)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total affected rows count</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataTable or tableName is null/empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database operation fails</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
    /// <remarks>
    /// This method uses parameterized queries to prevent SQL injection.
    /// Transaction management: If you pass a transaction, you must handle commit/rollback yourself.
    /// If no transaction is provided, each batch executes independently without transaction.
    /// Connection state is automatically managed - if the connection is closed, it will be opened and closed after the operation.
    /// </remarks>
    public static async Task<int> InsertAsync(this SqlConnection connection, DataTable dataTable, string tableName,
                                            SqlTransaction? transaction = null, int batchSize = DefaultBatchSize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataTable);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        if (dataTable.Rows.Count == 0)
        {
            return 0;
        }

        var shouldCloseConnection = EnsureConnectionOpen(connection);
        try
        {
            var totalAffected = 0;
            var rowCount = dataTable.Rows.Count;

            for (var batchStart = 0; batchStart < rowCount; batchStart += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentBatchSize = Math.Min(batchSize, rowCount - batchStart);
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;

                var (query, parameters) = BuildBatchInsertQuery(dataTable, tableName, batchStart, currentBatchSize);
                command.CommandText = query;

                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                totalAffected += await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return totalAffected;
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
    /// Builds a batch insert SQL query and its associated parameters for a given DataTable.
    /// </summary>
    /// <param name="dataTable">The DataTable containing the rows to insert.</param>
    /// <param name="tableName">The name of the target database table.</param>
    /// <param name="startIndex">The zero-based index of the first row in the DataTable to include in the batch.</param>
    /// <param name="count">The number of rows to include in the batch.</param>
    /// <returns>A tuple containing the SQL insert query string and a list of associated SQL parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataTable or tableName is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when startIndex or count is out of range for the DataTable rows.</exception>
    /// <remarks>
    /// This method generates a parameterized SQL query for bulk inserting rows from a DataTable to prevent SQL injection.
    /// The startIndex and count parameters control which subset of rows from the DataTable are processed in the batch.
    /// </remarks>
    private static (string query, List<SqlParameter> parameters) BuildBatchInsertQuery(DataTable dataTable, string tableName, int startIndex, int count)
    {
        var parameters = new List<SqlParameter>();
        var valuesClauses = new List<string>();

        // SQL Injection Protection: Escape table name and column names
        var escapedTableName = SqlIdentifierValidator.ValidateAndEscape(tableName, nameof(tableName));
        var escapedColumnNames = dataTable.Columns.Cast<DataColumn>()
            .Select(c => SqlIdentifierValidator.ValidateAndEscape(c.ColumnName, nameof(dataTable)))
            .ToList();

        for (var i = 0; i < count; i++)
        {
            var rowIndex = startIndex + i;
            var parameterNames = new List<string>();

            for (var j = 0; j < dataTable.Columns.Count; j++)
            {
                var paramName = $"@p{rowIndex}_{j}";
                parameterNames.Add(paramName);

                var value = dataTable.Rows[rowIndex][j];
                parameters.Add(new SqlParameter(paramName, value == DBNull.Value ? DBNull.Value : value));
            }

            valuesClauses.Add($"({string.Join(",", parameterNames)})");
        }

        var query = $"INSERT INTO {escapedTableName} ({string.Join(",", escapedColumnNames)}) VALUES {string.Join(",", valuesClauses)}";
        return (query, parameters);
    }
}
