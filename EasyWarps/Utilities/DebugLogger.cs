using StardewModdingAPI;
using EasyWarps.Core;

namespace EasyWarps.Utilities
{
    public static class DebugLogger
    {
        private static IMonitor? monitor;
        private static ModConfig? config;

        public static void Initialize(IMonitor logMonitor, ModConfig logConfig)
        {
            monitor = logMonitor;
            config = logConfig;
        }

        public static void Log(string message, LogLevel level)
        {
            if (monitor == null) return;

            if (level >= LogLevel.Warn)
            {
                monitor.Log(message, level);
                return;
            }

            if (config?.EnableDebugLogging == true)
            {
                monitor.Log(message, level);
            }
        }

        public static void Trace(string message)
        {
            if (monitor == null || config?.EnableDebugLogging != true) return;
            monitor.Log(message, LogLevel.Trace);
        }

        public static void Debug(string message)
        {
            if (monitor == null || config?.EnableDebugLogging != true) return;
            monitor.Log(message, LogLevel.Debug);
        }
    }
}
