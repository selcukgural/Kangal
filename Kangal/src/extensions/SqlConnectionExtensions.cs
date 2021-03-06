﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Kangal
{
    public static class SqlConnectionExtensions
    {
        /// <summary>
        /// Save DataTable to database.
        /// </summary>
        /// <param name="connection">SqlConnection</param>
        /// <param name="dataTable">DataTable</param>
        /// <param name="tableName">Table name</param>
        /// <param name="transaction">SqlTransaction</param>
        /// <returns>affected rows count</returns>
        public static int Save(this SqlConnection connection, DataTable dataTable, string tableName,
            SqlTransaction transaction = null)
        {
            if (dataTable == null) throw new ArgumentException("dataTable is null");
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("tableName is null");
            if (dataTable.Rows.Count == 0) return 0;
            var query = dataTable.MakeMeSaveQuery(tableName);
            if (string.IsNullOrEmpty(query)) return 0;

            var command = connection.CreateCommand();
            try
            {
                if (transaction != null) command.Transaction = transaction;
                command.CommandText = query;
                var affect = command.ExecuteNonQuery();
                if (transaction != null) command.Transaction.Commit();
                return affect;
            }
            catch
            {
                if (transaction != null) command.Transaction.Rollback();
                return 0;
            }
            finally
            {
                command.Dispose();
            }
        }
        /// <summary>
        /// The Entity save to database.
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="connection">SqlConnection</param>
        /// <param name="entity">Entity</param>
        /// <param name="transaction">SqlTransaction</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Saved entity count</returns>
        public static int Save<T>(this SqlConnection connection, T entity, SqlTransaction transaction = null, string tableName = null) where T : class
        {
            var entities = new List<T> {entity};
            return Save(connection, entities: entities, transaction: transaction, tableName: tableName);
        }
        /// <summary>
        /// The Entities save to database.
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="connection">SqlConnection</param>
        /// <param name="entities">Entities</param>
        /// <param name="transaction">SqlTransaction</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Saved entities count</returns>
        public static int Save<T>(this SqlConnection connection, IEnumerable<T> entities,
            SqlTransaction transaction = null, string tableName = null) where T : class
        {
            entities = entities.ToList();
            if (entities == null || !entities.Any()) throw new ArgumentNullException(nameof(entities));

            var query = entities.MakeMeSaveQuery(tableName);
            var command = connection.CreateCommand();
            try
            {
                command.CommandText = query;
                if (transaction != null) command.Transaction = transaction;
                var affect = command.ExecuteNonQuery();
                if (transaction != null) command.Transaction.Commit();
                return affect;
            }
            catch
            {
                if (transaction != null) command.Transaction.Rollback();
                return 0;
            }
            finally
            {
                command.Dispose();
            }
        }
        /// <summary>
        /// Select and get result returns back as entity type.
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="connection">SqlConnection</param>
        /// <param name="query">Select query</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this SqlConnection connection,string query) where T : class ,new ()
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));
            using (var command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = query;
                    return command.ExecuteReader().ToList<T>();
                }
                catch
                {
                    return Enumerable.Empty<T>();
                }
                finally
                {
                    command.Dispose();
                }
            }
        }
    }
}