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
using Microsoft.Extensions.Logging;
using Razor.Orm.Template.Interpreter;

namespace Razor.Orm
{
    public abstract class DaoGenerator
    {
        private ILogger logger;
        private RazorEngine engine;
        private Assembly assembly;
        private string assemblyName;
        private List<Type> types;
        private List<SyntaxTree> syntaxTrees;
        protected Transformers Transformers { get; }

        public DaoGenerator()
        {
            logger = this.CreateLogger();

            engine = RazorEngine.Create(builder =>
            {
                SqlModelDirective.Register(builder);
                FunctionsDirective.Register(builder);
                InheritsDirective.Register(builder);
                SectionDirective.Register(builder);
                builder.Features.Add(new SqlDocumentClassifierPassBase());
            });

            assembly = GetType().Assembly;
            assemblyName = Path.GetRandomFileName();
            types = new List<Type>();
            syntaxTrees = new List<SyntaxTree>();
            Transformers = new Transformers();
        }

        protected void Generate()
        {
            Setup();

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(@"namespace Razor.Orm.Template { 
                        internal static class GeneratedTemplateFactory {");

            foreach (var item in assembly.GetManifestResourceNames())
            {
                var name = item.Replace('.', '_');

                RazorSourceDocument razorSourceDocument = null;

                var stream = new MemoryStream();

                assembly.GetManifestResourceStream(item).CopyTo(stream);

                using (stream)
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {              
                    writer.WriteLine("@using System");
                    writer.WriteLine("@using System");
                    writer.WriteLine("@using System.Collections.Generic");
                    writer.WriteLine("@using System.Linq");
                    writer.WriteLine("@using System.Threading.Tasks");
                    writer.Flush();
                    stream.Position = 0;
                    razorSourceDocument = RazorSourceDocument.ReadFrom(stream, name, Encoding.UTF8);
                }

                var razorCodeDocument = RazorCodeDocument.Create(razorSourceDocument);
                engine.Process(razorCodeDocument);
                var generatedCode = razorCodeDocument.GetCSharpDocument().GeneratedCode;
                Parse(generatedCode);

                stringBuilder.Append($@"
                    private static {name}_GeneratedTemplate private_{name}_instance = new {name}_GeneratedTemplate();

                    internal static {name}_GeneratedTemplate {name}_Instance 
                    {{ 
                        get
                        {{
                            return private_{name}_instance;
                        }}
                    }} ");
            }

            stringBuilder.Append(" } }");

            var defaultTemplateFactoryCode = stringBuilder.ToString();

            Parse(defaultTemplateFactoryCode);
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))       
            .AddReferences(Resolve())
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

                foreach (var item in generatedAssembly.GetExportedTypes())
                {
                    GeneratedDao(types.First(e => e.IsAssignableFrom(item)), item);
                }
            }
        }

        protected abstract void GeneratedDao(Type interfaceType, Type generatedType);

        protected abstract void Setup();

