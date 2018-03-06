using System;
using System.Linq.Expressions;
using System.Text;

namespace Razor.Orm
{
    public abstract class QueryTemplate<TModel, TResult> : SqlTemplate<TModel>
    {
        public AsBind As<T>(Expression<Func<TResult, T>> expression)
        {
            return CompilationService.GetAs(expression);
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
