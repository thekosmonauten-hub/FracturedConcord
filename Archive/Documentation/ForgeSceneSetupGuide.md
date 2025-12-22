# Forge Scene Setup Guide

## Overview
This guide will help you set up the Forge Scene with access to Inventory and Stash, similar to the EquipmentScreen.

## Prerequisites
- Forge core systems are already implemented (ForgeSalvageSystem, ForgeCraftingSystem, etc.)
- EquipmentScreen scene exists as reference
- InventoryGridUI prefab/component available
- StashManager singleton exists

---

## Step 1: Create Forge Scene

1. **Create New Scene**: `Assets/Scenes/ForgeScene.unity`
2. **Add Canvas**: Create a Canvas (UI > Canvas)
3. **Set Canvas Settings**:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

---

## Step 2: Add Core Managers

### 2.1 Create Empty GameObject: "Managers"
Add these components/singletons:

1. **CharacterManager** (if not already in scene)
   - Should be a singleton that persists across scenes
   - Or use `CharacterManager.Instance` if it's a persistent singleton

2. **StashManager** (if not already in scene)
   - Should be a singleton: `StashManager.Instance`
   - If it doesn't exist, create it:
     ```csharp
     // Add StashManager component to Managers GameObject
     // Ensure it has Instance singleton pattern
     ```

3. **ForgeManager** (new - create this)
   - Create empty GameObject: "ForgeManager"
   - Add component: `ForgeManager.cs` (see Step 3)

---

## Step 3: Create ForgeManager Script

Create `Assets/Scripts/Forge/ForgeManager.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Main manager for Forge Scene
/// Handles UI initialization and scene setup
/// </summary>
public class ForgeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InventoryGridUI inventoryGrid;
    [SerializeField] private InventoryGridUI stashGrid;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject stashPanel;
    
    [Header("Buttons")]
    [SerializeField] private UnityEngine.UI.Button toggleInventoryButton;
    [SerializeField] private UnityEngine.UI.Button toggleStashButton;
    [SerializeField] private UnityEngine.UI.Button returnToTownButton;
    
    private void Start()
    {
        InitializeGrids();
        SetupButtons();
    }
    
    private void InitializeGrids()
    {
        // Configure Inventory Grid
        if (inventoryGrid != null)
        {
            inventoryGrid.gridMode = InventoryGridUI.GridMode.CharacterInventory;
            Debug.Log("[ForgeManager] Configured inventoryGrid as CharacterInventory");
        }
        
        // Configure Stash Grid
        if (stashGrid != null)
        {
            stashGrid.gridMode = InventoryGridUI.GridMode.GlobalStash;
            Debug.Log("[ForgeManager] Configured stashGrid as GlobalStash");
        }
    }
    
    private void SetupButtons()
    {
        if (toggleInventoryButton != null)
        {
            toggleInventoryButton.onClick.AddListener(ToggleInventory);
        }
        
        if (toggleStashButton != null)
        {
            toggleStashButton.onClick.AddListener(ToggleStash);
        }
        
        if (returnToTownButton != null)
        {
            returnToTownButton.onClick.AddListener(ReturnToTown);
        }
    }
    
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
    
    public void ToggleStash()
    {
        if (stashPanel != null)
        {
            stashPanel.SetActive(!stashPanel.activeSelf);
        }
    }
    
    private void ReturnToTown()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameUI");
    }
}
```

---

## Step 4: UI Layout Setup

### 4.1 Main Container Structure

Create this hierarchy under Canvas:

```
Canvas
├── ForgeMainPanel (Panel/Image)
│   ├── Header
│   │   ├── Title (Text: "FORGE")
│   │   └── ReturnButton (Button: "Return to Town")
│   │
│   ├── ContentArea (Horizontal Layout Group)
│   │   ├── LeftPanel (Panel)
│   │   │   ├── SalvageSection
│   │   │   └── CraftingSection
│   │   │
│   │   └── RightPanel (Panel)
│   │       ├── InventorySection
│   │       └── StashSection
│   │
│   └── MaterialDisplay (Panel)
│       └── MaterialGrid (Grid Layout Group)
```

### 4.2 Single Grid with Mode Toggle (Recommended)

**Create Panel**: `ItemStorageSection`
- Add **ONE** `InventoryGridUI` component
- Add a **Toggle** component (UI > Toggle)
- Label the Toggle: "Show Stash" (or similar)