        protected void Define<T>()
        {
            var type = typeof(T);

            if (!type.IsInterface)
            {
                throw new Exception("O tipo precisa ser uma interface.");
            }

            types.Add(type);

            var stringBuilder = new StringBuilder();
            var stringBuilderMap = new StringBuilder();

            stringBuilder.Append($@"namespace Razor.Orm.Generated {{ 
                public class {type.Name}_GenerateDao : global::Razor.Orm.Dao, {FullNameForCode(type)} {{ 
                public {type.Name}_GenerateDao(global::System.Data.SqlClient.SqlConnection sqlConnection, global::Razor.Orm.Transformers transformers) : 
                base(sqlConnection, transformers) {{ }}");

            stringBuilderMap.Append($@"protected override global::System.Tuple<string, global::System.Func<global::System.Data.SqlClient.SqlDataReader, int, object>>[][] GetMap() {{ 
                return new global::System.Tuple<string, global::System.Func<global::System.Data.SqlClient.SqlDataReader, int, object>>[][] {{ ");

            var methodIndex = 0;

            foreach (var method in type.GetMethods())
            {
                stringBuilder.Append("public ");

                var returnType = method.ReturnType;

                var returnTypeClassifier = Classifier(returnType);

                switch (returnTypeClassifier)
                {
                    case TypeClassifier.Void:
                        stringBuilder.Append("void");
                        break;
                    case TypeClassifier.Primitive:
                    case TypeClassifier.Enumerable:
                    case TypeClassifier.Object:
                        stringBuilder.Append(FullNameForCode(returnType));
                        break;
                }

                stringBuilder.Append($" {method.Name}(");

                var stringBuilderModel = new StringBuilder();
                var parameters = method.GetParameters();

                for (int i = 0; i < parameters.Length; i++)
                {
                    stringBuilder.Append($"{FullNameForCode(parameters[i].ParameterType)} {parameters[i].Name}");

                    stringBuilderModel.Append(parameters[i].Name);

                    if (i < parameters.Length - 1)
                    {
                        stringBuilder.Append(", ");
                        stringBuilderModel.Append(", ");
                    }
                }

                if (parameters.Length > 1)
                {
                    stringBuilderModel.Insert(0, "global::System.Tuple.Create(");
                    stringBuilderModel.Append(")");
                }

                if (stringBuilderModel.Length == 0)
                {
                    stringBuilderModel.Append("null");
                }

                stringBuilder.Append(") { ");

                var template = $"global::Razor.Orm.Template.GeneratedTemplateFactory.{type.Namespace.Replace('.', '_')}_{method.Name}_cshtml_Instance";

                switch (returnTypeClassifier)
                {
                    case TypeClassifier.Void:
                        stringBuilder.Append($"Execute({template}, {stringBuilderModel});");
                        break;
                    case TypeClassifier.Primitive:
                        stringBuilder.Append($"return Execute<{FullNameForCode(returnType)}>({template}, {stringBuilderModel});");
                        break;
                    case TypeClassifier.Enumerable:
                        GenerateExecuteWithParse(stringBuilder, stringBuilderMap, returnType.GenericTypeArguments[0], 
                            $"ExecuteEnumerable({template}, {stringBuilderModel}", ref methodIndex);
                        break;
                    case TypeClassifier.Object:
                        GenerateExecuteWithParse(stringBuilder, stringBuilderMap, returnType, $"Execute({template}, {stringBuilderModel}", ref methodIndex);
                        break;
                }

                stringBuilder.Append(" } ");
            }

            stringBuilder.Append(stringBuilderMap);

            stringBuilder.Append(" }; } } }");

            var code = stringBuilder.ToString();

            Parse(code);
        }

        private void Parse(string code)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8)).WithFilePath(assemblyName));
        }

        private void GenerateExecuteWithParse(StringBuilder stringBuilder, StringBuilder stringBuilderMap, Type type, string method, ref int methodIndex)
        {
            var columnIndex = 0;
            if (methodIndex > 0)
            {
                stringBuilderMap.Append(", ");
            }

            var stringBuilderTuple = new StringBuilder();
            stringBuilder.Append($"return {method}, {methodIndex}, d => {{ return ");
            GenerateReturnParse(stringBuilder, stringBuilderTuple, type, null, ref columnIndex);
            stringBuilderMap.Append(stringBuilderTuple);
            stringBuilderMap.Append("}");
            stringBuilder.Append("; });");
            methodIndex++;
        }

        private void GenerateReturnParse(StringBuilder stringBuilder, StringBuilder stringBuilderTuple, Type type, string path, ref int columnIndex)
        {
            switch (Classifier(type))
            {
                case TypeClassifier.Primitive:
                    stringBuilder.Append($" d.Get<{FullNameForCode(type)}>({columnIndex})");

                    if (stringBuilderTuple.Length == 0)
                    {
                        stringBuilderTuple.Append(@" new global::System.Tuple<string,
                            global::System.Func<global::System.Data.SqlClient.SqlDataReader, int, object>>[] { ");
                    }
                    else
                    {
                        stringBuilderTuple.Append(", ");
                    }

                    stringBuilderTuple.Append($"global::System.Tuple.Create(\"{path.ToLower()}\", GetTransform(typeof({FullNameForCode(type)})))");        
                    columnIndex++;
                    break;
                case TypeClassifier.Object:
                    stringBuilder.Append($" new {FullNameForCode(type)} () {{ ");

                    var properties = type.GetProperties();

                    for (int i = 0; i < properties.Length; i++)
                    {
                        var property = properties[i];
                        stringBuilder.Append($"{property.Name} = ");
                        GenerateReturnParse(stringBuilder, stringBuilderTuple, property.PropertyType, 
                            path != null ? $"{path}_{property.Name}" : property.Name, ref columnIndex);
                        if (i < properties.Length - 1)
                        {
                            stringBuilder.Append(", ");
                        }
                    }

                    stringBuilder.Append(" }");
                    break;
            }
        }

        private TypeClassifier Classifier(Type type)
        {
            if (type == typeof(void))
            {
                return TypeClassifier.Void;
            }
            else if (Transformers.Contains(type))
            {
                return TypeClassifier.Primitive;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return TypeClassifier.Enumerable;
            }
            else if (type.IsPublic && !type.IsAbstract && !type.IsInterface && type.GetConstructor(Type.EmptyTypes) != null)
            {
                return TypeClassifier.Object;
            }
            else
            {
                throw new Exception($"O tipo {FullNameForCode(type)} não pode ser classificado.");
            }
        }

        private string FullNameForCode(Type type)
        {
            if (type.IsGenericType)
            {
                return $"global::{type.FullName.Remove(type.FullName.IndexOf('`'))}<{ string.Join(',', type.GenericTypeArguments.Select(p => FullNameForCode(p)))}>";
            }
            else
            {
                return $"global::{type.FullName}";
            }
        }

        private IReadOnlyList<MetadataReference> Resolve()
        {
            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var references = dependencyContext.CompileLibraries.SelectMany(library => library.ResolveReferencePaths());

            if (!references.Any())
            {
                throw new Exception("Can't load metadata reference from the entry assembly. Make sure PreserveCompilationContext is set to true in *.csproj file");
            }

            var metadataRerefences = new List<MetadataReference>();

            metadataRerefences.Add(AssemblyMetadata.CreateFromFile(assembly.Location).GetReference());

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
