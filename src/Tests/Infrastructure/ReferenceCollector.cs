using System.Collections.Generic;
using System.Reflection;

namespace ManualMappingGuard.Tests.Infrastructure
{
  public static class ReferenceCollector
  {
    public static IReadOnlyCollection<Assembly> CollectReferences(params Assembly[] rootAssemblies)
    {
      var assemblies = new HashSet<Assembly>();

      foreach (var rootAssembly in rootAssemblies)
        Collect(rootAssembly);

      return assemblies;

      void Collect(Assembly assembly)
      {
        if (assemblies.Add(assembly))
        {
          foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
          {
            var referencedAssembly = Assembly.Load(referencedAssemblyName);
            Collect(referencedAssembly);
          }
        }
      }
    }
  }
}
