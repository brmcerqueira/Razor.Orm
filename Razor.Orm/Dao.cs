using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Razor.Orm
{
    public abstract class Dao
    {
        public Dao(SqlConnection sqlConnection, TemplateFactory templateFactory)
        {
            SqlConnection = sqlConnection;
            TemplateFactory = templateFactory;
            Map = GetMap();
        }

        protected abstract Tuple<string, Func<SqlDataReader, int, object>>[][] GetMap();

        private SqlConnection SqlConnection { get; }
        private TemplateFactory TemplateFactory { get; }
        private Tuple<string, Func<SqlDataReader, int, object>>[][] Map { get; }

        protected Func<SqlDataReader, int, object> GetTransform(Type type)
        {
            return RazorOrmRoot.GetTransform(type);
        }

        protected IEnumerable<T> ExecuteReader<T>(string template, int methodIndex, object model, Func<DataReader, T> transform)
        {
            using (var sqlCommand = SqlConnection.CreateCommand())
            {
                var sqlTemplate = TemplateFactory[template];

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
