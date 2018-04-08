using System;
using System.Data;
using System.Data.Common;

namespace Razor.Orm
{
    public class DataReader
    {
        private Func<object>[] functions;

        internal DataReader(DbDataReader dbDataReader, Tuple<string, Func<DbDataReader, int, object>>[] map)
        {
            functions = new Func<object>[map.Length];

            foreach (DataRow item in dbDataReader.GetSchemaTable().Rows)
            {
                var name = item["ColumnName"].ToString().ToLower();
                var ordinal = (int) item["ColumnOrdinal"];

                for (int i = 0; i < map.Length; i++)
                {
                    var tuple = map[i];
                    if (tuple.Item1 == name && tuple.Item2 != null)
                    {
                        functions[i] = () => tuple.Item2(dbDataReader, ordinal);
                        break;
                    }
                }
            }
        }

        public T Get<T>(int index)
        {
            var function = functions[index];
            return function != null ? (T) function() : default(T);
        }
    }
}
