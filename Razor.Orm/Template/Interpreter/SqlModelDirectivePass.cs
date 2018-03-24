using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Linq;

namespace Razor.Orm.Template.Interpreter
{
    public class SqlModelDirectivePass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
    {
        public SqlModelDirectivePass(DirectiveDescriptor directive, bool designTime)
        {
            Directive = directive;
            DesignTime = designTime;
        }

        // Runs after the @inherits directive
        public override int Order => 5;

        public DirectiveDescriptor Directive { get; }
        public bool DesignTime { get; }

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var intermediateNodeWalker = new SqlIntermediateNodeWalker(Directive);

            intermediateNodeWalker.Visit(documentNode);

            var modelType = "dynamic";

            for (var i = intermediateNodeWalker.Directives.Count - 1; i >= 0; i--)
            {
                var directive = intermediateNodeWalker.Directives[i];

                var tokens = directive.Tokens.ToArray();
                if (tokens.Length >= 1)
                {
                    modelType = tokens[0].Content;
                    break;
                }
            }

            if (DesignTime)
            {
                // Alias the TModel token to a known type.
                // This allows design time compilation to succeed for Razor files where the token isn't replaced.
                var typeName = $"global::{typeof(object).FullName}";
                var usingNode = new UsingDirectiveIntermediateNode()
                {
                    Content = $"TModel = {typeName}"
                };

                intermediateNodeWalker.Namespace?.Children.Insert(0, usingNode);
            }

            intermediateNodeWalker.Class.BaseType = intermediateNodeWalker.Class?.BaseType?.Replace("<TModel>", $"<{modelType}>");
        }
    }
}
