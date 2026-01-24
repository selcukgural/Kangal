using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> class.
/// </summary>
public static partial class ListExtensions
{
    /// <summary>
    /// Converts a collection of entities to XDocument.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entities">Collection of entities</param>
    /// <param name="rootName">Root element name</param>
    /// <returns>XDocument containing the entity data</returns>
    /// <exception cref="ArgumentNullException">Thrown when rootName is null or empty</exception>
    /// <remarks>
    /// Properties marked with IgnoreAttribute are excluded from the XML.
    /// ColumnAliasAttribute is respected for element naming.
    /// Each entity becomes a child element under the root, named after the entity type.
    /// Performance: Property reflection is cached outside the entity loop to minimize overhead.
    /// Null entities in the collection are skipped.
    /// </remarks>
    public static XDocument ToXDocument<T>(this IEnumerable<T?> entities, string rootName) where T : class
    {
        if (string.IsNullOrEmpty(rootName))
        {
            throw new ArgumentNullException(nameof(rootName));
        }

        var xDocument = new XDocument();
        
        // Use TableNameAttribute if present, otherwise use type name
        var tableNameAttr = typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).FirstOrDefault() as TableNameAttribute;
        var nodeName = tableNameAttr?.TableName ?? typeof(T).Name;
        
        xDocument.Add(new XElement(rootName));

        // Cache property mappings outside the loop
        var propertyMappings = PropertyMapper.GetPropertyMappings<T>();

        foreach (var entity in entities)
        {
            if (entity == null)
            {
                continue;
            }

            var xElement = new XElement(nodeName);

            foreach (var mapping in propertyMappings)
            {
                var value = mapping.Property.GetValue(entity);
                xElement.Add(new XElement(mapping.ColumnName, value ?? string.Empty));
            }

            xDocument.Root?.Add(xElement);
        }

        return xDocument;
    }
}
