namespace ManualMappingGuard.Tests.Infrastructure
{
    public static class Build
    {
#if DEBUG
        public static bool IsDebug => true;
#else
        public static bool IsDebug => false;
#endif
    }
}
