using System;

namespace Dexiled.Debugging
{
    /// <summary>
    /// No-op logger used to silence noisy debug output in specific files.
    /// </summary>
    public static class DevNullDebug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }
}
