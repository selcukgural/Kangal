# Kangal

Hemen hemen her projede kullanmak durumunda kaldığım kendimce yararlı olduğunu düşündüğüm Extension methodlarını paylaşmak istiyorum...

###DataTable

#####ToJson()
Bir DataTable nesnesini Json formatında geriye döndürür.
```csharp
        //AdventureWorks2014
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
            }
             var json = table.ToJson(JsonFormat.Showy);
             Console.WriteLine(json);
             Console.ReadKey();
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
    //İle Tarih, decimal ve double formatları için default değer verilebilir.
```

#####ToList<T>()
DataTable nesnesini bir class a map etmenizi sağlar.
```csharp
    class Person
    {
        [ColumnAlias("FirstName")] //maplenen property
        public string Name { get; set; }

        [ColumnAlias("LastName")] //maplenen property
        public string Surname { get; set; }

        public byte? Age { get; set; }
    }
    
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
            }
            var persons = table.ToList<Person>();
            foreach (var person in persons)
            {       
                Console.WriteLine(person.Name+" "+person.Surname);
            }
            Console.ReadKey();
        }
```

#####ChangeColumnName()
Mevcut bir kolonun adını değiştirir.

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
            }
            table.ChangeColumnName("FirstName","Name");
            Console.ReadKey();
        }
```
#####RemoveColumn()
Mevcut kolonu siler.
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
            }
            table.RemoveColumn("Title");
            Console.ReadKey();
        }
```
#####ToCsv()
DataTable içeriğini Csv formatında geriye döner.
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
            }
            var csv = table.ToCsv();
            Console.WriteLine(csv);
            Console.ReadKey();
        }
```
#####ToXDocument()
DataTable içeriğini XDocument formatında geriye döner.
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
            }
            var csv = table.ToXDocument();
            Console.WriteLine(csv.ToString());
            Console.ReadKey();
        }
```
###DataReader

#####ToList<T>()
DataReader nesnesini class a map eder ve geriye IEnumerable<T> döner.
```csharp
        private static readonly string m_query = "select top 5 * from Person.Person;";
        static void Main(string[] args)
        {
            IEnumerable<Person> persons;
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(m_query, connection))
                {
                    persons = command.ExecuteReader().ToList<Person>();
                }
            }
            foreach (var person in persons)
            {
                Console.WriteLine(person.Name+" "+person.Surname);
            }
            Console.ReadKey();
        }
```

#####ToDataTable()
DataReader dan dönen result setleri DataTable lara yükler ve geriye IEnumerable<DataTable> döner.
```csharp
        private static readonly string m_query = "select top 5 * from Person.Person;select top 5 * from Person.Address;";
        static void Main(string[] args)
        {
            IEnumerable<DataTable> dataTables;
            using (var connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(m_query, connection))
                {
                    dataTables = command.ExecuteReader().ToDataTable();
                }
            }
            Console.WriteLine($"DataTable count : {dataTables.Count()}");
            Console.ReadKey();
        }
```
