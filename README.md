# Kangal

Hemen hemen her projede kullanmak durumunda kaldığım kendimce yararlı olduğunu düşündüğüm Extension methodlarını paylaşmak istiyorum...

###DataTable
#####ToJson()
Bir DataTable nesnesini Json formatında geriye döndürür.
```csharp
        private static readonly string m_query = "select top 5 * from Person.Person;";

        static void Main(string[] args)
        {
            var table = new DataTable();

            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(m_query, connection))
                {
                    var reader = command.ExecuteReader();
                    table.Load(reader);
                }
                var json = table.ToJson(JsonFormat.Showy);
                Console.WriteLine(json);
                Console.ReadKey();
            }
        }
```
Parametre olarak JsonFormat ve JsonFormatSettings tipinde değerler alır.
```csharp
    public enum JsonFormat
    {
        Simple = 1,//düz json olarak işaretler
        Showy =2 //süslü json olarak işaretler
    }
    
    public JsonFormatSettings() { }
```

İle Tarih, decimal ve double formatları için default değer verilebilir.

#####ToList<T>()
DataTable nesnesini 
