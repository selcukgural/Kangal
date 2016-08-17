using System;
using System.Collections.Generic;
using System.Data;

namespace Kangal
{
    public static class TypeExtensions
    {
        #region DbTypes

        private static readonly Dictionary<Type, DbType> dbTypes = new Dictionary<Type, DbType>
                {
                    {
                        typeof(int), DbType.Int32
                    },
                    {
                        typeof(int?), DbType.Int32
                    },
                    {
                        typeof(long), DbType.Int64
                    },
                    {
                        typeof(long?), DbType.Int64
                    },
                    {
                        typeof(short), DbType.Int16
                    },
                    {
                        typeof(short?), DbType.Int16
                    },
                    {
                        typeof(string), DbType.AnsiString
                    },
                    {
                        typeof(byte[]), DbType.Binary
                    },
                    {
                        typeof(bool?), DbType.Boolean
                    },
                    {
                        typeof(bool), DbType.Boolean
                    },
                    {
                        typeof(decimal), DbType.Currency
                    },
                    {
                        typeof(decimal?), DbType.Currency
                    },
                    {
                        typeof(DateTime), DbType.DateTime
                    },
                    {
                        typeof(DateTime?), DbType.DateTime
                    },
                    {
                        typeof(char), DbType.AnsiStringFixedLength
                    },
                    {
                        typeof(char?), DbType.AnsiStringFixedLength
                    },
                    {
                        typeof(DateTimeOffset), DbType.DateTimeOffset
                    },
                    {
                        typeof(float), DbType.Double
                    },
                    {
                        typeof(float?), DbType.Double
                    },
                    {
                        typeof(byte), DbType.Byte
                    },
                    {
                        typeof(byte?), DbType.Byte
                    }
                };
        #endregion

        #region SqlDbTypes

        private static readonly Dictionary<Type, SqlDbType> sqlDbTypes = new Dictionary<Type, SqlDbType>
        {
            {typeof(byte[]), SqlDbType.Binary},
            {typeof(int), SqlDbType.Int},
            {typeof(int?), SqlDbType.Int},
            {typeof(long), SqlDbType.BigInt},
            {typeof(long?), SqlDbType.BigInt},
            {typeof(bool), SqlDbType.Bit},
            {typeof(bool?), SqlDbType.Bit},
            {typeof(byte), SqlDbType.TinyInt},
            {typeof(byte?), SqlDbType.TinyInt},
            {typeof(char), SqlDbType.Char},
            {typeof(char?), SqlDbType.Char},
            {typeof(DateTime), SqlDbType.DateTime},
            {typeof(DateTime?), SqlDbType.DateTime},
            {typeof(DateTimeOffset), SqlDbType.DateTimeOffset},
            {typeof(DateTimeOffset?), SqlDbType.DateTimeOffset},
            {typeof(decimal), SqlDbType.Decimal},
            {typeof(decimal?), SqlDbType.Decimal},
            {typeof(double), SqlDbType.Float},
            {typeof(double?), SqlDbType.Float},
            {typeof(string), SqlDbType.NVarChar},
            {typeof(char[]), SqlDbType.NVarChar},
            {typeof(short), SqlDbType.SmallInt},
            {typeof(short?), SqlDbType.SmallInt},
            {typeof(object), SqlDbType.Variant},
            {typeof(TimeSpan), SqlDbType.Time},
            {typeof(TimeSpan?), SqlDbType.Time},
            {typeof(Guid), SqlDbType.UniqueIdentifier},
            {typeof(Guid?), SqlDbType.UniqueIdentifier},
        };
        #endregion 

        public static DbType GetDbType(this Type type)
        {
            DbType dbType;
            dbTypes.TryGetValue(type, out dbType);
            return dbType;
        }

        public static SqlDbType GetSqlDbType(this Type type)
        {
            SqlDbType sqlDbType;
            sqlDbTypes.TryGetValue(type, out sqlDbType);
            return sqlDbType;
        }
    }
}