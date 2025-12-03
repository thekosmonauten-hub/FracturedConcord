using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles hover and click events for sub-nodes in choice nodes to show tooltips and select them
/// </summary>
public class SubNodeHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private AscendancySubNode subNode;
    private string choiceNodeName;
    private AscendancyTreeDisplay treeDisplay;
    private int subNodeIndex = -1;
    private Button subNodeButton;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = false;
    
    public void Initialize(AscendancySubNode subNodeData, string parentChoiceNodeName, AscendancyTreeDisplay display, int index)
    {
        subNode = subNodeData;
        choiceNodeName = parentChoiceNodeName;
        treeDisplay = display;
        subNodeIndex = index;
        
        // Ensure button exists for clicks
        SetupButton();
    }
    
    void SetupButton()
    {
        // Find or create button
        subNodeButton = GetComponent<Button>();
        if (subNodeButton == null)
        {
            subNodeButton = GetComponentInChildren<Button>(true);
        }
        
        if (subNodeButton == null)
        {
            // Add button component
            subNodeButton = gameObject.AddComponent<Button>();
            
            // Ensure image exists for button target
            Image img = GetComponent<Image>();
            if (img == null)
            {
                img = GetComponentInChildren<Image>(true);
            }
            if (img != null)
            {
                img.raycastTarget = true;
                subNodeButton.targetGraphic = img;
            }
        }
        
        // Remove existing listeners and add click handler
        if (subNodeButton != null)
        {
            subNodeButton.onClick.RemoveAllListeners();
            subNodeButton.onClick.AddListener(OnSubNodeClicked);
            subNodeButton.interactable = true;
        }
    }
    
    void OnSubNodeClicked()
    {
        if (subNode == null || treeDisplay == null || subNodeIndex < 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("[SubNodeHoverHandler] Cannot select subnode - missing data");
            return;
        }
        
        // Get progression from tree display
        var progression = treeDisplay.GetProgression();
        var ascendancy = treeDisplay.GetCurrentAscendancy();
        
        if (progression == null || ascendancy == null)
        {
            Debug.LogWarning("[SubNodeHoverHandler] Cannot select subnode - progression or ascendancy is null");
            return;
        }
        
        // Check if choice node is unlocked
        if (!progression.IsPassiveUnlocked(choiceNodeName))
        {
            Debug.LogWarning($"[SubNodeHoverHandler] Cannot select subnode - choice node '{choiceNodeName}' is not unlocked");
            return;
        }
        
        // Select the subnode
        bool selected = progression.SelectSubNode(choiceNodeName, subNodeIndex, ascendancy);
        
        if (selected)
        {
            Debug.Log($"<color=green>[SubNodeHoverHandler] ✓ Selected subnode {subNodeIndex} ('{subNode.name}') for choice node '{choiceNodeName}'</color>");
            
            // Notify tree display to refresh (hide subnodes, show selected one)
            treeDisplay.RefreshChoiceNodeDisplay(choiceNodeName);
        }
        else
        {
            Debug.LogWarning($"[SubNodeHoverHandler] Failed to select subnode {subNodeIndex} for '{choiceNodeName}'");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (subNode == null || treeDisplay == null) return;
        
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector2 position = rect.anchoredPosition;
            treeDisplay.ShowSubNodeTooltip(subNode, choiceNodeName, position);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (treeDisplay != null && treeDisplay.TooltipController != null)
        {
            treeDisplay.TooltipController.HideTooltip();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle click via button (OnSubNodeClicked)
        // This ensures proper button behavior
        if (subNodeButton != null && subNodeButton.interactable)
        {
            OnSubNodeClicked();
        }
    }
}

