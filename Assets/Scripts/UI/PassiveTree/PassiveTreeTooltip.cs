using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using PassiveTree;
using System.Collections;

/// <summary>
/// Tooltip system for displaying cell information on hover
/// Works with BoardTooltip.prefab to show cell data when hovering over cells
/// </summary>
public class PassiveTreeTooltip : MonoBehaviour
{
    [Header("Tooltip Settings")]
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private Canvas tooltipCanvas;
    [SerializeField] private bool followMouse = true;
    [SerializeField] private Vector2 mouseOffset = new Vector2(0, 50); // Position above mouse by default
    [SerializeField] private float showDelay = 0.3f;
    [SerializeField] private float hideDelay = 0.1f;
    [SerializeField] private bool debugMode = false;

    [Header("Animation")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Positioning")]
    [SerializeField] private bool keepOnScreen = true;
    [SerializeField] private Vector2 screenMargin = new Vector2(20, 20);

    // Runtime components
    private GameObject currentTooltip;
    private TextMeshProUGUI cellNameText;
    private TextMeshProUGUI cellDescriptionText;
    private CanvasGroup tooltipCanvasGroup;
    private RectTransform tooltipRectTransform;

    // State
    private CellController currentHoveredCell;
    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;
    private bool isTooltipVisible = false;

    // References
    private PassiveTreeManager treeManager;
    private BoardDataManager dataManager;
    private Camera mainCamera;

    void Start()
    {
        SetupTooltipSystem();
        
        // Debug Input System status
        if (debugMode)
        {
            Debug.Log($"[PassiveTreeTooltip] Input System Status:");
            Debug.Log($"  - Mouse.current: {(Mouse.current != null ? "Available" : "NULL")}");
            Debug.Log($"  - Input System enabled: {InputSystem.settings != null}");
        }
    }

    void Update()
    {
        if (isTooltipVisible && followMouse && currentTooltip != null)
        {
            UpdateTooltipPosition();
        }
    }

    /// <summary>
    /// Set up the tooltip system
    /// </summary>
    private void SetupTooltipSystem()
    {
        // Find required components
        treeManager = FindFirstObjectByType<PassiveTreeManager>();
        dataManager = FindFirstObjectByType<BoardDataManager>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        // Set up canvas if not assigned
        if (tooltipCanvas == null)
        {
            tooltipCanvas = FindFirstObjectByType<Canvas>();
            if (tooltipCanvas == null)
            {
                Debug.LogWarning("[PassiveTreeTooltip] No Canvas found. Creating one...");
                CreateTooltipCanvas();
            }
        }

        // Create tooltip from prefab if assigned
        if (tooltipPrefab != null)
        {
            CreateTooltipFromPrefab();
        }
        else
        {
            Debug.LogWarning("[PassiveTreeTooltip] No tooltip prefab assigned!");
        }

        Debug.Log("[PassiveTreeTooltip] Tooltip system setup complete");
    }

    /// <summary>
    /// Create a tooltip canvas
    /// </summary>
    private void CreateTooltipCanvas()
    {
        GameObject canvasGO = new GameObject("TooltipCanvas");
        tooltipCanvas = canvasGO.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.sortingOrder = 1000; // High sorting order to appear on top

        // Add CanvasScaler
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Add GraphicRaycaster
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    /// <summary>
    /// Create tooltip from prefab
    /// </summary>
    private void CreateTooltipFromPrefab()
    {
        if (tooltipPrefab == null) return;

        // Instantiate the tooltip prefab
        currentTooltip = Instantiate(tooltipPrefab, tooltipCanvas.transform);
        currentTooltip.name = "BoardTooltip";

        // Find TextMeshPro components
        TextMeshProUGUI[] textComponents = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI text in textComponents)
        {
            if (text.name.ToLower().Contains("name") || text.name.ToLower().Contains("cell_name"))
            {
                cellNameText = text;
                if (debugMode) Debug.Log($"[PassiveTreeTooltip] Found cell name text: {text.name}");
            }
            else if (text.name.ToLower().Contains("description") || text.name.ToLower().Contains("desc"))
            {
                cellDescriptionText = text;
                if (debugMode) Debug.Log($"[PassiveTreeTooltip] Found description text: {text.name}");
            }
        }

        // Get RectTransform and CanvasGroup
        tooltipRectTransform = currentTooltip.GetComponent<RectTransform>();
        tooltipCanvasGroup = currentTooltip.GetComponent<CanvasGroup>();

        // Create CanvasGroup if it doesn't exist
        if (tooltipCanvasGroup == null)
        {
            tooltipCanvasGroup = currentTooltip.AddComponent<CanvasGroup>();
        }

        // Initially hide the tooltip
        currentTooltip.SetActive(false);

        Debug.Log("[PassiveTreeTooltip] Created tooltip from prefab");
    }

    /// <summary>
    /// Show tooltip for a cell
    /// </summary>
    public void ShowTooltip(CellController cell)
    {
        if (cell == null || currentTooltip == null) return;

        // Stop any existing coroutines
        if (showCoroutine != null) StopCoroutine(showCoroutine);
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        currentHoveredCell = cell;

        // Start show coroutine
        showCoroutine = StartCoroutine(ShowTooltipCoroutine(cell));
    }

    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (currentTooltip == null) return;

        // Stop any existing coroutines
        if (showCoroutine != null) StopCoroutine(showCoroutine);
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        // Start hide coroutine
        hideCoroutine = StartCoroutine(HideTooltipCoroutine());
    }

