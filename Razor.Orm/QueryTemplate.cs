using System;
using System.Linq.Expressions;

namespace Razor.Orm
{
    public abstract class QueryTemplate<TModel, TResult> : SqlTemplate<TModel>
    {
        public AsBind As<T>(Expression<Func<TResult, T>> expression)
        {
            return expression.GetAsBind();
        }

        protected virtual void Write(AsBind value)
        {
            WriteLiteral(value.Content);
        }
    }

    public abstract class QueryTemplate<TResult> : QueryTemplate<dynamic, TResult>
    {

    }
}
