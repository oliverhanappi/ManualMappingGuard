using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ManualMappingGuard.Analyzers
{
    public static class MappingMethodDetection
    {
        public static bool IsMappingMethod(this IMethodSymbol method)
        {
            return method.GetAttributes().Any(a => a.AttributeClass.Name == "MappingMethodAttribute");
        }

        public static AttributeSyntax? GetMappingMethodAttributeSyntax(
            MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel)
        {
            return methodDeclarationSyntax.AttributeLists
                .SelectMany(l => l.Attributes)
                .FirstOrDefault(a => semanticModel.GetTypeInfo(a).Type?.Name == "MappingMethodAttribute");
        }
    }
}
