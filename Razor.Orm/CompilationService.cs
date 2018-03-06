using System;
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

namespace Razor.Orm
{
    public static class CompilationService
    {
        private static RazorEngine engine;
        private static IList<RazorCodeDocument> list;
        private static TemplateFactory _templateFactory;

        static CompilationService()
        {
            engine = RazorEngine.Create(builder =>
            {
                SqlModelDirective.Register(builder);
                FunctionsDirective.Register(builder);
                InheritsDirective.Register(builder);
                SectionDirective.Register(builder);
                builder.Features.Add(new SqlDocumentClassifierPassBase());
            });
            list = new List<RazorCodeDocument>();
        }

        public static void Add(RazorCodeDocument razorCodeDocument)
        {
            engine.Process(razorCodeDocument);
            list.Add(razorCodeDocument);
        }

        public static TemplateFactory TemplateFactory
        {
            get
            {
                if (_templateFactory == null)
                {
                    string assemblyName = Path.GetRandomFileName();

                    var trees = new List<SyntaxTree>();

                    Action<string> parse = c => trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(c, Encoding.UTF8)).WithFilePath(assemblyName));

                    var stringBuilder = new StringBuilder();

                    stringBuilder.Append(@"namespace Razor.Orm.Templates { 
                        public class DefaultTemplateFactory : global::Razor.Orm.TemplateFactory { 
                        public DefaultTemplateFactory() { ");

                    foreach (var item in list)
                    {
                        var source = item.Source;
                        stringBuilder.Append($"Add<{source.FilePath}_GeneratedTemplate>(\"{source.FilePath}\");");
                        parse(item.GetCSharpDocument().GeneratedCode);
                    }

                    stringBuilder.Append(@" } } }");

                    parse(stringBuilder.ToString());

                    CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                        .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                        .AddReferences(Resolve(DependencyContext.Load(Assembly.GetCallingAssembly())))
                        .AddSyntaxTrees(trees);


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

                            Console.WriteLine(builder.ToString());
                            errorMessages.ForEach(s => Console.WriteLine(s));
                        }

                        assemblyStream.Seek(0, SeekOrigin.Begin);

                        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);

                        _templateFactory = (TemplateFactory) Activator.CreateInstance(assembly.GetType("Razor.Orm.Templates.DefaultTemplateFactory", true, true));
                    }         
                }

                return _templateFactory;
            }
        }

        private static IReadOnlyList<MetadataReference> Resolve(DependencyContext dependencyContext)
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
