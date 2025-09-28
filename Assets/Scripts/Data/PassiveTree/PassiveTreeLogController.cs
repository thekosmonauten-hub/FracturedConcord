using UnityEngine;
using PassiveTree;

/// <summary>
/// Inspector-controllable logging settings for Passive Tree system
/// </summary>
public class PassiveTreeLogController : MonoBehaviour
{
    [Header("Log Level Control")]
    [SerializeField] private PassiveTreeLogger.LogLevel logLevel = PassiveTreeLogger.LogLevel.Warnings;
    [SerializeField] private bool enableCategoryLogging = false;
    
    [Header("Category Controls")]
    [SerializeField] private bool enableJsonLogging = false;
    [SerializeField] private bool enableCellLogging = false;
    [SerializeField] private bool enableTooltipLogging = false;
    [SerializeField] private bool enableManagerLogging = false;
    [SerializeField] private bool enableInputLogging = false;
    [SerializeField] private bool enableSpriteLogging = false;
    
    [Header("Quick Presets")]
    [SerializeField] private bool useQuietMode = true;
    [SerializeField] private bool useDebugMode = false;
    [SerializeField] private bool useVerboseMode = false;
    
    void Start()
    {
        ApplyLogSettings();
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyLogSettings();
        }
    }
    
    /// <summary>
    /// Apply the current log settings
    /// </summary>
    private void ApplyLogSettings()
    {
        // Apply preset modes
        if (useQuietMode)
        {
            logLevel = PassiveTreeLogger.LogLevel.Errors;
            enableCategoryLogging = false;
        }
        else if (useDebugMode)
        {
            logLevel = PassiveTreeLogger.LogLevel.Debug;
            enableCategoryLogging = true;
        }
        else if (useVerboseMode)
        {
            logLevel = PassiveTreeLogger.LogLevel.Verbose;
            enableCategoryLogging = true;
        }
        
        // Apply settings
        PassiveTreeLogger.SetLogLevel(logLevel);
        PassiveTreeLogger.EnableCategoryLogging = enableCategoryLogging;
        
        PassiveTreeLogger.SetCategoryLogging("json", enableJsonLogging);
        PassiveTreeLogger.SetCategoryLogging("cell", enableCellLogging);
        PassiveTreeLogger.SetCategoryLogging("tooltip", enableTooltipLogging);
        PassiveTreeLogger.SetCategoryLogging("manager", enableManagerLogging);
        PassiveTreeLogger.SetCategoryLogging("input", enableInputLogging);
        PassiveTreeLogger.SetCategoryLogging("sprite", enableSpriteLogging);
    }
    
    /// <summary>
    /// Set quiet mode (errors only)
    /// </summary>
    [ContextMenu("Set Quiet Mode")]
    public void SetQuietMode()
    {
        useQuietMode = true;
        useDebugMode = false;
        useVerboseMode = false;
        ApplyLogSettings();
    }
    
    /// <summary>
    /// Set debug mode (warnings and errors)
    /// </summary>
    [ContextMenu("Set Debug Mode")]
    public void SetDebugMode()
    {
        useQuietMode = false;
        useDebugMode = true;
        useVerboseMode = false;
        ApplyLogSettings();
    }
    
    /// <summary>
    /// Set verbose mode (all logs)
    /// </summary>
    [ContextMenu("Set Verbose Mode")]
    public void SetVerboseMode()
    {
        useQuietMode = false;
        useDebugMode = false;
        useVerboseMode = true;
        ApplyLogSettings();
    }
}













