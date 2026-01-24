using System;

namespace Kangal;

/// <summary>
/// Specifies that a property or field should be ignored during data mapping operations.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    
public sealed class IgnoreAttribute : Attribute;

/// <summary>
/// Specifies an alias for a property or field to map it to a different column name.
/// This attribute is used to customize column mappings for properties or fields
/// when interacting with data sources, such as databases or data tables.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ColumnAliasAttribute : Attribute
{
    /// <summary>
    /// Gets the alias name associated with a property or field.
    /// The alias can be used to map the property or field to a different name,
    /// such as a database column name or an alternative identifier.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Attribute used to define an alias for a property or field.
    /// This can be used to map a property or field to a different name,
    /// particularly useful for serialization or mapping scenarios.
    /// </summary>
    public ColumnAliasAttribute(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        
        Alias = alias;
    }
}

/// <summary>
/// Specifies the database table name associated with a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TableNameAttribute : Attribute
{
    /// <summary>
    /// Represents the name of the table associated with a class or entity.
    /// This property is used to specify a custom table name for the associated entity,
    /// and it is utilized by various operations such as data table conversions or
    /// XML document generation to determine the table name.
    /// </summary>
    public string TableName { get;  }

    /// <summary>
    /// Specifies the table name for a class when converting its instances to a data format such as a DataTable.
    /// This attribute is applied at the class level and ensures that the specified table name
    /// is used during the conversion process.
    /// </summary>
    public TableNameAttribute(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
            
        TableName = tableName;
    }
}