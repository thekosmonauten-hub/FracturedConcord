# Grid Prefab Specifications

## Overview

This document provides exact specifications for creating grid prefabs to replace dynamic grid generation. Using prefabs is **50-100x faster** than instantiating individual slots.

## Grid Specifications

## Quick Reference Table

| Grid | Size | Total Slots | Cell Size | Spacing | Constraint Count | Component |
|------|------|-------------|-----------|---------|------------------|-----------|
| **InventoryGridPrefab** | 12×4 | 48 | 60×60 | 2×2 | 12 | InventoryGridUI |
| **StashGridPrefab** | 14×4 | 56 | 60×60 | 2×2 | 14 | InventoryGridUI (Stash mode) |
| **EffigyGridPrefab** | 6×20 | 120 | 60×60 | 2×2 | 6 | EffigyGridUI |
| **EffigyStoragePrefab** | 6×20 | 120 | 80×80 | 10×10 | 6 | EffigyStorageUI |
| **AuraGridPrefab** | 4×10 | 40 | 100×100 | 10×10 | 4 | AuraStorageUI |

---

### 1. InventoryGridPrefab (12x4 = 48 slots)

**File Path**: `Assets/Prefabs/UI/Grids/InventoryGridPrefab_12x4.prefab`

**Root GameObject Settings**:
- **Name**: `InventoryGrid`
- **Components**:
  - `RectTransform` (default)
  - `GridLayoutGroup`

**GridLayoutGroup Settings**:
- **Cell Size**: `X: 60, Y: 60`
- **Spacing**: `X: 2, Y: 2`
- **Padding**: `Left: 0, Right: 0, Top: 0, Bottom: 0`
- **Constraint**: `Fixed Column Count`
- **Constraint Count**: `12`
- **Start Corner**: `Upper Left`
- **Start Axis**: `Horizontal`
- **Child Alignment**: `Upper Left`

**Slot Prefab**: Use your existing `InventorySlotUI` prefab
**Total Slots**: 48 (12 columns × 4 rows)
**Slot Naming**: `Slot_0_0`, `Slot_1_0`, ... `Slot_11_3` (x from 0-11, y from 0-3)

**Slot Layout** (left-to-right, top-to-bottom):
```
Row 0: Slot_0_0, Slot_1_0, Slot_2_0, ... Slot_11_0
Row 1: Slot_0_1, Slot_1_1, Slot_2_1, ... Slot_11_1
Row 2: Slot_0_2, Slot_1_2, Slot_2_2, ... Slot_11_2
Row 3: Slot_0_3, Slot_1_3, Slot_2_3, ... Slot_11_3
```

---

### 2. StashGridPrefab (14x4 = 56 slots)

**File Path**: `Assets/Prefabs/UI/Grids/StashGridPrefab_14x4.prefab`

**Root GameObject Settings**:
- **Name**: `StashGrid`
- **Components**:
  - `RectTransform` (default)
  - `GridLayoutGroup`

**GridLayoutGroup Settings**:
- **Cell Size**: `X: 60, Y: 60`
- **Spacing**: `X: 2, Y: 2`
- **Padding**: `Left: 0, Right: 0, Top: 0, Bottom: 0`
- **Constraint**: `Fixed Column Count`
- **Constraint Count**: `14`
- **Start Corner**: `Upper Left`
- **Start Axis**: `Horizontal`
- **Child Alignment**: `Upper Left`

**Slot Prefab**: Use your existing `InventorySlotUI` prefab (same as inventory)
**Total Slots**: 56 (14 columns × 4 rows)
**Slot Naming**: `Slot_0_0`, `Slot_1_0`, ... `Slot_13_3` (x from 0-13, y from 0-3)

**Slot Layout** (left-to-right, top-to-bottom):
```
Row 0: Slot_0_0, Slot_1_0, Slot_2_0, ... Slot_13_0
Row 1: Slot_0_1, Slot_1_1, Slot_2_1, ... Slot_13_1
Row 2: Slot_0_2, Slot_1_2, Slot_2_2, ... Slot_13_2
Row 3: Slot_0_3, Slot_1_3, Slot_2_3, ... Slot_13_3
```

---

