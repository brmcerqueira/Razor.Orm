using System.Data.SqlClient;

namespace Razor.Orm
{
    public struct SqlTemplateResult
    {
        public string Content { get; }
        public SqlParameter[] Parameters { get; }

        public SqlTemplateResult(string content, SqlParameter[] parameters)
        {
            Content = content;
            Parameters = parameters;
        }
    }
}
