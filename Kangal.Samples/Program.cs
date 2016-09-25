using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using Kangal.Attributes;

namespace Kangal.Samples
{
    class Program
    {
        private static readonly string connectionString =
            @"Data Source=KABASAKAL\SQLSERVER;Initial Catalog=Test;Integrated Security=true;";

        private static readonly string query = "select  * from Person;";
        static void Main(string[] args)
        {
            var getResults = SampleSqlConnectionExtensions.SqlConnection_Get();
            Console.ReadKey();
        }

        private static class SampleSqlConnectionExtensions
        {
            public static int SqlConnection_Save()
            {
                var persons = new List<Person>
                {
                    new Person("selçuk","güral",35),
                    new Person("songül","güral",30),
                    new Person("zeynep sare","güral",1)
                };
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.Save(persons);
                }
            }

            public static IEnumerable<Person> SqlConnection_Get()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.Get<Person>(query);
                }
            }
        }

        private static class SampleIDataReaderExtensions
        {
            public static IEnumerable<Person> IDataReader_ToList()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return new SqlCommand(query, connection).ExecuteReader().ToList<Person>();
                }
            }

            public static XDocument IDataReader_ToXDocument()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return new SqlCommand(query, connection).ExecuteReader().ToXDocument("persons", "person");
                }
            }

            public static IEnumerable<DataTable> IDataReader_IEnumerable_DataTable()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return new SqlCommand(String.Concat(query, query), connection).ExecuteReader().ToDataTable();
                }
            }
        }

        private static class SampleDataTablExtensions
        {
            public static IEnumerable<Person> DataTable_ToList()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var reader = new SqlCommand(query, connection).ExecuteReader();
                    dataTable.Load(reader);
                    return dataTable.ToList<Person>();
                }
            }

            public static void DataTable_ChangeColumnName()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var reader = new SqlCommand(query, connection).ExecuteReader();
                    dataTable.Load(reader);
                    dataTable.ChangeColumnName("FirstName", "NickName");
                }
            }

            public static void DataTable_RemoveColumn()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var reader = new SqlCommand(query, connection).ExecuteReader();
                    dataTable.Load(reader);
                    dataTable.RemoveColumn("FirstName");
                }
            }

            public static string DataTable_ToCsv()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var reader = new SqlCommand(query, connection).ExecuteReader();
                    dataTable.Load(reader);
                    return dataTable.ToCsv("-", true);
                }
            }

            public static XDocument DataTable_ToXDocument()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var reader = new SqlCommand(query, connection).ExecuteReader();
                    dataTable.Load(reader);
                    return dataTable.ToXDocument(xmlWriteMode: XmlWriteMode.WriteSchema, nodeName: "persons",
                        writeHierarchy: false);
                }
            }

            public static string DataTable_ToJson()
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var reader = new SqlCommand(query, connection).ExecuteReader();
                    dataTable.Load(reader);
                    return dataTable.ToJson(JsonFormat.Showy, new JsonFormatSettings("dd/MM/yyyy", "0:00.0"));
                }
            }
        }

        private static class SampleListExtensions
        {
            public static DataTable List_DataTable()
            {
                return new List<Person>
                {
                    new Person("selçuk", "güral", 35),
                    new Person("songül", "güral", 30),
                    new Person("zeynep sare", "güral", 1)
                }.ToDataTable();
            }

            public static XDocument List_ToXDocument()
            {
                return new List<Person>
                {
                    new Person("selçuk", "güral", 35),
                    new Person("songül", "güral", 30),
                    new Person("zeynep sare", "güral", 1)
                }.ToXDocument("persons");
            }
        }
    }


    public class Person
    {
        [ColumnAlias("FirstName")] 
        public string Name { get; set; }

        [ColumnAlias("LastName")]
        public string Surname { get; set; }

        [Ignore]
        public short Age { get; set; }

        public Person()
        {
            
        }
        public Person(string name,string surname,short age)
        {
            this.Name = name;
            this.Surname = surname;
            this.Age = age;
        }
    }
}
