using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles hover events for sub-nodes in choice nodes to show tooltips
/// </summary>
public class SubNodeHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AscendancySubNode subNode;
    private string choiceNodeName;
    private AscendancyTreeDisplay treeDisplay;
    
    public void Initialize(AscendancySubNode subNodeData, string parentChoiceNodeName, AscendancyTreeDisplay display)
    {
        subNode = subNodeData;
        choiceNodeName = parentChoiceNodeName;
        treeDisplay = display;
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
}

