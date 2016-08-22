using System.Data;
using System.Data.SqlClient;

namespace Kangal.UnitTests
{
    //internal static class SqlServer
    //{
    //    private static string _connectionString = @"Data Source=KABASAKAL\SQLSERVER;Initial Catalog=AdventureWorks2014;Integrated Security=true;";
    //    internal static SqlConnection GetSqlConnection(string connectionString = "")
    //    {
    //        return string.IsNullOrEmpty(connectionString)
    //            ? new SqlConnection(_connectionString)
    //            : new SqlConnection(connectionString);
    //    }
    //}

    internal static class TempDataTable
    {
        public static DataTable GetMyTable()
        {
            var table = new DataTable("myTable");
            
            table.Columns.AddRange(new DataColumn[3]
            {
                new DataColumn("FirstName"),
                new DataColumn("LastName"),  
                new DataColumn("Age")
            });
            table.Rows.Add("Selçuk", "Güral", 35);
            table.Rows.Add("Songül", "Güral", 30);
            table.Rows.Add("Zeynep Sare", "Güral", 1);
            table.AcceptChanges();
            return table;
        }
    }
}
