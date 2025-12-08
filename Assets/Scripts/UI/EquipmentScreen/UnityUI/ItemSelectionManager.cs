using UnityEngine;

/// <summary>
/// Manages item selection state for equipment/inventory interactions
/// Tracks which item is currently selected for equipping/moving
/// </summary>
public class ItemSelectionManager : MonoBehaviour
{
    public static ItemSelectionManager Instance { get; private set; }
    
    [Header("Selection State")]
    private BaseItem selectedItem;
    private int selectedInventoryIndex = -1;
    private EquipmentType? selectedEquipmentSlot = null;
    private bool isFromInventory = false;
    
    // Events
    public System.Action<BaseItem> OnItemSelected;
    public System.Action OnSelectionCleared;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Select an item from inventory
    /// </summary>
    public void SelectItemFromInventory(BaseItem item, int inventoryIndex)
    {
        selectedItem = item;
        selectedInventoryIndex = inventoryIndex;
        selectedEquipmentSlot = null;
        isFromInventory = true;
        
        Debug.Log($"[ItemSelection] Selected from inventory: {item?.GetDisplayName() ?? "NULL"} at index {inventoryIndex}");
        OnItemSelected?.Invoke(item);
    }
    
    /// <summary>
    /// Select an equipped item
    /// </summary>
    public void SelectItemFromEquipment(BaseItem item, EquipmentType slotType)
    {
        selectedItem = item;
        selectedInventoryIndex = -1;
        selectedEquipmentSlot = slotType;
        isFromInventory = false;
        
        Debug.Log($"[ItemSelection] Selected from equipment: {item?.GetDisplayName() ?? "NULL"} in slot {slotType}");
        OnItemSelected?.Invoke(item);
    }
    
    /// <summary>
    /// Clear current selection
    /// </summary>
    public void ClearSelection()
    {
        selectedItem = null;
        selectedInventoryIndex = -1;
        selectedEquipmentSlot = null;
        isFromInventory = false;
        
        Debug.Log($"[ItemSelection] Selection cleared");
        OnSelectionCleared?.Invoke();
    }
    
    /// <summary>
    /// Get currently selected item
    /// </summary>
    public BaseItem GetSelectedItem()
    {
        return selectedItem;
    }
    
    /// <summary>
    /// Check if an item is currently selected
    /// </summary>
    public bool HasSelection()
    {
        return selectedItem != null;
    }
    
    /// <summary>
    /// Get selected inventory index
    /// </summary>
    public int GetSelectedInventoryIndex()
    {
        return selectedInventoryIndex;
    }
    
    /// <summary>
    /// Get selected equipment slot
    /// </summary>
    public EquipmentType? GetSelectedEquipmentSlot()
    {
        return selectedEquipmentSlot;
    }
    
    /// <summary>
    /// Check if selection is from inventory
    /// </summary>
    public bool IsFromInventory()
    {
        return isFromInventory;
    }
    
    /// <summary>
    /// Check if item can be equipped in a specific slot
    /// </summary>
    public bool CanEquipToSlot(BaseItem item, EquipmentType targetSlot)
    {
        if (item == null) return false;
        
        // Check if equipment types match
        return item.equipmentType == targetSlot;
    }
}

