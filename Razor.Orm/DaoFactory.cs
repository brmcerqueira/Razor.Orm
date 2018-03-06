using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace Razor.Orm
{
    public abstract class DaoFactory
    {
        private ILogger logger;

        public DaoFactory(ILoggerFactory loggerFactory = null)
        {
            RazorOrmExtensions.LoggerFactory = loggerFactory;
            logger = this.CreateLogger();

            var engine = RazorEngine.Create(builder =>
            {
                SqlModelDirective.Register(builder);
                FunctionsDirective.Register(builder);
                InheritsDirective.Register(builder);
                SectionDirective.Register(builder);
                builder.Features.Add(new SqlDocumentClassifierPassBase());
            });

            var assemblyName = Path.GetRandomFileName();

            var syntaxTrees = new List<SyntaxTree>();

            Setup();

            var assembly = Assembly.GetCallingAssembly();

            Action<string> parse = c => syntaxTrees.Add(CSharpSyntaxTree.ParseText(SourceText.From(c, Encoding.UTF8)).WithFilePath(assemblyName));

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(@"namespace Razor.Orm.Templates { 
                        public class DefaultTemplateFactory : global::Razor.Orm.TemplateFactory { 
                        public DefaultTemplateFactory() { ");

            foreach (var item in assembly.GetManifestResourceNames())
            {
                var name = item.Replace('.', '_');
                RazorCodeDocument razorCodeDocument = RazorCodeDocument.Create(RazorSourceDocument.ReadFrom(assembly.GetManifestResourceStream(item), name));
                engine.Process(razorCodeDocument);
                logger?.LogInformation(razorCodeDocument.GetCSharpDocument().GeneratedCode);
                parse(razorCodeDocument.GetCSharpDocument().GeneratedCode);
                stringBuilder.Append($"Add<{name}_GeneratedTemplate>(\"{item}\");");
            }

            stringBuilder.Append(" } } }");

            var defaultTemplateFactoryCode = stringBuilder.ToString();

            logger?.LogInformation(defaultTemplateFactoryCode);

            parse(defaultTemplateFactoryCode);

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(Resolve(DependencyContext.Load(Assembly.GetCallingAssembly())))
            .AddSyntaxTrees(syntaxTrees);

            using (var assemblyStream = new MemoryStream())
            {
                var result = compilation.Emit(assemblyStream);

                if (!result.Success)
                {
                    List<Diagnostic> errorsDiagnostics = result.Diagnostics
                            .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
                            .ToList();

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("Failed to compile generated Razor template:");

                    var errorMessages = new List<string>();
                    foreach (Diagnostic diagnostic in errorsDiagnostics)
                    {
                        FileLinePositionSpan lineSpan = diagnostic.Location.SourceTree.GetMappedLineSpan(diagnostic.Location.SourceSpan);
                        string errorMessage = diagnostic.GetMessage();
                        string formattedMessage = $"- ({lineSpan.StartLinePosition.Line}:{lineSpan.StartLinePosition.Character}) {errorMessage}";

                        errorMessages.Add(formattedMessage);
                        builder.AppendLine(formattedMessage);
                    }

                    builder.AppendLine("\nSee CompilationErrors for detailed information");

                    if (logger != null)
                    {
                        logger.LogInformation(builder.ToString());
                        logger.LogInformation(string.Join('\n', errorMessages));
                    }
                }

                assemblyStream.Seek(0, SeekOrigin.Begin);

                var generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);

                TemplateFactory = (TemplateFactory)Activator.CreateInstance(generatedAssembly.GetType("Razor.Orm.Templates.DefaultTemplateFactory", true, true));
            }
        }

        protected abstract void Setup();

        public TemplateFactory TemplateFactory { get; private set; }

        private IReadOnlyList<MetadataReference> Resolve(DependencyContext dependencyContext)
        {
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var references = dependencyContext.CompileLibraries.SelectMany(library => library.ResolveReferencePaths());

            if (!references.Any())
            {
                throw new Exception(@"Can't load metadata reference from the entry assembly. 
                    Make sure PreserveCompilationContext is set to true in *.csproj file");
            }

            var metadataRerefences = new List<MetadataReference>();

            foreach (var reference in references)
            {
                if (libraryPaths.Add(reference))
                {
                    using (var stream = File.OpenRead(reference))
                    {
                        var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                        var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                        metadataRerefences.Add(assemblyMetadata.GetReference(filePath: reference));
                    }
                }
            }

            return metadataRerefences;
        }
    }
}
