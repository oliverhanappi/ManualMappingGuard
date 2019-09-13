using System;

namespace ManualMappingGuard
{
  [AttributeUsage(AttributeTargets.Parameter)]
  public class MappingTargetAttribute : Attribute
  {
  }
}
