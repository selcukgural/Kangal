using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Kangal;
using Kangal.Attributes;

namespace ConsoleApplication1
{
    class Program
    {
        private static readonly string m_connectionString =
            @"Data Source=KABASAKAL\SQLSERVER;Initial Catalog=AdventureWorks2014;Integrated Security=true;";

        private static readonly string m_query = "select top 5 * from Person.Person;select top 5 * from Person.Address;";
        static void Main(string[] args)
        {
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(m_query, connection))
                {
                    var persons = command.ExecuteReader().ToList<Person>().ToDataTable();
                }
            }
            Console.ReadKey();
        }
    }


    class Person
    {
        [ColumnAlias("FirstName")] 
        public string Name { get; set; }

        [ColumnAlias("LastName")]
        public string Surname { get; set; }

        public byte? Age { get; set; }
    }
}
