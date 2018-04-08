using System;
using System.Collections;
using System.Data.Common;
using System.Linq.Expressions;

namespace Razor.Orm
{
    public abstract class DaoFactory : DaoGenerator
    {
        private Hashtable hashtable;

        public DaoFactory()
        {
            hashtable = new Hashtable();
            Generate();
        }

        public T CreateDao<T>(DbConnection connection)
        {
            return (T)(hashtable[typeof(T)] as Func<DbConnection, Extractor, object>)(connection, Extractor);
        }

        protected override void GeneratedDao(Type interfaceType, Type generatedType)
        {
            var sqlConnectionType = typeof(DbConnection);
            var transformersType = typeof(Extractor);
            var sqlConnectionParameter = Expression.Parameter(sqlConnectionType, "sqlConnection");
            var transformersParameter = Expression.Parameter(transformersType, "extractors");
            hashtable.Add(interfaceType, Expression.Lambda<Func<DbConnection, Extractor, object>>(Expression.New(generatedType.GetConstructor(
            new Type[] { sqlConnectionType, transformersType }), new Expression[] { sqlConnectionParameter, transformersParameter }),
            new ParameterExpression[] { sqlConnectionParameter, transformersParameter }).Compile());
        }
    }
}
