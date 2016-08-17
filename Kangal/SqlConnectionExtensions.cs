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
            var enumerable = list as IList<T> ?? list.ToList();
            if (enumerable == null || !enumerable.Any()) throw new ArgumentNullException("list");
            tableName = string.IsNullOrEmpty(tableName) ? enumerable.FirstOrDefault().GetType().Name : tableName;
            var columns = string.Join(",",
                enumerable.FirstOrDefault().GetType().GetProperties().Select(e => "@" + e.Name));
            var query = $"INSERT INTO {tableName} ({columns.Replace("@", "")}) VALUES ({columns});";
            var affect = 0;
            var command = connection.CreateCommand();
            if (transaction != null) command.Transaction = transaction;

            foreach (var item in enumerable)
            {
                command.CommandText = query;
                command.Parameters.AddRange(
                    Helpers.SqlParameterHelper.CreateSqlParameters(item.GetType().GetProperties().ToArray(), item));
                affect += command.ExecuteNonQuery();
                command.Parameters.Clear();

            }
            return affect;
        }
    }
}