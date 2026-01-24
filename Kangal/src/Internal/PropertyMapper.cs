using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kangal;

internal sealed record PropertyMapping(PropertyInfo Property, string ColumnName, Type ColumnType);

/// <summary>
/// Provides functionality to map properties of a given class to column representations.
/// </summary>
/// <remarks>
/// This class is primarily used for caching and retrieving mappings between properties
/// of a type and their corresponding column representations in scenarios such as data
/// export or serialization. It leverages attributes to customize the mapping process.
/// </remarks>
internal static class PropertyMapper
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, List<PropertyMapping>> Cache = new();

    /// <summary>
    /// Retrieves a list of property mappings for a specified type.
    /// Each mapping associates a class property with a database column name and type,
    /// while skipping properties marked with the <c>IgnoreAttribute</c>.
    /// The result is cached for performance optimization.
    /// </summary>
    /// <typeparam name="T">The type for which property mappings are retrieved.</typeparam>
    /// <returns>A list of <c>PropertyMapping</c> objects representing the mappings for the specified type.</returns>
    public static List<PropertyMapping> GetPropertyMappings<T>()
    {
        return Cache.GetOrAdd(typeof(T), type =>
        {
            var mappings = new List<PropertyMapping>();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<IgnoreAttribute>() != null)
                {
                    continue;
                }

                // Security: Only include writable properties with public setters
                if (!property.CanWrite || property.SetMethod?.IsPublic != true)
                {
                    continue;
                }

                var columnAlias = property.GetCustomAttribute<ColumnAliasAttribute>();
                var columnName = columnAlias?.Alias ?? property.Name;

                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                var columnType = underlyingType ?? property.PropertyType;

                mappings.Add(new PropertyMapping(property, columnName, columnType));
            }
            return mappings;
        });
    }
}