**Setup Toggle to Switch Grid Mode**:
1. Select the Toggle GameObject
2. In Inspector, find **"On Value Changed"** event
3. Click **"+"** to add a listener
4. Drag the GameObject with `InventoryGridUI` component into the object field
5. In the dropdown, select: **`InventoryGridUI` > `SetGridModeFromBool(bool)`**
6. The Toggle's boolean value will automatically be passed:
   - **Checked (true)** = GlobalStash
   - **Unchecked (false)** = CharacterInventory

**Alternative: Using Dropdown/Enum**:
- If you prefer a dropdown instead of toggle:
  - Use **`SetGridModeFromInt(int)`** method
  - 0 = CharacterInventory, 1 = GlobalStash

### 4.3 Separate Inventory and Stash Sections (Alternative)

If you want separate panels:

**Create Panel**: `InventorySection`
- Add `InventoryGridUI` component
- Set `gridMode` to `CharacterInventory` (or leave default, ForgeManager will set it)
- Add `InventoryGridUI` reference to ForgeManager

**Create Panel**: `StashSection`
- Add `InventoryGridUI` component
- Set `gridMode` to `GlobalStash` (or leave default, ForgeManager will set it)
- Add `InventoryGridUI` reference to ForgeManager

---

## Step 5: Configure InventoryGridUI

### Option A: Single Grid with Toggle (Recommended)

1. Select the `InventoryGridUI` GameObject
2. **Component Settings**:
   - `Grid Width`: 10
   - `Grid Height`: 6
   - `Cell Size`: (60, 60)
   - `Spacing`: (2, 2)
   - `Grid Mode`: CharacterInventory (default - will be changed by toggle)
   - `Slot Prefab`: Assign your inventory slot prefab
   - `Grid Container`: Assign the container Transform

3. **Setup Toggle**:
   - Create a Toggle (UI > Toggle)
   - In Toggle's **"On Value Changed"** event:
     - Drag `InventoryGridUI` GameObject
     - Select: `InventoryGridUI.SetGridModeFromBool(bool)`
   - Toggle **checked** = Shows Stash
   - Toggle **unchecked** = Shows Inventory

### Option B: Separate Grids

**For Inventory Grid**:
1. Select the Inventory `InventoryGridUI` GameObject
2. **Component Settings**:
   - `Grid Width`: 10
   - `Grid Height`: 6
   - `Cell Size`: (60, 60)
   - `Spacing`: (2, 2)
   - `Grid Mode`: CharacterInventory
   - `Slot Prefab`: Assign your inventory slot prefab
   - `Grid Container`: Assign the container Transform

**For Stash Grid**:
1. Select the Stash `InventoryGridUI` GameObject
2. **Component Settings**:
   - `Grid Width`: 10
   - `Grid Height`: 6
   - `Cell Size`: (60, 60)
   - `Spacing`: (2, 2)
   - `Grid Mode`: GlobalStash
   - `Slot Prefab`: Assign your inventory slot prefab
   - `Grid Container`: Assign the container Transform

---

## Step 6: Wire Up ForgeManager

1. Select the **ForgeManager** GameObject
2. In Inspector, assign references:
   - **Inventory Grid**: Drag Inventory `InventoryGridUI` component
   - **Stash Grid**: Drag Stash `InventoryGridUI` component
   - **Inventory Panel**: Drag InventorySection panel
   - **Stash Panel**: Drag StashSection panel
   - **Toggle Inventory Button**: (if you have one)
   - **Toggle Stash Button**: (if you have one)
   - **Return To Town Button**: Drag your return button

---

## Step 7: Verify Managers Exist

### Check CharacterManager:
```csharp
// In ForgeManager.Start() or ForgeScene initialization:
if (CharacterManager.Instance == null)
{
    Debug.LogError("[ForgeManager] CharacterManager.Instance is null!");
}
else
{
    Debug.Log($"[ForgeManager] CharacterManager found. Character: {CharacterManager.Instance.GetCurrentCharacter()?.characterName}");
}
```

### Check StashManager:
```csharp
// In ForgeManager.Start() or ForgeScene initialization:
if (StashManager.Instance == null)
{
    Debug.LogError("[ForgeManager] StashManager.Instance is null!");
}
else
{
    Debug.Log("[ForgeManager] StashManager found.");
}
```

