using UnityEngine;
using UnityEngine.UI;
using PassiveTree;

/// <summary>
/// Creates and manages a non-intrusive Canvas specifically for world space tooltips
/// </summary>
public class WorldSpaceTooltipCanvas : MonoBehaviour
{
    [Header("Canvas Settings")]
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private int sortingOrder = 1000; // High sorting order to appear on top
    [SerializeField] private bool dontDestroyOnLoad = true;
    
    [Header("World Space Settings")]
    [SerializeField] private float worldSpaceScale = 0.01f; // Scale factor for world space
    [SerializeField] private Vector3 worldSpaceOffset = new Vector3(0, 0.5f, 0); // Offset above objects
    
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private JsonPassiveTreeTooltip tooltipSystem;
    
    // Runtime
    private Canvas tooltipCanvas;
    private CanvasScaler canvasScaler;
    private GraphicRaycaster graphicRaycaster;
    
    void Start()
    {
        if (createOnStart)
        {
            CreateTooltipCanvas();
        }
    }
    
    /// <summary>
    /// Create a dedicated Canvas for tooltips
    /// </summary>
    [ContextMenu("Create Tooltip Canvas")]
    public void CreateTooltipCanvas()
    {
        Debug.Log("[WorldSpaceTooltipCanvas] Creating tooltip Canvas...");
        
        // Find world camera if not assigned
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
            if (worldCamera == null)
            {
                worldCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        if (worldCamera == null)
        {
            Debug.LogError("[WorldSpaceTooltipCanvas] No camera found! Cannot create tooltip Canvas.");
            return;
        }
        
        // Create Canvas GameObject
        GameObject canvasObject = new GameObject("TooltipCanvas");
        canvasObject.transform.SetParent(transform);
        
        // Add Canvas component
        tooltipCanvas = canvasObject.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.sortingOrder = sortingOrder;
        tooltipCanvas.pixelPerfect = false;
        
        // Add CanvasScaler for consistent scaling
        canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for UI interaction
        graphicRaycaster = canvasObject.AddComponent<GraphicRaycaster>();
        graphicRaycaster.ignoreReversedGraphics = true;
        graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        
        // Configure for world space tooltips
        ConfigureForWorldSpace();
        
        // Don't destroy on load if requested
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(canvasObject);
        }
        
        Debug.Log("[WorldSpaceTooltipCanvas] Tooltip Canvas created successfully!");
        
        // Connect to tooltip system
        ConnectToTooltipSystem();
    }
    
    /// <summary>
    /// Configure Canvas for world space tooltip display
    /// </summary>
    private void ConfigureForWorldSpace()
    {
        if (tooltipCanvas == null) return;
        
        // Set up for world space coordinates
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.worldCamera = worldCamera;
        
        // Ensure tooltips appear on top of everything
        tooltipCanvas.sortingOrder = sortingOrder;
        
        Debug.Log($"[WorldSpaceTooltipCanvas] Canvas configured for world space (Sorting Order: {sortingOrder})");
    }
    
    /// <summary>
    /// Connect this Canvas to the tooltip system
    /// </summary>
    private void ConnectToTooltipSystem()
    {
        if (tooltipSystem == null)
        {
            tooltipSystem = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        }
        
        if (tooltipSystem != null)
        {
            // Set the Canvas as the parent for tooltips
            tooltipSystem.transform.SetParent(tooltipCanvas.transform);
            Debug.Log("[WorldSpaceTooltipCanvas] Connected to tooltip system");
        }
        else
        {
            Debug.LogWarning("[WorldSpaceTooltipCanvas] No JsonPassiveTreeTooltip found to connect to");
        }
    }
    
    /// <summary>
    /// Get the tooltip Canvas
    /// </summary>
    public Canvas GetTooltipCanvas()
    {
        return tooltipCanvas;
    }
    
    /// <summary>
    /// Get the world camera
    /// </summary>
    public Camera GetWorldCamera()
    {
        return worldCamera;
    }
    
    /// <summary>
    /// Update Canvas settings
    /// </summary>
    [ContextMenu("Update Canvas Settings")]
    public void UpdateCanvasSettings()
    {
        if (tooltipCanvas != null)
        {
            tooltipCanvas.sortingOrder = sortingOrder;
            ConfigureForWorldSpace();
            Debug.Log("[WorldSpaceTooltipCanvas] Canvas settings updated");
        }
    }
    
    /// <summary>
    /// Set the sorting order
    /// </summary>
    public void SetSortingOrder(int order)
    {
        sortingOrder = order;
        if (tooltipCanvas != null)
        {
            tooltipCanvas.sortingOrder = sortingOrder;
        }
    }
    
    /// <summary>
    /// Enable/disable the Canvas
    /// </summary>
    public void SetCanvasEnabled(bool enabled)
    {
        if (tooltipCanvas != null)
        {
            tooltipCanvas.enabled = enabled;
            Debug.Log($"[WorldSpaceTooltipCanvas] Canvas {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    void OnValidate()
    {
        // Update settings in editor
        if (Application.isPlaying && tooltipCanvas != null)
        {
            UpdateCanvasSettings();
        }
    }
}


























