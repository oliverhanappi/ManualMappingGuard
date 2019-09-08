using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
  public class RootPropertyEqualityComparer : IEqualityComparer<IPropertySymbol>
  {
    public bool Equals(IPropertySymbol? property1, IPropertySymbol? property2)
    {
      if (ReferenceEquals(property1, property2)) return true;
      if (ReferenceEquals(property1, null) || ReferenceEquals(property2, null)) return false;

      var rootProperty1 = GetRootProperty(property1);
      var rootProperty2 = GetRootProperty(property2);
      
      return rootProperty1.Equals(rootProperty2);
    }

    public int GetHashCode(IPropertySymbol property)
    {
      var rootProperty = GetRootProperty(property);
      return rootProperty.GetHashCode();
    }

    private IPropertySymbol GetRootProperty(IPropertySymbol property)
    {
      return property.OverriddenProperty != null
        ? GetRootProperty(property.OverriddenProperty)
        : property;
    }
  }
}
