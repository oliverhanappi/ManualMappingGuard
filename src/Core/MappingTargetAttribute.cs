using System;

namespace ManualMappingGuard
{
  [AttributeUsage(AttributeTargets.Parameter)]
  public sealed class MappingTargetAttribute : Attribute
  {
  }
}
