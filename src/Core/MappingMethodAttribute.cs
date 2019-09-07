using System;

namespace ManualMappingGuard
{
  [AttributeUsage(AttributeTargets.Method)]
  public class MappingMethodAttribute : Attribute
  {
  }
}
