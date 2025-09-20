using UnityEngine;
using PassiveTree;

/// <summary>
/// Helper script to set up tooltip system with your TooltipPanel.prefab
/// </summary>
public class TooltipSetupHelper : MonoBehaviour
{
    [Header("Tooltip Setup")]
    [SerializeField] private GameObject tooltipPanelPrefab;
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool enableTooltipLogging = true;
    [SerializeField] private bool createTooltipCanvas = true;
    [SerializeField] private bool useCustomCanvas = false;
    
    [Header("References")]
    [SerializeField] private JsonPassiveTreeTooltip tooltipSystem;
    [SerializeField] private WorldSpaceTooltipCanvas tooltipCanvas;
    [SerializeField] private Canvas customCanvas;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTooltipSystem();
        }
    }
    
    /// <summary>
    /// Set up the tooltip system
    /// </summary>
    [ContextMenu("Setup Tooltip System")]
    public void SetupTooltipSystem()
    {
        Debug.Log("[TooltipSetupHelper] Setting up tooltip system...");
        
        // Create tooltip Canvas first if requested
        if (createTooltipCanvas && !useCustomCanvas)
        {
            SetupTooltipCanvas();
        }
        else if (useCustomCanvas)
        {
            SetupCustomCanvas();
        }
        
        // Find or create JsonPassiveTreeTooltip
        if (tooltipSystem == null)
        {
            tooltipSystem = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        }
        
        if (tooltipSystem == null)
        {
            // Create tooltip system
            GameObject tooltipManager = new GameObject("TooltipManager");
            tooltipSystem = tooltipManager.AddComponent<JsonPassiveTreeTooltip>();
            Debug.Log("[TooltipSetupHelper] Created JsonPassiveTreeTooltip component");
        }
        
        // Assign the tooltip prefab
        if (tooltipPanelPrefab != null)
        {
            // Use reflection to set the private tooltipPrefab field
            var field = typeof(JsonPassiveTreeTooltip).GetField("tooltipPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(tooltipSystem, tooltipPanelPrefab);
                Debug.Log($"[TooltipSetupHelper] Assigned tooltip prefab: {tooltipPanelPrefab.name}");
            }
        }
        else
        {
            Debug.LogWarning("[TooltipSetupHelper] No tooltip prefab assigned! Please assign TooltipPanel.prefab in the Inspector.");
        }
        
        // Enable tooltip logging if requested
        if (enableTooltipLogging)
        {
            PassiveTreeLogger.SetCategoryLogging("tooltip", true);
            Debug.Log("[TooltipSetupHelper] Enabled tooltip logging");
        }
        
        Debug.Log("[TooltipSetupHelper] Tooltip system setup complete!");
    }
    
    /// <summary>
    /// Set up the tooltip Canvas
    /// </summary>
    private void SetupTooltipCanvas()
    {
        // Find or create WorldSpaceTooltipCanvas
        if (tooltipCanvas == null)
        {
            tooltipCanvas = FindFirstObjectByType<WorldSpaceTooltipCanvas>();
        }
        
        if (tooltipCanvas == null)
        {
            // Create tooltip Canvas manager
            GameObject canvasManager = new GameObject("TooltipCanvasManager");
            tooltipCanvas = canvasManager.AddComponent<WorldSpaceTooltipCanvas>();
            Debug.Log("[TooltipSetupHelper] Created WorldSpaceTooltipCanvas component");
        }
        
        // Create the Canvas
        tooltipCanvas.CreateTooltipCanvas();
        Debug.Log("[TooltipSetupHelper] Tooltip Canvas setup complete");
    }
    
    /// <summary>
    /// Set up using custom Canvas
    /// </summary>
    private void SetupCustomCanvas()
    {
        if (customCanvas == null)
        {
            Debug.LogError("[TooltipSetupHelper] Custom Canvas is not assigned! Please assign your Canvas in the Inspector.");
            return;
        }
        
        Debug.Log($"[TooltipSetupHelper] Using custom Canvas: {customCanvas.name}");
        
        // Ensure the custom Canvas is properly configured for tooltips
        if (customCanvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogWarning("[TooltipSetupHelper] Custom Canvas is in World Space mode. Tooltips may not display correctly. Consider using Screen Space Overlay or Screen Space Camera mode.");
        }
        
        // Set a high sorting order to ensure tooltips appear on top
        if (customCanvas.sortingOrder < 100)
        {
            customCanvas.sortingOrder = 100;
            Debug.Log($"[TooltipSetupHelper] Set Canvas sorting order to {customCanvas.sortingOrder} for tooltip visibility");
        }
    }
    
    /// <summary>
    /// Test the tooltip system
    /// </summary>
    [ContextMenu("Test Tooltip System")]
    public void TestTooltipSystem()
    {
        if (tooltipSystem == null)
        {
            Debug.LogError("[TooltipSetupHelper] No tooltip system found! Run Setup Tooltip System first.");
            return;
        }
        
        // Find a cell to test with
        var testCell = FindFirstObjectByType<CellController>();
        if (testCell != null)
        {
            Debug.Log($"[TooltipSetupHelper] Testing tooltip with cell at {testCell.GetGridPosition()}");
            tooltipSystem.ShowTooltip(testCell);
            
            // Hide after 3 seconds
            Invoke(nameof(HideTestTooltip), 3f);
        }
        else
        {
            Debug.LogError("[TooltipSetupHelper] No CellController found to test with!");
        }
    }
    
    private void HideTestTooltip()
    {
        if (tooltipSystem != null)
        {
            tooltipSystem.HideTooltip();
            Debug.Log("[TooltipSetupHelper] Test tooltip hidden");
        }
    }
    
    /// <summary>
    /// Enable tooltip logging for debugging
    /// </summary>
    [ContextMenu("Enable Tooltip Logging")]
    public void EnableTooltipLogging()
    {
        PassiveTreeLogger.SetCategoryLogging("tooltip", true);
        Debug.Log("[TooltipSetupHelper] Tooltip logging enabled");
    }
    
    /// <summary>
    /// Disable tooltip logging
    /// </summary>
    [ContextMenu("Disable Tooltip Logging")]
    public void DisableTooltipLogging()
    {
        PassiveTreeLogger.SetCategoryLogging("tooltip", false);
        Debug.Log("[TooltipSetupHelper] Tooltip logging disabled");
    }
}
