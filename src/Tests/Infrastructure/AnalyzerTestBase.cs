using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace ManualMappingGuard.Tests.Infrastructure
{
    public abstract class AnalyzerTestBase<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        protected virtual string CodeTemplate => "{0}";
        
        protected virtual IEnumerable<Assembly> References
        {
            get
            {
                yield return typeof(object).Assembly;
                yield return GetType().Assembly;
                yield return typeof(MappingMethodAttribute).Assembly;
            }
        }

        protected async Task<ImmutableArray<Diagnostic>> Analyze(string code,
            CancellationToken cancellationToken = default)
        {
            var references = GetAllReferences();
            if (Build.IsDebug)
            {
                TestContext.WriteLine("References:");
                TestContext.WriteLine();
                
                foreach (var reference in references) 
                    TestContext.WriteLine($"  - {reference.FullName}");
            }

            var analyzer = new TAnalyzer();
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

            var actualCode = String.Format(CodeTemplate, code);

            if (Build.IsDebug)
            {
                TestContext.WriteLine("Analyzed code:");
                TestContext.WriteLine();
                TestContext.WriteLine("--------------------------------------------------------------");
                TestContext.WriteLine(actualCode);
                TestContext.WriteLine("--------------------------------------------------------------");
            }
            
            var syntaxTree = CSharpSyntaxTree.ParseText(actualCode);
            var compilation = CSharpCompilation.Create(TestContext.CurrentContext.Test.FullName)
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(references.Select(a => MetadataReference.CreateFromFile(a.Location)))
                .WithAnalyzers(analyzers);

            var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzers, cancellationToken);

            if (Build.IsDebug)
            {
                TestContext.WriteLine();
                TestContext.WriteLine("Diagnostics:");
                foreach (var diagnostic in diagnostics) 
                    TestContext.WriteLine($"  - {diagnostic}");
            }
            
            return diagnostics;
        }

        private IReadOnlyCollection<Assembly> GetAllReferences()
        {
            var assemblies = new HashSet<Assembly>();

            foreach (var reference in References)
            {
                Collect(reference);
            }

            void Collect(Assembly assembly)
            {
                if (assemblies.Add(assembly))
                {
                    foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
                    {
                        var referencedAssembly = Assembly.Load(referencedAssemblyName);
                        Collect(referencedAssembly);
                    }
                }
            }

            return assemblies;
        }
    }
}
