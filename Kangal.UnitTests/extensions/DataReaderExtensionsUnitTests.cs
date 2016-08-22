using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kangal.UnitTests
{
    [TestClass]
    public class DataReaderExtensionsUnitTests
    {
        //private static readonly string _query = "SELECT * FROM [HumanResources].[Employee]";
        //private readonly SqlConnection sqlConnection = SqlServer.GetSqlConnection();
        private readonly DataTable myTable = TempDataTable.GetMyTable();

        [TestMethod]
        public void ToList_DataReader()
        {
            var myList = myTable.CreateDataReader().ToList<Person>();
            Assert.AreEqual(3,myList.Count());
            Assert.AreEqual(true,myList.Any(e=> e.Name.Equals("Zeynep Sare")));
        }

        [TestMethod]
        public void ToDataTable_DataReader()
        {


            var personTable = Person.GetPersons().ToDataTable();
            Assert.AreEqual(3, personTable.Rows.Count);
            Assert.AreEqual("Selçuk",personTable.Select("Age = 35").FirstOrDefault().ItemArray[0]);
        }
    }
}
