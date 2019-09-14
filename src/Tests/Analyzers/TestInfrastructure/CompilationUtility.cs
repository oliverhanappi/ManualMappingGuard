using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace ManualMappingGuard.Analyzers.TestInfrastructure
{
  public static class CompilationUtility
  {
    private static int s_nextId = 1;
    
    private static readonly Lazy<IReadOnlyCollection<MetadataReference>> s_metadataReferences
      = new Lazy<IReadOnlyCollection<MetadataReference>>(CreateMetadataReferences);

    private static IEnumerable<Type> GetReferencedTypes()
    {
      yield return typeof(object);
      yield return typeof(MappingMethodAttribute);
      yield return typeof(MappingTargetAttribute);
      yield return typeof(UnmappedPropertyAttribute);
    }
    
    private static IReadOnlyCollection<MetadataReference> CreateMetadataReferences()
    {
      var assemblies = ReferenceCollector
        .CollectReferences(GetReferencedTypes().Select(t => t.Assembly).ToArray());

      return assemblies
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .ToList();
    }

    public static Compilation Compile(string code)
    {
      var nextId = Interlocked.Increment(ref s_nextId);
      var assemblyName = $"T{nextId}_{TestContext.CurrentContext.Test.FullName}";
      var syntaxTree = CSharpSyntaxTree.ParseText(code);
      return CSharpCompilation.Create(assemblyName, new[] {syntaxTree}, s_metadataReferences.Value);
    }
  }
}
