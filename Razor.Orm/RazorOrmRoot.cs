using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    public static class RazorOrmRoot
    {
        private static Hashtable asBinds = new Hashtable();
        private static Hashtable transformers = new Hashtable();
        internal static ILoggerFactory LoggerFactory { get; set; }

        static RazorOrmRoot()
        {
            RegisterTransform((s, i) => s.IsDBNull(i) ? null : s.GetString(i));
            RegisterTransform((s, i) => s.GetInt64(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (long?)null : s.GetInt64(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? null : s.GetSqlBytes(i).Buffer);
            RegisterTransform((s, i) => s.GetByte(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (byte?)null : s.GetByte(i));
            RegisterTransform((s, i) => s.GetDateTime(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (DateTime?)null : s.GetDateTime(i));
            RegisterTransform((s, i) => s.GetDecimal(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (decimal?)null : s.GetDecimal(i));
            RegisterTransform((s, i) => s.GetFloat(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (float?)null : s.GetFloat(i));
            RegisterTransform((s, i) => s.GetInt32(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (int?)null : s.GetInt32(i));
            RegisterTransform((s, i) => s.GetInt16(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (short?)null : s.GetInt16(i));
            RegisterTransform((s, i) => s.GetTimeSpan(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (TimeSpan?)null : s.GetTimeSpan(i));
            RegisterTransform((s, i) => s.GetGuid(i));
            RegisterTransform((s, i) => s.IsDBNull(i) ? (Guid?)null : s.GetGuid(i));
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
                stringBuilder.Append("as '");
                stringBuilder.Append("'");
                var result = stringBuilder.ToString();
                asBinds.Add(expression, result);
                return result;
            }
        }

        internal static TypeClassifier Classifier(this Type type)
        {
            if (type == typeof(void))
            {
                return TypeClassifier.Void;
            }
            else if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime))
            {
                return TypeClassifier.Primitive;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return TypeClassifier.Enumerable;
            }
            else if (type.IsPublic && !type.IsAbstract && !type.IsInterface && type.GetConstructor(Type.EmptyTypes) != null)
            {
                return TypeClassifier.Object;
            }
            else
            {
                throw new Exception($"O tipo {type.FullNameForCode()} não pode ser classificado.");
            }
        }

        internal static string FullNameForCode(this Type type)
        {
            if (type.IsGenericType)
            {
                return $"global::{type.FullName.Remove(type.FullName.IndexOf('`'))}<{ string.Join(',', type.GenericTypeArguments.Select(p => p.FullNameForCode()))}>";
            }
            else
            {
                return $"global::{type.FullName}";
            }
        }

        internal static Func<SqlDataReader, int, object> GetTransform(Type type)
        {
            return (Func<SqlDataReader, int, object>) transformers[type];
        }

        internal static void RegisterTransform<T>(Func<SqlDataReader, int, T> function)
        {
            Func<SqlDataReader, int, object> castFunction = (s, i) => function(s, i);
            transformers.Add(typeof(T), castFunction);
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
