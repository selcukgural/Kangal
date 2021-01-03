# Kangal

Hemen hemen her projede kullanmak durumunda kaldığım kendimce yararlı olduğunu düşündüğüm Extension methodlarını paylaşmak istiyorum...

Projenin başka bir kütüphaneye bağımlılığı yoktur.

### SqlConnection

##### Save(this SqlConnection connection, DataTable dataTable, string tableName,SqlTransaction transaction = null)
DataTable içerisindeki kayıtları veritabanına kayıt eder ve geriye kayıt sayısını döner.
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

##### Save<T>(this SqlConnection connection, T entity, SqlTransaction transaction = null, string tableName = null) where T : class
POCO nesnesini veritabanına kayıt eder.
```csharp
var person = new Person("zeynep sare","güral",1)
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    return connection.Save(person);
}
```

##### Save<T>(this SqlConnection connection, IEnumerable<T> entities,SqlTransaction transaction = null, string tableName = null) where T : class
POCO nesnesini veritabanına kayıt eder.
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
