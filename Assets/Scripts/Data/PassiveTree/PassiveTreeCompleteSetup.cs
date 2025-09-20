using UnityEngine;
using PassiveTree;

/// <summary>
/// Complete setup script for the passive tree system with character stats integration
/// Sets up all components and applies TypeScript data with proper stat mapping
/// </summary>
public class PassiveTreeCompleteSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool applyTypeScriptData = true;
    [SerializeField] private bool setupStatsIntegration = true;
    [SerializeField] private bool debugMode = false;

    [Header("Components")]
    [SerializeField] private PassiveTreeDataSetup dataSetup;
    [SerializeField] private PassiveTreeStatsIntegration statsIntegration;
    [SerializeField] private PassiveTreeTooltipSetup tooltipSetup;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCompletePassiveTreeSystem();
        }
    }

    /// <summary>
    /// Set up the complete passive tree system
    /// </summary>
    [ContextMenu("Setup Complete Passive Tree System")]
    public void SetupCompletePassiveTreeSystem()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Setting up complete passive tree system...");

        // Step 1: Setup data system
        SetupDataSystem();

        // Step 2: Apply TypeScript data if requested
        if (applyTypeScriptData)
        {
            ApplyTypeScriptData();
        }

        // Step 3: Setup stats integration if requested
        if (setupStatsIntegration)
        {
            SetupStatsIntegration();
        }

        // Step 4: Setup tooltip system
        SetupTooltipSystem();

        // Step 5: Validate everything
        ValidateSetup();

        Debug.Log("[PassiveTreeCompleteSetup] Complete passive tree system setup finished!");
    }

    /// <summary>
    /// Setup the data system
    /// </summary>
    private void SetupDataSystem()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Setting up data system...");

        // Find or create data setup
        if (dataSetup == null)
        {
            dataSetup = FindFirstObjectByType<PassiveTreeDataSetup>();
            if (dataSetup == null)
            {
                GameObject dataSetupGO = new GameObject("PassiveTreeDataSetup");
                dataSetup = dataSetupGO.AddComponent<PassiveTreeDataSetup>();
            }
        }

        // Run data setup
        dataSetup.SetupPassiveTreeData();

        Debug.Log("[PassiveTreeCompleteSetup] Data system setup complete");
    }

    /// <summary>
    /// Apply TypeScript data to the board data
    /// </summary>
    private void ApplyTypeScriptData()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Applying TypeScript data...");

        // Find the CorePassiveTreeData asset
        string[] guids = UnityEditor.AssetDatabase.FindAssets("CorePassiveTreeData t:PassiveTreeBoardData");
        
        if (guids.Length == 0)
        {
            Debug.LogWarning("[PassiveTreeCompleteSetup] CorePassiveTreeData asset not found. Creating one...");
            CreateCorePassiveTreeDataAsset();
            return;
        }

        string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        PassiveTreeBoardData existingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveTreeBoardData>(assetPath);

        if (existingAsset == null)
        {
            Debug.LogError("[PassiveTreeCompleteSetup] Could not load CorePassiveTreeData asset.");
            return;
        }

        // Create converted data
        PassiveTreeBoardData convertedData = TypeScriptDataConverter.ConvertCoreBoardData();
        
        // Copy the converted data to the existing asset
        CopyBoardDataToAsset(convertedData, existingAsset);
        
        // Mark as dirty and save
        UnityEditor.EditorUtility.SetDirty(existingAsset);
        UnityEditor.AssetDatabase.SaveAssets();
        
        Debug.Log("[PassiveTreeCompleteSetup] TypeScript data applied successfully");
    }

    /// <summary>
    /// Setup stats integration
    /// </summary>
    private void SetupStatsIntegration()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Setting up stats integration...");

        // Find or create stats integration
        if (statsIntegration == null)
        {
            statsIntegration = FindFirstObjectByType<PassiveTreeStatsIntegration>();
            if (statsIntegration == null)
            {
                GameObject statsIntegrationGO = new GameObject("PassiveTreeStatsIntegration");
                statsIntegration = statsIntegrationGO.AddComponent<PassiveTreeStatsIntegration>();
            }
        }

        // Setup the integration
        statsIntegration.SetupIntegration();

        Debug.Log("[PassiveTreeCompleteSetup] Stats integration setup complete");
    }

    /// <summary>
    /// Setup tooltip system
    /// </summary>
    private void SetupTooltipSystem()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Setting up tooltip system...");

        // Find or create tooltip setup
        if (tooltipSetup == null)
        {
            tooltipSetup = FindFirstObjectByType<PassiveTreeTooltipSetup>();
            if (tooltipSetup == null)
            {
                GameObject tooltipSetupGO = new GameObject("PassiveTreeTooltipSetup");
                tooltipSetup = tooltipSetupGO.AddComponent<PassiveTreeTooltipSetup>();
            }
        }

        // Setup the tooltip system
        tooltipSetup.SetupTooltipSystem();

        Debug.Log("[PassiveTreeCompleteSetup] Tooltip system setup complete");
    }

    /// <summary>
    /// Validate the complete setup
    /// </summary>
    private void ValidateSetup()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Validating setup...");

        bool isValid = true;

        // Check data setup
        if (dataSetup == null)
        {
            Debug.LogError("[PassiveTreeCompleteSetup] Data setup not found!");
            isValid = false;
        }

        // Check stats integration
        if (setupStatsIntegration && statsIntegration == null)
        {
            Debug.LogError("[PassiveTreeCompleteSetup] Stats integration not found!");
            isValid = false;
        }

        // Check tooltip setup
        if (tooltipSetup == null)
        {
            Debug.LogError("[PassiveTreeCompleteSetup] Tooltip setup not found!");
            isValid = false;
        }

        // Validate stat mappings
        if (statsIntegration != null)
        {
            statsIntegration.ValidateStatMappings();
        }

        if (isValid)
        {
            Debug.Log("[PassiveTreeCompleteSetup] ✅ Setup validation passed!");
        }
        else
        {
            Debug.LogError("[PassiveTreeCompleteSetup] ❌ Setup validation failed!");
        }
    }

    /// <summary>
    /// Create CorePassiveTreeData asset if it doesn't exist
    /// </summary>
    private void CreateCorePassiveTreeDataAsset()
    {
        #if UNITY_EDITOR
        string assetPath = "Assets/Scripts/Data/PassiveTree/ExtensionBoards/Core/CorePassiveTreeData.asset";
        
        // Create the asset
        PassiveTreeBoardData newAsset = TypeScriptDataConverter.ConvertCoreBoardData();
        UnityEditor.AssetDatabase.CreateAsset(newAsset, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        
        Debug.Log($"[PassiveTreeCompleteSetup] Created CorePassiveTreeData asset at: {assetPath}");
        #endif
    }

    /// <summary>
    /// Copy board data from source to target asset
    /// </summary>
    private void CopyBoardDataToAsset(PassiveTreeBoardData source, PassiveTreeBoardData target)
    {
        #if UNITY_EDITOR
        // Copy all serialized fields
        var fields = typeof(PassiveTreeBoardData).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // Check if field has SerializeField attribute
            if (field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
            {
                object value = field.GetValue(source);
                field.SetValue(target, value);
            }
        }
        
        Debug.Log("[PassiveTreeCompleteSetup] Copied TypeScript data to existing asset");
        #endif
    }

    /// <summary>
    /// Test the complete system
    /// </summary>
    [ContextMenu("Test Complete System")]
    public void TestCompleteSystem()
    {
        Debug.Log("[PassiveTreeCompleteSetup] Testing complete system...");

        // Test data system
        if (dataSetup != null)
        {
            dataSetup.TestDataSystem();
        }

        // Test stats integration
        if (statsIntegration != null)
        {
            statsIntegration.ValidateStatMappings();
        }

        // Test tooltip system
        if (tooltipSetup != null)
        {
            tooltipSetup.TestTooltipSystem();
        }

        Debug.Log("[PassiveTreeCompleteSetup] System test complete");
    }

    /// <summary>
    /// Get setup status
    /// </summary>
    public string GetSetupStatus()
    {
        string status = "Passive Tree System Status:\n";
        status += $"Data Setup: {(dataSetup != null ? "✅" : "❌")}\n";
        status += $"Stats Integration: {(statsIntegration != null ? "✅" : "❌")}\n";
        status += $"Tooltip Setup: {(tooltipSetup != null ? "✅" : "❌")}\n";
        status += $"TypeScript Data Applied: {(applyTypeScriptData ? "✅" : "❌")}\n";
        
        return status;
    }
}
