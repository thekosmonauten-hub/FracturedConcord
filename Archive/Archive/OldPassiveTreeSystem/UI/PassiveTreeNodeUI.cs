using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using PassiveTree;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UI component for individual passive tree nodes
/// Handles visual states, tooltips, and interactions
/// </summary>
public class PassiveTreeNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image nodeBackground;
    [SerializeField] private Image nodeIcon;
    [SerializeField] private TextMeshProUGUI nodeNameText;
    [SerializeField] private TextMeshProUGUI nodeCostText;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("Tooltip Prefab")]
    [Tooltip("Assign a tooltip prefab here. It will be instantiated and positioned when hovering over this node.")]
    [SerializeField] private GameObject tooltipPrefab;
    [Tooltip("Offset for positioning the tooltip relative to the node")]
    [SerializeField] private Vector2 tooltipOffset = new Vector2(0f, 50f);
    [Tooltip("Whether to use the prefab tooltip instead of the built-in tooltip panel")]
    [SerializeField] private bool usePrefabTooltip = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color allocatedColor = Color.green;
    [SerializeField] private Color unavailableColor = Color.red;
    [SerializeField] private Color hoverColor = Color.yellow;
    
    [Header("Custom Sprites")]
    [SerializeField] private bool useCustomSprites = true;
    
    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;
    
    [Header("Tooltip Settings")]
    [SerializeField] private float tooltipDelay = 0.5f;
    [SerializeField] private bool showTooltipOnHover = true;
    
    // Runtime data
    private PassiveNode node;
    private PassiveTreeBoardUI boardUI;
    private NodeVisualState currentState = NodeVisualState.Unavailable;
    private Vector3 originalScale;
    private bool isHovered = false;
    private float hoverTimer = 0f;
    private bool tooltipShown = false;
    
    // Tooltip prefab instance
    private GameObject tooltipInstance;
    private TextMeshProUGUI passiveNameText;
    private TextMeshProUGUI passiveDescriptionText;
    
    private void Awake()
    {
        // Get or create UI components
        if (nodeBackground == null)
        {
            nodeBackground = GetComponent<Image>();
        }
        
        if (nodeNameText == null)
        {
            nodeNameText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Store original scale for animations
        originalScale = transform.localScale;
        
        // CRITICAL: Hide tooltip initially and ensure it stays hidden
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        // Also hide tooltip text directly if it exists
        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
        }
        
        // Initialize prefab tooltip if assigned
        Debug.Log($"[{gameObject.name}] Awake - usePrefabTooltip: {usePrefabTooltip}, tooltipPrefab: {(tooltipPrefab != null ? tooltipPrefab.name : "NULL")}");
        
        if (usePrefabTooltip && tooltipPrefab != null)
        {
            InitializePrefabTooltip();
        }
        else if (usePrefabTooltip && tooltipPrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] usePrefabTooltip is true but tooltipPrefab is null! Please assign a tooltip prefab in the inspector.");
        }
    }
    
    private void Update()
    {
        // Handle tooltip delay
        if (isHovered && showTooltipOnHover && !tooltipShown)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= tooltipDelay)
            {
                ShowTooltip();
            }
        }
    }
    
    /// <summary>
    /// Initialize the node UI with node data
    /// </summary>
    public void Initialize(PassiveNode nodeData, PassiveTreeBoardUI board)
    {
        node = nodeData;
        boardUI = board;
        
        // Set up UI elements
        if (nodeNameText != null)
        {
            nodeNameText.text = node.name;
        }
        
        if (nodeCostText != null)
        {
            nodeCostText.text = node.cost.ToString();
        }
        
        // Set node color based on type
        if (nodeBackground != null)
        {
            nodeBackground.color = GetNodeTypeColor(node.type);
        }
        
        // Set up tooltip
        SetupTooltip();
        
        // Set initial state
        SetState(NodeVisualState.Unavailable);
    }
    
    /// <summary>
    /// Get the current visual state of the node
    /// </summary>
    public NodeVisualState CurrentState => currentState;
    
    /// <summary>
    /// Get whether custom sprites are enabled for this node
    /// </summary>
    public bool GetUseCustomSprites() => useCustomSprites;
    
    [ContextMenu("Test Sprite Rendering")]
    public void TestSpriteRendering()
    {
        if (nodeBackground == null)
        {
            Debug.LogError("[PassiveTreeNodeUI] Node background is null!");
            return;
        }
        
        // Test with a solid color to see if the image component works
        var originalColor = nodeBackground.color;
        nodeBackground.color = Color.red;
        
        // Restore original color after 2 seconds
        StartCoroutine(RestoreColorAfterDelay(originalColor, 2f));
    }
    
    private System.Collections.IEnumerator RestoreColorAfterDelay(Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (nodeBackground != null)
        {
            nodeBackground.color = originalColor;
        }
    }
    
    /// <summary>
    /// Set the visual state of the node
    /// </summary>
    public void SetState(NodeVisualState state)
    {
        currentState = state;
        UpdateVisualState();
    }
    
    /// <summary>
    /// Update the visual appearance based on current state
    /// </summary>
    private void UpdateVisualState()
    {
        if (nodeBackground == null) return;
        
        // Update sprite if using custom sprites
        if (useCustomSprites && node != null && boardUI != null)
        {
            UpdateNodeSprite();
        }
        
        Color targetColor = GetStateColor(currentState);
        
        // Apply color with hover effect if needed
        if (isHovered)
        {
            targetColor = Color.Lerp(targetColor, hoverColor, 0.3f);
        }
        
        nodeBackground.color = targetColor;
    }
    
    /// <summary>
    /// Update the node sprite based on node type and theme
    /// </summary>
    private void UpdateNodeSprite()
    {
        if (nodeBackground == null || node == null) return;
        
        // Get sprite from the board's sprite manager
        var spriteManager = boardUI.GetSpriteManager();
        if (spriteManager != null)
        {
            Sprite nodeSprite = node.GetNodeSprite(spriteManager);
            if (nodeSprite != null)
            {
                nodeBackground.sprite = nodeSprite;
            }
            else
            {
                Debug.LogWarning($"[PassiveTreeNodeUI] No sprite found for node '{node.name}' (Type: {node.type})");
            }
        }
        else
        {
            Debug.LogWarning($"[PassiveTreeNodeUI] No sprite manager available for node '{node.name}'");
        }
    }
    
    /// <summary>
    /// Get color for a specific visual state
    /// </summary>
    private Color GetStateColor(NodeVisualState state)
    {
        switch (state)
        {
            case NodeVisualState.Available:
                return availableColor;
            case NodeVisualState.Allocated:
                return allocatedColor;
            case NodeVisualState.Unavailable:
                return unavailableColor;
            default:
                return Color.gray;
        }
    }
    
    /// <summary>
    /// Get color for a specific node type
    /// </summary>
    private Color GetNodeTypeColor(NodeType type)
    {
        switch (type)
        {
            case NodeType.Main:
                return Color.green;
            case NodeType.Notable:
                return Color.blue;
            case NodeType.Keystone:
                return Color.magenta;
            case NodeType.Small:
                return Color.white;
            case NodeType.Travel:
                return Color.cyan;
            case NodeType.Extension:
                return Color.yellow;
            default:
                return Color.gray;
        }
    }
    
    /// <summary>
    /// Set up the tooltip with node information
    /// </summary>
    private void SetupTooltip()
    {
        if (usePrefabTooltip && tooltipPrefab != null)
        {
            SetupPrefabTooltip();
        }
        else if (tooltipText != null)
        {
            SetupBuiltInTooltip();
        }
    }
    
    /// <summary>
    /// Set up the built-in tooltip with node information
    /// </summary>
    private void SetupBuiltInTooltip()
    {
        if (tooltipText == null) return;
        
        string tooltip = $"{node.name}\n";
        tooltip += $"{node.description}\n\n";
        
        if (node.maxRank > 1)
        {
            tooltip += $"Rank: {node.currentRank}/{node.maxRank}\n";
        }
        
        if (node.requirements.Count > 0)
        {
            tooltip += "\nRequirements:\n";
            foreach (var requirement in node.requirements)
            {
                tooltip += $"• {requirement}\n";
            }
        }
        
        tooltipText.text = tooltip;
        
        // CRITICAL: Ensure tooltip is hidden after setting up content
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Initialize the prefab tooltip
    /// </summary>
    private void InitializePrefabTooltip()
    {
        Debug.Log($"[{gameObject.name}] InitializePrefabTooltip called - usePrefabTooltip: {usePrefabTooltip}, tooltipPrefab: {(tooltipPrefab != null ? tooltipPrefab.name : "NULL")}");
        
        if (tooltipPrefab == null) 
        {
            Debug.LogWarning($"[{gameObject.name}] Tooltip prefab is null!");
            return;
        }
        
        // Create tooltip instance as child of the node initially
        tooltipInstance = Instantiate(tooltipPrefab, transform);
        tooltipInstance.name = "NodeTooltip";
        
        Debug.Log($"[{gameObject.name}] Created tooltip instance: {tooltipInstance.name}");
        
        // Move tooltip to top-level Canvas to avoid hierarchy rendering issues
        MoveTooltipToTopLevel();
        
        // Ensure tooltip appears on top by setting proper Canvas settings
        EnsureTooltipOnTop();
        
        // Find the PassiveName and PassiveDescription TextMeshProUGUI components in the prefab
        TextMeshProUGUI[] allTextComponents = tooltipInstance.GetComponentsInChildren<TextMeshProUGUI>();
        
        Debug.Log($"[{gameObject.name}] Found {allTextComponents.Length} TextMeshProUGUI components in tooltip prefab:");
        foreach (TextMeshProUGUI textComponent in allTextComponents)
        {
            Debug.Log($"  - {textComponent.name} (GameObject: {textComponent.gameObject.name})");
        }
        
        foreach (TextMeshProUGUI textComponent in allTextComponents)
        {
            if (textComponent.name.Contains("PassiveName") || textComponent.name.Contains("Name"))
            {
                passiveNameText = textComponent;
                Debug.Log($"[{gameObject.name}] Found PassiveName component: {textComponent.name}");
            }
            else if (textComponent.name.Contains("PassiveDescription") || textComponent.name.Contains("Description"))
            {
                passiveDescriptionText = textComponent;
                Debug.Log($"[{gameObject.name}] Found PassiveDescription component: {textComponent.name}");
            }
        }
        
        // Fallback: if we can't find the specific components, use the first two TextMeshProUGUI components
        if (passiveNameText == null && allTextComponents.Length > 0)
        {
            passiveNameText = allTextComponents[0];
            Debug.LogWarning($"[{gameObject.name}] Using fallback for PassiveName: {passiveNameText.name}");
        }
        
        if (passiveDescriptionText == null && allTextComponents.Length > 1)
        {
            passiveDescriptionText = allTextComponents[1];
            Debug.LogWarning($"[{gameObject.name}] Using fallback for PassiveDescription: {passiveDescriptionText.name}");
        }
        
        if (passiveNameText == null)
        {
            Debug.LogError($"[{gameObject.name}] Tooltip prefab does not contain any TextMeshProUGUI components for PassiveName!");
        }
        
        if (passiveDescriptionText == null)
        {
            Debug.LogError($"[{gameObject.name}] Tooltip prefab does not contain any TextMeshProUGUI components for PassiveDescription!");
        }
        
        // Set up the tooltip content
        SetupPrefabTooltip();
        
        // Hide the tooltip initially
        tooltipInstance.SetActive(false);
        
        Debug.Log($"[{gameObject.name}] Tooltip initialization complete");
    }
    
    /// <summary>
    /// Set up the prefab tooltip with node information
    /// </summary>
    private void SetupPrefabTooltip()
    {
        if (passiveNameText == null && passiveDescriptionText == null) 
        {
            Debug.LogWarning($"[{gameObject.name}] No tooltip text components found!");
            return;
        }
        
        if (node == null)
        {
            if (passiveNameText != null) passiveNameText.text = "No node data";
            if (passiveDescriptionText != null) passiveDescriptionText.text = "No node data";
            return;
        }
        
        // Set the passive name (node name)
        if (passiveNameText != null)
        {
            string nodeName = !string.IsNullOrEmpty(node.name) ? node.name : node.id;
            passiveNameText.text = nodeName;
            Debug.Log($"[{gameObject.name}] Set PassiveName to: {nodeName}");
        }
        
        // Set the passive description (node effects)
        if (passiveDescriptionText != null)
        {
            string nodeDescription = !string.IsNullOrEmpty(node.description) ? node.description : "No effects available";
            passiveDescriptionText.text = nodeDescription;
            Debug.Log($"[{gameObject.name}] Set PassiveDescription to: {nodeDescription}");
        }
    }
    
    // Pointer event handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        if (boardUI != null && node != null)
        {
            boardUI.OnNodeClicked(node);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] OnPointerEnter - usePrefabTooltip: {usePrefabTooltip}, tooltipInstance: {(tooltipInstance != null ? tooltipInstance.name : "NULL")}");
        
        isHovered = true;
        hoverTimer = 0f;
        tooltipShown = false;
        UpdateVisualState();
        
        // Animate scale using Unity's built-in animation system
        StartCoroutine(AnimateScale(originalScale * hoverScale, animationDuration));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        hoverTimer = 0f;
        tooltipShown = false;
        UpdateVisualState();
        
        // Hide tooltip immediately
        HideTooltip();
        
        // Animate scale back using Unity's built-in animation system
        StartCoroutine(AnimateScale(originalScale, animationDuration));
    }
    
    /// <summary>
    /// Public access to the node data
    /// </summary>
    public PassiveNode Node => node;
    
    /// <summary>
    /// Check if node is allocated
    /// </summary>
    public bool IsAllocated => currentState == NodeVisualState.Allocated;
    
    /// <summary>
    /// Check if node is available for allocation
    /// </summary>
    public bool IsAvailable => currentState == NodeVisualState.Available;
    
    /// <summary>
    /// Show the tooltip
    /// </summary>
    private void ShowTooltip()
    {
        Debug.Log($"[{gameObject.name}] ShowTooltip called - showTooltipOnHover: {showTooltipOnHover}, usePrefabTooltip: {usePrefabTooltip}, tooltipInstance: {(tooltipInstance != null ? tooltipInstance.name : "NULL")}");
        
        if (showTooltipOnHover)
        {
            if (usePrefabTooltip && tooltipInstance != null)
            {
                // Ensure tooltip is on top layer before showing
                EnsureTooltipOnTop();
                
                // Show prefab tooltip
                tooltipInstance.SetActive(true);
                Debug.Log($"[{gameObject.name}] Activated prefab tooltip");
                
                // Position the tooltip
                PositionPrefabTooltip();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Cannot show prefab tooltip - usePrefabTooltip: {usePrefabTooltip}, tooltipInstance: {(tooltipInstance != null ? "EXISTS" : "NULL")}");
                
                // Show built-in tooltip
                if (tooltipPanel != null)
                {
                    tooltipPanel.SetActive(true);
                }
                if (tooltipText != null)
                {
                    tooltipText.gameObject.SetActive(true);
                }
            }
            tooltipShown = true;
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Tooltip not shown - showTooltipOnHover is false");
        }
    }
    
    /// <summary>
    /// Move tooltip to a dedicated tooltip Canvas that renders last
    /// </summary>
    private void MoveTooltipToTopLevel()
    {
        if (tooltipInstance == null) return;
        
        // Find or create a dedicated tooltip Canvas
        Canvas tooltipCanvas = GameObject.Find("TooltipCanvas")?.GetComponent<Canvas>();
        if (tooltipCanvas == null)
        {
            // Create a new GameObject for the tooltip Canvas
            GameObject tooltipCanvasGO = new GameObject("TooltipCanvas");
            tooltipCanvas = tooltipCanvasGO.AddComponent<Canvas>();
            tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tooltipCanvas.sortingOrder = 9999; // Maximum sorting order
            tooltipCanvas.overrideSorting = true;
            
            // Add GraphicRaycaster for interaction
            tooltipCanvasGO.AddComponent<GraphicRaycaster>();
            
            // Make it a child of the scene root
            tooltipCanvasGO.transform.SetParent(null);
            
            Debug.Log($"[{gameObject.name}] Created dedicated TooltipCanvas with sorting order: {tooltipCanvas.sortingOrder}");
        }
        
        // Move tooltip to the dedicated Canvas
        tooltipInstance.transform.SetParent(tooltipCanvas.transform, false);
        Debug.Log($"[{gameObject.name}] Moved tooltip to dedicated TooltipCanvas: {tooltipCanvas.name}");
    }
    
    /// <summary>
    /// Ensure tooltip appears on top of other UI elements
    /// </summary>
    private void EnsureTooltipOnTop()
    {
        if (tooltipInstance == null) return;
        
        // Ensure the parent tooltip Canvas has maximum sorting order
        Canvas parentCanvas = tooltipInstance.GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.name == "TooltipCanvas")
        {
            parentCanvas.sortingOrder = 9999;
            parentCanvas.overrideSorting = true;
            Debug.Log($"[{gameObject.name}] Updated parent TooltipCanvas sorting order to: {parentCanvas.sortingOrder}");
        }
        
        // Get or add Canvas component to ensure proper rendering
        var canvas = tooltipInstance.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = tooltipInstance.AddComponent<Canvas>();
        }
        
        // Set Canvas to screen space overlay mode and ensure it renders on top
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Maximum sorting order to appear above everything
        canvas.overrideSorting = true; // Ensure sorting order is respected
        
        // Get or add GraphicRaycaster for proper interaction
        var raycaster = tooltipInstance.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = tooltipInstance.AddComponent<GraphicRaycaster>();
        }
        
        // Ensure the tooltip has proper RectTransform settings
        var tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            // Set anchor to center for proper positioning
            tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tooltipRect.pivot = new Vector2(0.5f, 0.5f);
        }
        
        // Note: Child UI elements will inherit the Canvas sorting order automatically
        
        Debug.Log($"[{gameObject.name}] Tooltip Canvas configured - Sorting Order: {canvas.sortingOrder}, Render Mode: {canvas.renderMode}, Override Sorting: {canvas.overrideSorting}");
    }
    
    /// <summary>
    /// Position the prefab tooltip relative to the node
    /// </summary>
    private void PositionPrefabTooltip()
    {
        if (tooltipInstance == null) return;
        
        var tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        var nodeRect = GetComponent<RectTransform>();
        
        if (tooltipRect != null && nodeRect != null)
        {
            // Try to get the Canvas for coordinate conversion
            Canvas canvas = FindFirstObjectByType<Canvas>();
            
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For ScreenSpaceOverlay, use screen coordinates
                Vector3 nodeScreenPos = Camera.main != null ? 
                    Camera.main.WorldToScreenPoint(nodeRect.position) : 
                    nodeRect.position;
                
                // Position tooltip relative to screen position with offset
                Vector2 screenPos = new Vector2(nodeScreenPos.x, nodeScreenPos.y) + tooltipOffset;
                tooltipRect.position = screenPos;
                
                Debug.Log($"[{gameObject.name}] Positioned tooltip at screen position: {screenPos}");
            }
            else
            {
                // Fallback: use local positioning with offset
                tooltipRect.anchoredPosition = tooltipOffset;
                Debug.Log($"[{gameObject.name}] Used fallback positioning with offset: {tooltipOffset}");
            }
        }
    }
    
    /// <summary>
    /// Hide the tooltip
    /// </summary>
    private void HideTooltip()
    {
        if (usePrefabTooltip && tooltipInstance != null)
        {
            // Hide prefab tooltip
            tooltipInstance.SetActive(false);
        }
        else
        {
            // Hide built-in tooltip
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
            if (tooltipText != null)
            {
                tooltipText.gameObject.SetActive(false);
            }
        }
        tooltipShown = false;
    }
    
    /// <summary>
    /// Animate scale using Unity's built-in coroutine system
    /// </summary>
    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Use smooth step for easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    /// <summary>
    /// Debug tooltip visibility
    /// </summary>
    [ContextMenu("Debug Tooltip Visibility")]
    public void DebugTooltipVisibility()
    {
        Debug.Log($"[{gameObject.name}] Tooltip Debug - Use Prefab: {usePrefabTooltip}, Shown: {tooltipShown}, Hovered: {isHovered}");
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
    /// Initialize prefab tooltip (for manual setup)
    /// </summary>
    [ContextMenu("Initialize Prefab Tooltip")]
    public void InitializePrefabTooltipManual()
    {
        if (tooltipPrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No tooltip prefab assigned!");
            return;
        }
        
        usePrefabTooltip = true;
        InitializePrefabTooltip();
    }
    
    /// <summary>
    /// Test show prefab tooltip
    /// </summary>
    [ContextMenu("Test Show Prefab Tooltip")]
    public void TestShowPrefabTooltip()
    {
        if (tooltipInstance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No tooltip instance found. Initialize first!");
            return;
        }
        
        // Ensure tooltip is on top
        EnsureTooltipOnTop();
        
        // Show and position tooltip
        tooltipInstance.SetActive(true);
        PositionPrefabTooltip();
        tooltipShown = true;
        
        Debug.Log($"[{gameObject.name}] Tooltip shown and positioned");
    }
    
    /// <summary>
    /// Test tooltip layering (Context Menu)
    /// </summary>
    [ContextMenu("Test Tooltip Layering")]
    public void TestTooltipLayering()
    {
        if (tooltipInstance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No tooltip instance found. Initialize first!");
            return;
        }
        
        Debug.Log($"[{gameObject.name}] === Tooltip Layering Test ===");
        
        // Check Canvas settings
        var canvas = tooltipInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas Render Mode: {canvas.renderMode}");
            Debug.Log($"Canvas Sorting Order: {canvas.sortingOrder}");
            Debug.Log($"Canvas Override Sorting: {canvas.overrideSorting}");
        }
        else
        {
            Debug.LogWarning("No Canvas component found on tooltip!");
        }
        
        // Check RectTransform settings
        var tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            Debug.Log($"Tooltip Anchors: Min({tooltipRect.anchorMin}), Max({tooltipRect.anchorMax})");
            Debug.Log($"Tooltip Pivot: {tooltipRect.pivot}");
            Debug.Log($"Tooltip Position: {tooltipRect.anchoredPosition}");
        }
        
        // Force ensure tooltip is on top
        EnsureTooltipOnTop();
        
        Debug.Log($"[{gameObject.name}] === End Tooltip Layering Test ===");
    }
    
    /// <summary>
    /// Test hide prefab tooltip
    /// </summary>
    [ContextMenu("Test Hide Prefab Tooltip")]
    public void TestHidePrefabTooltip()
    {
        if (usePrefabTooltip && tooltipInstance != null)
        {
            tooltipInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Cannot test prefab tooltip - not initialized or not using prefab tooltip");
        }
    }
    
    /// <summary>
    /// Test node allocation (Context Menu)
    /// </summary>
    [ContextMenu("Test Node Allocation")]
    public void TestNodeAllocation()
    {
        Debug.Log($"[{gameObject.name}] === Node Allocation Test ===");
        Debug.Log($"Node ID: {node?.id}");
        Debug.Log($"Node Position: {node?.position}");
        Debug.Log($"Node Type: {node?.type}");
        Debug.Log($"Current Rank: {node?.currentRank}/{node?.maxRank}");
        Debug.Log($"Cost: {node?.cost}");
        
        if (boardUI != null)
        {
            Debug.Log("BoardUI reference exists");
        }
        else
        {
            Debug.LogWarning("BoardUI reference is null!");
        }
        
        Debug.Log($"[{gameObject.name}] === End Node Allocation Test ===");
    }
    
    /// <summary>
    /// Test new tooltip setup (Context Menu)
    /// </summary>
    [ContextMenu("Test New Tooltip Setup")]
    public void TestNewTooltipSetup()
    {
        Debug.Log($"[{gameObject.name}] === New Tooltip Setup Test ===");
        Debug.Log($"usePrefabTooltip: {usePrefabTooltip}");
        Debug.Log($"tooltipPrefab: {(tooltipPrefab != null ? tooltipPrefab.name : "NULL")}");
        Debug.Log($"tooltipInstance: {(tooltipInstance != null ? tooltipInstance.name : "NULL")}");
        Debug.Log($"passiveNameText: {(passiveNameText != null ? passiveNameText.name : "NULL")}");
        Debug.Log($"passiveDescriptionText: {(passiveDescriptionText != null ? passiveDescriptionText.name : "NULL")}");
        
        if (node != null)
        {
            Debug.Log($"Node Name: {node.name}");
            Debug.Log($"Node Description: {node.description}");
        }
        
        Debug.Log($"[{gameObject.name}] === End New Tooltip Setup Test ===");
    }
    
    /// <summary>
    /// Manually initialize tooltip (Context Menu)
    /// </summary>
    [ContextMenu("Manual Initialize Tooltip")]
    public void ManualInitializeTooltip()
    {
        Debug.Log($"[{gameObject.name}] === Manual Tooltip Initialization ===");
        
        if (tooltipInstance != null)
        {
            Debug.Log("Tooltip instance already exists, destroying it first...");
            DestroyImmediate(tooltipInstance);
            tooltipInstance = null;
        }
        
        if (usePrefabTooltip && tooltipPrefab != null)
        {
            Debug.Log("Initializing tooltip...");
            InitializePrefabTooltip();
            Debug.Log($"Tooltip initialization complete. Instance: {(tooltipInstance != null ? tooltipInstance.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning("Cannot initialize tooltip - usePrefabTooltip or tooltipPrefab is null!");
        }
        
        Debug.Log($"[{gameObject.name}] === End Manual Tooltip Initialization ===");
    }
    
    /// <summary>
    /// Force tooltip to top layer by moving to Canvas root (Context Menu)
    /// </summary>
    [ContextMenu("Force Tooltip to Top Layer")]
    public void ForceTooltipToTopLayer()
    {
        if (tooltipInstance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No tooltip instance to move!");
            return;
        }
        
        // Use the new method to move tooltip to top level
        MoveTooltipToTopLevel();
        
        // Re-apply top layer settings
        EnsureTooltipOnTop();
        
        // Reposition the tooltip
        PositionPrefabTooltip();
    }
    
    /// <summary>
    /// Test tooltip prefab structure (Context Menu)
    /// </summary>
    [ContextMenu("Test Tooltip Prefab Structure")]
    public void TestTooltipPrefabStructure()
    {
        Debug.Log($"[{gameObject.name}] === Tooltip Prefab Structure Test ===");
        
        if (tooltipPrefab == null)
        {
            Debug.LogError("Tooltip prefab is null!");
            return;
        }
        
        Debug.Log($"Tooltip prefab name: {tooltipPrefab.name}");
        
        // Check if the prefab has a Canvas component
        Canvas prefabCanvas = tooltipPrefab.GetComponent<Canvas>();
        if (prefabCanvas != null)
        {
            Debug.Log($"Prefab has Canvas - Render Mode: {prefabCanvas.renderMode}, Sorting Order: {prefabCanvas.sortingOrder}");
        }
        else
        {
            Debug.LogWarning("Prefab does not have a Canvas component!");
        }
        
        // Check all TextMeshProUGUI components in the prefab
        TextMeshProUGUI[] prefabTextComponents = tooltipPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log($"Prefab has {prefabTextComponents.Length} TextMeshProUGUI components:");
        
        foreach (TextMeshProUGUI textComponent in prefabTextComponents)
        {
            Debug.Log($"  - {textComponent.name} (GameObject: {textComponent.gameObject.name})");
            Debug.Log($"    Text: '{textComponent.text}'");
        }
        
        // Check for Image components (background)
        Image[] prefabImages = tooltipPrefab.GetComponentsInChildren<Image>();
        Debug.Log($"Prefab has {prefabImages.Length} Image components:");
        
        foreach (Image image in prefabImages)
        {
            Debug.Log($"  - {image.name} (GameObject: {image.gameObject.name})");
        }
        
        Debug.Log($"[{gameObject.name}] === End Tooltip Prefab Structure Test ===");
    }
    
    /// <summary>
    /// Check tooltip prefab assignment (Context Menu)
    /// </summary>
    [ContextMenu("Check Tooltip Prefab Assignment")]
    public void CheckTooltipPrefabAssignment()
    {
        Debug.Log($"[{gameObject.name}] === Tooltip Prefab Assignment Check ===");
        
        // Check this instance
        Debug.Log($"This instance - usePrefabTooltip: {usePrefabTooltip}");
        Debug.Log($"This instance - tooltipPrefab: {(tooltipPrefab != null ? tooltipPrefab.name : "NULL")}");
        
        // Check the original NodePrefab
        GameObject originalPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
        if (originalPrefab != null)
        {
            PassiveTreeNodeUI originalUI = originalPrefab.GetComponent<PassiveTreeNodeUI>();
            if (originalUI != null)
            {
                Debug.Log($"Original Prefab - usePrefabTooltip: {originalUI.usePrefabTooltip}");
                Debug.Log($"Original Prefab - tooltipPrefab: {(originalUI.tooltipPrefab != null ? originalUI.tooltipPrefab.name : "NULL")}");
            }
        }
        
        // Check all NodePrefab instances in scene
        PassiveTreeNodeUI[] allNodeUIs = FindObjectsByType<PassiveTreeNodeUI>(FindObjectsSortMode.None);
        Debug.Log($"Total NodePrefab instances in scene: {allNodeUIs.Length}");
        
        int assignedCount = 0;
        foreach (PassiveTreeNodeUI nodeUI in allNodeUIs)
        {
            if (nodeUI.usePrefabTooltip && nodeUI.tooltipPrefab != null)
            {
                assignedCount++;
            }
        }
        Debug.Log($"NodePrefab instances with tooltip prefab assigned: {assignedCount}/{allNodeUIs.Length}");
        
        Debug.Log($"[{gameObject.name}] === End Tooltip Prefab Assignment Check ===");
    }
    
    /// <summary>
    /// Force show tooltip for testing (Context Menu)
    /// </summary>
    [ContextMenu("Force Show Tooltip")]
    public void ForceShowTooltip()
    {
        Debug.Log($"[{gameObject.name}] === Force Show Tooltip Test ===");
        
        if (tooltipInstance == null)
        {
            Debug.LogWarning("No tooltip instance found! Trying to initialize...");
            if (usePrefabTooltip && tooltipPrefab != null)
            {
                InitializePrefabTooltip();
            }
            else
            {
                Debug.LogError("Cannot initialize tooltip - missing prefab or settings!");
                return;
            }
        }
        
        if (tooltipInstance != null)
        {
            // Set up tooltip content
            SetupPrefabTooltip();
            
            // Move to top level
            MoveTooltipToTopLevel();
            
            // Ensure it's on top
            EnsureTooltipOnTop();
            
            // Show the tooltip
            tooltipInstance.SetActive(true);
            
            // Position it
            PositionPrefabTooltip();
            
            Debug.Log("Tooltip should now be visible!");
        }
        else
        {
            Debug.LogError("Failed to create tooltip instance!");
        }
        
        Debug.Log($"[{gameObject.name}] === End Force Show Tooltip Test ===");
    }
    
    /// <summary>
    /// Debug tooltip Canvas hierarchy (Context Menu)
    /// </summary>
    [ContextMenu("Debug Tooltip Canvas Hierarchy")]
    public void DebugTooltipCanvasHierarchy()
    {
        Debug.Log($"[{gameObject.name}] === Tooltip Canvas Hierarchy Debug ===");
        
        if (tooltipInstance == null)
        {
            Debug.LogWarning("No tooltip instance found!");
            return;
        }
        
        // Check tooltip instance hierarchy
        Transform current = tooltipInstance.transform;
        int depth = 0;
        while (current != null)
        {
            Canvas canvas = current.GetComponent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"Level {depth}: {current.name} - Canvas Sorting Order: {canvas.sortingOrder}, Override: {canvas.overrideSorting}");
            }
            else
            {
                Debug.Log($"Level {depth}: {current.name} - No Canvas");
            }
            current = current.parent;
            depth++;
        }
        
        // Check all Canvases in scene
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Total Canvases in scene: {allCanvases.Length}");
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"Canvas: {canvas.name} - Sorting Order: {canvas.sortingOrder}, Override: {canvas.overrideSorting}");
        }
        
        Debug.Log($"[{gameObject.name}] === End Tooltip Canvas Hierarchy Debug ===");
    }
}
