using System.Collections.Generic;

namespace Razor.Orm
{
    public struct SqlTemplateResult
    {
        public string Content { get; }
        public KeyValuePair<string, object>[] Parameters { get; }

        public SqlTemplateResult(string content, KeyValuePair<string, object>[] parameters)
        {
            Content = content;
            Parameters = parameters;
        }
    }
}