    /// <summary>
    /// Show tooltip coroutine with delay
    /// </summary>
    private IEnumerator ShowTooltipCoroutine(CellController cell)
    {
        // Wait for show delay
        yield return new WaitForSeconds(showDelay);

        // Check if we're still hovering the same cell
        if (currentHoveredCell != cell) yield break;

        // Update tooltip content
        UpdateTooltipContent(cell);

        // Show the tooltip
        currentTooltip.SetActive(true);
        isTooltipVisible = true;

        // Position the tooltip
        UpdateTooltipPosition();

        // Animate in
        if (useAnimations)
        {
            yield return StartCoroutine(AnimateTooltip(true));
        }

        if (debugMode)
        {
            Debug.Log($"[PassiveTreeTooltip] Showing tooltip for cell {cell.GridPosition}");
        }
    }

    /// <summary>
    /// Hide tooltip coroutine with delay
    /// </summary>
    private IEnumerator HideTooltipCoroutine()
    {
        // Wait for hide delay
        yield return new WaitForSeconds(hideDelay);

        // Animate out
        if (useAnimations && isTooltipVisible)
        {
            yield return StartCoroutine(AnimateTooltip(false));
        }

        // Hide the tooltip
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(false);
        }

        isTooltipVisible = false;
        currentHoveredCell = null;

