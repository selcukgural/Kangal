# Kangal

Hemen hemen her projede kullanmak durumunda kaldığım kendimce yararlı olduğunu düşündüğüm Extension methodlarını paylaşmak istiyorum...

Projenin başka bir kütüphaneye bağımlılığı yoktur.

###SqlConnection

#####Save(this SqlConnection connection, DataTable dataTable, string tableName,SqlTransaction transaction = null)
DataTable içerisindeki kayıtları veritabanına kayıt eder.
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
