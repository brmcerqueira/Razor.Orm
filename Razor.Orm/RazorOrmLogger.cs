﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;

namespace Razor.Orm
{
    public static class RazorOrmLogger
    {
        private static Hashtable asBinds = new Hashtable();
        private static ILoggerFactory _loggerFactory = null;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = new LoggerFactory();
                }

                return _loggerFactory;
            }
        }

        internal static ILogger<T> CreateLogger<T>(this T item)
        {
            return LoggerFactory?.CreateLogger<T>();
        }

        internal static string GetAsBind<T, TResult>(this Expression<Func<T, TResult>> expression)
        {
            if (asBinds.ContainsKey(expression))
            {
                return (string)asBinds[expression];
            }
            else
            {
                var stringBuilder = new StringBuilder();
                Stringify(stringBuilder, expression.Body);
                stringBuilder.Insert(0, "as '");
                stringBuilder.Append("'");
                var result = stringBuilder.ToString();
                asBinds.Add(expression, result);
                return result;
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