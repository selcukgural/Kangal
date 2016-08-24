using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Kangal;
using Kangal.Attributes;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        private static string m_connectionString = @"Data Source=KABASAKAL\SQLSERVER;Initial Catalog=AdventureWorks2014;Integrated Security=true;";
        private static string m_query = "select top 5 * from Sales.SalesPersonQuotaHistory;";
        static void Main(string[] args)
        {
            var table = new DataTable();
            //var stopWatch = new Stopwatch();
            //var personList = new List<Person>();
            //for (int i = 0; i < 5000; i++)
            //{
            //    personList.Add(new Person
            //    {
            //        Age = 35,
            //        Name = "Selçuk " + i,
            //        Surname = "Güral " + i
            //    });
            //}


            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                //var aa = connection.Save(personList.FirstOrDefault());
                using (var command = new SqlCommand(m_query, connection))
                {
                    var reader = command.ExecuteReader();
                    table.Load(reader);
                }
                //stopWatch.Start();
                //var newton = JsonConvert.SerializeObject(table,Formatting.Indented);
                //stopWatch.Stop();
                //Console.WriteLine(stopWatch.ElapsedMilliseconds+ " newton");
                //stopWatch.Reset();
                //stopWatch.Start();
                var json = table.ToJson(JsonFormat.Showy);
                //stopWatch.Stop();
                //Console.WriteLine(stopWatch.ElapsedMilliseconds + " benim");
                //var list = table.ToList<Person>();
                //connection.Save(personList);
                Console.ReadKey();
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
        [ColumnAlias("FirstName")]
        public string Name { get; set; }

        [ColumnAlias("LastName")]
        public string Surname { get; set; }

        public byte? Age { get; set; }
    }
}
