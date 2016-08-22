using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kangal.UnitTests
{
    [TestClass]
    public class DataTableExtensionsUnitTests
    {
        private readonly DataTable myTable = TempDataTable.GetMyTable();
        [TestMethod]
        public void ToList_DataTable()
        {
            var personList = myTable.ToList<Person>();
            Assert.AreEqual(3,personList.Count());
            Assert.AreEqual(true,personList.Any(e=> e.Name.Equals("Songül")));
        }
        [TestMethod]
        public void ChangeColumnName_DataTable()
        {
            myTable.ChangeColumnName("FirstName","Name");
            Assert.AreEqual("Name", myTable.Columns[0].ColumnName);
        }
        [TestMethod]
        public void RemoveColumn_DataTable()
        {
            myTable.RemoveColumn("FirstName");
            Assert.AreEqual(2, myTable.Columns.Count);
        }
    }
}