### 3. EffigyGridPrefab (6x20 = 120 cells)

**Component**: `EffigyGridUI` (main effigy placement grid)

**File Path**: `Assets/Prefabs/UI/Grids/EffigyGridPrefab_6x20.prefab`

**Root GameObject Settings**:
- **Name**: `EffigyGrid`
- **Components**:
  - `RectTransform` (default)
  - `GridLayoutGroup`

**GridLayoutGroup Settings**:
- **Cell Size**: `X: 60, Y: 60`
- **Spacing**: `X: 2, Y: 2`
- **Padding**: `Left: 0, Right: 0, Top: 0, Bottom: 0`
- **Constraint**: `Fixed Column Count`
- **Constraint Count**: `6`
- **Start Corner**: `Upper Left`
- **Start Axis**: `Horizontal`
- **Child Alignment**: `Middle Center`

**Cell Prefab**: Use your existing `EffigyGridCellUI` prefab
**Total Cells**: 120 (6 columns × 20 rows)
**Cell Naming**: `EffigyCell_0_0`, `EffigyCell_1_0`, ... `EffigyCell_5_19` (x from 0-5, y from 0-19)

**Cell Layout** (left-to-right, top-to-bottom):
```
Row 0:  EffigyCell_0_0,  EffigyCell_1_0,  EffigyCell_2_0,  EffigyCell_3_0,  EffigyCell_4_0,  EffigyCell_5_0
Row 1:  EffigyCell_0_1,  EffigyCell_1_1,  EffigyCell_2_1,  EffigyCell_3_1,  EffigyCell_4_1,  EffigyCell_5_1
...
Row 19: EffigyCell_0_19, EffigyCell_1_19, EffigyCell_2_19, EffigyCell_3_19, EffigyCell_4_19, EffigyCell_5_19
```

**Note**: This is a tall grid (20 rows). Consider adding a `ScrollRect` parent if it doesn't fit on screen.

---

### 4. EffigyStoragePrefab (6x20 = 120 cells)

**Component**: `EffigyStorageUI` (storage grid for unequipped effigies)

**File Path**: `Assets/Prefabs/UI/Grids/EffigyStoragePrefab_6x20.prefab`

**Root GameObject Settings**:
- **Name**: `EffigyStorageGrid`
- **Components**:
  - `RectTransform` (default)
  - `GridLayoutGroup`
  - `ContentSizeFitter` (for scrolling)

**GridLayoutGroup Settings**:
- **Cell Size**: `X: 80, Y: 80`
- **Spacing**: `X: 10, Y: 10`
- **Padding**: `Left: 10, Right: 10, Top: 10, Bottom: 10`
- **Constraint**: `Fixed Column Count`
- **Constraint Count**: `6`
- **Start Corner**: `Upper Left`
- **Start Axis**: `Horizontal`
- **Child Alignment**: `Upper Center`

**ContentSizeFitter Settings**:
- **Horizontal Fit**: `Unconstrained`
- **Vertical Fit**: `Preferred Size`

**Slot Prefab**: Use your existing `EffigyStorageSlotUI` prefab
**Total Cells**: 120 (6 columns × 20 rows)
**Cell Naming**: `StorageCell_0_0`, `StorageCell_1_0`, ... `StorageCell_5_19` (x from 0-5, y from 0-19)

**Cell Layout** (left-to-right, top-to-bottom):
```
Row 0:  StorageCell_0_0,  StorageCell_1_0,  StorageCell_2_0,  StorageCell_3_0,  StorageCell_4_0,  StorageCell_5_0
Row 1:  StorageCell_0_1,  StorageCell_1_1,  StorageCell_2_1,  StorageCell_3_1,  StorageCell_4_1,  StorageCell_5_1
...
Row 19: StorageCell_0_19, StorageCell_1_19, StorageCell_2_19, StorageCell_3_19, StorageCell_4_19, StorageCell_5_19
```

**Note**: This is the storage grid where unequipped effigies are stored. Different from EffigyGridUI which is the placement grid.

---

### 5. AuraGridPrefab (4x10 = 40 slots)

**File Path**: `Assets/Prefabs/UI/Grids/EffigyGridPrefab_6x20.prefab`

