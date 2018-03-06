using Microsoft.AspNetCore.Razor.Language;

namespace Razor.Orm
{
    public static class SqlModelDirective
    {
        public static IRazorEngineBuilder Register(IRazorEngineBuilder builder)
        {
            var directive = DirectiveDescriptor.CreateDirective(
            "model",
            DirectiveKind.SingleLine,
            configure =>
            {
                configure.AddTypeToken();
                configure.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                configure.Description = "Model type bind";
            });

            builder.AddDirective(directive);
            builder.Features.Add(new SqlModelDirectivePass(directive, builder.DesignTime));
            return builder;
        }
    }
}
