using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
    public static class Diagnostics
    {
        public const string Category = "ManualMappingGuard";
        
        public const string UnmappedPropertyId = "MMG0001";
        public static readonly DiagnosticDescriptor UnmappedProperty = new DiagnosticDescriptor(
            id: UnmappedPropertyId,
            title: "Unmapped property",
            messageFormat: "Property {0} is not mapped.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public const string UnsuitableMethodSignatureId = "MMG0002";
        public static readonly DiagnosticDescriptor UnsuitableMethodSignature = new DiagnosticDescriptor(
            id: UnsuitableMethodSignatureId,
            title: "Unsuitable method signature",
            messageFormat: "This method is not a suitable mapping method because it has no return value.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
