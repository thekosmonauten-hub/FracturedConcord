# InventoryGridUI Mode Toggle Setup

## Overview
`InventoryGridUI` now supports changing between `CharacterInventory` and `GlobalStash` modes at runtime using Unity's built-in event system.

## Available Methods

### 1. `SetGridMode(GridMode newMode)`
Direct method to set grid mode.

**Usage in Code**:
```csharp
inventoryGrid.SetGridMode(InventoryGridUI.GridMode.GlobalStash);
```

**Usage in Unity Events**:
- Drag `InventoryGridUI` GameObject
- Select: `InventoryGridUI > SetGridMode(GridMode)`
- Choose mode from dropdown

### 2. `SetGridModeFromBool(bool useStash)` ⭐ **Recommended for Toggles**
Perfect for Toggle components.

**Parameters**:
- `true` = GlobalStash
- `false` = CharacterInventory

**Usage in Unity Events**:
1. Add Toggle component (UI > Toggle)
2. In Toggle's **"On Value Changed"** event:
   - Click **"+"** to add listener
   - Drag `InventoryGridUI` GameObject
   - Select: `InventoryGridUI > SetGridModeFromBool(bool)`
3. The Toggle's boolean value is automatically passed

### 3. `SetGridModeFromInt(int modeIndex)`
Perfect for Dropdown components.

**Parameters**:
- `0` = CharacterInventory
- `1` = GlobalStash

**Usage in Unity Events**:
1. Add Dropdown component (UI > Dropdown)
2. In Dropdown's **"On Value Changed"** event:
   - Click **"+"** to add listener
   - Drag `InventoryGridUI` GameObject
   - Select: `InventoryGridUI > SetGridModeFromInt(int)`
3. The Dropdown's value (0 or 1) is automatically passed

## Step-by-Step: Toggle Setup

### Using a Toggle Component

1. **Create Toggle**:
   - Right-click in Hierarchy > UI > Toggle
   - Name it: "StashToggle" or "InventoryToggle"

2. **Configure Toggle**:
   - **Label Text**: "Show Stash" (or "Inventory / Stash")
   - **Is On**: false (default shows Inventory)

3. **Wire Up Event**:
   - Select the Toggle
   - In Inspector, scroll to **"On Value Changed"**
   - Click **"+"** button
   - Drag your `InventoryGridUI` GameObject into the object field
   - In the dropdown, select: **`InventoryGridUI` > `SetGridModeFromBool(bool)`**

4. **Test**:
   - Play the scene
   - Toggle ON = Shows Stash items
   - Toggle OFF = Shows Inventory items

## Visual Example

```
Toggle (Is On: false)
  └─ On Value Changed
      └─ InventoryGridUI.SetGridModeFromBool(bool)
          └─ Automatically passes: false = Inventory, true = Stash
```

## Benefits

✅ **No Code Required** - Pure Unity Editor setup  
✅ **Automatic Event Binding** - Grid subscribes/unsubscribes automatically  
✅ **Automatic Refresh** - Grid updates immediately when mode changes  
✅ **Works with Any Event** - Can be called from buttons, toggles, dropdowns, etc.

## Notes

- The grid automatically:
  - Unsubscribes from old data source events
  - Subscribes to new data source events
  - Refreshes the display
- Safe to call multiple times (checks if mode already matches)
- Works at runtime - no need to restart scene

