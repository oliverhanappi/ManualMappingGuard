using System;
using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
    public static class CompilationExtensions
    {
        public static ITypeSymbol GetExistingTypeByMetadataName(this Compilation compilation, string metadataName)
        {
            var type = compilation.GetTypeByMetadataName(metadataName);
            if (type == null)
                throw new ArgumentException($"Failed to get type {metadataName}.");

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
    }
}
