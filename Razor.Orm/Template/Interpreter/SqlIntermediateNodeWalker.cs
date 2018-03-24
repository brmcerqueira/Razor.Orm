using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Collections.Generic;

namespace Razor.Orm.Template.Interpreter
{
    public class SqlIntermediateNodeWalker : IntermediateNodeWalker
    {
        public NamespaceDeclarationIntermediateNode Namespace { get; private set; }

        public ClassDeclarationIntermediateNode Class { get; private set; }

        public IList<DirectiveIntermediateNode> Directives { get; }
        public DirectiveDescriptor DirectiveDescriptor { get; }

        public SqlIntermediateNodeWalker(DirectiveDescriptor directiveDescriptor)
        {
            Directives = new List<DirectiveIntermediateNode>();
            DirectiveDescriptor = directiveDescriptor;
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
            if (node.Directive == DirectiveDescriptor)
            {
                Directives.Add(node);
            }
        }
    }
}
