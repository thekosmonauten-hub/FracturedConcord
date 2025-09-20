using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Centralized logging system for Passive Tree to control debug output
    /// </summary>
    public static class PassiveTreeLogger
    {
        public enum LogLevel
        {
            None = 0,
            Errors = 1,
            Warnings = 2,
            Info = 3,
            Debug = 4,
            Verbose = 5
        }
        
        [Header("Logging Settings")]
        public static LogLevel CurrentLogLevel = LogLevel.Warnings; // Default to warnings and errors only
        public static bool EnableCategoryLogging = false;
        
        // Category flags for specific systems
        public static bool EnableJsonLogging = false;
        public static bool EnableCellLogging = false;
        public static bool EnableTooltipLogging = false;
        public static bool EnableManagerLogging = false;
        public static bool EnableInputLogging = false;
        public static bool EnableSpriteLogging = false;
        
        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(string message, string category = "")
        {
            if (CurrentLogLevel >= LogLevel.Errors)
            {
                Debug.LogError($"[PassiveTree] {message}");
            }
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(string message, string category = "")
        {
            if (CurrentLogLevel >= LogLevel.Warnings)
            {
                Debug.LogWarning($"[PassiveTree] {message}");
            }
        }
        
        /// <summary>
        /// Log an info message
        /// </summary>
        public static void LogInfo(string message, string category = "")
        {
            if (CurrentLogLevel >= LogLevel.Info)
            {
                Debug.Log($"[PassiveTree] {message}");
            }
        }
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void LogDebug(string message, string category = "")
        {
            if (CurrentLogLevel >= LogLevel.Debug)
            {
                Debug.Log($"[PassiveTree] {message}");
            }
        }
        
        /// <summary>
        /// Log a verbose message
        /// </summary>
        public static void LogVerbose(string message, string category = "")
        {
            if (CurrentLogLevel >= LogLevel.Verbose)
            {
                Debug.Log($"[PassiveTree] {message}");
            }
        }
        
        /// <summary>
        /// Log with category filtering
        /// </summary>
        public static void LogCategory(string message, string category)
        {
            if (!EnableCategoryLogging) return;
            
            switch (category.ToLower())
            {
                case "json":
                    if (EnableJsonLogging) Debug.Log($"[PassiveTree-JSON] {message}");
                    break;
                case "cell":
                    if (EnableCellLogging) Debug.Log($"[PassiveTree-Cell] {message}");
                    break;
                case "tooltip":
                    if (EnableTooltipLogging) Debug.Log($"[PassiveTree-Tooltip] {message}");
                    break;
                case "manager":
                    if (EnableManagerLogging) Debug.Log($"[PassiveTree-Manager] {message}");
                    break;
                case "input":
                    if (EnableInputLogging) Debug.Log($"[PassiveTree-Input] {message}");
                    break;
                case "sprite":
                    if (EnableSpriteLogging) Debug.Log($"[PassiveTree-Sprite] {message}");
                    break;
                default:
                    Debug.Log($"[PassiveTree-{category}] {message}");
                    break;
            }
        }
        
        /// <summary>
        /// Set log level from Inspector or code
        /// </summary>
        public static void SetLogLevel(LogLevel level)
        {
            CurrentLogLevel = level;
            Debug.Log($"[PassiveTree] Log level set to: {level}");
        }
        
        /// <summary>
        /// Enable/disable specific category logging
        /// </summary>
        public static void SetCategoryLogging(string category, bool enabled)
        {
            switch (category.ToLower())
            {
                case "json":
                    EnableJsonLogging = enabled;
                    break;
                case "cell":
                    EnableCellLogging = enabled;
                    break;
                case "tooltip":
                    EnableTooltipLogging = enabled;
                    break;
                case "manager":
                    EnableManagerLogging = enabled;
                    break;
                case "input":
                    EnableInputLogging = enabled;
                    break;
                case "sprite":
                    EnableSpriteLogging = enabled;
                    break;
            }
        }
    }
}

