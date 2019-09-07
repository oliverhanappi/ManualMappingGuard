using Microsoft.CodeAnalysis;

namespace ManualMappingGuard.Analyzers
{
  public static class Diagnostics
  {
    public const string Declaration = "Declaration";
    public const string Mapping = "Mapping";

    public const string MissingMappingTargetTypeId = "MMG0001";

    public static readonly DiagnosticDescriptor MissingMappingTargetType = new DiagnosticDescriptor(
      id: MissingMappingTargetTypeId,
      title: "Missing mapping target type",
      messageFormat: "Unable to determine target type of mapping. Ensure that this method returns a value.",
      category: Declaration,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true);

    public const string UnmappedPropertyId = "MMG1001";

    public static readonly DiagnosticDescriptor UnmappedProperty = new DiagnosticDescriptor(
      id: UnmappedPropertyId,
      title: "Unmapped property",
      messageFormat: "Property {0} is not mapped.",
      category: Mapping,
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true);
  }
}
