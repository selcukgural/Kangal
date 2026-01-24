using System.Collections.Generic;
using System.Reflection;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="System.Data.IDataReader"/> class.
/// </summary>
/// <remarks>
/// This class supports customization of entity mapping through attributes like ColumnAliasAttribute
/// and IgnoreAttribute, allowing flexibility when working with DataReader schema and entity properties.
/// Property reflection is cached outside row loops to minimize overhead.
/// </remarks>
public static partial class DataReaderExtensions
{
    /// <summary>
    /// Builds property mappings for an entity type.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="properties">PropertyInfo array</param>
    /// <returns>List of PropertyMapping objects</returns>
    /// <remarks>
    /// This helper method is used internally to build property-to-column mappings,
    /// respecting ColumnAliasAttribute and IgnoreAttribute decorations.
    /// </remarks>
    private static List<PropertyMapping> BuildPropertyMappings<T>(PropertyInfo[] properties)
    {
        var mappings = new List<PropertyMapping>();

        foreach (var property in properties)
        {
            var ignoreAttribute = property.GetCustomAttribute<IgnoreAttribute>();

            if (ignoreAttribute != null)
            {
                continue;
            }

            // Security: Only include writable properties with public setters
            if (!property.CanWrite || property.SetMethod?.IsPublic != true)
            {
                continue;
            }

            var columnAliasAttribute = property.GetCustomAttribute<ColumnAliasAttribute>();
            var columnName = columnAliasAttribute?.Alias ?? property.Name;

            mappings.Add(new PropertyMapping
            {
                Property = property,
                ColumnName = columnName
            });
        }

        return mappings;
    }

    /// <summary>
    /// Internal class for mapping properties to column names.
    /// </summary>
    private sealed class PropertyMapping
    {
        /// <summary>
        /// Gets or sets the property info.
        /// </summary>
        public required PropertyInfo Property { get; init; }

        /// <summary>
        /// Gets or sets the column name from a database.
        /// </summary>
        public required string ColumnName { get; init; } 
    }
}