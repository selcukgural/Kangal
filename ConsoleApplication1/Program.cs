using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Kangal;

namespace ConsoleApplication1
{
    class Program
    {
        private static string m_connectionString = "Data Source=.;Initial Catalog=Test;Integrated Security=true;";
        private static string m_query = "SELECT TOP 10 * FROM PERSON; SELECT TOP 200 * FROM PERSON;";
        static void Main(string[] args)
        {
            var table = new DataTable();
            var stopWatch = new Stopwatch();
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(m_query, connection))
                {
                    var reader = command.ExecuteReader();
                    table.Load(reader);
                }
            }
            stopWatch.Start();
            var genericList = table.ToList<Person>();
            stopWatch.Stop();
            Console.WriteLine($"DataTable => ToList<T> {stopWatch.Elapsed}. Kayıt sayısı: {genericList.Count()}");
            stopWatch.Reset();
            stopWatch.Start();
            var dataTable = genericList.ToDataTable();
            stopWatch.Stop();
            Console.WriteLine($"List<T> => ToDataTable {stopWatch.Elapsed}. Kayıt sayısı {dataTable.Rows.Count}");
            Console.ReadKey();
        }
    }

    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte? Age { get; set; }
    }
}
