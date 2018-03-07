using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Razor.Orm
{
    public abstract class SqlTemplate<TModel> : ISqlTemplate
    {
        protected TModel Model { get; private set; }
        private ILogger logger;
        private StringBuilder stringBuilder;
        private int parametersIndex;
        private IList<SqlParameter> parameters;

        public SqlTemplate()
        {
            logger = this.CreateLogger();
            stringBuilder = new StringBuilder();
            parametersIndex = 0;
            parameters = new List<SqlParameter>();
        }

        public SqlTemplateResult Process(object model)
        {
            stringBuilder.Clear();
            parameters.Clear();

            var modelType = model.GetType();
            var typeInfo = modelType.GetTypeInfo();

            if (typeInfo.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() && modelType.FullName.Contains("AnonymousType"))
            {
                IDictionary<string, object> expando = new ExpandoObject();
                foreach (var propertyDescriptor in typeInfo.GetProperties())
                {
                    var obj = propertyDescriptor.GetValue(model);
                    expando.Add(propertyDescriptor.Name, obj);
                }

                Model = (TModel) expando;
            }
            else
            {
                Model = (TModel) model;
            }

            Execute();

            var sql = stringBuilder.ToString();
            var sqlParameters = parameters.ToArray();

            if (logger != null)
            {
                logger.LogInformation(sql);
                logger.LogInformation(string.Join('\n', sqlParameters.Select(e => $"{e.ParameterName} -> {e.Value}")));
            }

            return new SqlTemplateResult(sql, sqlParameters);
        }

        protected virtual void Write(object value)
        {
            var key = string.Format("@p{0}", parametersIndex);
            parametersIndex++;
            stringBuilder.Append(key);
            parameters.Add(new SqlParameter(key, value));
        }

        protected virtual void WriteLiteral(string value)
        {
            stringBuilder.Append(value);
        }

        protected abstract void Execute();
    }
}
