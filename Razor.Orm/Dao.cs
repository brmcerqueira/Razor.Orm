using Razor.Orm.Template;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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

        private SqlCommand CreateCommand(ISqlTemplate sqlTemplate, object model)
        {
            var sqlCommand = SqlConnection.CreateCommand();

            var sqlTemplateResult = sqlTemplate.Process(model);

            sqlCommand.CommandText = sqlTemplateResult.Content;

            sqlCommand.Parameters.AddRange(sqlTemplateResult.Parameters);

            return sqlCommand;
        }

        protected void Execute(ISqlTemplate sqlTemplate, object model)
        {
            using (var sqlCommand = CreateCommand(sqlTemplate, model))
            {
                sqlCommand.ExecuteNonQuery();
            }
        }

        protected IEnumerable<T> ExecuteEnumerable<T>(ISqlTemplate sqlTemplate, object model, int methodIndex, Func<DataReader, T> parse)
        {
            using (var sqlCommand = CreateCommand(sqlTemplate, model))
            {
                using (var sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.Default))
                {
                    var dataReader = new DataReader(sqlDataReader, Map[methodIndex]);
                    while (sqlDataReader.Read())
                    {
                        yield return parse(dataReader);
                    }
                }
            }  
        }

        protected T Execute<T>(ISqlTemplate sqlTemplate, object model)
        {
            var result = default(T);

            using (var sqlCommand = CreateCommand(sqlTemplate, model))
            {
                using (var sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.Default))
                {
                    if (sqlDataReader.Read())
                    {
                        result = (T) GetTransform(typeof(T)).Invoke(sqlDataReader, 0); 
                    }
                }
            }

            return result;
        }

        protected T Execute<T>(ISqlTemplate sqlTemplate, object model, int methodIndex, Func<DataReader, T> parse)
        {
            return ExecuteEnumerable(sqlTemplate, model, methodIndex, parse).FirstOrDefault();
        }
    }
}
