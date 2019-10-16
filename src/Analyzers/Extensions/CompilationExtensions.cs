using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ManualMappingGuard.Analyzers.Extensions
{
  public static class CompilationExtensions
  {
    public static ITypeSymbol GetExistingTypeByMetadataName(this Compilation compilation, string metadataName)
    {
      var type = compilation.GetTypeByMetadataName(metadataName);
      if (type == null)
        throw new ArgumentException($"Failed to get type {metadataName}.", nameof(metadataName));

      return type;
    }

    public static ITypeSymbol GetExistingType(this Compilation compilation, Type type)
    {
      return compilation.GetExistingTypeByMetadataName(type.FullName);
    }

    public static ITypeSymbol GetExistingType<T>(this Compilation compilation)
    {
      return compilation.GetExistingType(typeof(T));
    }

    public static bool TypeIsDerivedOrEqual(this Compilation compilation, ITypeSymbol typeToCheck, ITypeSymbol baseType)
    {
      return compilation.ClassifyConversion(typeToCheck, baseType).IsImplicit;
    }
  }
}
