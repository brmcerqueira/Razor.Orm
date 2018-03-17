using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Razor.Orm
{
    public abstract class Dao
    {
        public Dao(SqlConnection sqlConnection, Transformers transformers)
        {
            SqlConnection = sqlConnection;
            Transformers = transformers;
            Map = GetMap();
        }

        protected abstract Tuple<string, Func<SqlDataReader, int, object>>[][] GetMap();

        private SqlConnection SqlConnection { get; }
        private Transformers Transformers { get; }
        private Tuple<string, Func<SqlDataReader, int, object>>[][] Map { get; }

        protected Func<SqlDataReader, int, object> GetTransform(Type type)
        {
            return Transformers.GetTransform(type);
        }

        protected IEnumerable<T> ExecuteReader<T>(ISqlTemplate sqlTemplate, int methodIndex, object model, Func<DataReader, T> transform)
        {
            using (var sqlCommand = SqlConnection.CreateCommand())
            {
                var sqlTemplateResult = sqlTemplate.Process(model);

                sqlCommand.CommandText = sqlTemplateResult.Content;

                sqlCommand.Parameters.AddRange(sqlTemplateResult.Parameters);

                using (var sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.Default))
                {
                    var dataReader = new DataReader(sqlDataReader, Map[methodIndex]);
                    while (sqlDataReader.Read())
                    {
                        yield return transform(dataReader);
                    }
                }
            }  
        }
    }
}
