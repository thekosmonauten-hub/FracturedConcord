using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Individual warrant item in the locker grid. Implements IWarrantDragPayload
/// so it can be dragged from the locker into sockets.
/// Also supports click-to-select for fusion UI and hover tooltips.
/// </summary>
public class WarrantLockerItem : MonoBehaviour, IWarrantDragPayload,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color magicColor = Color.blue;
    [SerializeField] private Color rareColor = Color.yellow;
    [SerializeField] private Color uniqueColor = Color.magenta;
    
    private WarrantDefinition warrantDefinition;
    private WarrantLockerGrid ownerGrid;
    private RectTransform dragGhost;
    private Image dragGhostImage;
    private Canvas rootCanvas;
    private bool isDragging;
    
    // Selection callback for fusion UI
    private System.Action<WarrantDefinition, WarrantLockerItem> selectionCallback;
    private bool isSelectionMode = false;
    
    // Tooltip support
    private ItemTooltipManager tooltipManager;
    private PointerEventData lastPointerEvent;
    
    public string WarrantId => warrantDefinition != null ? warrantDefinition.warrantId : null;
    public Sprite Icon => iconImage != null ? iconImage.sprite : null;
    
    public WarrantDefinition Definition => warrantDefinition;
    
    public void Initialize(WarrantDefinition definition, WarrantLockerGrid grid = null)
    {
        ownerGrid = grid ?? ownerGrid;
        warrantDefinition = definition;
        UpdateVisuals();
        
        // Get tooltip manager (use singleton instance)
        if (tooltipManager == null)
        {
            tooltipManager = ItemTooltipManager.Instance;
        }
    }
    
    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null)
            {
                Debug.LogError("[WarrantLockerItem] No Canvas found in parent hierarchy! Drag-and-drop requires a Canvas.");
            }
        }
        
        // Ensure we have a Graphic component on this GameObject for drag events
        var rootImage = GetComponent<Image>();
        if (rootImage == null)
        {
            rootImage = gameObject.AddComponent<Image>();
            rootImage.color = new Color(1, 1, 1, 0.01f); // Nearly transparent but not fully (for raycasting)
            Debug.LogWarning("[WarrantLockerItem] Added Image component to root GameObject for drag events");
        }
        rootImage.raycastTarget = true;
        
        // Ensure Image components can receive raycasts for drag initiation
        if (iconImage != null)
        {
            iconImage.raycastTarget = true;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
        }
        
        // Verify EventSystem exists
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            Debug.LogError("[WarrantLockerItem] No EventSystem found in scene! Drag-and-drop requires an EventSystem.");
        }
    }
    
    private void UpdateVisuals()
    {
        if (warrantDefinition == null)
        {
            if (iconImage != null) iconImage.enabled = false;
            if (backgroundImage != null) backgroundImage.enabled = false;
            return;
        }
        
        if (iconImage != null)
        {
            iconImage.sprite = warrantDefinition.icon;
            iconImage.enabled = warrantDefinition.icon != null;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = GetRarityColor(warrantDefinition.rarity);
            backgroundImage.enabled = true;
        }
    }
    
    private Color GetRarityColor(WarrantRarity rarity)
    {
        switch (rarity)
        {
            case WarrantRarity.Common:
                return commonColor;
            case WarrantRarity.Magic:
                return magicColor;
            case WarrantRarity.Rare:
                return rareColor;
            case WarrantRarity.Unique:
                return uniqueColor;
            default:
                return commonColor;
        }
    }
    
    /// <summary>
    /// Sets the selection callback for click-to-select mode (used by fusion UI).
    /// </summary>
    public void SetSelectionCallback(System.Action<WarrantDefinition, WarrantLockerItem> callback)
    {
        selectionCallback = callback;
        isSelectionMode = callback != null;
    }
    
    /// <summary>
    /// Clears the selection callback, disabling click-to-select mode.
    /// </summary>
    public void ClearSelectionCallback()
    {
        selectionCallback = null;
        isSelectionMode = false;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // If in selection mode, handle click as selection
        if (isSelectionMode && selectionCallback != null && warrantDefinition != null)
        {
            selectionCallback.Invoke(warrantDefinition, this);
            return;
        }
        
        // Otherwise, ignore clicks (drag-and-drop will handle it)
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        lastPointerEvent = eventData;
        ShowTooltip();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            HideTooltip();
        }
    }
    
    private void ShowTooltip()
    {
        if (warrantDefinition == null)
            return;
        
        if (tooltipManager == null)
        {
            tooltipManager = ItemTooltipManager.Instance;
        }
        
        if (tooltipManager == null)
        {
            Debug.LogWarning("[WarrantLockerItem] ItemTooltipManager not found. Tooltips will not be displayed.");
            return;
        }
        
        // Get NotableDatabase from owner grid if available
        WarrantNotableDatabase notableDb = null;
        if (ownerGrid != null)
        {
            notableDb = ownerGrid.GetNotableDatabase();
        }
        
        // Build tooltip data
        var subtitle = "Warrant Locker";
        var data = WarrantTooltipUtility.BuildSingleWarrantData(warrantDefinition, subtitle, notableDb);
        if (data == null)
        {
            tooltipManager?.HideTooltip();
            return;
        }
        
        // Show tooltip at mouse position
        var position = lastPointerEvent != null ? lastPointerEvent.position : (Vector2)Input.mousePosition;
        tooltipManager?.ShowWarrantTooltip(data, position);
    }
    
    private void HideTooltip()
    {
        if (tooltipManager == null)
        {
            tooltipManager = ItemTooltipManager.Instance;
        }
        
        tooltipManager?.HideTooltip();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Allow dragging even in selection mode - drag-and-drop takes priority over click-to-select
        // If user starts dragging, they want drag-and-drop, not click-to-select
        
        Debug.Log($"[WarrantLockerItem] OnBeginDrag called on {gameObject.name}");
        
        if (warrantDefinition == null)
        {
            Debug.LogWarning($"[WarrantLockerItem] Cannot drag: warrantDefinition is null on {gameObject.name}");
            return;
        }
        
        if (string.IsNullOrEmpty(WarrantId))
        {
            Debug.LogWarning($"[WarrantLockerItem] Cannot drag: WarrantId is null or empty on {gameObject.name}");
            return;
        }
        
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null)
            {
                Debug.LogError("[WarrantLockerItem] Cannot create drag ghost: No Canvas found!");
                return;
            }
        }
        
        isDragging = true;
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        
        // Hide tooltip when dragging starts
        HideTooltip();
        
        CreateDragGhost();
        UpdateDragGhostPosition(eventData);
        Debug.Log($"[WarrantLockerItem] Started dragging warrant {WarrantId} from {gameObject.name}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        UpdateDragGhostPosition(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Always destroy drag ghost immediately, regardless of drop target
        DestroyDragGhost();
        
        // Check if we successfully dropped onto a socket or fusion slot
        var droppedOnSocket = eventData.pointerCurrentRaycast.gameObject?.GetComponent<WarrantSocketView>();
        var droppedOnFusionSlot = eventData.pointerCurrentRaycast.gameObject?.GetComponent<WarrantFusionSlot>();
        
        if (droppedOnSocket == null && droppedOnFusionSlot == null)
        {
            Debug.Log($"[WarrantLockerItem] Drag ended: warrant {WarrantId} not dropped on a valid target (socket or fusion slot)");
        }
        else if (droppedOnFusionSlot != null)
        {
            // The OnDrop handler in WarrantFusionSlot will handle the assignment
            Debug.Log($"[WarrantLockerItem] Dropped warrant {WarrantId} onto fusion slot");
        }
        
        // Reset drag visuals after checking drop target
        ResetDragVisuals();
    }
    
    public void OnAssignmentAccepted(WarrantSocketView target, string replacedWarrantId)
    {
        ResetDragVisuals();
        
        if (ownerGrid == null)
        {
            Debug.LogWarning("[WarrantLockerItem] No owner grid assigned; cannot update inventory state.");
            return;
        }

        ownerGrid.HandleWarrantAssigned(this);

        if (!string.IsNullOrEmpty(replacedWarrantId))
        {
            ownerGrid.ReturnWarrantToLocker(replacedWarrantId);
        }
    }
    
    private void CreateDragGhost()
    {
        DestroyDragGhost();
        
        if (rootCanvas == null)
        {
            Debug.LogWarning("[WarrantLockerItem] Cannot create drag ghost: rootCanvas is null");
            return;
        }
        
        // Use iconImage sprite if available, otherwise use backgroundImage or a fallback
        Sprite ghostSprite = null;
        Vector2 ghostSize = new Vector2(80, 80); // Default size
        
        if (iconImage != null && iconImage.sprite != null)
        {
            ghostSprite = iconImage.sprite;
            ghostSize = iconImage.rectTransform.rect.size;
        }
        else if (backgroundImage != null)
        {
            // Create a colored square if no sprite
            ghostSize = backgroundImage.rectTransform.rect.size;
        }
        
        var ghostObj = new GameObject("WarrantLockerDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        ghostObj.transform.SetParent(rootCanvas.transform, false);
        
        // Set to highest sibling so it renders on top
        ghostObj.transform.SetAsLastSibling();
        
        dragGhost = ghostObj.GetComponent<RectTransform>();
        dragGhostImage = ghostObj.GetComponent<Image>();
        dragGhostImage.sprite = ghostSprite;
        
        if (ghostSprite == null && backgroundImage != null)
        {
            // Create a solid color square for the ghost
            dragGhostImage.color = backgroundImage.color;
        }
        else
        {
            dragGhostImage.color = Color.white;
        }
        
        dragGhostImage.raycastTarget = false;
        
        var group = ghostObj.GetComponent<CanvasGroup>();
        group.alpha = 0.9f;
        group.blocksRaycasts = false;
        group.ignoreParentGroups = true;
        
        dragGhost.sizeDelta = ghostSize;
        dragGhost.localScale = Vector3.one;
        
        Debug.Log($"[WarrantLockerItem] Created drag ghost with size {ghostSize}");
    }
    
    private void DestroyDragGhost()
    {
        if (dragGhost != null)
        {
            Destroy(dragGhost.gameObject);
            dragGhost = null;
            dragGhostImage = null;
        }
    }
    
    private void UpdateDragGhostPosition(PointerEventData eventData)
    {
        if (dragGhost == null || eventData == null) return;
        dragGhost.position = eventData.position;
    }

    private void ResetDragVisuals()
    {
        isDragging = false;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        DestroyDragGhost();
        
        // Hide tooltip after drag ends
        HideTooltip();
    }
}

