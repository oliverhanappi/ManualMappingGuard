using System;
using System.Collections.Generic;
using System.Linq;
using ManualMappingGuard;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ManualMappingGuard.Tests.Infrastructure
{
  public static class CompilationUtility
  {
    private static readonly Lazy<IReadOnlyCollection<MetadataReference>> s_metadataReferences
      = new Lazy<IReadOnlyCollection<MetadataReference>>(CreateMetadataReferences);

    private static IReadOnlyCollection<MetadataReference> CreateMetadataReferences()
    {
      var assemblies = ReferenceCollector
        .CollectReferences(typeof(object).Assembly, typeof(MappingMethodAttribute).Assembly);

      return assemblies
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .ToList();
    }

    public static Compilation Compile(string code)
    {
      var assemblyName = $"DynamicCompilation{Guid.NewGuid():N}";
      var syntaxTree = CSharpSyntaxTree.ParseText(code);
      return CSharpCompilation.Create(assemblyName, new[] {syntaxTree}, s_metadataReferences.Value);
    }
  }
}
