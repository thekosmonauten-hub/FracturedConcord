using UnityEngine;
using PassiveTree;

/// <summary>
/// Complete setup for static tooltip system with proper cell sizing
/// </summary>
public class CompleteTooltipSetup : MonoBehaviour
{
    [Header("Complete Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool enableTooltipLogging = true;
    
    [Header("Cell Size Settings")]
    [SerializeField] private Vector2 targetCellSize = new Vector2(128f, 128f); // 128x128 pixels to match sprite size
    [SerializeField] private float pixelsPerUnit = 100f;
    
    [Header("Tooltip Panel Settings")]
    [SerializeField] private string defaultNameText = "Select a Passive";
    [SerializeField] private string defaultDescriptionText = "Hover over a passive node to see its details";
    [SerializeField] private bool showOnStart = false;
    
    [Header("References")]
    [SerializeField] private StaticTooltipPanel tooltipPanel;
    [SerializeField] private CellSizeFixer cellSizeFixer;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            RunCompleteSetup();
        }
    }
    
    /// <summary>
    /// Run the complete setup process
    /// </summary>
    [ContextMenu("Run Complete Setup")]
    public void RunCompleteSetup()
    {
        Debug.Log("[CompleteTooltipSetup] Starting complete tooltip setup...");
        
        // Step 1: Fix cell sizes
        SetupCellSizes();
        
        // Step 2: Setup tooltip system
        SetupTooltipSystem();
        
        Debug.Log("[CompleteTooltipSetup] Complete setup finished!");
    }
    
    /// <summary>
    /// Setup cell sizes to match 128x128 sprites
    /// </summary>
    private void SetupCellSizes()
    {
        Debug.Log("[CompleteTooltipSetup] Step 1: Setting up cell sizes...");
        
        // Find or create CellSizeFixer
        if (cellSizeFixer == null)
        {
            cellSizeFixer = FindFirstObjectByType<CellSizeFixer>();
        }
        
        if (cellSizeFixer == null)
        {
            // Create cell size fixer
            GameObject sizeFixerObject = new GameObject("CellSizeFixer");
            cellSizeFixer = sizeFixerObject.AddComponent<CellSizeFixer>();
            Debug.Log("[CompleteTooltipSetup] Created CellSizeFixer component");
        }
        
        // Configure cell size fixer
        cellSizeFixer.SetTargetCellSize(targetCellSize);
        cellSizeFixer.SetPixelsPerUnit(pixelsPerUnit);
        
        // Fix all cell sizes
        cellSizeFixer.FixAllCellSizes();
        
        Debug.Log("[CompleteTooltipSetup] Cell sizes configured and fixed");
    }
    
    /// <summary>
    /// Setup the static tooltip system
    /// </summary>
    private void SetupTooltipSystem()
    {
        Debug.Log("[CompleteTooltipSetup] Step 2: Setting up tooltip system...");
        
        // Find or create StaticTooltipPanel
        if (tooltipPanel == null)
        {
            tooltipPanel = FindFirstObjectByType<StaticTooltipPanel>();
        }
        
        if (tooltipPanel == null)
        {
            Debug.Log("[CompleteTooltipSetup] No StaticTooltipPanel found! Please create a tooltip panel and add the StaticTooltipPanel component to it.");
            return;
        }
        
        // Configure tooltip panel
        tooltipPanel.SetDefaultTexts(defaultNameText, defaultDescriptionText);
        
        if (showOnStart)
        {
            tooltipPanel.ShowTooltip();
        }
        else
        {
            tooltipPanel.HideTooltip();
        }
        
        // Connect to hover events
        ConnectToHoverEvents();
        
        // Enable tooltip logging if requested
        if (enableTooltipLogging)
        {
            PassiveTreeLogger.SetCategoryLogging("tooltip", true);
            Debug.Log("[CompleteTooltipSetup] Enabled tooltip logging");
        }
        
        Debug.Log("[CompleteTooltipSetup] Tooltip system configured");
    }
    
    /// <summary>
    /// Connect to hover events from all CellControllers
    /// </summary>
    private void ConnectToHoverEvents()
    {
        // Find all CellControllers in the scene
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        Debug.Log($"[CompleteTooltipSetup] Found {allCells.Length} CellControllers to connect to");
        
        foreach (CellController cell in allCells)
        {
            // Add a component to handle hover events for this cell
            CellHoverHandler hoverHandler = cell.GetComponent<CellHoverHandler>();
            if (hoverHandler == null)
            {
                hoverHandler = cell.gameObject.AddComponent<CellHoverHandler>();
            }
            
            // Set the tooltip panel reference
            hoverHandler.SetTooltipPanel(tooltipPanel);
        }
        
        Debug.Log("[CompleteTooltipSetup] Connected hover events to all cells");
    }
    
    /// <summary>
    /// Test the complete system
    /// </summary>
    [ContextMenu("Test Complete System")]
    public void TestCompleteSystem()
    {
        Debug.Log("[CompleteTooltipSetup] Testing complete system...");
        
        // Test cell sizes
        if (cellSizeFixer != null)
        {
            cellSizeFixer.DebugCellSizes();
        }
        
        // Test tooltip system
        if (tooltipPanel != null)
        {
            var testCell = FindFirstObjectByType<CellController>();
            if (testCell != null)
            {
                Debug.Log($"[CompleteTooltipSetup] Testing tooltip with cell at {testCell.GetGridPosition()}");
                tooltipPanel.UpdateTooltipContent(testCell);
                
                // Hide after 3 seconds
                Invoke(nameof(HideTestTooltip), 3f);
            }
        }
    }
    
    private void HideTestTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.HideTooltip();
            Debug.Log("[CompleteTooltipSetup] Test tooltip hidden");
        }
    }
    
    /// <summary>
    /// Show tooltip panel
    /// </summary>
    [ContextMenu("Show Tooltip Panel")]
    public void ShowTooltipPanel()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.ShowTooltip();
            Debug.Log("[CompleteTooltipSetup] Tooltip panel shown");
        }
    }
    
    /// <summary>
    /// Hide tooltip panel
    /// </summary>
    [ContextMenu("Hide Tooltip Panel")]
    public void HideTooltipPanel()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.HideTooltip();
            Debug.Log("[CompleteTooltipSetup] Tooltip panel hidden");
        }
    }
}


