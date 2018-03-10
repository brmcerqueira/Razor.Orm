using System;
using System.Data;
using System.Data.SqlClient;

namespace Razor.Orm
{
    public class DataReader
    {
        private Func<object>[] functions;

        internal DataReader(SqlDataReader sqlDataReader, Tuple<string, Type>[] map)
        {
            functions = new Func<object>[map.Length];

            foreach (DataRow item in sqlDataReader.GetSchemaTable().Rows)
            {
                var name = item["ColumnName"].ToString().ToLower();
                var ordinal = (int) item["ColumnOrdinal"];

                for (int i = 0; i < map.Length; i++)
                {
                    var tuple = map[i];
                    if (tuple.Item1 == name)
                    {
                        var function = RazorOrmRoot.GetTransform(tuple.Item2);
                        functions[i] = () => function(sqlDataReader, ordinal);
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
