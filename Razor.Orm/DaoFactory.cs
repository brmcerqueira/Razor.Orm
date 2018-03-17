﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    public abstract class DaoFactory
    {
        private ILogger logger;
        protected Transformers Transformers { get; }
        private string assemblyName;
        private List<Type> types;
        private List<SyntaxTree> syntaxTrees;
        private Hashtable hashtable;

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

            Transformers = new Transformers();
            assemblyName = Path.GetRandomFileName();
            types = new List<Type>();
            syntaxTrees = new List<SyntaxTree>();
            hashtable = new Hashtable();

            Setup();

            var assembly = Assembly.GetCallingAssembly();

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(@"namespace Razor.Orm.Templates { 
                        internal static class GeneratedTemplateFactory {");

            foreach (var item in assembly.GetManifestResourceNames())
            {
                var name = item.Replace('.', '_');
                var razorCodeDocument = RazorCodeDocument.Create(RazorSourceDocument.ReadFrom(assembly.GetManifestResourceStream(item), name));
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

                var sqlConnectionType = typeof(SqlConnection);
                var transformersType = typeof(Transformers);

                foreach (var item in generatedAssembly.GetExportedTypes())
                {
                    var sqlConnectionParameter = Expression.Parameter(sqlConnectionType, "sqlConnection");
                    var transformersParameter = Expression.Parameter(transformersType, "transformers");               
                    hashtable.Add(types.First(e => e.IsAssignableFrom(item)), Expression.Lambda<Func<SqlConnection, Transformers, object>>(Expression.New(item.GetConstructor(
                    new Type[] { sqlConnectionType, transformersType }), new Expression[] { sqlConnectionParameter, transformersParameter }), 
                    new ParameterExpression[] { sqlConnectionParameter, transformersParameter }).Compile());
                }
            }
        }

        private void Parse(string code)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8)).WithFilePath(assemblyName));
        }

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
                var parameters = method.GetParameters();

                if (parameters.Length > 1)
                {
                    throw new Exception($"O metodo {method.Name} não pode ter mais de 1 parametro.");
                }

                stringBuilder.Append("public ");

                var returnType = method.ReturnType;

                var returnTypeClassifier = Classifier(returnType);

                switch (returnTypeClassifier)
                {
                    case TypeClassifier.Void:
                        stringBuilder.Append("void");
                        break;
                    case TypeClassifier.Enumerable:
                    case TypeClassifier.Object:
                        stringBuilder.Append(FullNameForCode(returnType));
                        break;
                }

                stringBuilder.Append($" {method.Name}(");

                ParameterInfo parameter = parameters.Length == 1 ? parameters[0] : null;

                if (parameter != null)
                {
                    stringBuilder.Append($"{FullNameForCode(parameter.ParameterType)} {parameter.Name}");
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
                        stringBuilder.Append($@"return ExecuteReader(
                            global::Razor.Orm.Templates.GeneratedTemplateFactory.{type.Namespace.Replace('.', '_')}_{method.Name}_cshtml_Instance, 
                            {methodIndex}, ");
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

            var code = stringBuilder.ToString();

            Parse(code);
        }

        private void GenerateReturnTypeCallback(StringBuilder stringBuilderCallback, StringBuilder stringBuilderTuple, string path, Type type, ref int columnIndex)
        {
            switch (Classifier(type))
            {
                case TypeClassifier.Primitive:
                    stringBuilderCallback.Append($" d.Get<{FullNameForCode(type)}>({columnIndex})");

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
                    stringBuilderCallback.Append($" new {FullNameForCode(type)} () {{ ");

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

        public T CreateDao<T>(SqlConnection sqlConnection)
        {
            return (T) (hashtable[typeof(T)] as Func<SqlConnection, Transformers, object>)(sqlConnection, Transformers);
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

        internal string FullNameForCode(Type type)
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
