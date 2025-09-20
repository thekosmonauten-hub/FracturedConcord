using UnityEngine;
using PassiveTree;

/// <summary>
/// Quick setup script for passive tree tooltips
/// Add this to any GameObject in your scene and run the setup
/// </summary>
public class QuickTooltipSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool debugMode = true;

    void Start()
    {
        if (setupOnStart)
        {
            SetupTooltips();
        }
    }

    /// <summary>
    /// Quick setup for tooltip system
    /// </summary>
    [ContextMenu("Quick Setup Tooltips")]
    public void SetupTooltips()
    {
        Debug.Log("🔧 [QuickTooltipSetup] Starting quick tooltip setup...");

        // 1. Find or create PassiveTreeTooltipSetup
        PassiveTreeTooltipSetup setup = FindFirstObjectByType<PassiveTreeTooltipSetup>();
        if (setup == null)
        {
            GameObject setupGO = new GameObject("PassiveTreeTooltipSetup");
            setup = setupGO.AddComponent<PassiveTreeTooltipSetup>();
            Debug.Log("✅ Created PassiveTreeTooltipSetup");
        }

        // 2. Run the setup
        setup.SetupTooltipSystem();

        // 3. Verify the setup
        VerifySetup();

        Debug.Log("🎉 [QuickTooltipSetup] Quick tooltip setup complete!");
    }

    /// <summary>
    /// Verify that the tooltip system is working
    /// </summary>
    private void VerifySetup()
    {
        // Check for required components
        JsonPassiveTreeTooltip tooltip = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        JsonBoardDataManager dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        CellController[] cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);

        Debug.Log("🔍 [QuickTooltipSetup] Verification Results:");
        Debug.Log($"  - JsonPassiveTreeTooltip: {(tooltip != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"  - JsonBoardDataManager: {(dataManager != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"  - CellControllers: {(cells.Length > 0 ? $"✅ Found {cells.Length} cells" : "❌ No cells found")}");

        if (dataManager != null)
        {
            Debug.Log($"  - JSON Data: {(dataManager.IsDataLoaded ? "✅ Loaded" : "❌ Not loaded")}");
        }

        // Test tooltip if possible
        if (tooltip != null && cells.Length > 0)
        {
            Debug.Log("🧪 [QuickTooltipSetup] Testing tooltip with first cell...");
            tooltip.ShowTooltip(cells[0]);
            
            // Hide after 3 seconds
            Invoke(nameof(HideTestTooltip), 3f);
        }
    }

    /// <summary>
    /// Hide the test tooltip
    /// </summary>
    private void HideTestTooltip()
    {
        JsonPassiveTreeTooltip tooltip = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        if (tooltip != null)
        {
            tooltip.HideTooltip();
            Debug.Log("🧪 [QuickTooltipSetup] Test tooltip hidden");
        }
    }

    /// <summary>
    /// Debug the current state
    /// </summary>
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("🔍 [QuickTooltipSetup] Current State Debug:");
        
        // Check for EventSystem
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log($"  - EventSystem: {(eventSystem != null ? "✅ Found" : "❌ Missing - This is required for tooltips!")}");
        
        // Check for GraphicRaycaster
        var raycaster = FindFirstObjectByType<UnityEngine.UI.GraphicRaycaster>();
        Debug.Log($"  - GraphicRaycaster: {(raycaster != null ? "✅ Found" : "❌ Missing - This is required for tooltips!")}");
        
        // Check for Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        Debug.Log($"  - Canvas: {(canvas != null ? "✅ Found" : "❌ Missing - This is required for tooltips!")}");
        
        // Check for Input System
        Debug.Log($"  - Input System: {(UnityEngine.InputSystem.InputSystem.settings != null ? "✅ Available" : "❌ Not available")}");
        Debug.Log($"  - Mouse Input: {(UnityEngine.InputSystem.Mouse.current != null ? "✅ Available" : "❌ Not available")}");
    }

    /// <summary>
    /// Create missing required components
    /// </summary>
    [ContextMenu("Create Missing Components")]
    public void CreateMissingComponents()
    {
        Debug.Log("🔧 [QuickTooltipSetup] Creating missing components...");

        // Create EventSystem if missing
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ Created EventSystem");
        }

        // Create Canvas if missing
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("✅ Created Canvas");
        }

        Debug.Log("🎉 [QuickTooltipSetup] Missing components created!");
    }
}
