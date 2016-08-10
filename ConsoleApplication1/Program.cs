﻿using System;
using System.Collections.Generic;
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
        private static string m_query = "SELECT TOP 100 * FROM PERSON;";
        static void Main(string[] args)
        {
            var table = new DataTable();
            //var stopWatch = new Stopwatch();
            var personList = new List<Person>();
            for (int i = 0; i < 5000; i++)
            {
                personList.Add(new Person
                {
                    Age = 35,
                    FirstName = "Selçuk "+i,
                    LastName = "Güral "+i
                });
            }

            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                //using (var command = new SqlCommand(m_query, connection))
                //{
                //    var reader = command.ExecuteReader();
                //    table.Load(reader);
                //}
                //var list = table.ToList<Person>();
                connection.Save(personList);
            }

            

            //stopWatch.Start();
            //var genericList = table.ToList<Person>();
            //stopWatch.Stop();
            //Console.WriteLine($"DataTable => ToList<T> {stopWatch.Elapsed}. Kayıt sayısı: {genericList.Count()}");
            //stopWatch.Reset();
            //stopWatch.Start();
            //var dataTable = genericList.ToDataTable();
            //stopWatch.Stop();
            //Console.WriteLine($"List<T> => ToDataTable {stopWatch.Elapsed}. Kayıt sayısı {dataTable.Rows.Count}");
            //Console.ReadKey();
        }
    }

    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte? Age { get; set; }
    }
}