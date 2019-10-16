using System.Linq;
using ManualMappingGuard.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ManualMappingGuard.Analyzers
{
    public static class MappingMethodDetection
    {
        public static bool IsMappingMethod(this IMethodSymbol method, Compilation compilation)
        {
            return method.GetAttributes().Any(a => a.AttributeClass.InheritsFromOrEquals(compilation.GetExistingType<MappingMethodAttribute>()));
        }

        public static AttributeSyntax? GetMappingMethodAttributeSyntax(
            MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel)
        {
            return methodDeclarationSyntax.AttributeLists
                .SelectMany(l => l.Attributes)
                .FirstOrDefault(a => semanticModel.Compilation.TypeIsDerivedOrEqual(semanticModel.GetTypeInfo(a).Type, semanticModel.Compilation.GetExistingType<MappingMethodAttribute>()));
        }
    }
}
