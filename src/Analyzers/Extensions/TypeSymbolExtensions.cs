using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers.Extensions
{
  public static class TypeSymbolExtensions
  {
    public static IReadOnlyCollection<IPropertySymbol> GetInstanceProperties(this ITypeSymbol type)
    {
      var properties = type.GetMembers().OfType<IPropertySymbol>().Where(m => !m.IsStatic).ToList();
      var overriddenProperties = properties.Select(p => p.OverriddenProperty).ToList();

      if (type.BaseType != null)
      {
        var baseProperties = type.BaseType.GetInstanceProperties();
        properties.AddRange(baseProperties.Except(overriddenProperties));
      }

      return properties;
    }

    public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol typeToMatch)
    {
      ITypeSymbol typeToSearch = type;

      while (typeToSearch != null)
      {
        if (typeToSearch.Equals(typeToMatch))
        {
          return true;
        }

        typeToSearch = typeToSearch.BaseType;
      }

      return false;
    }
  }
}
