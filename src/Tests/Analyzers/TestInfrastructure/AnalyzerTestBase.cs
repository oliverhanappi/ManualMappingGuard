using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers.TestInfrastructure
{
  public abstract class AnalyzerTestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    protected virtual string CodeTemplate => "{0}";

    protected async Task<ImmutableArray<Diagnostic>> Analyze(string code,
      CancellationToken cancellationToken = default)
    {
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

      var compilation = CompilationUtility.Compile(actualCode).WithAnalyzers(analyzers);
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
  }
}
