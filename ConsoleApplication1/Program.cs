using System;
using System.Data;
using System.Data.SqlClient;
using Kangal;
using Kangal.Attributes;

namespace ConsoleApplication1
{
    class Program
    {
        private static readonly string m_connectionString =
            @"Data Source=.;Initial Catalog=WebSql;Integrated Security=true;";

        private static readonly string m_query = "select  * from USERS;";
        static void Main(string[] args)
        {
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                var table = new DataTable();
                using (var command = new SqlCommand(m_query, connection))
                {
                   var reader= command.ExecuteReader();
                   table.Load(reader);
                    var aa = table.ToList<Users>();
                }
            }
            Console.ReadKey();
        }
    }


    class Users
    {
        [ColumnAlias("FirstName")] 
        public string Name { get; set; }

        [ColumnAlias("LastName")]
        public string Surname { get; set; }

        public string UserName { get; set; }

        [Ignore]
        public string Password { get; set; }
    }
}
