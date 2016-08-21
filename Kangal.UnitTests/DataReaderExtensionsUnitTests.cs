using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kangal.UnitTests
{
    [TestClass]
    public class DataReaderExtensionsUnitTests
    {
        private static readonly string _query = "SELECT * FROM [HumanResources].[Employee]";
        private readonly SqlConnection sqlConnection = SqlServer.GetSqlConnection();

        [TestMethod]
        public void ToList()
        {
            using (sqlConnection)
            {
                sqlConnection.Open();

                var employees = new SqlCommand(_query, sqlConnection).ExecuteReader().ToList<Employee>();
                Assert.AreEqual(290,employees.Count());
            }
        }

        [TestMethod]
        public void ToDataTable()
        {
            using (sqlConnection)
            {
                sqlConnection.Open();
                var table= new DataTable();
                table.Load(new SqlCommand(_query, sqlConnection).ExecuteReader());
                Assert.AreEqual(290, table.Rows.Count);
            }
        }
    }
}
