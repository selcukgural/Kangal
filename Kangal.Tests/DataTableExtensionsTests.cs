using System.Data;

namespace Kangal.Tests;

public class DataTableExtensionsTests
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        [ColumnAlias("Full_Name")]
        public string FullName { get; set; }
        
        [Ignore]
        public string Secret { get; set; }

        public int? NullableInt { get; set; }
    }

    [Fact]
    public void ToList_ShouldHandleNullDataTable()
    {
        DataTable dt = null;
        Assert.Throws<ArgumentNullException>(() => dt.ToList<TestEntity>());
    }

    [Fact]
    public void ToList_ShouldHandleEmptyDataTable()
    {
        var dt = new DataTable();
        var result = dt.ToList<TestEntity>();
        Assert.Empty(result);
    }

    [Fact]
    public void ToList_ShouldHandleDBNullForNonNullableValueType()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Rows.Add(DBNull.Value);

        var result = dt.ToList<TestEntity>().ToList();
        Assert.Single(result);
        Assert.Equal(0, result[0].Id); // default(int)
    }

    [Fact]
    public void ToList_ShouldHandleDBNullForNullableValueType()
    {
        var dt = new DataTable();
        dt.Columns.Add("NullableInt", typeof(int));
        dt.Rows.Add(DBNull.Value);

        var result = dt.ToList<TestEntity>().ToList();
        Assert.Single(result);
        Assert.Null(result[0].NullableInt);
    }

    [Fact]
    public void ToCsv_ShouldHandleNullDataTable()
    {
        DataTable dt = null;
        Assert.Throws<ArgumentNullException>(() => dt.ToCsv());
    }

    [Fact]
    public void ToCsv_ShouldHandleEmptyDataTable()
    {
        var dt = new DataTable();
        var csv = dt.ToCsv();
        Assert.Equal("", csv);
    }

    [Fact]
    public void ToCsv_ShouldEscapeSpecialCharacters()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1", typeof(string));
        dt.Rows.Add("Value with ; semicolon");
        dt.Rows.Add("Value with \n newline");
        dt.Rows.Add("Value with \" quotes");

        var csv = dt.ToCsv(";");
        
        // Current implementation does NOT escape, so this will fail or produce incorrect CSV
        Assert.Contains("\"Value with ; semicolon\"", csv);
        Assert.Contains("\"Value with \n newline\"", csv);
        Assert.Contains("\"Value with \"\" quotes\"", csv);
    }

    [Fact]
    public void ToCsv_ShouldHandleMultiCharDelimiter()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1", typeof(string));
        dt.Rows.Add("Value");

        var csv = dt.ToCsv("||");
        Assert.Equal("Value", csv);
    }

    [Fact]
    public void ToXDocument_ShouldHandleEmptyDataTable()
    {
        var dt = new DataTable();
        var doc = dt.ToXDocument();
        Assert.NotNull(doc);
    }

    [Fact]
    public void ToXDocument_ShouldNotModifyOriginalDataTableName()
    {
        var dt = new DataTable("OriginalName");
        dt.Columns.Add("Id", typeof(int));
        dt.Rows.Add(1);

        var originalName = dt.TableName;
        var doc = dt.ToXDocument(nodeName: "NewName");
        
        Assert.Equal("OriginalName", dt.TableName);
    }
}
