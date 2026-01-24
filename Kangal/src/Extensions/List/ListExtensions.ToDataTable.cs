using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> class.
/// </summary>
public static partial class ListExtensions
{
    /// <summary>
    /// Converts a generic list to DataTable.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityList">Collection of entities</param>
    /// <returns>DataTable containing the entity data</returns>
    /// <exception cref="ArgumentNullException">Thrown when the first non-null entity cannot be found</exception>
    /// <remarks>
    /// Properties marked with IgnoreAttribute are excluded from the DataTable.
    /// ColumnAliasAttribute is respected for column naming.
    /// Nullable types are converted to System.Object in the DataTable columns.
    /// Performance: Property reflection is cached outside the row loop to minimize overhead.
    /// If the collection is empty or all entities are null, an empty DataTable is returned.
    /// </remarks>
    public static DataTable ToDataTable<T>(this IEnumerable<T?> entityList) where T : class
    {
        var dataTable = new DataTable();
        
        // Use TableNameAttribute if present
        if (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).FirstOrDefault() is TableNameAttribute tableNameAttr)
        {
            dataTable.TableName = tableNameAttr.TableName;
        }

        // Cache property mappings outside the loop
        var propertyMappings = PropertyMapper.GetPropertyMappings<T>();

        // Build columns
        foreach (var mapping in propertyMappings)
        {
            dataTable.Columns.Add(mapping.ColumnName, mapping.ColumnType);
        }

        // Populate rows
        foreach (var entity in entityList)
        {
            if (entity == null)
            {
                continue;
            }

            var values = new object[propertyMappings.Count];
            for (int i = 0; i < propertyMappings.Count; i++)
            {
                values[i] = propertyMappings[i].Property.GetValue(entity) ?? DBNull.Value;
            }
            dataTable.Rows.Add(values);
        }

        return dataTable;
    }
}
