using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Kangal
{
    public static class SqlConnectionExtensions
    {
        public static int Save(this SqlConnection connection, DataTable dataTable, string tableName, SqlTransaction transaction = null)
        {
            if (dataTable == null) throw new ArgumentException("dataTable is null");
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("tableName is null");
            if (dataTable.Rows.Count == 0) return 0;
            var query = dataTable.MakeMeSaveQuery(tableName);
            if (string.IsNullOrEmpty(query)) return 0;

            using (var command = connection.CreateCommand())
            {
                if (transaction != null) command.Transaction = transaction;
                command.CommandText = query;
                return command.ExecuteNonQuery();
            }
        }

        public static int Save<T>(this SqlConnection connection, T entity, SqlTransaction transaction = null, string tableName = null) where T : class
        {
            var list = new List<T>() {entity};
            return Save(connection, list: list, transaction: transaction, tableName: tableName);
        }
        public static int Save<T>(this SqlConnection connection, IEnumerable<T> list, SqlTransaction transaction = null,string tableName = null) where  T : class 
        {
            list = list.ToList();
            if (list == null || !list.Any()) throw new ArgumentNullException(nameof(list));

            var query = list.MakeMeSaveQuery(tableName);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                if (transaction != null) command.Transaction = transaction;
                return command.ExecuteNonQuery();
            }
        }
    }
}