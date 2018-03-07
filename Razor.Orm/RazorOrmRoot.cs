using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace Razor.Orm
{
    internal static class RazorOrmRoot
    {
        private static Hashtable hashtable = new Hashtable();
        internal static ILoggerFactory LoggerFactory { get; set; }

        internal static TemplateFactory TemplateFactory { get; set; }

        internal static ILogger<T> CreateLogger<T>(this T item)
        {
            return LoggerFactory?.CreateLogger<T>();
        }

        internal static AsBind GetAsBind<T, TResult>(this Expression<Func<T, TResult>> expression)
        {
            if (hashtable.ContainsKey(expression))
            {
                return (AsBind) hashtable[expression];
            }
            else
            {
                var stringBuilder = new StringBuilder();
                Stringify(stringBuilder, expression.Body);
                stringBuilder.Insert(0, "as '");
                stringBuilder.Append("'");
                var result = new AsBind(stringBuilder.ToString());
                hashtable.Add(expression, result);
                return result;
            }
        }

        internal static string FullNameForCode(this Type type)
        {
            if (type.IsGenericType)
            {
                return $"{type.FullName.Remove(type.FullName.IndexOf('`'))}<{ string.Join(',', type.GenericTypeArguments.Select(p => p.FullNameForCode()))}>";
            }
            else
            {
                return type.FullName;
            }
        }

        private static void Stringify(StringBuilder stringBuilder, Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                stringBuilder.Insert(0, memberExpression.Member.Name);

                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    stringBuilder.Insert(0, "_");
                    Stringify(stringBuilder, memberExpression.Expression);
                }
            }
        }
    }
}
