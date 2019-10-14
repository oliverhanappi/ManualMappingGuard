using System;
using System.Collections.Generic;

namespace ManualMappingGuard
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public class UnmappedPropertiesAttribute : Attribute
  {
    public IReadOnlyCollection<string> PropertyNames { get; }

    public UnmappedPropertiesAttribute(params string[] propertyNames)
    {
      PropertyNames = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
    }
  }
}
