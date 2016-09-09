﻿using System;
using System.Data;
using System.Data.SqlClient;
using Kangal;
using Kangal.Attributes;

namespace ConsoleApplication1
{
    class Program
    {
        private static readonly string m_connectionString =
            @"Data Source=KABASAKAL\SQLSERVER;Initial Catalog=Test;Integrated Security=true;";

        private static readonly string m_query = "select  * from Person;";
        static void Main(string[] args)
        {
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                var table = new DataTable();
                using (var command = new SqlCommand(m_query, connection))
                {
                   var reader= command.ExecuteReader().ToList<Person>().ToDataTable();
                   //table.Load(reader);
                   //var aa = table.ToList<Person>();

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

        [Ignore]
        public short Age { get; set; }
    }
}