---

## Step 8: Access Inventory/Stash from Forge Code

### Accessing Character Inventory:
```csharp
// Get items from character inventory
var character = CharacterManager.Instance?.GetCurrentCharacter();
if (character != null)
{
    var inventoryItems = CharacterManager.Instance.inventoryItems;
    // Use items for salvaging
    foreach (var item in inventoryItems)
    {
        // Salvage item
        ForgeSalvageSystem.SalvageItem(item, character);
    }
}
```

### Accessing Stash:
```csharp
// Get items from stash
if (StashManager.Instance != null)
{
    var stashItems = StashManager.Instance.stashItems;
    // Use items for salvaging
    foreach (var item in stashItems)
    {
        if (item != null)
        {
            var character = CharacterManager.Instance?.GetCurrentCharacter();
            if (character != null)
            {
                // Convert ItemData to BaseItem if needed
                BaseItem baseItem = item.sourceItem;
                if (baseItem != null)
                {
                    ForgeSalvageSystem.SalvageItem(baseItem, character);
                    // Remove from stash after salvaging
                    StashManager.Instance.RemoveItem(item);
                }
            }
        }
    }
}
```

---

## Step 9: Quick Reference - Component Checklist

- [ ] Canvas created
- [ ] ForgeManager GameObject with ForgeManager script
- [ ] CharacterManager accessible (singleton or in scene)
- [ ] StashManager accessible (singleton or in scene)
- [ ] Inventory Panel with InventoryGridUI component
- [ ] Stash Panel with InventoryGridUI component
- [ ] Grid Mode set correctly (CharacterInventory / GlobalStash)
- [ ] Slot Prefab assigned to both grids
- [ ] Grid Container assigned to both grids
- [ ] ForgeManager references wired up
- [ ] Return to Town button wired

---

## Step 10: Testing

1. **Load Forge Scene** in Play Mode
2. **Check Console** for initialization messages
3. **Verify Grids Load**:
   - Inventory should show character's items
   - Stash should show stash items
4. **Test Drag & Drop**:
   - Drag items from inventory to salvage area
   - Drag items from stash to salvage area
5. **Test Material Display**:
   - After salvaging, materials should appear in MaterialDisplay

---

## Troubleshooting

### Inventory Not Showing:
- Check `CharacterManager.Instance` is not null
- Verify `inventoryGrid.gridMode == CharacterInventory`
- Check `CharacterManager.Instance.inventoryItems` has items
- Ensure `InventoryGridUI` has `slotPrefab` assigned

### Stash Not Showing:
- Check `StashManager.Instance` is not null
- Verify `stashGrid.gridMode == GlobalStash`
- Check `StashManager.Instance.stashItems` has items
- Ensure `InventoryGridUI` has `slotPrefab` assigned

### Grids Not Updating:
- Check `refreshOnEnable` is true on `InventoryGridUI`
- Verify event subscriptions in `OnEnable()`
- Manually call `RefreshFromDataSource()` if needed

---

## Example: Accessing Items for Salvage

```csharp
// In your salvage button handler:
public void OnSalvageButtonClicked()
{
    var character = CharacterManager.Instance?.GetCurrentCharacter();
    if (character == null) return;
    
    // Get selected item from inventory or stash
    // (You'll need to implement item selection logic)
    BaseItem itemToSalvage = GetSelectedItem();
    
    if (itemToSalvage != null)
    {
        bool success = ForgeSalvageSystem.SalvageItem(itemToSalvage, character);
        if (success)
        {
            // Remove from inventory/stash
            RemoveItemFromSource(itemToSalvage);
            
            // Refresh grids
            if (inventoryGrid != null) inventoryGrid.RefreshFromDataSource();
            if (stashGrid != null) stashGrid.RefreshFromDataSource();
            
            // Update material display
            UpdateMaterialDisplay();
        }
    }
}
```

---

## Notes

- Both Inventory and Stash use the same `InventoryGridUI` component
- The `GridMode` enum determines which data source is used
- Grids automatically bind to managers based on `GridMode`
- Items can be dragged between inventory and stash (if you implement drag handlers)
- Materials are stored in `Character.forgeMaterials` and persist with saves

