# Assigning Grid Prefabs to Components

## Overview

This guide shows you exactly where to assign each grid prefab in the EquipmentScreen scene.

## Step-by-Step Assignment

### 1. Open EquipmentScreen Scene

1. **Open Unity Editor**
2. **Open Scene**: `Assets/Scenes/EquipmentScreen.unity`
3. **Select the root EquipmentScreen GameObject** (or find the specific grid components)

---

## Component Locations

### InventoryGridUI Component

**Location**: Find the GameObject with `InventoryGridUI` component (usually named "InventoryGrid" or similar)

**Fields to Set**:
- ✅ **Grid Prefab**: Drag `InventoryGridPrefab_12x4.prefab` here
- ❌ **Slot Prefab**: Leave empty (not needed when using grid prefab)
- **Grid Width**: `12` (should match prefab)
- **Grid Height**: `4` (should match prefab)

**How to Find**:
1. In Hierarchy, search for "Inventory" or "InventoryGrid"
2. Select the GameObject
3. In Inspector, find `InventoryGridUI` component
4. Assign prefab to **Grid Prefab** field

---

### StashGridUI Component

**Location**: Find the GameObject with `InventoryGridUI` component set to `GlobalStash` mode (usually named "StashGrid" or similar)

**Fields to Set**:
- ✅ **Grid Prefab**: Drag `StashGridPrefab_14x4.prefab` here
- ❌ **Slot Prefab**: Leave empty (not needed when using grid prefab)
- **Grid Width**: `14` (should match prefab)
- **Grid Height**: `4` (should match prefab)
- **Grid Mode**: Should be `GlobalStash`

**How to Find**:
1. In Hierarchy, search for "Stash" or "StashGrid"
2. Select the GameObject
3. In Inspector, find `InventoryGridUI` component
4. Verify **Grid Mode** is `GlobalStash`
5. Assign prefab to **Grid Prefab** field

---

### EffigyGridUI Component

**Location**: Find the GameObject with `EffigyGridUI` component (usually named "EffigyGrid" or similar)

**Purpose**: This is the **main effigy placement grid** where effigies are equipped/placed.

**Fields to Set**:
- ✅ **Grid Prefab**: Drag `EffigyGridPrefab_6x20.prefab` here
- ❌ **Cell Prefab**: Leave empty (not needed when using grid prefab)

**Note**: EffigyGridUI uses constants for grid size (GRID_WIDTH = 6, GRID_HEIGHT = 20), so you don't need to set width/height manually.

**How to Find**:
1. In Hierarchy, search for "Effigy" or "EffigyGrid"
2. Select the GameObject
3. In Inspector, find `EffigyGridUI` component
4. Look for **Grid Prefab** field in the "References" section
5. Assign prefab to **Grid Prefab** field

---

### EffigyStorageUI Component

**Location**: Find the GameObject with `EffigyStorageUI` component (usually named "EffigyStorage" or similar)

**Purpose**: This is the **storage grid for unequipped effigies** (different from EffigyGridUI).

**Fields to Set**:
- ✅ **Grid Prefab**: Drag `EffigyStoragePrefab_6x20.prefab` here
- ❌ **Cell Prefab**: Leave empty (not needed when using grid prefab)
- **Grid Columns**: `6` (should match prefab)
- **Grid Rows**: `20` (should match prefab)

**How to Find**:
1. In Hierarchy, search for "EffigyStorage" or "EffigyStorageGrid"
2. Select the GameObject
3. In Inspector, find `EffigyStorageUI` component
4. Look for **Grid Prefab** field in the "References" section
5. Assign prefab to **Grid Prefab** field

**Note**: EffigyStorageUI and EffigyGridUI are **two different components**:
- **EffigyGridUI**: Where effigies are placed/equipped (main grid)
- **EffigyStorageUI**: Where unequipped effigies are stored (storage grid)

---

### AuraStorageUI Component

