using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;

namespace Razor.Orm
{
    public static class CompilationService
    {
        public static RazorEngine Engine { get; }

        static CompilationService()
        {
            Engine = RazorEngine.Create(builder =>
            {
                SqlModelDirective.Register(builder);
                builder.Features.Add(new SqlDocumentClassifierPassBase());
            });
        }

        public static ISqlTemplate CreateCompilation(string compilationContent)
        {
            string assemblyName = Path.GetRandomFileName();

            SourceText sourceText = SourceText.From(compilationContent, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText).WithFilePath(assemblyName);

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(Resolve(DependencyContext.Load(Assembly.GetCallingAssembly())))
                .AddSyntaxTrees(syntaxTree);

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

                return (ISqlTemplate)Activator.CreateInstance(assembly.GetType("Razor.Orm.CompiledTemplates.GeneratedTemplate", true, true));
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
