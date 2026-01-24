using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="System.Data.DataTable"/> class.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Changes a DataTable column name.
    /// </summary>
    /// <param name="dataTable">DataTable to modify</param>
    /// <param name="currentColumnName">Current column name</param>
    /// <param name="newColumnName">New column name</param>
    /// <exception cref="ArgumentNullException">Thrown when currentColumnName or newColumnName is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when current and new column names are the same, or column not found</exception>
    public static void ChangeColumnName(this DataTable dataTable, string currentColumnName, string newColumnName)
    {
        if (string.IsNullOrEmpty(currentColumnName))
        {
            throw new ArgumentNullException(nameof(currentColumnName));
        }

        if (string.IsNullOrEmpty(newColumnName))
        {
            throw new ArgumentNullException(nameof(newColumnName));
        }

        if (currentColumnName.Equals(newColumnName))
        {
            throw new ArgumentException("Current and new column names cannot be the same.");
        }

        var colIndex = dataTable.Columns.IndexOf(currentColumnName);

        if (colIndex == -1)
        {
            throw new ArgumentException($"Column '{currentColumnName}' not found.", nameof(currentColumnName));
        }

        dataTable.Columns[colIndex].ColumnName = newColumnName;
        dataTable.AcceptChanges();
    }

    /// <summary>
    /// Removes a column from the DataTable.
    /// </summary>
    /// <param name="dataTable">DataTable to modify</param>
    /// <param name="columnName">Name of the column to remove</param>
    /// <exception cref="ArgumentNullException">Thrown when the columnName is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when column not found</exception>
    public static void RemoveColumn(this DataTable dataTable, string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentNullException(nameof(columnName));
        }

        var colIndex = dataTable.Columns.IndexOf(columnName);

        if (colIndex == -1)
        {
            throw new ArgumentException($"Column '{columnName}' not found.", nameof(columnName));
        }

        dataTable.Columns.RemoveAt(colIndex);
        dataTable.AcceptChanges();
    }
    
    /// <summary>
    /// Builds a property map for the specified type, mapping property names and column aliases to their corresponding PropertyInfo objects.
    /// </summary>
    /// <typeparam name="T">The type for which the property map is generated.</typeparam>
    /// <returns>A dictionary where the keys are property names or aliases, and the values are their corresponding PropertyInfo objects.</returns>
    private static Dictionary<string, PropertyInfo> BuildPropertyMap<T>()
    {
        var propertyMap = new Dictionary<string, PropertyInfo>();
        var properties = typeof(T).GetProperties();

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

            propertyMap[property.Name] = property;

            var columnAlias = property.GetCustomAttribute<ColumnAliasAttribute>();

            if (columnAlias != null)
            {
                propertyMap[columnAlias.Alias] = property;
            }
        }

        return propertyMap;
    }

    /// <summary>
    /// Maps the values from a DataRow to the properties of an entity instance.
    /// </summary>
    /// <typeparam name="T">The type of the entity to be populated.</typeparam>
    /// <param name="row">The DataRow containing the data to map.</param>
    /// <param name="entity">The instance of the entity to populate with values.</param>
    /// <param name="propertyMap">
    /// A dictionary mapping column names in the DataRow to the corresponding properties
    /// of the entity type.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the DataRow, entity, or propertyMap is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when a property in the entity cannot be found or assigned a value.
    /// </exception>
    private static void MapRowToEntity<T>(DataRow row, T entity, Dictionary<string, PropertyInfo> propertyMap)
    {
        foreach (DataColumn column in row.Table.Columns)
        {
            if (string.IsNullOrEmpty(column.ColumnName) || !propertyMap.TryGetValue(column.ColumnName, out var property))
            {
                continue;
            }

            var value = row[column];

            try
            {
                if (value == DBNull.Value)
                {
                    if (Nullable.GetUnderlyingType(property.PropertyType) != null || !property.PropertyType.IsValueType)
                    {
                        property.SetValue(entity, null);
                    }
                    // For non-nullable value types, keep default value (don't set)
                }
                else
                {
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    
                    // Type-safe conversion with proper error handling
                    object? convertedValue;
                    if (targetType.IsAssignableFrom(value.GetType()))
                    {
                        // Direct assignment if types are compatible
                        convertedValue = value;
                    }
                    else if (targetType.IsEnum)
                    {
                        // Handle enum conversions
                        convertedValue = Enum.ToObject(targetType, value);
                    }
                    else
                    {
                        // Use Convert.ChangeType for basic type conversions
                        convertedValue = Convert.ChangeType(value, targetType);
                    }
                    
                    property.SetValue(entity, convertedValue);
                }
            }
            catch (InvalidCastException ex)
            {
                throw new ArgumentException(
                    $"Cannot convert value of type '{value.GetType().Name}' from column '{column.ColumnName}' " +
                    $"to property type '{property.PropertyType.Name}'.", ex);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(
                    $"Invalid format for value '{value}' in column '{column.ColumnName}' " +
                    $"when converting to property type '{property.PropertyType.Name}'.", ex);
            }
            catch (OverflowException ex)
            {
                throw new ArgumentException(
                    $"Value '{value}' in column '{column.ColumnName}' is too large or too small " +
                    $"for property type '{property.PropertyType.Name}'.", ex);
            }
        }
    }
}