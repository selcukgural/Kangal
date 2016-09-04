# Kangal

Hemen hemen her projede kullanmak durumunda kaldığım kendimce yararlı olduğunu düşündüğüm Extension methodlarını paylaşmak istiyorum...

###DataTable
#####ToJson()
Bir DataTable nesnesini Json formatında geriye döndürür.
```csharp
var dataTable = new DataTable();
.
.
.
var json = dataTable.ToJson();
```
Parametre olarak JsonFormat ve JsonFormatSettings tipinde değerler alır.
```csharp
    public enum JsonFormat
    {
        Simple = 1,//düz json olarak işaretler
        Showy =2 //süslü json olarak işaretler
    }
```