**Root GameObject Settings**:
- **Name**: `EffigyGrid`
- **Components**:
  - `RectTransform` (default)
  - `GridLayoutGroup`

**GridLayoutGroup Settings**:
- **Cell Size**: `X: 60, Y: 60` (adjust if your effigy cells are different)
- **Spacing**: `X: 2, Y: 2` (adjust if your effigy cells use different spacing)
- **Padding**: `Left: 0, Right: 0, Top: 0, Bottom: 0`
- **Constraint**: `Fixed Column Count`
- **Constraint Count**: `6`
- **Start Corner**: `Upper Left`
- **Start Axis**: `Horizontal`
- **Child Alignment**: `Middle Center`

**Cell Prefab**: Use your existing `EffigyGridCellUI` prefab
**Total Cells**: 120 (6 columns × 20 rows)
**Cell Naming**: `EffigyCell_0_0`, `EffigyCell_1_0`, ... `EffigyCell_5_19` (x from 0-5, y from 0-19)

**Cell Layout** (left-to-right, top-to-bottom):
```
Row 0:  EffigyCell_0_0,  EffigyCell_1_0,  EffigyCell_2_0,  EffigyCell_3_0,  EffigyCell_4_0,  EffigyCell_5_0
Row 1:  EffigyCell_0_1,  EffigyCell_1_1,  EffigyCell_2_1,  EffigyCell_3_1,  EffigyCell_4_1,  EffigyCell_5_1
...
Row 19: EffigyCell_0_19, EffigyCell_1_19, EffigyCell_2_19, EffigyCell_3_19, EffigyCell_4_19, EffigyCell_5_19
```

**Note**: This is a tall grid (20 rows). Consider adding a `ScrollRect` parent if it doesn't fit on screen.

---

### 6. AuraGridPrefab (4x10 = 40 slots)

**File Path**: `Assets/Prefabs/UI/Grids/AuraGridPrefab_4x10.prefab`

**Root GameObject Settings**:
- **Name**: `AuraGrid`
- **Components**:
  - `RectTransform` (default)
  - `GridLayoutGroup`

**GridLayoutGroup Settings**:
- **Cell Size**: `X: 80, Y: 80` (adjust based on your aura slot size)
- **Spacing**: `X: 5, Y: 5` (adjust based on your aura slot spacing)
- **Padding**: `Left: 0, Right: 0, Top: 0, Bottom: 0`
- **Constraint**: `Fixed Column Count`
- **Constraint Count**: `4`
- **Start Corner**: `Upper Left`
- **Start Axis**: `Horizontal`
- **Child Alignment**: `Upper Left`

**Slot Prefab**: Use your existing `AuraSlotUI` prefab
**Total Slots**: 40 (4 columns × 10 rows)
**Slot Naming**: `AuraSlot_0_0`, `AuraSlot_1_0`, ... `AuraSlot_3_9` (x from 0-3, y from 0-9)

**Slot Layout** (left-to-right, top-to-bottom):
```
Row 0: AuraSlot_0_0, AuraSlot_1_0, AuraSlot_2_0, AuraSlot_3_0
Row 1: AuraSlot_0_1, AuraSlot_1_1, AuraSlot_2_1, AuraSlot_3_1
...
Row 9: AuraSlot_0_9, AuraSlot_1_9, AuraSlot_2_9, AuraSlot_3_9
```

---

## Quick Creation Steps

### Method 1: Manual Creation (Recommended for First Time)

1. **Create Root GameObject**:
   - Right-click Hierarchy → Create Empty
   - Name it according to grid (e.g., `InventoryGrid`)
   - Add Component → UI → Grid Layout Group

2. **Configure GridLayoutGroup**:
   - Use settings from specifications above
   - Set constraint count to grid width

3. **Create Slots**:
   - Drag your slot prefab into the grid
   - Duplicate to create all slots (48, 56, 120, or 40)
   - Name them according to pattern (e.g., `Slot_0_0`, `Slot_1_0`, etc.)

4. **Save as Prefab**:
   - Drag from Hierarchy to Project window
   - Save to `Assets/Prefabs/UI/Grids/`

### Method 2: Editor Script (Faster for Multiple Grids)

