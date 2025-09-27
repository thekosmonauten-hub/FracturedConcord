using UnityEngine;
using TMPro;
using PassiveTree;

/// <summary>
/// Setup helper for static tooltip panel system
/// </summary>
public class StaticTooltipSetupHelper : MonoBehaviour
{
    [Header("Static Tooltip Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool enableTooltipLogging = true;
    
    [Header("Panel Settings")]
    [SerializeField] private string defaultNameText = "Select a Passive";
    [SerializeField] private string defaultDescriptionText = "Hover over a passive node to see its details";
    [SerializeField] private bool showOnStart = false;
    
    [Header("References")]
    [SerializeField] private StaticTooltipPanel tooltipPanel;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupStaticTooltipSystem();
        }
    }
    
    /// <summary>
    /// Set up the static tooltip system
    /// </summary>
    [ContextMenu("Setup Static Tooltip System")]
    public void SetupStaticTooltipSystem()
    {
        Debug.Log("[StaticTooltipSetupHelper] Setting up static tooltip system...");
        
        // Find or create StaticTooltipPanel
        if (tooltipPanel == null)
        {
            tooltipPanel = FindFirstObjectByType<StaticTooltipPanel>();
        }
        
        if (tooltipPanel == null)
        {
            // Create static tooltip panel
            GameObject tooltipPanelObject = new GameObject("StaticTooltipPanel");
            tooltipPanel = tooltipPanelObject.AddComponent<StaticTooltipPanel>();
            Debug.Log("[StaticTooltipSetupHelper] Created StaticTooltipPanel component");
        }
        
        // Configure panel settings
        ConfigureTooltipPanel();
        
        // Connect to hover events
        ConnectToHoverEvents();
        
        // Enable tooltip logging if requested
        if (enableTooltipLogging)
        {
            PassiveTreeLogger.SetCategoryLogging("tooltip", true);
            Debug.Log("[StaticTooltipSetupHelper] Enabled tooltip logging");
        }
        
        Debug.Log("[StaticTooltipSetupHelper] Static tooltip system setup complete!");
    }
    
    /// <summary>
    /// Configure the tooltip panel
    /// </summary>
    private void ConfigureTooltipPanel()
    {
        if (tooltipPanel == null) return;
        
        // Set default texts
        tooltipPanel.SetDefaultTexts(defaultNameText, defaultDescriptionText);
        
        // Show or hide on start
        if (showOnStart)
        {
            tooltipPanel.ShowTooltip();
        }
        else
        {
            tooltipPanel.HideTooltip();
        }
        
        Debug.Log("[StaticTooltipSetupHelper] Tooltip panel configured");
    }
    
    /// <summary>
    /// Connect to hover events from all CellControllers
    /// </summary>
    private void ConnectToHoverEvents()
    {
        // Find all CellControllers in the scene
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        Debug.Log($"[StaticTooltipSetupHelper] Found {allCells.Length} CellControllers to connect to");
        
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
        
        Debug.Log("[StaticTooltipSetupHelper] Connected hover events to all cells");
    }
    
    /// <summary>
    /// Test the static tooltip system
    /// </summary>
    [ContextMenu("Test Static Tooltip System")]
    public void TestStaticTooltipSystem()
    {
        if (tooltipPanel == null)
        {
            Debug.LogError("[StaticTooltipSetupHelper] No static tooltip panel found! Run Setup Static Tooltip System first.");
            return;
        }
        
        // Find a cell to test with
        var testCell = FindFirstObjectByType<CellController>();
        if (testCell != null)
        {
            Debug.Log($"[StaticTooltipSetupHelper] Testing static tooltip with cell at {testCell.GetGridPosition()}");
            tooltipPanel.UpdateTooltipContent(testCell);
            
            // Hide after 3 seconds
            Invoke(nameof(HideTestTooltip), 3f);
        }
        else
        {
            Debug.LogError("[StaticTooltipSetupHelper] No CellController found to test with!");
        }
    }
    
    private void HideTestTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.HideTooltip();
            Debug.Log("[StaticTooltipSetupHelper] Test tooltip hidden");
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
            Debug.Log("[StaticTooltipSetupHelper] Tooltip panel shown");
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
            Debug.Log("[StaticTooltipSetupHelper] Tooltip panel hidden");
        }
    }
    
    /// <summary>
    /// Enable tooltip logging for debugging
    /// </summary>
    [ContextMenu("Enable Tooltip Logging")]
    public void EnableTooltipLogging()
    {
        PassiveTreeLogger.SetCategoryLogging("tooltip", true);
        Debug.Log("[StaticTooltipSetupHelper] Tooltip logging enabled");
    }
    
    /// <summary>
    /// Disable tooltip logging
    /// </summary>
    [ContextMenu("Disable Tooltip Logging")]
    public void DisableTooltipLogging()
    {
        PassiveTreeLogger.SetCategoryLogging("tooltip", false);
        Debug.Log("[StaticTooltipSetupHelper] Tooltip logging disabled");
    }
}












