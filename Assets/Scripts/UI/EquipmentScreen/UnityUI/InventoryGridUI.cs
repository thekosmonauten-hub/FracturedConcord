using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Unity UI version of inventory grid
/// Manages dynamic generation and layout of inventory slots
/// </summary>
public class InventoryGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 6;
    public Vector2 cellSize = new Vector2(60, 60);
    public Vector2 spacing = new Vector2(2, 2);
    
    [Header("References")]
    public GameObject slotPrefab;
    public Transform gridContainer;
    
    [Header("Character Inventory Binding")]
    [SerializeField] private bool bindToCharacterInventory = true;
    [SerializeField] private bool refreshOnEnable = true;
    
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private CharacterManager boundCharacterManager;
    
    void Start()
    {
        GenerateGrid();
        if (bindToCharacterInventory && refreshOnEnable)
        {
            RefreshFromCharacterInventory();
        }
    }

    private void OnEnable()
    {
        if (!bindToCharacterInventory)
            return;

        TryBindCharacterManager();
        if (boundCharacterManager != null)
        {
            boundCharacterManager.OnItemAdded += HandleCharacterItemAdded;
            if (refreshOnEnable)
            {
                RefreshFromCharacterInventory();
            }
        }
    }

    private void OnDisable()
    {
        if (boundCharacterManager != null)
        {
            boundCharacterManager.OnItemAdded -= HandleCharacterItemAdded;
        }
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
                    
                    slots.Add(slotUI);
                }
            }
        }
        
        Debug.Log($"[InventoryGridUI] Generated {slots.Count} slots ({gridWidth}x{gridHeight})");
    }
    
    void OnSlotClicked(int x, int y)
    {
        Debug.Log($"[InventoryGridUI] Slot clicked: ({x}, {y})");
        // TODO: Implement slot click behavior
    }
    
    void OnSlotHovered(int x, int y)
    {
        // TODO: Implement slot hover behavior (tooltips, etc.)
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

    public void RefreshFromCharacterInventory()
    {
        if (!bindToCharacterInventory)
            return;

        if (!TryBindCharacterManager())
        {
            Debug.LogWarning("[InventoryGridUI] CharacterManager not available; cannot refresh inventory grid.");
            return;
        }

        List<BaseItem> items = boundCharacterManager.inventoryItems;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotUI slot = slots[i];
            if (slot == null)
                continue;

            if (items != null && i < items.Count && items[i] != null)
            {
                BaseItem item = items[i];
                slot.SetOccupied(true, item.itemIcon, item.GetDisplayName());
            }
            else
            {
                slot.SetOccupied(false);
            }
        }
    }

    private bool TryBindCharacterManager()
    {
        if (boundCharacterManager != null)
            return true;

        boundCharacterManager = CharacterManager.Instance;
        return boundCharacterManager != null;
    }

    private void HandleCharacterItemAdded(BaseItem _)
    {
        RefreshFromCharacterInventory();
    }
}

