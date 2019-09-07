using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
  public static class MethodSymbolExtensions
  {
    public static bool IsMappingMethod(this IMethodSymbol method)
    {
      return method.GetAttributes().Any(a => a.AttributeClass.Name == "MappingMethodAttribute");
    }

    public static ITypeSymbol? GetMappingTargetType(this IMethodSymbol method, Compilation compilation)
    {
      if (method.ReturnsVoid)
        return null;

      var returnType = method.ReturnType;

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
