using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ManualMappingGuard.Analyzers
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class MappingAnalyzer : DiagnosticAnalyzer
  {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
      ImmutableArray.Create(Diagnostics.UnmappedProperty, Diagnostics.MissingMappingTargetType);

    public override void Initialize(AnalysisContext context)
    {
      if (context == null)
        throw new ArgumentNullException(nameof(context));

      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
      context.RegisterSyntaxNodeAction(OnMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private void OnMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
      var method = (IMethodSymbol) context.ContainingSymbol;
      if (!method.IsMappingMethod())
        return;

      var methodDeclarationSyntax = (MethodDeclarationSyntax) context.Node;
      var mappingMethodAttributeSyntax = MappingMethodDetection
        .GetMappingMethodAttributeSyntax(methodDeclarationSyntax, context.SemanticModel);

      var location = mappingMethodAttributeSyntax?.GetLocation() ?? methodDeclarationSyntax.Identifier.GetLocation();

      var mappingTargetType = method.GetMappingTargetType(context.Compilation);
      if (mappingTargetType == null)
      {
        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MissingMappingTargetType, location));
        return;
      }

      var mappingTargetProperties = mappingTargetType.GetMappingTargetProperties();
      var mappedProperties = new HashSet<IPropertySymbol>();

      foreach (var assignment in context.Node.DescendantNodes().OfType<AssignmentExpressionSyntax>())
      {
        var assignmentTarget = context.SemanticModel.GetSymbolInfo(assignment.Left);
        if (assignmentTarget.Symbol is IPropertySymbol targetProperty)
          mappedProperties.Add(targetProperty);
      }

      var unmappedProperties = mappingTargetProperties.Except(mappedProperties, new RootPropertyEqualityComparer());
      foreach (var unmappedProperty in unmappedProperties.OrderBy(p => p.Name))
      {
        var diagnostic = Diagnostic.Create(Diagnostics.UnmappedProperty, location, unmappedProperty.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
