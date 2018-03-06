using System;
using System.Linq.Expressions;
using System.Text;

namespace Razor.Orm
{
    public abstract class QueryTemplate<TModel, TResult> : SqlTemplate<TModel>
    {
        public object As<T>(Expression<Func<TResult, T>> expression)
        {
            WriteLiteral(CompilationService.GetAs(expression));
            return null;
        }
    }

    public abstract class QueryTemplate<TResult> : QueryTemplate<dynamic, TResult>
    {

    }
}
