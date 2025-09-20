using UnityEngine;
using PassiveTree;

/// <summary>
/// Setup helper for world space 3D tooltips (no Canvas required)
/// </summary>
public class WorldSpaceTooltipSetupHelper : MonoBehaviour
{
    [Header("3D Tooltip Setup")]
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool enableTooltipLogging = true;
    
    [Header("3D Tooltip Settings")]
    [SerializeField] private float tooltipDistance = 2f;
    [SerializeField] private Vector3 tooltipOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private bool followMouse = true;
    [SerializeField] private bool faceCamera = true;
    
    [Header("References")]
    [SerializeField] private WorldSpaceTooltip3D tooltipSystem;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            Setup3DTooltipSystem();
        }
    }
    
    /// <summary>
    /// Set up the 3D tooltip system
    /// </summary>
    [ContextMenu("Setup 3D Tooltip System")]
    public void Setup3DTooltipSystem()
    {
        Debug.Log("[WorldSpaceTooltipSetupHelper] Setting up 3D tooltip system...");
        
        // Find or create WorldSpaceTooltip3D
        if (tooltipSystem == null)
        {
            tooltipSystem = FindFirstObjectByType<WorldSpaceTooltip3D>();
        }
        
        if (tooltipSystem == null)
        {
            // Create 3D tooltip system
            GameObject tooltipManager = new GameObject("3DTooltipManager");
            tooltipSystem = tooltipManager.AddComponent<WorldSpaceTooltip3D>();
            Debug.Log("[WorldSpaceTooltipSetupHelper] Created WorldSpaceTooltip3D component");
        }
        
        // Assign the tooltip prefab
        if (tooltipPrefab != null)
        {
            // Use reflection to set the private tooltipPrefab field
            var field = typeof(WorldSpaceTooltip3D).GetField("tooltipPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(tooltipSystem, tooltipPrefab);
                Debug.Log($"[WorldSpaceTooltipSetupHelper] Assigned tooltip prefab: {tooltipPrefab.name}");
            }
        }
        else
        {
            Debug.LogWarning("[WorldSpaceTooltipSetupHelper] No tooltip prefab assigned! Will create dynamic 3D tooltip.");
        }
        
        // Configure 3D tooltip settings
        Configure3DTooltipSettings();
        
        // Enable tooltip logging if requested
        if (enableTooltipLogging)
        {
            PassiveTreeLogger.SetCategoryLogging("tooltip", true);
            Debug.Log("[WorldSpaceTooltipSetupHelper] Enabled tooltip logging");
        }
        
        Debug.Log("[WorldSpaceTooltipSetupHelper] 3D tooltip system setup complete!");
    }
    
    /// <summary>
    /// Configure 3D tooltip settings
    /// </summary>
    private void Configure3DTooltipSettings()
    {
        if (tooltipSystem == null) return;
        
        // Use reflection to set private fields
        var tooltipDistanceField = typeof(WorldSpaceTooltip3D).GetField("tooltipDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (tooltipDistanceField != null)
        {
            tooltipDistanceField.SetValue(tooltipSystem, tooltipDistance);
        }
        
        var tooltipOffsetField = typeof(WorldSpaceTooltip3D).GetField("tooltipOffset", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (tooltipOffsetField != null)
        {
            tooltipOffsetField.SetValue(tooltipSystem, tooltipOffset);
        }
        
        var followMouseField = typeof(WorldSpaceTooltip3D).GetField("followMouse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (followMouseField != null)
        {
            followMouseField.SetValue(tooltipSystem, followMouse);
        }
        
        var faceCameraField = typeof(WorldSpaceTooltip3D).GetField("faceCamera", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (faceCameraField != null)
        {
            faceCameraField.SetValue(tooltipSystem, faceCamera);
        }
        
        Debug.Log("[WorldSpaceTooltipSetupHelper] 3D tooltip settings configured");
    }
    
    /// <summary>
    /// Test the 3D tooltip system
    /// </summary>
    [ContextMenu("Test 3D Tooltip System")]
    public void Test3DTooltipSystem()
    {
        if (tooltipSystem == null)
        {
            Debug.LogError("[WorldSpaceTooltipSetupHelper] No 3D tooltip system found! Run Setup 3D Tooltip System first.");
            return;
        }
        
        // Find a cell to test with
        var testCell = FindFirstObjectByType<CellController>();
        if (testCell != null)
        {
            Debug.Log($"[WorldSpaceTooltipSetupHelper] Testing 3D tooltip with cell at {testCell.GetGridPosition()}");
            tooltipSystem.ShowTooltip(testCell);
            
            // Hide after 5 seconds
            Invoke(nameof(HideTestTooltip), 5f);
        }
        else
        {
            Debug.LogError("[WorldSpaceTooltipSetupHelper] No CellController found to test with!");
        }
    }
    
    private void HideTestTooltip()
    {
        if (tooltipSystem != null)
        {
            tooltipSystem.HideTooltip();
            Debug.Log("[WorldSpaceTooltipSetupHelper] Test 3D tooltip hidden");
        }
    }
    
    /// <summary>
    /// Enable tooltip logging for debugging
    /// </summary>
    [ContextMenu("Enable Tooltip Logging")]
    public void EnableTooltipLogging()
    {
        PassiveTreeLogger.SetCategoryLogging("tooltip", true);
        Debug.Log("[WorldSpaceTooltipSetupHelper] Tooltip logging enabled");
    }
    
    /// <summary>
    /// Disable tooltip logging
    /// </summary>
    [ContextMenu("Disable Tooltip Logging")]
    public void DisableTooltipLogging()
    {
        PassiveTreeLogger.SetCategoryLogging("tooltip", false);
        Debug.Log("[WorldSpaceTooltipSetupHelper] Tooltip logging disabled");
    }
}