**Location**: Find the GameObject with `AuraStorageUI` component (usually named "AuraStorage" or "AuraGrid")

**Fields to Set**:
- ✅ **Grid Prefab**: Drag `AuraGridPrefab_4x10.prefab` here
- ❌ **Slot Prefab**: Leave empty (not needed when using grid prefab)
- **Grid Columns**: `4` (should match prefab)
- **Grid Rows**: `10` (should match prefab)

**How to Find**:
1. In Hierarchy, search for "Aura" or "AuraStorage"
2. Select the GameObject
3. In Inspector, find `AuraStorageUI` component
4. Assign prefab to **Grid Prefab** field

---

## Troubleshooting: EffigyGridUI Grid Prefab Field

If you can't find the **Grid Prefab** field in EffigyGridUI:

### Option 1: Check Field Name
The field might be named slightly differently. Look for:
- `Grid Prefab`
- `gridPrefab`
- `Complete Grid Prefab`
- Any field in the "References" section

### Option 2: Verify Component is Updated
1. Check that `EffigyGridUI.cs` has the `gridPrefab` field
2. The field should be in the `[Header("References")]` section
3. It should be marked with `[SerializeField] private GameObject gridPrefab;`
    
### Option 3: Manual Check
1. Select the EffigyGrid GameObject
2. In Inspector, expand the `EffigyGridUI` component
3. Look in the "References" section
4. You should see:
   - `Cell Prefab` (for individual cells)
   - `Grid Prefab` (for complete grid) ← This is what you need
   - `Grid Container`

### Option 4: Reimport Script
If the field still doesn't appear:
1. Right-click `EffigyGridUI.cs` in Project window
2. Select "Reimport"
3. Check Inspector again

---

## Verification Checklist

After assigning all prefabs:

- [ ] InventoryGridUI has `InventoryGridPrefab_12x4.prefab` assigned
- [ ] StashGridUI (InventoryGridUI with GlobalStash mode) has `StashGridPrefab_14x4.prefab` assigned
- [ ] EffigyGridUI has `EffigyGridPrefab_6x20.prefab` assigned
- [ ] EffigyStorageUI has `EffigyStoragePrefab_6x20.prefab` assigned
- [ ] AuraStorageUI has `AuraGridPrefab_4x10.prefab` assigned
- [ ] All Slot Prefab/Cell Prefab fields are empty (not needed)
- [ ] Grid sizes match prefab sizes

---

## Testing

After assignment:

1. **Save the scene**
2. **Play the game**
3. **Load EquipmentScreen**
4. **Check Console** for log messages:
   - Should see: `"Loaded X slots from prefab grid - FAST PATH"`
   - Should NOT see: `"Generated X slots progressively - SLOW PATH"`

If you see "FAST PATH" messages, the prefabs are working correctly!

---

## Common Issues

### Prefab Not Loading?
- Check prefab is in correct location
- Verify prefab has correct number of slots
- Check console for errors

### Still Using Slow Path?
- Verify Grid Prefab field is assigned (not Slot Prefab)
- Check that prefab has correct component types
- Ensure prefab slots are direct children of grid root

### Wrong Grid Size?
- Update Grid Width/Height in component to match prefab
- Or recreate prefab with correct size

---

## Quick Reference

| Component | Prefab Field | Prefab Name | Grid Size | Purpose |
|-----------|--------------|-------------|-----------|---------|
| InventoryGridUI | Grid Prefab | InventoryGridPrefab_12x4 | 12×4 | Player inventory |
| InventoryGridUI (Stash) | Grid Prefab | StashGridPrefab_14x4 | 14×4 | Global stash |
| EffigyGridUI | Grid Prefab | EffigyGridPrefab_6x20 | 6×20 | Main effigy placement grid |
| EffigyStorageUI | Grid Prefab | EffigyStoragePrefab_6x20 | 6×20 | Storage for unequipped effigies |
| AuraStorageUI | Grid Prefab | AuraGridPrefab_4x10 | 4×10 | Aura storage |

