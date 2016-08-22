using System;
using System.Collections;
using System.Collections.Generic;
using Kangal.Attributes;

namespace Kangal.UnitTests
{
    internal class Employee
    {
        public int BusinessEntityID { get; set; }
        public string NationalIDNumber { get; set; }
        public string LoginID { get; set; }
        public string JobTitle { get; set; }
        public DateTime BirthDate { get; set; }
        public string MaritalStatus { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }

    }

    internal class Person
    {
        [ColumnAlias("FirstName")]
        public string Name { get; set; }
        [ColumnAlias("LastName")]
        public string Surname { get; set; }
        public byte Age { get; set; }

        internal static IEnumerable<Person> GetPersons()
        {
            var personList = new List<Person>
            {
                new Person
                {
                    Name = "Songül",
                    Surname = "Güral",
                    Age = 30
                },
                new Person
                {
                    Name = "Zeynep Sare",
                    Surname = "Güral",
                    Age = 1
                },
                new Person
                {
                    Name = "Selçuk",
                    Surname = "Güral",
                    Age = 35
                }
            };
            return personList;
        }
    }
}
