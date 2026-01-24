using System.Data;

namespace Kangal.Tests;

public class ListExtensionsTests
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        [ColumnAlias("Full_Name")]
        public string FullName { get; set; }
        
        [Ignore]
        public string Secret { get; set; }
    }

    [Fact]
    public void AsDataTable_ShouldConvertListToDataTable()
    {
        var list = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", FullName = "Full Test1", Secret = "Keep" },
            new TestEntity { Id = 2, Name = "Test2", FullName = "Full Test2", Secret = "Away" }
        };

        var dt = list.ToDataTable();

        Assert.Equal(2, dt.Rows.Count);
        Assert.Equal(3, dt.Columns.Count);
        Assert.Contains("Id", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        Assert.Contains("Name", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        Assert.Contains("Full_Name", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        Assert.DoesNotContain("Secret", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
    }

    [Fact]
    public void AsDataTable_ShouldHandleMultipleEnumeration()
    {
        int enumerationCount = 0;
        IEnumerable<TestEntity> GetEntities()
        {
            enumerationCount++;
            yield return new TestEntity { Id = 1, Name = "Test1" };
        }

        var entities = GetEntities();
        entities.ToDataTable();

        // Currently it calls ToList() then FirstOrDefault(e => e != null)
        // Wait, ToList() is called first, so it should only enumerate once?
        // Let's check the code:
        /*
        var entities = entityList.ToList();
        if (entities.Count == 0)
        {
            return dataTable;
        }
        // Find the first non-null entity for schema definition
        var firstEntity = entities.FirstOrDefault(e => e != null);
        */
        // Yes, ToList() is called at the beginning, so it materializes the collection.
        // But what if it's a large collection? It copies everything to a list.
        
        Assert.Equal(1, enumerationCount);
    }

    [TableName("CustomTable")]
    public class TestEntityWithTable
    {
        public int Id { get; set; }
    }

    [Fact]
    public void AsDataTable_ShouldUseTableNameAttribute()
    {
        var list = new List<TestEntityWithTable> { new TestEntityWithTable { Id = 1 } };
        var dt = list.ToDataTable();
        Assert.Equal("CustomTable", dt.TableName);
    }

    [Fact]
    public void AsDataTable_ShouldReturnEmptyDataTableForEmptyList()
    {
        var list = new List<TestEntity>();
        var dt = list.ToDataTable();
        Assert.Equal(0, dt.Rows.Count);
        // It should still have columns if we can determine them from T
        Assert.Equal(3, dt.Columns.Count);
    }

    [Fact]
    public void AsDataTable_ShouldReturnEmptyDataTableForNullOnlyList()
    {
        var list = new List<TestEntity> { null, null };
        var dt = list.ToDataTable();
        Assert.Equal(0, dt.Rows.Count);
        Assert.Equal(3, dt.Columns.Count);
    }
    [Fact]
    public void AsXDocument_ShouldConvertListToXDocument()
    {
        var list = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", FullName = "Full Test1" }
        };

        var doc = list.ToXDocument("Root");

        Assert.Equal("Root", doc.Root.Name.LocalName);
        var child = doc.Root.Elements().First();
        Assert.Equal(nameof(TestEntity), child.Name.LocalName);
        Assert.Equal("1", child.Element("Id").Value);
        Assert.Equal("Test1", child.Element("Name").Value);
        Assert.Equal("Full Test1", child.Element("Full_Name").Value);
    }
    [Fact]
    public void AsXDocument_ShouldUseTableNameAttribute()
    {
        var list = new List<TestEntityWithTable> { new TestEntityWithTable { Id = 1 } };
        var doc = list.ToXDocument("Root");
        Assert.Equal("CustomTable", doc.Root.Elements().First().Name.LocalName);
    }
}
