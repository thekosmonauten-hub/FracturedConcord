using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Unity UI version of inventory grid
/// Manages dynamic generation and layout of inventory slots
/// Supports both character inventory and global stash
/// </summary>
public class InventoryGridUI : MonoBehaviour
{
    public enum GridMode
    {
        CharacterInventory,  // Character-specific inventory
        GlobalStash         // Shared stash across all characters
    }
    
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 6;
    public Vector2 cellSize = new Vector2(60, 60);
    public Vector2 spacing = new Vector2(2, 2);
    
    [Header("References")]
    public GameObject slotPrefab;
    public Transform gridContainer;
    
    [Header("Data Source")]
    [Tooltip("Which storage system this grid displays")]
    public GridMode gridMode = GridMode.CharacterInventory;
    [SerializeField] private bool refreshOnEnable = true;
    
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private CharacterManager boundCharacterManager;
    private StashManager boundStashManager;
    
    void Start()
    {
        GenerateGrid();
        if (refreshOnEnable)
        {
            // Defer refresh to next frame to prevent blocking scene load
            StartCoroutine(DeferredRefresh());
        }
    }

    private void OnEnable()
    {
        TryBindManagers();
        SubscribeToEvents();
        
        if (refreshOnEnable)
        {
            // Defer refresh to next frame to prevent blocking scene load
            StartCoroutine(DeferredRefresh());
        }
    }
    
    /// <summary>
    /// Defer refresh to next frame to prevent blocking scene initialization
    /// </summary>
    private System.Collections.IEnumerator DeferredRefresh()
    {
        yield return null; // Wait one frame
        RefreshFromDataSource();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Unsubscribe from all events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (boundCharacterManager != null)
        {
            boundCharacterManager.OnItemAdded -= HandleCharacterItemAdded;
        }
        
        if (boundStashManager != null)
        {
            boundStashManager.OnStashChanged -= HandleStashChanged;
        }
    }
    
    /// <summary>
    /// Subscribe to events based on current grid mode
    /// </summary>
    private void SubscribeToEvents()
    {
        if (gridMode == GridMode.CharacterInventory)
        {
            if (boundCharacterManager != null)
            {
                boundCharacterManager.OnItemAdded += HandleCharacterItemAdded;
            }
        }
        else if (gridMode == GridMode.GlobalStash)
        {
            if (boundStashManager != null)
            {
                boundStashManager.OnStashChanged += HandleStashChanged;
            }
        }
    }
    
    /// <summary>
    /// Change grid mode at runtime (can be called from Unity events)
    /// </summary>
    public void SetGridMode(GridMode newMode)
    {
        if (gridMode == newMode)
        {
            return; // No change needed
        }
        
        // Unsubscribe from old events
        UnsubscribeFromEvents();
        
        // Change mode
        gridMode = newMode;
        
        // Rebind to managers
        TryBindManagers();
        
        // Subscribe to new events
        SubscribeToEvents();
        
        // Refresh grid with new data source
        RefreshFromDataSource();
        
        Debug.Log($"[InventoryGridUI] Grid mode changed to: {newMode}");
    }
    
    /// <summary>
    /// Set grid mode from boolean (for Toggle On Value Changed)
    /// true = GlobalStash, false = CharacterInventory
    /// </summary>
    public void SetGridModeFromBool(bool useStash)
    {
        SetGridMode(useStash ? GridMode.GlobalStash : GridMode.CharacterInventory);
    }
    
    /// <summary>
    /// Set grid mode from integer (for Dropdown/Enum)
    /// 0 = CharacterInventory, 1 = GlobalStash
    /// </summary>
    public void SetGridModeFromInt(int modeIndex)
    {
        GridMode newMode = modeIndex == 0 ? GridMode.CharacterInventory : GridMode.GlobalStash;
        SetGridMode(newMode);
    }
    
    private void HandleStashChanged()
    {
        RefreshFromDataSource();
    }
    
    public void GenerateGrid()
    {
        // Clear existing slots
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();
        
        // Set up GridLayoutGroup
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        
        // Generate slots
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridContainer);
                slotObj.name = $"Slot_{x}_{y}";
                
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetPosition(x, y);
                    
                    // Capture coordinates for lambda
                    int capturedX = x;
                    int capturedY = y;
                    slotUI.OnSlotClicked += () => OnSlotClicked(capturedX, capturedY);
                    slotUI.OnSlotHovered += () => OnSlotHovered(capturedX, capturedY);
                    
