using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private string assemblyName;
        private List<SyntaxTree> syntaxTrees;

        public DaoFactory(ILoggerFactory loggerFactory = null)
        {
            RazorOrmRoot.LoggerFactory = loggerFactory;
            logger = this.CreateLogger();

            var engine = RazorEngine.Create(builder =>
            {
                SqlModelDirective.Register(builder);
                FunctionsDirective.Register(builder);
                InheritsDirective.Register(builder);
                SectionDirective.Register(builder);
                builder.Features.Add(new SqlDocumentClassifierPassBase());
            });

            assemblyName = Path.GetRandomFileName();

            syntaxTrees = new List<SyntaxTree>();

            Setup();

            var assembly = Assembly.GetCallingAssembly();

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(@"namespace Razor.Orm.Templates { 
                        public class GeneratedTemplateFactory : global::Razor.Orm.TemplateFactory { 
                        public GeneratedTemplateFactory() { ");

            foreach (var item in assembly.GetManifestResourceNames())
            {
                var name = item.Replace('.', '_');
                RazorCodeDocument razorCodeDocument = RazorCodeDocument.Create(RazorSourceDocument.ReadFrom(assembly.GetManifestResourceStream(item), name));
                engine.Process(razorCodeDocument);
                var generatedCode = razorCodeDocument.GetCSharpDocument().GeneratedCode;
                logger?.LogInformation(generatedCode);
                Parse(generatedCode);
                stringBuilder.Append($"Add(\"{item}\", new {name}_GeneratedTemplate());");
            }

            stringBuilder.Append(" } } }");

            var defaultTemplateFactoryCode = stringBuilder.ToString();

            logger?.LogInformation(defaultTemplateFactoryCode);

            Parse(defaultTemplateFactoryCode);

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

                RazorOrmRoot.TemplateFactory = (TemplateFactory) Activator.CreateInstance(generatedAssembly.GetType("Razor.Orm.Templates.GeneratedTemplateFactory", true, true));
            }
        }

        private void Parse(string code)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8)).WithFilePath(assemblyName));
        }

        protected abstract void Setup();

        protected void RegisterTransform<T>(Func<SqlDataReader, int, T> function)
        {
            RazorOrmRoot.RegisterTransform(function);
        }

        protected void Define<T>() 
        {
            var type = typeof(T);
      
            if (!type.IsInterface)
            {
                throw new Exception("O tipo precisa ser uma interface.");
            }

            var stringBuilder = new StringBuilder();
            var stringBuilderMap = new StringBuilder();

            stringBuilder.Append($@"namespace Razor.Orm.Generated {{ 
                public class {type.Name}_GenerateDao : global::Razor.Orm.Dao, {type.FullNameForCode()} {{ 
                public {type.Name}_GenerateDao(global::System.Data.SqlClient.SqlConnection sqlConnection) : 
                base(sqlConnection) {{ }}");

            stringBuilderMap.Append($@"protected override global::System.Tuple<string, global::System.Func<global::System.Data.SqlClient.SqlDataReader, int, object>>[][] GetMap() {{ 
                return new global::System.Tuple<string, global::System.Func<global::System.Data.SqlClient.SqlDataReader, int, object>>[][] {{ ");

            var methodIndex = 0;

            foreach (var method in type.GetMethods())
            {
                var parameters = method.GetParameters();

                if (parameters.Length > 1)
                {
                    throw new Exception($"O metodo {method.Name} não pode ter mais de 1 parametro.");
                }

                stringBuilder.Append("public ");

                var returnType = method.ReturnType;

                var returnTypeClassifier = returnType.Classifier();

                switch (returnTypeClassifier)
                {
                    case TypeClassifier.Void:
                        stringBuilder.Append("void");
                        break;
                    case TypeClassifier.Enumerable:
                    case TypeClassifier.Object:
                        stringBuilder.Append(returnType.FullNameForCode());
                        break;
                }

                stringBuilder.Append($" {method.Name}(");

                ParameterInfo parameter = parameters.Length == 1 ? parameters[0] : null;

                if (parameter != null)
                {                   
                    stringBuilder.Append($"{parameter.ParameterType.FullNameForCode()} {parameter.Name}");
                }

                stringBuilder.Append(") { ");

                switch (returnTypeClassifier)
                {
                    case TypeClassifier.Void:
                        break;
                    case TypeClassifier.Enumerable:
                        var columnIndex = 0;
                        if (methodIndex > 0)
                        {
                            stringBuilderMap.Append(", ");
                        }

                        var stringBuilderTuple = new StringBuilder();
                        stringBuilder.Append($"return ExecuteReader(\"{type.Namespace}.{method.Name}.cshtml\", {methodIndex}, ");
                        stringBuilder.Append(parameter != null ? $"{parameter.Name}, " : "null, ");
                        stringBuilder.Append("d => { return ");
                        GenerateReturnTypeCallback(stringBuilder, stringBuilderTuple, null, returnType.GenericTypeArguments[0], ref columnIndex);
                        stringBuilderMap.Append(stringBuilderTuple);
                        stringBuilderMap.Append("}");
                        stringBuilder.Append("; });");
                        methodIndex++;
                        break;
                    case TypeClassifier.Object:
                        break;
                }

                stringBuilder.Append(" } ");
            }

            stringBuilder.Append(stringBuilderMap);

            stringBuilder.Append(" }; } } }");

            logger?.LogInformation(stringBuilder.ToString());
        }

        private void GenerateReturnTypeCallback(StringBuilder stringBuilderCallback, StringBuilder stringBuilderTuple, string path, Type type, ref int columnIndex)
        {
            switch (type.Classifier())
            {
                case TypeClassifier.Primitive:
                    stringBuilderCallback.Append($" d.Get<{type.FullNameForCode()}>({columnIndex})");

                    if (stringBuilderTuple.Length == 0)
                    {
                        stringBuilderTuple.Append(@" new global::System.Tuple<string,
                            global::System.Func<global::System.Data.SqlClient.SqlDataReader, int, object>>[] { ");
                    }
                    else
                    {
                        stringBuilderTuple.Append(", ");
                    }

                    stringBuilderTuple.Append($"global::System.Tuple.Create(\"{path.ToLower()}\", GetTransform(typeof({type.FullNameForCode()})))");        
                    columnIndex++;
                    break;
                case TypeClassifier.Object:
                    stringBuilderCallback.Append($" new {type.FullNameForCode()} () {{ ");

                    var properties = type.GetProperties();

                    for (int i = 0; i < properties.Length; i++)
                    {
                        var property = properties[i];
                        stringBuilderCallback.Append($"{property.Name} = ");
                        GenerateReturnTypeCallback(stringBuilderCallback, stringBuilderTuple, path != null ? $"{path}_{property.Name}" : property.Name, 
                            property.PropertyType, ref columnIndex);
                        if (i < properties.Length - 1)
                        {
                            stringBuilderCallback.Append(", ");
                        }
                    }

                    stringBuilderCallback.Append(" }");
                    break;
            }
        }

        private IReadOnlyList<MetadataReference> Resolve(DependencyContext dependencyContext)
        {
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var references = dependencyContext.CompileLibraries.SelectMany(library => library.ResolveReferencePaths());

            if (!references.Any())
            {
                throw new Exception("Can't load metadata reference from the entry assembly. Make sure PreserveCompilationContext is set to true in *.csproj file");
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
