using System;
using System.Collections.Generic;
using System.Data;

namespace Kangal;

/// <summary>
/// Provides extension methods for working with DataTable objects, including the ability
/// to convert DataTable records to generic collections and other utility functionalities.
/// </summary>
/// <remarks>
/// This class supports customization of entity mapping through attributes like ColumnAliasAttribute
/// and IgnoreAttribute, allowing flexibility when working with DataTable schema and entity properties.
/// </remarks>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Converts the rows of a DataTable into a generic list of entities of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the entities to convert the DataTable rows into. Must have a parameterless constructor.</typeparam>
    /// <param name="dataTable">The DataTable to be converted to a list of entities.</param>
    /// <returns>A list of entities of the specified type populated with the data from the DataTable rows.</returns>
    /// <remarks>
    /// This method uses reflection to map DataTable columns to the properties of the target entity type.
    /// It supports custom mappings via attributes such as ColumnAliasAttribute.
    /// Properties marked with IgnoreAttribute are excluded from the mapping.
    /// Caching is utilized to optimize the performance of property reflection within the mapping process.
    /// If the DataTable is null, an ArgumentNullException is thrown. An empty DataTable returns an empty list.
    /// </remarks>
    public static List<T> ToList<T>(this DataTable dataTable) where T : new()
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        if (dataTable.Rows.Count == 0)
        {
            return [];
        }

        var propertyMap = BuildPropertyMap<T>();
        var entities = new List<T>(dataTable.Rows.Count);

        foreach (DataRow row in dataTable.Rows)
        {
            var entity = new T();
            MapRowToEntity(row, entity, propertyMap);
            entities.Add(entity);
        }

        return entities;
    }
}