                    // Drag & drop events
                    slotUI.OnDragStarted += (dragX, dragY) => OnDragBegin(dragX, dragY);
                    slotUI.OnDragging += (dragX, dragY, pos) => OnDragUpdate(dragX, dragY, pos);
                    slotUI.OnDragEnded += (dragX, dragY) => OnDragEnd(dragX, dragY);
                    
                    slots.Add(slotUI);
                }
            }
        }
        
        Debug.Log($"[InventoryGridUI] Generated {slots.Count} slots ({gridWidth}x{gridHeight})");
    }
    
    void OnSlotClicked(int x, int y)
    {
        int index = y * gridWidth + x;
        Debug.Log($"[InventoryGridUI] Slot clicked: ({x}, {y}), Index: {index} [Mode: {gridMode}]");
        
        List<BaseItem> items = GetItemList();
        if (items == null)
        {
            Debug.LogWarning($"[InventoryGridUI] {gridMode} not available");
            return;
        }
        
        // Check if slot has an item
        if (index >= 0 && index < items.Count && items[index] != null)
        {
            BaseItem clickedItem = items[index];
            
            // Check if selection manager exists
            var selectionManager = ItemSelectionManager.Instance;
            if (selectionManager == null)
            {
                Debug.LogWarning("[InventoryGridUI] ItemSelectionManager not found!");
                return;
            }
            
            // If an item is already selected, try to swap/equip
            if (selectionManager.HasSelection())
            {
                HandleItemSwap(index, clickedItem, selectionManager);
            }
            else
            {
                // Select this item
                selectionManager.SelectItemFromInventory(clickedItem, index);
                
                // Visual feedback
                if (index < slots.Count && slots[index] != null)
                {
                    slots[index].SetSelected(true);
                }
            }
        }
        else
        {
            // Empty slot clicked - clear selection
            var selectionManager = ItemSelectionManager.Instance;
            if (selectionManager != null && selectionManager.HasSelection())
            {
                selectionManager.ClearSelection();
                RefreshAllSlotVisuals();
            }
        }
    }
    
    private void HandleItemSwap(int targetIndex, BaseItem targetItem, ItemSelectionManager selectionManager)
    {
        BaseItem selectedItem = selectionManager.GetSelectedItem();
        
        if (selectionManager.IsFromInventory())
        {
            // Swapping two inventory items
            int sourceIndex = selectionManager.GetSelectedInventoryIndex();
            
            if (sourceIndex != targetIndex)
            {
                SwapInventoryItems(sourceIndex, targetIndex);
            }
            
            selectionManager.ClearSelection();
            RefreshFromCharacterInventory();
        }
        else
        {
            // Unequipping to inventory - handled elsewhere
            Debug.Log("[InventoryGridUI] Item from equipment clicked - use equipment slot to unequip");
        }
    }
    
    private void SwapInventoryItems(int index1, int index2)
    {
        List<BaseItem> items = GetItemList();
        if (items == null) return;
        
        if (index1 < 0 || index1 >= items.Count || index2 < 0 || index2 >= items.Count)
            return;
        
        // Swap items
        if (gridMode == GridMode.CharacterInventory)
        {
            BaseItem temp = items[index1];
            items[index1] = items[index2];
            items[index2] = temp;
        }
        else if (gridMode == GridMode.GlobalStash)
        {
            if (boundStashManager != null)
            {
                boundStashManager.SwapItems(index1, index2);
            }
        }
        
        Debug.Log($"[InventoryGridUI] Swapped items at {index1} and {index2} in {gridMode}");
    }
    
    private void RefreshAllSlotVisuals()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.SetSelected(false);
            }
        }
    }
    
    void OnSlotHovered(int x, int y)
    {
        int index = y * gridWidth + x;
        
        List<BaseItem> items = GetItemList();
        if (items == null) return;
        
        // Show tooltip for item
        if (index >= 0 && index < items.Count && items[index] != null)
        {
            BaseItem hoveredItem = items[index];
            
            if (ItemTooltipManager.Instance != null && slots[index] != null)
            {
                Vector2 position = slots[index].transform.position;
                
                // Show equipment tooltip
                ItemTooltipManager.Instance.ShowEquipmentTooltip(hoveredItem, position);
            }
        }
        else
        {
            // Hide tooltip on empty slot
            if (ItemTooltipManager.Instance != null)
            {
                ItemTooltipManager.Instance.HideTooltip();
            }
        }
    }
    
    public void SortInventory()
    {
        Debug.Log("[InventoryGridUI] Sort inventory requested");
        // TODO: Implement sorting logic
    }
    
    public void FilterInventory(string filterType)
    {
        Debug.Log($"[InventoryGridUI] Filter inventory by: {filterType}");
        // TODO: Implement filtering logic
    }
    
    public InventorySlotUI GetSlot(int x, int y)
    {
        int index = y * gridWidth + x;
        if (index >= 0 && index < slots.Count)
            return slots[index];
        return null;
    }

    /// <summary>
    /// Refresh grid from appropriate data source (inventory or stash)
    /// </summary>
    public void RefreshFromDataSource()
    {
        List<BaseItem> items = GetItemList();
        if (items == null)
        {
            Debug.LogWarning($"[InventoryGridUI] {gridMode} not available; cannot refresh grid.");
            // Clear all slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].SetOccupied(false, null, null, null);
                }
            }
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotUI slot = slots[i];
            if (slot == null)
                continue;

            if (items != null && i < items.Count && items[i] != null)
            {
                BaseItem item = items[i];
                slot.SetOccupied(true, item.itemIcon, item.GetDisplayName(), item);
            }
            else
            {
                slot.SetOccupied(false, null, null, null);
            }
        }
        
        Debug.Log($"[InventoryGridUI] Refreshed {gridMode} grid with {items.Count} items");
    }
    
    /// <summary>
    /// Legacy method name for backwards compatibility
    /// </summary>
    public void RefreshFromCharacterInventory()
    {
        RefreshFromDataSource();
    }
    
    /// <summary>
    /// Get the appropriate item list based on grid mode
    /// </summary>
    private List<BaseItem> GetItemList()
    {
        if (gridMode == GridMode.CharacterInventory)
        {
            if (!TryBindCharacterManager() || boundCharacterManager == null)
                return null;
            return boundCharacterManager.inventoryItems;
        }
        else if (gridMode == GridMode.GlobalStash)
        {
            if (!TryBindStashManager() || boundStashManager == null)
                return null;
            return boundStashManager.stashItems;
        }
        return null;
    }
    
    /// <summary>
    /// Try to bind to CharacterManager
    /// </summary>
    private bool TryBindCharacterManager()
    {
        if (boundCharacterManager != null)
            return true;

        boundCharacterManager = CharacterManager.Instance;
        return boundCharacterManager != null;
    }
    
    /// <summary>
    /// Try to bind to StashManager
    /// </summary>
    private bool TryBindStashManager()
    {
        if (boundStashManager != null)
            return true;

        boundStashManager = StashManager.Instance;
        return boundStashManager != null;
    }
    
    /// <summary>
    /// Try to bind to appropriate manager based on grid mode
    /// </summary>
    private void TryBindManagers()
    {
        if (gridMode == GridMode.CharacterInventory)
        {
            TryBindCharacterManager();
        }
        else if (gridMode == GridMode.GlobalStash)
        {
            TryBindStashManager();
        }
    }

    private void HandleCharacterItemAdded(BaseItem _)
    {
        if (gridMode == GridMode.CharacterInventory)
        {
            RefreshFromDataSource();
        }
    }
    
    // ====================
    // DRAG & DROP HANDLERS
    // ====================
    
    private int dragSourceIndex = -1;
    private Vector2 lastDragPosition;
    
    private void OnDragBegin(int x, int y)
    {
        dragSourceIndex = y * gridWidth + x;
        Debug.Log($"[InventoryGridUI] Drag begin from slot ({x}, {y}), Index: {dragSourceIndex}");
    }
    
    private void OnDragUpdate(int x, int y, Vector2 screenPosition)
    {
        // Store the last drag position for drop detection
        lastDragPosition = screenPosition;
    }
    
    private void OnDragEnd(int x, int y)
    {
        Debug.Log($"[InventoryGridUI] Drag end at slot ({x}, {y})");
        
        // Get mouse position from PointerEventData (stored during drag)
        Vector2 mousePosition = lastDragPosition;
        
        // Find what slot we're dropping on
        InventorySlotUI dropSlot = FindSlotAtScreenPosition(mousePosition);
        
        if (dropSlot != null)
        {
            (int dropX, int dropY) = dropSlot.GetPosition();
            int dropIndex = dropY * gridWidth + dropX;
            
            Debug.Log($"[InventoryGridUI] Dropped on slot ({dropX}, {dropY}), Index: {dropIndex}");
            
            // Swap items if both are valid
            if (dragSourceIndex >= 0 && dropIndex >= 0 && dragSourceIndex != dropIndex)
            {
                SwapInventoryItems(dragSourceIndex, dropIndex);
                RefreshFromCharacterInventory();
            }
        }
        else
        {
            // Check if dropped on equipment slot
            EquipmentSlotUI equipSlot = FindEquipmentSlotAtScreenPosition(mousePosition);
            if (equipSlot != null)
            {
                HandleDragToEquipmentSlot(dragSourceIndex, equipSlot);
            }
            else
            {
                Debug.Log("[InventoryGridUI] Dropped outside valid area");
            }
        }
        
        dragSourceIndex = -1;
    }
    
    private InventorySlotUI FindSlotAtScreenPosition(Vector2 screenPosition)
    {
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            
            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition))
            {
                return slot;
            }
        }
        return null;
    }
    
    private EquipmentSlotUI FindEquipmentSlotAtScreenPosition(Vector2 screenPosition)
    {
        var equipmentSlots = FindObjectsByType<EquipmentSlotUI>(FindObjectsSortMode.None);
        
        foreach (var slot in equipmentSlots)
        {
            if (slot == null) continue;
            
            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition))
            {
                return slot;
            }
        }
        return null;
    }
    
    private void HandleDragToEquipmentSlot(int inventoryIndex, EquipmentSlotUI targetSlot)
    {
        Debug.Log($"[InventoryGridUI] HandleDragToEquipmentSlot called: inventoryIndex={inventoryIndex} [Mode: {gridMode}]");
        
        // Only allow equipping from character inventory, not stash
        if (gridMode != GridMode.CharacterInventory)
        {
            Debug.LogWarning("[InventoryGridUI] Cannot equip items from stash! Move to inventory first.");
            return;
        }
        
        if (!TryBindCharacterManager())
        {
            Debug.LogError("[InventoryGridUI] ❌ Failed to bind CharacterManager!");
            return;
        }
        
        List<BaseItem> items = boundCharacterManager.inventoryItems;
        
        if (inventoryIndex < 0 || inventoryIndex >= items.Count)
        {
            Debug.LogError($"[InventoryGridUI] ❌ Invalid inventory index: {inventoryIndex} (inventory has {items.Count} items)");
            return;
        }
        
        if (items[inventoryIndex] == null)
        {
            Debug.LogError($"[InventoryGridUI] ❌ Item at index {inventoryIndex} is NULL!");
            return;
        }
        
        BaseItem item = items[inventoryIndex];
        EquipmentType slotType = targetSlot.GetSlotType();
        
        Debug.Log($"[InventoryGridUI] Attempting to equip '{item.itemName}' (type: {item.equipmentType}) to slot {slotType}");
        
        // Check if item can be equipped in this slot (1-handed weapons can go in both MainHand and OffHand)
        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null)
        {
            Debug.LogError("[InventoryGridUI] ❌ EquipmentManager.Instance is NULL!");
            return;
        }
        
        if (!equipmentManager.CanItemBeEquippedInSlot(item, slotType))
        {
            Debug.LogWarning($"[InventoryGridUI] ❌ Item type mismatch: {item.equipmentType} cannot go in {slotType} slot!");
            return;
        }
        
        Debug.Log($"[InventoryGridUI] Calling EquipmentManager.EquipItem({item.itemName}, {slotType})...");
        bool success = equipmentManager.EquipItem(item, slotType);
        
        if (!success)
        {
            Debug.LogError($"[InventoryGridUI] ❌ EquipmentManager.EquipItem() returned FALSE for {item.itemName} to {slotType}!");
            return;
        }
        
        Debug.Log($"[InventoryGridUI] ✅ EquipmentManager.EquipItem() returned SUCCESS for {item.itemName} to {slotType}");
        
        // Remove from inventory
        if (inventoryIndex >= 0 && inventoryIndex < boundCharacterManager.inventoryItems.Count)
        {
            boundCharacterManager.inventoryItems.RemoveAt(inventoryIndex);
            Debug.Log($"[InventoryGridUI] Removed item from inventory at index {inventoryIndex}");
        }
        
        // Refresh inventory display (should no longer show the item)
        RefreshFromCharacterInventory();
        
        // NOW update the equipment slot visual
        Debug.Log($"[InventoryGridUI] Updating equipment slot visual for {slotType}...");
        
        // Get the item from EquipmentManager (it should be equipped now)
        BaseItem equippedFromManager = equipmentManager.GetEquippedItem(slotType);
        Debug.Log($"[InventoryGridUI] EquipmentManager.GetEquippedItem({slotType}) = {(equippedFromManager != null ? equippedFromManager.itemName : "NULL")}");
        
        // Set it on the slot
        targetSlot.SetEquippedItem(equippedFromManager);
        
        Debug.Log($"[InventoryGridUI] ✅ DRAG-EQUIP COMPLETE!");
    }
}


