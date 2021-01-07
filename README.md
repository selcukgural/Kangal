# Kangal

Kangal is `extension` library for data operations. Like `DataReader`, `SqlConnection` or `DataTable`.

The project has no dependence.

# Some Features
#### DataTable
 In the `DataTable` records returns back as `IEnumerable<T>`
```csharp
public static IEnumerable<T> ToList<T>(this DataTable dataTable) where T : new()
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var dataTable = new DataTable();
    var reader = new SqlCommand(query, connection).ExecuteReader();
    dataTable.Load(reader);
    return dataTable.ToList<Person>();
}
```
<br>`DataTable`'s content convert to **Csv** formatted string.
```csharp
public static string ToCsv(this DataTable dataTable, string comma = null,bool ignoreNull = false)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var dataTable = new DataTable();
    var reader = new SqlCommand(query, connection).ExecuteReader();
    dataTable.Load(reader);
    return dataTable.ToCsv("-", true);
}
```
<br>`DataTable`'s content convert to `XDocument`.
```csharp
public static XDocument ToXDocument(this DataTable dataTable,XmlWriteMode xmlWriteMode = XmlWriteMode.IgnoreSchema,string nodeName = null,bool writeHierarchy = true)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var dataTable = new DataTable();
    var reader = new SqlCommand(query, connection).ExecuteReader();
    dataTable.Load(reader);
    return dataTable.ToXDocument(xmlWriteMode: XmlWriteMode.WriteSchema, nodeName: "persons",
        writeHierarchy: false);
}
```
<br>`DataTable`'s content convert to `Json`.
```csharp
public static string ToJson(this DataTable dataTable,JsonFormat jsonFormat = JsonFormat.Simple,JsonFormatSettings jsonFormatSettings = null)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var dataTable = new DataTable();
    var reader = new SqlCommand(query, connection).ExecuteReader();
    dataTable.Load(reader);
    return dataTable.ToJson(JsonFormat.Showy, new JsonFormatSettings("dd/MM/yyyy", "0:00.0"));
}
```
<br>You can change `DataTable` column name.
```csharp
public static void ChangeColumnName(this DataTable dataTable, string currentColumnName, string newColumnName)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var dataTable = new DataTable();
    var reader = new SqlCommand(query, connection).ExecuteReader();
    dataTable.Load(reader);
    dataTable.ChangeColumnName("FirstName", "NickName");
}
```
<br>You can remove `DataTable` column
```csharp
public static void RemoveColumn(this DataTable dataTable, string columnName)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var dataTable = new DataTable();
    var reader = new SqlCommand(query, connection).ExecuteReader();
    dataTable.Load(reader);
    dataTable.RemoveColumn("FirstName");
}
```

#
#### SqlConnection


You can insert to **MSSQL** database to your `DataTable`
```csharp
Save(this SqlConnection connection, DataTable dataTable, string tableName,SqlTransaction transaction = null)
```
```csharp
var dataTable = new DataTable();
dataTable.Columns.Add("FirstName",typeof(string));
dataTable.Columns.Add("LastName", typeof(string));
dataTable.Columns.Add("Age", typeof(short));
dataTable.Rows.Add("selçuk", "güral", 35);
dataTable.Rows.Add("songül", "güral", 30);
dataTable.Rows.Add("zeynep sare", "güral", 1);
dataTable.AcceptChanges();

using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    return connection.Save(dataTable);
}
```
<br>Save your **.Net POCO** objects
``` csharp
public static int Save<T>(this SqlConnection connection, IEnumerable<T> entities, SqlTransaction transaction = null, string tableName = null) where T : class
```
```csharp
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
```
<br>Getting sql query result and **object mapping**. Also you can use some useful **Attributes**. Like `ColumnAlias` and `Ignore`
```csharp
public static IEnumerable<T> Get<T>(this SqlConnection connection,string query) where T : class ,new ()
```
```csharp
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


public static IEnumerable<Person> IDataReader_ToList()
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        return new SqlCommand(query, connection).ExecuteReader().ToList<Person>();
    }
}
```
#
#### DataReader
You can convert `IDataReader` object to `IEnumerable<T>`
```csharp
public static IEnumerable<T> ToList<T>(this IDataReader reader) where T :class, new()
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    return new SqlCommand(query, connection).ExecuteReader().ToList<Person>();
}
```
<br>Also it is possible convert `IDataReader` object to `XDocument`
```csharp
public static XDocument ToXDocument(this IDataReader reader,string rootName,string nodeName)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    return new SqlCommand(query, connection).ExecuteReader().ToXDocument("persons", "person");
}
```

<br>In the records `IDataReader` object returns back as `IEnumerable<DataTable>`
```csharp
public static IEnumerable<DataTable> ToDataTable(this IDataReader reader)
```
```csharp
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    return new SqlCommand(query, connection).ExecuteReader().ToDataTable();
}
```
#
#### List
The Generic `List` convert to `DataTable`
```csharp
public static DataTable ToDataTable<T>(this IEnumerable<T> entityList) where T : class
```
```csharp
return new List<Person>
{
    new Person("selçuk", "güral", 35),
    new Person("songül", "güral", 30),
    new Person("zeynep sare", "güral", 1)
}.ToDataTable();
```

<br>The Entities convert to `XDocument`.
```csharp
public static XDocument ToXDocument<T>(this IEnumerable<T> entities,string rootName) where T : class
```
```csharp
return new List<Person>
{
    new Person("selçuk", "güral", 35),
    new Person("songül", "güral", 30),
    new Person("zeynep sare", "güral", 1)
}.ToXDocument("persons");
```