        if (debugMode)
        {
            Debug.Log("[PassiveTreeTooltip] Hiding tooltip");
        }
    }

    /// <summary>
    /// Update tooltip content with cell data
    /// </summary>
    private void UpdateTooltipContent(CellController cell)
    {
        if (cell == null) return;

        // Get node data if available
        PassiveNodeData nodeData = null;
        if (dataManager != null)
        {
            nodeData = dataManager.GetNodeData(cell.GridPosition);
        }

        // Update cell name
        if (cellNameText != null)
        {
            string cellName = nodeData != null ? nodeData.NodeName : $"Cell ({cell.GridPosition.x},{cell.GridPosition.y})";
            cellNameText.text = cellName;
        }

        // Update description
        if (cellDescriptionText != null)
        {
            string description = nodeData != null ? nodeData.Description : cell.NodeDescription;
            
            // Add additional info if no description
            if (string.IsNullOrEmpty(description))
            {
                description = GetDefaultDescription(cell.NodeType);
            }

            // Add status and cost info
            string status = GetCellStatus(cell);
            int cost = nodeData != null ? nodeData.SkillPointsCost : GetDefaultCost(cell.NodeType);
            
            description += $"\n\n<size=80%><color=#FFD700>Type:</color> {cell.NodeType}\n";
            description += $"<color=#00FF00>Cost:</color> {cost} skill points\n";
            description += $"<color=#87CEEB>Status:</color> {status}</size>";

            cellDescriptionText.text = description;
        }
    }

    /// <summary>
    /// Update tooltip position with smart positioning to avoid flickering
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (currentTooltip == null || tooltipRectTransform == null) return;

        Vector2 position;
        Vector2 tooltipSize = tooltipRectTransform.sizeDelta;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (followMouse)
        {
            // Follow mouse position using new Input System
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                // Smart positioning: try to place tooltip above mouse first
                Vector2 preferredOffset = new Vector2(0, tooltipSize.y + 20); // Above mouse with gap
                position = mousePos + preferredOffset;
                
                // If tooltip would go off screen at top, place it below mouse
                if (position.y + tooltipSize.y > screenSize.y - screenMargin.y)
                {
                    position = mousePos + new Vector2(0, -tooltipSize.y - 20); // Below mouse with gap
                }
                
                // If still off screen at bottom, place it to the right of mouse
                if (position.y < screenMargin.y)
                {
                    position = mousePos + new Vector2(tooltipSize.x + 20, 0); // Right of mouse with gap
                }
                
                // If still off screen at right, place it to the left of mouse
                if (position.x + tooltipSize.x > screenSize.x - screenMargin.x)
                {
                    position = mousePos + new Vector2(-tooltipSize.x - 20, 0); // Left of mouse with gap
                }
            }
            else
            {
                // Fallback to screen center if mouse is not available
                position = new Vector2(Screen.width / 2f, Screen.height / 2f) + mouseOffset;
                Debug.LogWarning("[PassiveTreeTooltip] Mouse.current is null, using screen center as fallback");
            }
        }
        else if (currentHoveredCell != null)
        {
            // Position near the cell
            Vector3 cellWorldPos = currentHoveredCell.transform.position;
            Vector3 cellScreenPos = mainCamera.WorldToScreenPoint(cellWorldPos);
            position = new Vector2(cellScreenPos.x, cellScreenPos.y) + mouseOffset;
        }
        else
        {
            return;
        }

        // Final screen boundary check
        if (keepOnScreen)
        {
            // Adjust X position
            if (position.x + tooltipSize.x > screenSize.x - screenMargin.x)
            {
                position.x = screenSize.x - tooltipSize.x - screenMargin.x;
            }
            if (position.x < screenMargin.x)
            {
                position.x = screenMargin.x;
            }

            // Adjust Y position
            if (position.y + tooltipSize.y > screenSize.y - screenMargin.y)
            {
                position.y = screenSize.y - tooltipSize.y - screenMargin.y;
            }
            if (position.y < screenMargin.y)
            {
                position.y = screenMargin.y;
            }
        }

        tooltipRectTransform.position = position;
    }

    /// <summary>
    /// Animate tooltip fade in/out
    /// </summary>
    private IEnumerator AnimateTooltip(bool fadeIn)
    {
        if (tooltipCanvasGroup == null) yield break;

        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = curve.Evaluate(t);
            tooltipCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            yield return null;
        }

        tooltipCanvasGroup.alpha = endAlpha;
    }

    /// <summary>
    /// Get cell status string
    /// </summary>
    private string GetCellStatus(CellController cell)
    {
        if (cell.IsPurchased)
            return "Purchased";
        else if (cell.IsUnlocked)
            return "Available";
        else if (cell.IsAvailable)
            return "Locked";
        else
            return "Unavailable";
    }

    /// <summary>
    /// Get default description for node type
    /// </summary>
    private string GetDefaultDescription(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return "Your starting point in the passive tree";
            case NodeType.Travel:
                return "A basic travel node";
            case NodeType.Extension:
                return "An extension point for connecting other boards";
            case NodeType.Notable:
                return "A notable passive with significant effects";
            case NodeType.Small:
                return "A small passive node";
            case NodeType.Keystone:
                return "A powerful keystone passive";
            default:
                return "A passive tree node";
        }
    }

    /// <summary>
    /// Get default cost for node type
    /// </summary>
    private int GetDefaultCost(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return 0;
            case NodeType.Travel:
                return 1;
            case NodeType.Extension:
                return 0;
            case NodeType.Notable:
                return 2;
            case NodeType.Small:
                return 1;
            case NodeType.Keystone:
                return 1;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Set the tooltip prefab
    /// </summary>
    public void SetTooltipPrefab(GameObject prefab)
    {
        tooltipPrefab = prefab;
        if (currentTooltip != null)
        {
            DestroyImmediate(currentTooltip);
        }
        CreateTooltipFromPrefab();
    }

    /// <summary>
    /// Test the tooltip system
    /// </summary>
    [ContextMenu("Test Tooltip")]
    public void TestTooltip()
    {
        CellController[] cells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        if (cells.Length > 0)
        {
            ShowTooltip(cells[0]);
            Debug.Log("[PassiveTreeTooltip] Testing tooltip with first cell");
        }
    }

    /// <summary>
    /// Force hide tooltip
    /// </summary>
    [ContextMenu("Force Hide Tooltip")]
    public void ForceHideTooltip()
    {
        HideTooltip();
    }

    /// <summary>
    /// Debug Input System status
    /// </summary>
    [ContextMenu("Debug Input System Status")]
    public void DebugInputSystemStatus()
    {
        Debug.Log("=== INPUT SYSTEM DEBUG ===");
        Debug.Log($"Mouse.current: {(Mouse.current != null ? "Available" : "NULL")}");
        Debug.Log($"Input System enabled: {InputSystem.settings != null}");
        
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Debug.Log($"Mouse position: {mousePos}");
        }
        
        Debug.Log("=========================");
    }
}