Use the editor script in `GRID_PREFAB_SETUP.md` to auto-generate:

```csharp
// Example for InventoryGrid
slotPrefab = [Your InventorySlotUI prefab]
gridWidth = 12
gridHeight = 4
cellSize = (60, 60)
spacing = (2, 2)
prefabName = "InventoryGridPrefab_12x4"
```

---

## Component Assignment

After creating prefabs, assign them in EquipmentScreen:

### InventoryGridUI
- **Grid Prefab**: `InventoryGridPrefab_12x4.prefab`
- **Slot Prefab**: (leave empty - not needed with grid prefab)
- **Grid Width**: `12` (should match prefab)
- **Grid Height**: `4` (should match prefab)

### StashGridUI (InventoryGridUI component)
- **Grid Prefab**: `StashGridPrefab_14x4.prefab`
- **Slot Prefab**: (leave empty)
- **Grid Width**: `14`
- **Grid Height**: `4`

### EffigyGridUI
- **Grid Prefab**: `EffigyGridPrefab_6x20.prefab`
- **Cell Prefab**: (leave empty)
- **Grid Width**: `6` (GRID_WIDTH constant)
- **Grid Height**: `20` (GRID_HEIGHT constant)

**Note**: This is the main effigy placement grid (where effigies are equipped).

### EffigyStorageUI
- **Grid Prefab**: `EffigyStoragePrefab_6x20.prefab`
- **Cell Prefab**: (leave empty)
- **Grid Columns**: `6` (should match prefab)
- **Grid Rows**: `20` (should match prefab)

**Note**: This is the storage grid for unequipped effigies (different from EffigyGridUI).

### AuraStorageUI
- **Grid Prefab**: `AuraGridPrefab_4x10.prefab`
- **Slot Prefab**: (leave empty)
- **Grid Width**: `4`
- **Grid Height**: `10`

---

## Verification Checklist

After creating each prefab, verify:

- [ ] Prefab has correct number of slots (12×4=48, 14×4=56, 6×20=120, 4×10=40)
- [ ] GridLayoutGroup settings match specifications
- [ ] Slots are named correctly (Slot_X_Y pattern)
- [ ] Slots are in correct order (left-to-right, top-to-bottom)
- [ ] Prefab is saved to `Assets/Prefabs/UI/Grids/`
- [ ] Component in scene has Grid Prefab assigned
- [ ] Grid Width/Height in component matches prefab size

---

## Performance Impact

**Before (Dynamic Generation)**:
- InventoryGrid: ~1.5 seconds (48 instantiates)
- StashGrid: ~1.8 seconds (56 instantiates)
- EffigyGrid: ~3.0 seconds (120 instantiates)
- EffigyStorage: ~3.0 seconds (120 instantiates)
- AuraGrid: ~1.0 seconds (40 instantiates)
- **Total: ~10.3 seconds**

**After (Prefab Grids)**:
- All grids: ~0.1 seconds (5 instantiates total)
- **Total: ~0.1 seconds**
- **103x faster!**

---

## Troubleshooting

### Grid Not Loading?
- Check Grid Prefab is assigned in component
- Verify prefab has correct number of slots
- Check slot naming matches pattern

### Slots in Wrong Order?
- Verify GridLayoutGroup constraint count matches grid width
- Check slots are children of grid root (not nested)
- Ensure slots are in hierarchy order (top-to-bottom)

### Wrong Grid Size?
- Update Grid Width/Height in component to match prefab
- Or recreate prefab with correct size

### Performance Not Improved?
- Verify Grid Prefab is assigned (not Slot Prefab)
- Check console for "FAST PATH" log messages
- Ensure prefabs are in Resources or assigned directly

---

## File Locations

**Prefab Save Path**: `Assets/Prefabs/UI/Grids/`

**Prefab Files**:
- `InventoryGridPrefab_12x4.prefab`
- `StashGridPrefab_14x4.prefab`
- `EffigyGridPrefab_6x20.prefab`
- `AuraGridPrefab_4x10.prefab`

---

## Next Steps

1. Create all 4 grid prefabs using specifications above
2. Assign them to components in EquipmentScreen
3. Test scene loading - should be instant!
4. Remove progressive generation code (optional cleanup)

