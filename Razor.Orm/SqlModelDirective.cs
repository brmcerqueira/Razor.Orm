using Microsoft.AspNetCore.Razor.Language;

namespace Razor.Orm
{
    public static partial class SqlModelDirective
    {
        public static readonly DirectiveDescriptor directive = DirectiveDescriptor.CreateDirective(
            "model",
            DirectiveKind.SingleLine,
            builder =>
            {
                builder.AddTypeToken();
                builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                builder.Description = "Model type bind";
            });

        public static IRazorEngineBuilder Register(IRazorEngineBuilder builder)
        {
            builder.AddDirective(directive);
            builder.Features.Add(new SqlModelDirectivePass(builder.DesignTime));
            return builder;
        }
    }
}
