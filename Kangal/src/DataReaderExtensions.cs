using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kangal
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<T> ToList<T>(this IDataReader reader) where T :class, new()
        {
            if (reader == null || reader.FieldCount == 0) return Enumerable.Empty<T>();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable.ToList<T>();
        }
        public static IEnumerable<DataTable> ToDataTable(this IDataReader reader)
        {
            if (reader == null || reader.FieldCount == 0) return Enumerable.Empty<DataTable>();

            var tableList = new List<DataTable>();

            while (reader.Read())
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);
                tableList.Add(dataTable);
                if (!reader.IsClosed) continue;
                reader.Dispose();
                break;
            }
            return tableList;
        }
    }
}
