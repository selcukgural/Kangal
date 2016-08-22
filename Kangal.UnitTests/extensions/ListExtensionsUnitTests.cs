using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kangal.UnitTests.extensions
{
    [TestClass]
    public class ListExtensionsUnitTests
    {
        [TestMethod]
        public void ToDataTable_List()
        {
            var myTable = Person.GetPersons().ToDataTable();
            Assert.AreEqual(3,myTable.Rows.Count);
            Assert.AreEqual(3,myTable.Columns.Count);
        }
    }
}
