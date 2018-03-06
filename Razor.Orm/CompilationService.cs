using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
    public static class CompilationService
    {
        private static RazorEngine engine;
        private static string assemblyName;
        private static List<SyntaxTree> list;
        private static Hashtable hashtable;
        private static ILoggerFactory _loggerFactory;
        private static ILogger logger;
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
            assemblyName = Path.GetRandomFileName();
            list = new List<SyntaxTree>();
            hashtable = new Hashtable();
            _loggerFactory = null;
        }

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                return _loggerFactory;
            }
            set
            {
                _loggerFactory = value;
                logger = _loggerFactory.CreateLogger("CompilationService");
            }
        }

        private static void Parse(string code)
        {
            list.Add(CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8)).WithFilePath(assemblyName));
        }

        public static void Start()
        {
            var assembly = Assembly.GetCallingAssembly();

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
                Parse(razorCodeDocument.GetCSharpDocument().GeneratedCode);
                stringBuilder.Append($"Add<{name}_GeneratedTemplate>(\"{item}\");");
            }

            stringBuilder.Append(" } } }");

            var defaultTemplateFactoryCode = stringBuilder.ToString();

            logger?.LogInformation(defaultTemplateFactoryCode);

            Parse(defaultTemplateFactoryCode);
        }

        public static AsBind GetAs<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            if (hashtable.ContainsKey(expression))
            {
                return (AsBind) hashtable[expression];
            }
            else
            {
                var stringBuilder = new StringBuilder();
                Stringify(stringBuilder, expression.Body);
                stringBuilder.Insert(0, "as '");
                stringBuilder.Append("'");
                var result = new AsBind(stringBuilder.ToString());
                hashtable.Add(expression, result);
                return result;
            }
        }

        private static void Stringify(StringBuilder stringBuilder, Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                stringBuilder.Insert(0, memberExpression.Member.Name);

                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    stringBuilder.Insert(0, "_");
                    Stringify(stringBuilder, memberExpression.Expression);
                }
            }
        }

        public static TemplateFactory TemplateFactory
        {
            get
            {
                if (_templateFactory == null)
                {
                    CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                        .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                        .AddReferences(Resolve(DependencyContext.Load(Assembly.GetCallingAssembly())))
                        .AddSyntaxTrees(list);

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
