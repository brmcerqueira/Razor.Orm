using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Razor.Orm
{
    public abstract class Dao
    {
        public Dao(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        private SqlConnection SqlConnection { get; }

        protected IEnumerable<T> ExecuteReader<T>(string index, object model, Func<SqlDataReader, T> transform)
        {
            using (var sqlCommand = SqlConnection.CreateCommand())
            {
                var sqlTemplate = RazorOrmRoot.TemplateFactory[index];

                var sqlTemplateResult = sqlTemplate.Process(model);

                sqlCommand.CommandText = sqlTemplateResult.Content;

                sqlCommand.Parameters.AddRange(sqlTemplateResult.Parameters);

                using (var sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.Default))
                {
                    while (sqlDataReader.Read())
                    {
                        yield return transform(sqlDataReader);
                    }
                }
            }  
        }
    }
}
