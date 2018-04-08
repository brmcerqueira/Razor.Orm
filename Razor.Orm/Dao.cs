using Razor.Orm.Template;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Razor.Orm
{
    public abstract class Dao
    {
        public Dao(DbConnection connection, Extractor extractor)
        {
            Connection = connection;
            Extractor = extractor;
            Map = GetMap();
        }

        protected abstract Tuple<string, Func<DbDataReader, int, object>>[][] GetMap();

        private DbConnection Connection { get; }
        private Extractor Extractor { get; }
        private Tuple<string, Func<DbDataReader, int, object>>[][] Map { get; }

        protected Func<DbDataReader, int, object> Get(Type type)
        {
            return Extractor.Get(type);
        }

        private DbCommand CreateCommand(ISqlTemplate sqlTemplate, object model)
        {
            var command = Connection.CreateCommand();

            sqlTemplate.Process(command, model);

            return command;
        }

        protected void Execute(ISqlTemplate sqlTemplate, object model)
        {
            using (var command = CreateCommand(sqlTemplate, model))
            {
                command.ExecuteNonQuery();
            }
        }

        protected IEnumerable<T> ExecuteEnumerable<T>(ISqlTemplate sqlTemplate, object model, int methodIndex, Func<DataReader, T> parse)
        {
            using (var command = CreateCommand(sqlTemplate, model))
            {
                using (var dbDataReader = command.ExecuteReader(CommandBehavior.Default))
                {
                    var dataReader = new DataReader(dbDataReader, Map[methodIndex]);
                    while (dbDataReader.Read())
                    {
                        yield return parse(dataReader);
                    }
                }
            }  
        }

        protected T Execute<T>(ISqlTemplate sqlTemplate, object model)
        {
            var result = default(T);

            using (var command = CreateCommand(sqlTemplate, model))
            {
                using (var dbDataReader = command.ExecuteReader(CommandBehavior.Default))
                {
                    if (dbDataReader.Read())
                    {
                        result = (T) Get(typeof(T)).Invoke(dbDataReader, 0); 
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
