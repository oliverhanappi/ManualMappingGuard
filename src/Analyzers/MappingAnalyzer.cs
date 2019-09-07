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

      var mappingTargetType = method.GetMappingTargetType(context.Compilation);
      if (mappingTargetType == null)
      {
        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MissingMappingTargetType, context.Node.GetLocation()));
        return;
      }

      var mappingTargetProperties = mappingTargetType.GetMappingTargetProperties();
      var mappedProperties = new HashSet<IPropertySymbol>();

      var methodDeclarationSyntax = (MethodDeclarationSyntax) context.Node;
      var assignmentExpressionSyntaxes = methodDeclarationSyntax.DescendantNodes()
        .OfType<AssignmentExpressionSyntax>()
        .ToList();

      foreach (var assignmentExpressionSyntax in assignmentExpressionSyntaxes)
      {
        var memberAccessExpressionSyntax = (MemberAccessExpressionSyntax) assignmentExpressionSyntax.Left;
        var accessedType = context.SemanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression);
        if (!accessedType.Type.Equals(mappingTargetType))
          continue;

        var properties = accessedType.Type.GetMembers(memberAccessExpressionSyntax.Name.ToString())
          .OfType<IPropertySymbol>();

        foreach (var propertySymbol in properties)
          mappedProperties.Add(propertySymbol);
      }

      foreach (var notAssignedProperty in mappingTargetProperties.Except(mappedProperties).OrderBy(p => p.Name))
      {
        var diagnostic = Diagnostic.Create(Diagnostics.UnmappedProperty, methodDeclarationSyntax.GetLocation(), notAssignedProperty.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
