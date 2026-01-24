using System;
using System.Collections.Generic;
using System.Data;

namespace Kangal;

/// <summary>
/// Provides extension methods for System.Data.DataTable,
/// enabling additional functionality to map DataTable rows
/// to strongly typed objects or perform transformations.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Converts the rows of a DataTable into an enumerable collection of entities of a specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of entities to map from the DataTable rows. The type must have a parameterless constructor.
    /// </typeparam>
    /// <param name="dataTable">
    /// The DataTable containing the rows to convert. Must not be null.
    /// </param>
    /// <returns>
    /// An IEnumerable of entities of type T, populated with data from the DataTable rows.
    /// Returns an empty enumerable if the DataTable has no rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided DataTable is null.
    /// </exception>
    public static IEnumerable<T> ToEnumerable<T>(this DataTable dataTable) where T : new()
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        if (dataTable.Rows.Count == 0)
        {
            yield break;
        }

        var propertyMap = BuildPropertyMap<T>();

        foreach (DataRow row in dataTable.Rows)
        {
            var entity = new T();
            MapRowToEntity(row, entity, propertyMap);
            yield return entity;
        }
    }
}