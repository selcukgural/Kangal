using System;

namespace Kangal.Attributes
{
    /// <summary>
    /// Ignoring property or field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    
    public class IgnoreAttribute : Attribute{}

    /// <summary>
    /// Change property or field name with alias
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnAliasAttribute : Attribute
    {
        public string Alias { get;  private set; }
        public ColumnAliasAttribute(string alias)
        {
            if(string.IsNullOrEmpty(alias)) throw new ArgumentNullException(nameof(alias));
            this.Alias = alias;
        }
    }

    /// <summary>
    /// Set Table name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public string TableName { get; private set; }
        public TableNameAttribute(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            this.TableName = tableName;
        }
    }
}