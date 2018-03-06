using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Collections.Generic;

namespace Razor.Orm
{
    public class SqlModelIntermediateNodeWalker : IntermediateNodeWalker
    {
        public NamespaceDeclarationIntermediateNode Namespace { get; private set; }

        public ClassDeclarationIntermediateNode Class { get; private set; }

        public IList<DirectiveIntermediateNode> ModelDirectives { get; }

        public SqlModelIntermediateNodeWalker()
        {
            ModelDirectives = new List<DirectiveIntermediateNode>();
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationIntermediateNode node)
        {
            if (Namespace == null)
            {
                Namespace = node;
            }

            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationIntermediateNode node)
        {
            if (Class == null)
            {
                Class = node;
            }

            base.VisitClassDeclaration(node);
        }

        public override void VisitDirective(DirectiveIntermediateNode node)
        {
            if (node.Directive == SqlModelDirective.directive)
            {
                ModelDirectives.Add(node);
            }
        }
    }
}
