using System;
using System.Collections.Generic;
using System.Data;

namespace Kangal;

public static partial class DataReaderExtensions
{
    /// <summary>
    /// Converts IDataReader records to a generic list.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="reader">IDataReader to read from</param>
    /// <returns>List of entities</returns>
    /// <exception cref="ArgumentException">Thrown when the column name is invalid or type casting fails</exception>
    /// <remarks>
    /// Performance: Property reflection is cached outside the row loop to minimize overhead.
    /// This method reads all records and materializes them immediately, allowing the reader to be disposed of.
    /// </remarks>
    public static List<T> ToList<T>(this IDataReader reader) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        
        if (reader.FieldCount == 0)
        {
            return [];
        }

        var entities = new List<T>();
        var properties = typeof(T).GetProperties();

        var propertyMappings = BuildPropertyMappings<T>(properties);

        while (reader.Read() && !reader.IsClosed)
        {
            var entity = new T();

            foreach (var mapping in propertyMappings)
            {
                try
                {
                    var columnValue = reader[mapping.ColumnName];

                    mapping.Property.SetValue(entity, columnValue == DBNull.Value ? null : columnValue, null);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException($"Invalid column name: '{mapping.ColumnName}'. Column does not exist in the result set.");
                }
                catch (Exception ex) when (ex is not ArgumentException)
                {
                    var columnValue = reader[mapping.ColumnName];
                    var valueTypeName = columnValue.GetType().FullName;
                    throw new ArgumentException($"Cannot cast value of type '{valueTypeName}' to property type '{mapping.Property.PropertyType.FullName}' for column '{mapping.ColumnName}'.", ex);
                }
            }

            entities.Add(entity);
        }

        return entities;
    }
}
