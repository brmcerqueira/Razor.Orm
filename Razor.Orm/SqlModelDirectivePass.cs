using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Linq;

namespace Razor.Orm
{
    public class SqlModelDirectivePass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
    {
        private readonly bool designTime;

        public SqlModelDirectivePass(bool designTime)
        {
            this.designTime = designTime;
        }

        // Runs after the @inherits directive
        public override int Order => 5;

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var visitor = new SqlModelIntermediateNodeWalker();

            visitor.Visit(documentNode);

            var modelType = "dynamic";

            for (var i = visitor.ModelDirectives.Count - 1; i >= 0; i--)
            {
                var directive = visitor.ModelDirectives[i];

                var tokens = directive.Tokens.ToArray();
                if (tokens.Length >= 1)
                {
                    modelType = tokens[0].Content;
                    break;
                }
            }

            if (designTime)
            {
                // Alias the TModel token to a known type.
                // This allows design time compilation to succeed for Razor files where the token isn't replaced.
                var typeName = $"global::{typeof(object).FullName}";
                var usingNode = new UsingDirectiveIntermediateNode()
                {
                    Content = $"TModel = {typeName}"
                };

                visitor.Namespace?.Children.Insert(0, usingNode);
            }

            visitor.Class.BaseType = visitor.Class?.BaseType?.Replace("<TModel>", $"<{modelType}>");
        }
    }
}
