using System;

namespace ManualMappingGuard
{
  [AttributeUsage(AttributeTargets.Method)]
  public sealed class MappingMethodAttribute : Attribute
  {
  }
}
