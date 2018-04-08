using System;
using System.Collections;
using System.Data.Common;

namespace Razor.Orm
{
    public class Extractor
    {
        private Hashtable extractors = new Hashtable();

        internal Extractor()
        {
            Register((s, i) => s.IsDBNull(i) ? null : s.GetString(i));
            Register((s, i) => s.GetInt64(i));
            Register((s, i) => s.IsDBNull(i) ? (long?)null : s.GetInt64(i));
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
            Register((s, i) => s.GetGuid(i));
            Register((s, i) => s.IsDBNull(i) ? (Guid?)null : s.GetGuid(i));
        }

        internal bool Contains(Type type)
        {
            return extractors.ContainsKey(type);
        }

        internal Func<DbDataReader, int, object> Get(Type type)
        {
            return (Func<DbDataReader, int, object>) extractors[type];
        }

        public void Register<T>(Func<DbDataReader, int, T> function)
        {
            Func<DbDataReader, int, object> castFunction = (s, i) => function(s, i);
            extractors.Add(typeof(T), castFunction);
        }
    }
}
