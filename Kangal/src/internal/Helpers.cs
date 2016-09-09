using Kangal.Attributes;

namespace Kangal
{
    internal static class Helpers
    {
        public static class TableNameHelper
        {
            internal static string GetTableName(object entity,TableNameAttribute tableNameAttribute = null)
            {
                return string.IsNullOrEmpty(tableNameAttribute?.Name)
                    ? entity.GetType().Name
                    : tableNameAttribute.Name;
            }
        }
    }
}