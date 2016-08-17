using System;

namespace Kangal.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute{}

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
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public string Name { get; private set; }
        public TableNameAttribute(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            this.Name = name;
        }
    }
}