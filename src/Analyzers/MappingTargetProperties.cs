using System.Collections.Generic;
using System.Linq;
using ManualMappingGuard.Analyzers.Extensions;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
  public static class MappingTargetProperties
  {
    public static IReadOnlyCollection<IPropertySymbol> GetMappingTargetProperties(this ITypeSymbol type)
    {
      return type.GetInstanceProperties()
        .Where(p => p.DeclaredAccessibility == Accessibility.Public)
        .Where(p => p.SetMethod != null && p.SetMethod.DeclaredAccessibility == Accessibility.Public)
        .Where(p => !p.IsStatic)
        .Where(p => !p.IsIndexer)
        .Where(p => !p.IsImplicitlyDeclared)
        .ToList();
    }
  }
}
