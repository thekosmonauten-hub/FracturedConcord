using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

/// <summary>
/// Individual slot in the fusion UI for placing warrants.
/// Supports click-to-select from WarrantLockerGrid and drag-and-drop from locker.
/// </summary>
public class WarrantFusionSlot : MonoBehaviour, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual References")]
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image warrantIcon;
    [SerializeField] private TextMeshProUGUI warrantNameText;
    [SerializeField] private TextMeshProUGUI slotLabelText;
    [SerializeField] private Image rarityBorder;
    
    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color magicColor = Color.blue;
    [SerializeField] private Color rareColor = Color.yellow;
    [SerializeField] private Color uniqueColor = Color.magenta;
    [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Header("Slot Configuration")]
    [SerializeField] private string slotLabel = "Slot";
    [SerializeField] private bool isInteractable = true;
    
    private int slotIndex;
    private WarrantDefinition currentWarrant;
    private WarrantFusionUI parentUI;
    private WarrantLockerItem sourceLockerItem;
    private WarrantIconLibrary iconLibrary;
    
    // Tooltip support
    private ItemTooltipManager tooltipManager;
    private PointerEventData lastPointerEvent;
    
    public WarrantDefinition CurrentWarrant => currentWarrant;
    public System.Action<int, WarrantDefinition> OnWarrantChanged;
    
    public void Initialize(int index, WarrantFusionUI parent)
    {
        slotIndex = index;
        parentUI = parent;
        
        // Get icon library from parent UI
        if (parentUI != null)
        {
            iconLibrary = parentUI.GetIconLibrary();
        }
        
        if (slotLabelText != null && !string.IsNullOrEmpty(slotLabel))
        {
            slotLabelText.text = slotLabel;
        }
        
        // Ensure the slot can receive clicks (and tooltips for output slot)
        // Output slot (index -1) should always allow tooltips
        bool shouldBeInteractable = isInteractable || slotIndex == -1;
        SetInteractable(shouldBeInteractable);
        
        UpdateVisuals();
    }
    
    public void SetWarrant(WarrantDefinition warrant)
    {
        currentWarrant = warrant;
        sourceLockerItem = null;
        UpdateVisuals();
        OnWarrantChanged?.Invoke(slotIndex, warrant);
    }
    
    public void SetWarrantFromLocker(WarrantDefinition warrant, WarrantLockerItem lockerItem)
    {
        currentWarrant = warrant;
        sourceLockerItem = lockerItem;
        UpdateVisuals();
        OnWarrantChanged?.Invoke(slotIndex, warrant);
    }
    
    public void ClearSlot()
    {
        currentWarrant = null;
        sourceLockerItem = null;
        UpdateVisuals();
        OnWarrantChanged?.Invoke(slotIndex, null);
    }
    
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        
        // Output slot (slotIndex == -1) should always allow tooltips, but not drops/clicks
        bool allowTooltips = interactable || slotIndex == -1;
        bool allowDrops = interactable && slotIndex != -1; // Output slot doesn't accept drops
        
        // Ensure we can receive pointer clicks and drops (but not for output slot)
        var button = GetComponent<Button>();
        if (button == null && interactable && slotIndex != -1)
        {
            // Add a Button component for click handling (transparent, just for events)
            button = gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None; // No visual transition
            button.targetGraphic = slotBackground;
        }
        else if (button != null)
        {
            // Disable button for output slot, enable for input slots
            button.interactable = interactable && slotIndex != -1;
        }
        
        // Ensure background image can receive raycasts for tooltips (always for output slot)
        if (slotBackground != null)
        {
            slotBackground.raycastTarget = allowTooltips;
        }
        
        // Ensure icon image can also receive raycasts for tooltips
        if (warrantIcon != null)
        {
            warrantIcon.raycastTarget = allowTooltips;
        }
        
        // Ensure the root GameObject has a Graphic component for pointer events (for tooltips)
        var rootImage = GetComponent<Image>();
        if (rootImage == null && allowTooltips)
        {
            rootImage = gameObject.AddComponent<Image>();
            rootImage.color = new Color(1, 1, 1, 0.01f); // Nearly transparent for raycasting
        }
        if (rootImage != null)
        {
            rootImage.raycastTarget = allowTooltips;
        }
    }
    
    public WarrantLockerItem GetLockerItem()
    {
        return sourceLockerItem;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Output slot (slotIndex == -1) should not respond to clicks
        if (slotIndex == -1)
            return;
        
        if (!isInteractable)
            return;
        
        // Right-click: Remove warrant from slot
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentWarrant != null)
            {
                // Return warrant to locker if available
                if (sourceLockerItem != null && parentUI != null)
                {
                    var lockerGrid = parentUI.lockerGridRef;
                    if (lockerGrid != null)
                    {
                        lockerGrid.ReturnWarrantToLocker(currentWarrant.warrantId);
                    }
                }
                
                ClearSlot();
                Debug.Log($"[WarrantFusionSlot] Right-clicked: Removed warrant from slot {slotIndex}");
            }
            return;
        }
        
        // Left-click: Show locker for selection
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Notify parent UI that this slot was clicked
            if (parentUI != null)
            {
                parentUI.OnSlotClicked(this);
            }
        }
    }
    
    /// <summary>
    /// Handle drag-and-drop from WarrantLockerItem
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[WarrantFusionSlot] OnDrop called on slot {slotIndex}. pointerDrag: {eventData?.pointerDrag?.name}");
        
        // Output slot (slotIndex == -1) should not accept drops
        if (slotIndex == -1)
        {
            Debug.LogWarning("[WarrantFusionSlot] OnDrop: Output slot does not accept drops");
            return;
        }
        
        if (!isInteractable)
        {
            Debug.LogWarning($"[WarrantFusionSlot] OnDrop: Slot {slotIndex} is not interactable");
            return;
        }
        
        if (eventData == null || eventData.pointerDrag == null)
        {
            Debug.LogWarning("[WarrantFusionSlot] OnDrop: eventData or pointerDrag is null");
            return;
        }
        
        // Get the dragged warrant from the event data
        var draggedItem = eventData.pointerDrag.GetComponent<WarrantLockerItem>();
        if (draggedItem == null)
        {
            Debug.LogWarning($"[WarrantFusionSlot] OnDrop: No WarrantLockerItem found in pointerDrag '{eventData.pointerDrag.name}'. Component types: {string.Join(", ", eventData.pointerDrag.GetComponents<Component>().Select(c => c.GetType().Name))}");
            return;
        }
        
        var warrant = draggedItem.Definition;
        if (warrant == null)
        {
            Debug.LogWarning("[WarrantFusionSlot] OnDrop: WarrantLockerItem has no warrant definition");
            return;
        }
        
        // Check if warrant is valid for fusion
        if (warrant.rarity == WarrantRarity.Unique)
        {
            Debug.LogWarning($"[WarrantFusionSlot] Cannot fuse Unique warrant: {warrant.warrantId}");
            return;
        }
        
        if (warrant.isBlueprint)
        {
            Debug.LogWarning($"[WarrantFusionSlot] Cannot fuse blueprint warrant: {warrant.warrantId}");
            return;
        }
        
        // If slot already has a warrant, return it to locker (if available)
        if (currentWarrant != null && sourceLockerItem != null && parentUI != null)
        {
            var lockerGrid = parentUI.lockerGridRef;
            if (lockerGrid != null)
            {
                lockerGrid.ReturnWarrantToLocker(currentWarrant.warrantId);
            }
        }
        
        // Assign the dropped warrant to this slot
        SetWarrantFromLocker(warrant, draggedItem);
        
        // Hide the locker panel after drop
        if (parentUI != null)
        {
            parentUI.HideLockerPanel();
        }
        
        // Force destroy the drag ghost if it still exists
        if (draggedItem != null)
        {
            // The draggedItem should handle cleanup, but we can force it here if needed
            var lockerItem = draggedItem.GetComponent<WarrantLockerItem>();
            if (lockerItem != null)
            {
                // Access the private dragGhost field via reflection if needed
                // For now, rely on OnEndDrag being called properly
            }
        }
        
        Debug.Log($"[WarrantFusionSlot] Dropped warrant '{warrant.warrantId}' into slot {slotIndex}");
    }
    
    /// <summary>
    /// Called by WarrantFusionUI when a warrant is selected from the locker grid.
    /// </summary>
    public void AssignWarrantFromSelection(WarrantDefinition warrant, WarrantLockerItem lockerItem)
    {
        if (warrant == null)
            return;
        
        // Check if warrant is valid for fusion
        if (warrant.rarity == WarrantRarity.Unique)
        {
            Debug.LogWarning($"[WarrantFusionSlot] Cannot fuse Unique warrant: {warrant.warrantId}");
            return;
        }
        
        if (warrant.isBlueprint)
        {
            Debug.LogWarning($"[WarrantFusionSlot] Cannot fuse blueprint warrant: {warrant.warrantId}");
            return;
        }
        
        // If slot already has a warrant, return it to locker (if available)
        if (currentWarrant != null && sourceLockerItem != null && parentUI != null)
        {
            // Return previous warrant to locker
            var lockerGrid = parentUI.lockerGridRef;
            if (lockerGrid != null)
            {
                lockerGrid.ReturnWarrantToLocker(currentWarrant.warrantId);
            }
        }
        
        SetWarrantFromLocker(warrant, lockerItem);
        
        Debug.Log($"[WarrantFusionSlot] Assigned warrant '{warrant.warrantId}' to slot {slotIndex}");
    }
    
    private void UpdateVisuals()
    {
        if (currentWarrant == null)
        {
            // Empty slot
            if (warrantIcon != null)
            {
                warrantIcon.enabled = false;
            }
            
            if (warrantNameText != null)
            {
                warrantNameText.text = "";
            }
            
            if (rarityBorder != null)
            {
                rarityBorder.color = emptyColor;
            }
            
            if (slotBackground != null)
            {
                slotBackground.color = emptyColor;
            }
        }
        else
        {
            // Has warrant
            if (warrantIcon != null)
            {
                // Get icon from warrant, or assign one from library if missing
                Sprite iconToUse = currentWarrant.icon;
                if (iconToUse == null && iconLibrary != null)
                {
                    // Try to get icon by warrant ID first
                    iconToUse = iconLibrary.GetIcon(currentWarrant.warrantId);
                    
                    // If still null, get a random icon from the pool
                    if (iconToUse == null)
                    {
                        iconToUse = iconLibrary.GetRandomIcon();
                        // Assign it to the warrant so it persists
                        if (iconToUse != null)
                        {
                            currentWarrant.icon = iconToUse;
                            Debug.Log($"[WarrantFusionSlot] Assigned random icon from library to warrant '{currentWarrant.warrantId}'");
                        }
                    }
                }
                
                warrantIcon.sprite = iconToUse;
                warrantIcon.enabled = iconToUse != null;
            }
            
            if (warrantNameText != null)
            {
                warrantNameText.text = currentWarrant.displayName;
            }
            
            Color rarityColor = GetRarityColor(currentWarrant.rarity);
            
            if (rarityBorder != null)
            {
                rarityBorder.color = rarityColor;
            }
            
            if (slotBackground != null)
            {
                slotBackground.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.3f);
            }
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
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        lastPointerEvent = eventData;
        if (currentWarrant != null)
        {
            ShowTooltip();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    
    private void ShowTooltip()
    {
        if (currentWarrant == null)
        {
            Debug.Log($"[WarrantFusionSlot] ShowTooltip: No warrant in slot {slotIndex}");
            return;
        }
        
        if (tooltipManager == null)
        {
            tooltipManager = ItemTooltipManager.Instance;
        }
        
        if (tooltipManager == null)
        {
            Debug.LogWarning("[WarrantFusionSlot] ItemTooltipManager.Instance is null. Tooltips will not be displayed. Make sure ItemTooltipManager exists in the scene.");
            return;
        }
        
        // Get NotableDatabase from parent UI if available
        WarrantNotableDatabase notableDb = null;
        if (parentUI != null)
        {
            notableDb = parentUI.GetNotableDatabase();
            if (notableDb == null)
            {
                Debug.LogWarning("[WarrantFusionSlot] GetNotableDatabase() returned null. Tooltip may be incomplete.");
            }
        }
        else
        {
            Debug.LogWarning("[WarrantFusionSlot] parentUI is null. Cannot get NotableDatabase for tooltip.");
        }
        
        // Determine subtitle based on slot type
        string subtitle = "Fusion Slot";
        if (parentUI != null)
        {
            if (this == parentUI.GetOutputSlot())
            {
                subtitle = "Fused Warrant (Output)";
            }
            else
            {
                subtitle = $"Fusion Input Slot {slotIndex + 1}";
            }
        }
        
        // Build tooltip data
        var data = WarrantTooltipUtility.BuildSingleWarrantData(currentWarrant, subtitle, notableDb);
        if (data == null)
        {
            Debug.LogWarning($"[WarrantFusionSlot] BuildSingleWarrantData returned null for warrant '{currentWarrant.warrantId}'");
            tooltipManager?.HideTooltip();
            return;
        }
        
        // Show tooltip at mouse position
        var position = lastPointerEvent != null ? lastPointerEvent.position : (Vector2)Input.mousePosition;
        Debug.Log($"[WarrantFusionSlot] Showing tooltip for warrant '{currentWarrant.warrantId}' at position {position}");
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
}

