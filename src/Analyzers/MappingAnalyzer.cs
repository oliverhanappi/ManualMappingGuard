using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ManualMappingGuard.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MappingAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Diagnostics.UnmappedProperty, Diagnostics.UnsuitableMethodSignature);

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
            var methodSymbol = (IMethodSymbol) context.ContainingSymbol;
            if (!IsMappingMethod(context.Compilation, methodSymbol))
                return;

            var mappingTargetType = GetMappingTargetType(context.Compilation, methodSymbol);
            if (mappingTargetType == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnsuitableMethodSignature, context.Node.GetLocation()));
                return;
            }

            var allProperties = mappingTargetType.GetMembers().OfType<IPropertySymbol>().ToList();
            var assignedProperties = new HashSet<IPropertySymbol>();

            var methodDeclarationSyntax = (MethodDeclarationSyntax) context.Node;
            var assignmentExpressionSyntaxes = methodDeclarationSyntax.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .ToList();

            foreach (var assignmentExpressionSyntax in assignmentExpressionSyntaxes)
            {
                SymbolFinder.FindReferencesAsync()
                var memberGroup = context.SemanticModel.GetMemberGroup(assignmentExpressionSyntax);

                var xInfo = context.SemanticModel.GetTypeInfo(assignmentExpressionSyntax.Left);
                var memberAccessExpressionSyntax = (MemberAccessExpressionSyntax) assignmentExpressionSyntax.Left;
                var accessedType = context.SemanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression);
                if (!accessedType.Type.Equals(mappingTargetType))
                    continue;

                var properties = accessedType.Type.GetMembers(memberAccessExpressionSyntax.Name.ToString())
                    .OfType<IPropertySymbol>();
                
                foreach (var propertySymbol in properties)
                    assignedProperties.Add(propertySymbol);
            }

            foreach (var notAssignedProperty in allProperties.Except(assignedProperties).OrderBy(p => p.Name))
            {
                var diagnostic = Diagnostic.Create(Diagnostics.UnmappedProperty, methodDeclarationSyntax.GetLocation(), notAssignedProperty.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsMappingMethod(Compilation compilation, IMethodSymbol methodSymbol)
        {
            var mappingMethodAttributeType = compilation
                .GetExistingTypeByMetadataName("ManualMappingGuard.MappingMethodAttribute");

            return methodSymbol.GetAttributes().Any(a => a.AttributeClass.Equals(mappingMethodAttributeType));
        }

        private static ITypeSymbol? GetMappingTargetType(Compilation compilation, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.ReturnsVoid)
                return null;

            var returnType = methodSymbol.ReturnType;
            
            var taskType = compilation.GetExistingType<Task>();
            if (returnType.Equals(taskType))
                return null;

            var taskResultType = compilation.GetExistingType(typeof(Task<>));
            if (taskResultType.Equals(returnType.OriginalDefinition) && returnType is INamedTypeSymbol namedReturnType)
                return namedReturnType.TypeArguments.Single();

            return returnType;
        }
    }
}
