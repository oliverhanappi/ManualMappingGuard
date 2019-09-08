using System.Linq;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
  public static class MappingMethodDetection
  {
    public static bool IsMappingMethod(this IMethodSymbol method)
    {
      return method.GetAttributes().Any(a => a.AttributeClass.Name == "MappingMethodAttribute");
    }
  }
}