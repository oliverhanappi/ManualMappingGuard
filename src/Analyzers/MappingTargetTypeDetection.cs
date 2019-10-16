using System.Linq;
using System.Threading.Tasks;
using ManualMappingGuard.Analyzers.Extensions;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
  public static class MappingTargetTypeDetection 
  {
    public static ITypeSymbol? GetMappingTargetType(this IMethodSymbol method, Compilation compilation)
    {
      var returnType = GetReturnType(method, compilation);
      var mappingTargetParameterType = GetMappingTargetParameterType(method, compilation);

      if (returnType != null && mappingTargetParameterType != null)
        return null;

      return returnType ?? mappingTargetParameterType;
    }

    private static ITypeSymbol? GetReturnType(IMethodSymbol method, Compilation compilation)
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

    private static ITypeSymbol? GetMappingTargetParameterType(IMethodSymbol method, Compilation compilation)
    {
      var mappingTargetTypes = method.Parameters
        .Where(p => p.GetAttributes().Any(a => a.AttributeClass.InheritsFromOrEquals(compilation.GetExistingType<MappingTargetAttribute>())))
        .Select(p => p.Type)
        .ToList();

      return mappingTargetTypes.Count == 1 ? mappingTargetTypes[0] : null;
    }
  }
}
