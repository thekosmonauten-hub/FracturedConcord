using UnityEngine;
using UnityEngine.EventSystems;
using PassiveTree;

/// <summary>
/// Handles hover events for individual cells and connects them to the static tooltip panel
/// </summary>
public class CellHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private StaticTooltipPanel tooltipPanel;
    
    // Runtime
    private CellController cellController;
    
    void Start()
    {
        // Get the CellController component
        cellController = GetComponent<CellController>();
        if (cellController == null)
        {
            Debug.LogError($"[CellHoverHandler] No CellController found on {gameObject.name}!");
        }
    }
    
    /// <summary>
    /// Set the tooltip panel reference
    /// </summary>
    public void SetTooltipPanel(StaticTooltipPanel panel)
    {
        tooltipPanel = panel;
    }
    
    /// <summary>
    /// Handle mouse enter
    /// </summary>
    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (tooltipPanel != null && cellController != null)
        {
            PassiveTreeLogger.LogCategory($"Mouse entered cell at {cellController.GridPosition}", "tooltip");
            tooltipPanel.UpdateTooltipContent(cellController);
        }
    }
    
    /// <summary>
    /// Handle mouse exit
    /// </summary>
    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            PassiveTreeLogger.LogCategory("Mouse exited cell", "tooltip");
            tooltipPanel.HideTooltip();
        }
    }
    
    /// <summary>
    /// Manually trigger tooltip update (for testing)
    /// </summary>
    [ContextMenu("Test Tooltip Update")]
    public void TestTooltipUpdate()
    {
        if (tooltipPanel != null && cellController != null)
        {
            Debug.Log($"[CellHoverHandler] Testing tooltip update for cell at {cellController.GridPosition}");
            tooltipPanel.UpdateTooltipContent(cellController);
        }
        else
        {
            Debug.LogError("[CellHoverHandler] Tooltip panel or cell controller not found!");
        }
    }
}
