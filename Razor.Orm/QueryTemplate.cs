using System;
using System.Linq.Expressions;

namespace Razor.Orm
{
    public abstract class QueryTemplate<TModel, TResult> : SqlTemplate<TModel>
    {
        public EscapeString As<T>(Expression<Func<TResult, T>> expression)
        {
            return new EscapeString(expression.GetAsBind());
        }
    }

    public abstract class QueryTemplate<TResult> : QueryTemplate<dynamic, TResult>
    {

    }
}
