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

            var command = connection.CreateCommand();
            if (transaction != null) command.Transaction = transaction;
            command.CommandText = query;
            return command.ExecuteNonQuery();
        }

        public static int Save<T>(this SqlConnection connection, IEnumerable<T> list, SqlTransaction transaction = null, string tableName = null)
        {
            list = list.ToList();
            if (list == null || !list.Any()) throw new ArgumentNullException(nameof(list));
            tableName = string.IsNullOrEmpty(tableName) ? list.First().GetType().Name : tableName;
            var columns = string.Join(",",
                list.First().GetType().GetProperties().Select(e => "@" + e.Name));
            var query = $"INSERT INTO {tableName} ({columns.Replace("@", "")}) VALUES ({columns});";
            var affect = 0;
            var command = connection.CreateCommand();
            if (transaction != null) command.Transaction = transaction;

            foreach (var item in list)
            {
                command.CommandText = query;
                command.Parameters.AddRange(
                    Helpers.SqlParameterHelper.CreateSqlParameters(item.GetType().GetProperties(), item));
                affect += command.ExecuteNonQuery();
                command.Parameters.Clear();
            }
            return affect;
        }
    }
}