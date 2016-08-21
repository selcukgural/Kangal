using System.Data.SqlClient;

namespace Kangal.UnitTests
{
    internal static class SqlServer
    {
        private static string _connectionString = @"Data Source=KABASAKAL\SQLSERVER;Initial Catalog=AdventureWorks2014;Integrated Security=true;";
        internal static SqlConnection GetSqlConnection(string connectionString = "")
        {
            return string.IsNullOrEmpty(connectionString)
                ? new SqlConnection(_connectionString)
                : new SqlConnection(connectionString);
        }
    }
}
