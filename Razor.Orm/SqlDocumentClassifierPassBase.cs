using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Razor.Orm
{
    public class SqlDocumentClassifierPassBase : DocumentClassifierPassBase
    {
        protected override string DocumentKind => "razor.orm.sql.template";

        protected override bool IsMatch(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode) => true;

        protected override void OnDocumentStructureCreated(
            RazorCodeDocument codeDocument,
            NamespaceDeclarationIntermediateNode @namespace,
            ClassDeclarationIntermediateNode @class,
            MethodDeclarationIntermediateNode method)
        {
            base.OnDocumentStructureCreated(codeDocument, @namespace, @class, method);

            @namespace.Content = "Razor.Orm.CompiledTemplates";

            @class.ClassName = "GeneratedTemplate";
            @class.BaseType = "global::Razor.Orm.SqlTemplate<TModel>";
            @class.Modifiers.Clear();
            @class.Modifiers.Add("public");

            method.MethodName = "Execute";
            method.Modifiers.Clear();
            method.Modifiers.Add("protected");
            method.Modifiers.Add("override");
            method.ReturnType = "void";
        }
    }
}
