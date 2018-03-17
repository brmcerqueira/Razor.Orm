using System;
using System.Collections;
using System.Data.SqlClient;

namespace Razor.Orm
{
    public class Transformers
    {
        private Hashtable transformers = new Hashtable();

        internal Transformers()
        {
            Register((s, i) => s.IsDBNull(i) ? null : s.GetString(i));
            Register((s, i) => s.GetInt64(i));
            Register((s, i) => s.IsDBNull(i) ? (long?)null : s.GetInt64(i));
            Register((s, i) => s.IsDBNull(i) ? null : s.GetSqlBytes(i).Buffer);
            Register((s, i) => s.GetByte(i));
            Register((s, i) => s.IsDBNull(i) ? (byte?)null : s.GetByte(i));
            Register((s, i) => s.GetDateTime(i));
            Register((s, i) => s.IsDBNull(i) ? (DateTime?)null : s.GetDateTime(i));
            Register((s, i) => s.GetDecimal(i));
            Register((s, i) => s.IsDBNull(i) ? (decimal?)null : s.GetDecimal(i));
            Register((s, i) => s.GetFloat(i));
            Register((s, i) => s.IsDBNull(i) ? (float?)null : s.GetFloat(i));
            Register((s, i) => s.GetInt32(i));
            Register((s, i) => s.IsDBNull(i) ? (int?)null : s.GetInt32(i));
            Register((s, i) => s.GetInt16(i));
            Register((s, i) => s.IsDBNull(i) ? (short?)null : s.GetInt16(i));
            Register((s, i) => s.GetTimeSpan(i));
            Register((s, i) => s.IsDBNull(i) ? (TimeSpan?)null : s.GetTimeSpan(i));
            Register((s, i) => s.GetGuid(i));
            Register((s, i) => s.IsDBNull(i) ? (Guid?)null : s.GetGuid(i));
        }

        internal bool Contains(Type type)
        {
            return transformers.ContainsKey(type);
        }

        internal Func<SqlDataReader, int, object> GetTransform(Type type)
        {
            return (Func<SqlDataReader, int, object>) transformers[type];
        }

        public void Register<T>(Func<SqlDataReader, int, T> function)
        {
            Func<SqlDataReader, int, object> castFunction = (s, i) => function(s, i);
            transformers.Add(typeof(T), castFunction);
        }
    }
}
