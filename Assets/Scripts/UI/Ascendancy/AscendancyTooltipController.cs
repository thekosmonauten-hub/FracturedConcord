using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Controls the Ascendancy tooltip display.
/// Spawns and positions tooltip when hovering over nodes.
/// </summary>
public class AscendancyTooltipController : MonoBehaviour
{
    [Header("Tooltip Prefab")]
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private Transform tooltipContainer;
    
    [Header("Positioning")]
    [SerializeField] private Vector2 tooltipOffset = new Vector2(100f, 0f); // Offset from node
    [SerializeField] private bool followMouse = false;
    [SerializeField] private float followSpeed = 10f;
    
    [Header("Name Formatting")]
    [Tooltip("Remove suffix from node names (e.g., 'NodeName_1' → 'NodeName')")]
    [SerializeField] private bool cleanNodeNames = true;
    [Tooltip("Suffix patterns to remove (e.g., '_1', '_2', etc.)")]
    [SerializeField] private string suffixPattern = "_";
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = false;
    
    private GameObject currentTooltip;
    private RectTransform tooltipRect;
    private Canvas parentCanvas;
    
    void Awake()
    {
        // Find parent canvas for screen space calculations
        parentCanvas = GetComponentInParent<Canvas>();
    }
    
    /// <summary>
    /// Show tooltip for a sub-node (choice node option)
    /// </summary>
    public void ShowSubNodeTooltip(AscendancySubNode subNode, string choiceNodeName, Vector2 nodePosition)
    {
        if (subNode == null || tooltipPrefab == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[AscendancyTooltip] Cannot show sub-node tooltip - missing data or prefab");
            return;
        }
        
        // Hide previous tooltip
        HideTooltip();
        
        // Spawn tooltip
        Transform parent = tooltipContainer != null ? tooltipContainer : transform;
        currentTooltip = Instantiate(tooltipPrefab, parent);
        currentTooltip.name = $"Tooltip_SubNode_{subNode.name}";
        
        tooltipRect = currentTooltip.GetComponent<RectTransform>();
        
        // Populate tooltip with sub-node data
        PopulateSubNodeTooltip(subNode, choiceNodeName);
        
        // Position tooltip
        if (tooltipRect != null)
        {
            Vector2 targetPosition = nodePosition + tooltipOffset;
            tooltipRect.anchoredPosition = targetPosition;
            
            // Clamp to screen bounds
            ClampTooltipToScreen();
        }
        
        currentTooltip.SetActive(true);
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTooltip] Showing sub-node tooltip for: {subNode.name}");
    }
    
    /// <summary>
    /// Show tooltip for a passive node
    /// </summary>
    public void ShowTooltip(AscendancyPassive passive, Vector2 nodePosition)
    {
        if (passive == null || tooltipPrefab == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[AscendancyTooltip] Cannot show tooltip - missing data or prefab");
            return;
        }
        
        // Hide previous tooltip
        HideTooltip();
        
        // Spawn tooltip
        Transform parent = tooltipContainer != null ? tooltipContainer : transform;
        currentTooltip = Instantiate(tooltipPrefab, parent);
        currentTooltip.name = $"Tooltip_{passive.name}";
        
        tooltipRect = currentTooltip.GetComponent<RectTransform>();
        
        // Populate tooltip
        PopulateTooltip(passive);
        
        // Position tooltip
        if (tooltipRect != null)
        {
            Vector2 targetPosition = nodePosition + tooltipOffset;
            tooltipRect.anchoredPosition = targetPosition;
            
            // Clamp to screen bounds (prevent tooltip from going off-screen)
            ClampTooltipToScreen();
        }
        
        currentTooltip.SetActive(true);
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTooltip] Showing tooltip for: {passive.name}");
    }
    
    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
            tooltipRect = null;
            
            if (showDebugLogs)
                Debug.Log("[AscendancyTooltip] Hid tooltip");
        }
    }
    
    /// <summary>
    /// Populate tooltip with sub-node data
    /// </summary>
    void PopulateSubNodeTooltip(AscendancySubNode subNode, string choiceNodeName)
    {
        if (currentTooltip == null) return;
        
        // Find text components by name
        TextMeshProUGUI[] allTexts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        foreach (var text in allTexts)
        {
            string objName = text.gameObject.name;
            
            if (objName == "AscendancyName" || objName == "NodeName" || objName == "Name")
            {
                text.text = subNode.name;
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTooltip] Set sub-node name: {subNode.name}");
            }
            else if (objName == "AscendancyNodeDescription" || objName == "Description" || objName == "DescriptionText")
            {
                // Combine choice node name and sub-node description
                string description = !string.IsNullOrEmpty(subNode.description) 
                    ? subNode.description 
                    : $"Option for {choiceNodeName}";
                text.text = description;
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTooltip] Set sub-node description: {description}");
            }
        }
        
        // Add node type indicator
        TextMeshProUGUI[] allTypeTexts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in allTypeTexts)
        {
            if (text.gameObject.name == "NodeType" || text.gameObject.name == "Type")
            {
                text.text = "SUB-NODE OPTION";
            }
        }
    }
    
    /// <summary>
    /// Populate tooltip with passive data
    /// </summary>
    void PopulateTooltip(AscendancyPassive passive)
    {
        if (currentTooltip == null) return;
        
        // Clean node name if enabled
        string displayName = GetCleanNodeName(passive.name);
        
        // Find text components by name
        TextMeshProUGUI[] allTexts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        foreach (var text in allTexts)
        {
            string objName = text.gameObject.name;
            
            if (objName == "AscendancyName" || objName == "NodeName" || objName == "Name")
            {
                text.text = displayName;
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTooltip] Set name: {displayName} (original: {passive.name})");
            }
            else if (objName == "AscendancyNodeDescription" || objName == "Description" || objName == "DescriptionText")
            {
                text.text = passive.description;
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTooltip] Set description: {passive.description}");
            }
        }
        
        // Add node type indicator
        TextMeshProUGUI[] allTypeTexts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in allTypeTexts)
        {
            if (text.gameObject.name == "NodeType" || text.gameObject.name == "Type")
            {
                text.text = passive.nodeType == AscendancyNodeType.Major ? "MAJOR NODE" : 
                           (passive.nodeType == AscendancyNodeType.Start ? "START" : "MINOR NODE");
            }
        }
    }
    
    /// <summary>
    /// Clean node name by removing suffix
    /// </summary>
    string GetCleanNodeName(string nodeName)
    {
        if (!cleanNodeNames || string.IsNullOrEmpty(nodeName))
            return nodeName;
        
        // Check if name contains suffix pattern
        if (!string.IsNullOrEmpty(suffixPattern) && nodeName.Contains(suffixPattern))
        {
            // Find last occurrence of suffix pattern
            int lastIndex = nodeName.LastIndexOf(suffixPattern);
            
            if (lastIndex > 0)
            {
                // Check if everything after suffix is a number
                string afterSuffix = nodeName.Substring(lastIndex + suffixPattern.Length);
                
                // If it's a number or empty, remove the suffix
                if (string.IsNullOrEmpty(afterSuffix) || int.TryParse(afterSuffix, out _))
                {
                    string cleanName = nodeName.Substring(0, lastIndex);
                    return cleanName;
                }
            }
        }
        
        return nodeName;
    }
    
    /// <summary>
    /// Clamp tooltip to stay within screen bounds
    /// </summary>
    void ClampTooltipToScreen()
    {
        if (tooltipRect == null || parentCanvas == null) return;
        
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return;
        
        Vector2 tooltipPos = tooltipRect.anchoredPosition;
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        // Clamp X
        float minX = -canvasWidth / 2f + tooltipSize.x / 2f;
        float maxX = canvasWidth / 2f - tooltipSize.x / 2f;
        tooltipPos.x = Mathf.Clamp(tooltipPos.x, minX, maxX);
        
        // Clamp Y
        float minY = -canvasHeight / 2f + tooltipSize.y / 2f;
        float maxY = canvasHeight / 2f - tooltipSize.y / 2f;
        tooltipPos.y = Mathf.Clamp(tooltipPos.y, minY, maxY);
        
        tooltipRect.anchoredPosition = tooltipPos;
    }
    
    /// <summary>
    /// Update tooltip position (for follow mouse mode)
    /// </summary>
    void Update()
    {
        if (followMouse && currentTooltip != null && tooltipRect != null)
        {
            // Follow mouse cursor using new Input System
            Vector2 screenMousePos;
            if (Mouse.current != null)
            {
                screenMousePos = Mouse.current.position.ReadValue();
            }
            else
            {
                // Fallback to screen center if mouse is not available
                screenMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            }
            
            // Convert screen position to local canvas position
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(),
                screenMousePos,
                parentCanvas.worldCamera,
                out mousePos
            );
            
            Vector2 targetPos = mousePos + tooltipOffset;
            tooltipRect.anchoredPosition = Vector2.Lerp(
                tooltipRect.anchoredPosition,
                targetPos,
                Time.deltaTime * followSpeed
            );
            
            ClampTooltipToScreen();
        }
    }
    
    void OnDestroy()
    {
        HideTooltip();
    }
}

