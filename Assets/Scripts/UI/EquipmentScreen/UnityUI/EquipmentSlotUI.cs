using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Unity UI version of equipment slot
/// Handles equipment slot display, interaction, and events
/// Supports drag-and-drop from inventory
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler, 
    IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [Header("References")]
    public Image backgroundImage;
    public Image itemIconImage;
    public TextMeshProUGUI slotLabel;
    public TextMeshProUGUI itemNameLabel;
    public Button button; // Optional: Unity Button component for better click detection
    
    [Header("Settings")]
    public EquipmentType slotType;
    public Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color occupiedColor = new Color(0.2f, 0.3f, 0.4f, 0.9f);
    public Color hoverColor = new Color(0.3f, 0.4f, 0.5f, 1f);
    
    public event Action<EquipmentType> OnSlotClicked;
    public event Action<EquipmentType, Vector2> OnSlotHovered;
    
    private BaseItem equippedItem = null;
    
    private void Awake()
    {
        // Auto-find Button component if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        // Wire up Button onClick if available
        if (button != null)
        {
            button.onClick.AddListener(() => OnPointerClick(null));
            Debug.Log($"[EquipmentSlotUI] Button component wired up for {slotType}");
        }
    }
    
    public void Initialize(EquipmentType type, string labelText)
    {
        slotType = type;
        if (slotLabel != null)
            slotLabel.text = labelText;
        
        UpdateVisual();
    }
    
    public void SetEquippedItem(BaseItem item)
    {
        equippedItem = item;
        Debug.Log($"[EquipmentSlotUI] {slotType} SetEquippedItem called with: {(item != null ? item.itemName : "NULL")}");
        UpdateVisual();
    }
    
    public BaseItem GetEquippedItem()
    {
        return equippedItem;
    }
    
    private void UpdateVisual()
    {
        if (equippedItem != null)
        {
            Debug.Log($"[EquipmentSlotUI] UpdateVisual for {slotType}: Showing {equippedItem.itemName}");
            
            if (backgroundImage != null)
                backgroundImage.color = occupiedColor;
            
            if (itemIconImage != null && equippedItem.itemIcon != null)
            {
                itemIconImage.sprite = equippedItem.itemIcon;
                itemIconImage.enabled = true;
                Debug.Log($"[EquipmentSlotUI] ✅ Set icon sprite for {slotType}");
            }
            else
            {
                Debug.LogWarning($"[EquipmentSlotUI] ⚠️ No icon: itemIconImage={itemIconImage != null}, sprite={equippedItem.itemIcon != null}");
            }
            
            if (itemNameLabel != null)
            {
                itemNameLabel.text = equippedItem.itemName;
                itemNameLabel.enabled = true;
            }
        }
        else
        {
            Debug.Log($"[EquipmentSlotUI] UpdateVisual for {slotType}: Clearing (no item equipped)");
            
            if (backgroundImage != null)
                backgroundImage.color = emptyColor;
            
            if (itemIconImage != null)
                itemIconImage.enabled = false;
            
            if (itemNameLabel != null)
                itemNameLabel.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"<color=yellow>[EquipmentSlotUI] OnPointerClick called for {slotType}</color>");
        Debug.Log($"<color=yellow>  Equipped Item: {(equippedItem != null ? equippedItem.itemName : "NULL")}</color>");
        Debug.Log($"<color=yellow>  Subscribers to OnSlotClicked: {OnSlotClicked?.GetInvocationList().Length ?? 0}</color>");
        
        // Fire the event (for EquipmentScreenUI)
        OnSlotClicked?.Invoke(slotType);
        
        // ALSO: Directly show tooltip if item is equipped and no subscribers
        // This ensures tooltip works even if event system isn't wired up
        if (equippedItem != null && ItemTooltipManager.Instance != null)
        {
            // Check if no one is handling the event (event system not wired)
            if (OnSlotClicked == null || OnSlotClicked.GetInvocationList().Length == 0)
            {
                Debug.Log($"<color=lime>[EquipmentSlotUI] No event subscribers, directly showing tooltip for {equippedItem.itemName}</color>");
                ItemTooltipManager.Instance.ShowEquippedTooltip(equippedItem);
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedItem != null)
        {
            OnSlotHovered?.Invoke(slotType, eventData.position);
        }
        
        // Visual feedback
        Color originalColor = backgroundImage.color;
        backgroundImage.color = hoverColor;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisual(); // Restore original color
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }
    
    // ====================
    // DRAG & DROP HANDLER
    // ====================
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[EquipmentSlotUI] Item dropped on {slotType} slot");
        
        // Get the dragged item from DragVisualHelper
        var dragHelper = DragVisualHelper.Instance;
        if (dragHelper == null || !dragHelper.IsDragging())
        {
            Debug.LogWarning("[EquipmentSlotUI] No drag helper or not dragging!");
            return;
        }
        
        BaseItem draggedItem = dragHelper.GetDraggedItem();
        if (draggedItem == null)
        {
            Debug.LogWarning("[EquipmentSlotUI] Dragged item is null!");
            return;
        }
        
        // Check if item is equipment
        if (draggedItem != null)
        {
            // Check if item can be equipped in this slot
            if (draggedItem.equipmentType == slotType)
            {
                Debug.Log($"[EquipmentSlotUI] Valid drop: {draggedItem.itemName} → {slotType}");
                
                // The actual equipping will be handled by InventoryGridUI.HandleDragToEquipmentSlot()
                // This is just visual feedback
                
                // Highlight to show valid drop
                backgroundImage.color = Color.green * 0.5f;
            }
            else
            {
                Debug.LogWarning($"[EquipmentSlotUI] Invalid drop: {draggedItem.itemName} cannot go in {slotType} slot!");
                
                // Flash red for invalid drop
                backgroundImage.color = Color.red * 0.5f;
            }
        }
        
        // Reset color after brief delay
        Invoke(nameof(UpdateVisual), 0.2f);
    }
    
    /// <summary>
    /// Get the slot type (for drop validation)
    /// </summary>
    public EquipmentType GetSlotType()
    {
        return slotType;
    }
}


