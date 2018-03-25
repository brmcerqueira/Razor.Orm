using System;
using System.Collections;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Razor.Orm
{
    public abstract class DaoFactory : DaoGenerator
    {
        private Hashtable hashtable;

        public DaoFactory()
        {
            hashtable = new Hashtable();
            Generate(GetType().Assembly);
        }

        public T CreateDao<T>(SqlConnection sqlConnection)
        {
            return (T)(hashtable[typeof(T)] as Func<SqlConnection, Transformers, object>)(sqlConnection, Transformers);
        }

        protected override void GeneratedDao(Type interfaceType, Type generatedType)
        {
            var sqlConnectionType = typeof(SqlConnection);
            var transformersType = typeof(Transformers);
            var sqlConnectionParameter = Expression.Parameter(sqlConnectionType, "sqlConnection");
            var transformersParameter = Expression.Parameter(transformersType, "transformers");
            hashtable.Add(interfaceType, Expression.Lambda<Func<SqlConnection, Transformers, object>>(Expression.New(generatedType.GetConstructor(
            new Type[] { sqlConnectionType, transformersType }), new Expression[] { sqlConnectionParameter, transformersParameter }),
            new ParameterExpression[] { sqlConnectionParameter, transformersParameter }).Compile());
        }
    }
}
