using System;
using System.Data;
using System.Data.SqlClient;

namespace Razor.Orm
{
    public class DataReader
    {
        private Func<object>[] functions;

        internal DataReader(SqlDataReader sqlDataReader, string[] map)
        {
            functions = new Func<object>[map.Length];

            foreach (DataRow item in sqlDataReader.GetSchemaTable().Rows)
            {
                var name = item["ColumnName"].ToString().ToLower();
                var ordinal = (int) item["ColumnOrdinal"];
                var type = (SqlDbType)(int) item["ProviderType"];

                for (int i = 0; i < map.Length; i++)
                {
                    if (map[i] == name)
                    {
                        switch (type)
                        {
                            case SqlDbType.BigInt:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (long?) null : sqlDataReader.GetInt64(ordinal);
                                };
                                break;
                            case SqlDbType.Binary:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetSqlBytes(ordinal).Buffer;
                                };
                                break;
                            case SqlDbType.Bit:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (byte?) null : sqlDataReader.GetByte(ordinal);
                                };
                                break;
                            case SqlDbType.Char:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetString(ordinal);
                                };
                                break;
                            case SqlDbType.Date:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (DateTime?)null : sqlDataReader.GetDateTime(ordinal);
                                };
                                break;
                            case SqlDbType.DateTime:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (DateTime?)null : sqlDataReader.GetDateTime(ordinal);
                                };
                                break;
                            case SqlDbType.DateTime2:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (DateTime?)null : sqlDataReader.GetDateTime(ordinal);
                                };
                                break;
                            case SqlDbType.DateTimeOffset:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (DateTime?)null : sqlDataReader.GetDateTime(ordinal);
                                };
                                break;
                            case SqlDbType.Decimal:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (decimal?)null : sqlDataReader.GetDecimal(ordinal);
                                };
                                break;
                            case SqlDbType.Float:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (float?)null : sqlDataReader.GetFloat(ordinal);
                                };
                                break;
                            case SqlDbType.Image:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetSqlBytes(ordinal).Buffer;
                                };
                                break;
                            case SqlDbType.Int:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (int?)null : sqlDataReader.GetInt32(ordinal);
                                };
                                break;
                            case SqlDbType.Money:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (decimal?) null : sqlDataReader.GetDecimal(ordinal);
                                };
                                break;
                            case SqlDbType.NChar:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetString(ordinal);
                                };
                                break;
                            case SqlDbType.NText:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetString(ordinal);
                                };
                                break;
                            case SqlDbType.NVarChar:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetString(ordinal);
                                };
                                break;
                            case SqlDbType.Real:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (decimal?)null : sqlDataReader.GetDecimal(ordinal);
                                };
                                break;
                            case SqlDbType.SmallDateTime:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (DateTime?)null : sqlDataReader.GetDateTime(ordinal);
                                };
                                break;
                            case SqlDbType.SmallInt:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (short?)null : sqlDataReader.GetInt16(ordinal);
                                };
                                break;
                            case SqlDbType.SmallMoney:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (decimal?)null : sqlDataReader.GetDecimal(ordinal);
                                };
                                break;
                            case SqlDbType.Structured:
                                break;
                            case SqlDbType.Text:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetString(ordinal);
                                };
                                break;
                            case SqlDbType.Time:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (TimeSpan?)null : sqlDataReader.GetTimeSpan(ordinal);
                                };
                                break;
                            case SqlDbType.Timestamp:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (TimeSpan?)null : sqlDataReader.GetTimeSpan(ordinal);
                                };
                                break;
                            case SqlDbType.TinyInt:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (byte?)null : sqlDataReader.GetByte(ordinal);
                                };
                                break;
                            case SqlDbType.Udt:
                                break;
                            case SqlDbType.UniqueIdentifier:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? (Guid?) null : sqlDataReader.GetGuid(ordinal);
                                };
                                break;
                            case SqlDbType.VarBinary:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetSqlBytes(ordinal).Buffer;
                                };
                                break;
                            case SqlDbType.VarChar:
                                functions[i] = () =>
                                {
                                    return sqlDataReader.IsDBNull(ordinal) ? null : sqlDataReader.GetString(ordinal);
                                };
                                break;
                            case SqlDbType.Variant:
                                break;
                            case SqlDbType.Xml:
                                break;
                        }
                        break;
                    }
                }
            }
        }

        public T Get<T>(int index)
        {
            return (T) functions[index]();
        }
    }
}
