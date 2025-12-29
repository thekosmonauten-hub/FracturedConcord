# Grid Helper Script Usage Guide

## Overview

The `CreateGridPrefab` editor script makes it easy to create grid prefabs with all slots already set up. This is **much faster** than manually creating grids or using dynamic generation.

## How to Access

**Menu**: `FracturedConcord → Create Grid Prefab`

This opens a window where you can configure and create grid prefabs.

## Step-by-Step Usage

### Step 1: Open the Tool

1. In Unity Editor, go to: **FracturedConcord → Create Grid Prefab**
2. A window will open with all the configuration options

### Step 2: Use Quick Presets (Recommended)

The tool has **4 quick preset buttons** for the standard grids:

1. **Click "InventoryGrid (12×4)"** button
   - Automatically sets: 12×4, 60×60 cells, 2×2 spacing
   - Sets prefab name: `InventoryGridPrefab_12x4`

2. **Click "StashGrid (14×4)"** button
   - Automatically sets: 14×4, 60×60 cells, 2×2 spacing
   - Sets prefab name: `StashGridPrefab_14x4`

3. **Click "EffigyGrid (6×20)"** button
   - Automatically sets: 6×20, 60×60 cells, 2×2 spacing
   - Sets prefab name: `EffigyGridPrefab_6x20`

4. **Click "AuraGrid (4×10)"** button
   - Automatically sets: 4×10, 100×100 cells, 10×10 spacing
   - Sets prefab name: `AuraGridPrefab_4x10`

### Step 3: Assign Slot Prefab

1. **Drag your slot prefab** into the "Slot Prefab" field
   - For InventoryGrid: Use `InventorySlotUI` prefab
   - For StashGrid: Use `InventorySlotUI` prefab (same as inventory)
   - For EffigyGrid: Use `EffigyGridCellUI` prefab
   - For AuraGrid: Use `AuraSlotUI` prefab

### Step 4: Adjust Settings (Optional)

If you didn't use a preset, or want to customize:

- **Grid Width**: Number of columns
- **Grid Height**: Number of rows
- **Cell Size**: Size of each cell (X, Y)
- **Spacing**: Space between cells (X, Y)
- **Prefab Name**: Name for the saved prefab
- **Save Path**: Where to save the prefab (default: `Assets/Prefabs/UI/Grids/`)

### Step 5: Create the Prefab

1. **Click "Create Grid Prefab"** button
2. The tool will:
   - Create a GameObject with GridLayoutGroup
   - Instantiate all slots as children
   - Name slots correctly (`Slot_0_0`, `Slot_1_0`, etc.)
   - Save as prefab at the specified path
   - Select the prefab in Project window

3. **Success dialog** will show:
   - Prefab name
   - Grid size
   - Total slots
   - Save path

## Example: Creating InventoryGridPrefab

1. **Open tool**: `FracturedConcord → Create Grid Prefab`
2. **Click preset**: "InventoryGrid (12×4)" button
3. **Assign prefab**: Drag `InventorySlotUI` prefab to "Slot Prefab" field
4. **Verify settings**:
   - Grid Width: 12
   - Grid Height: 4
   - Cell Size: (60, 60)
   - Spacing: (2, 2)
   - Prefab Name: `InventoryGridPrefab_12x4`
5. **Click**: "Create Grid Prefab"
6. **Done!** Prefab is created at `Assets/Prefabs/UI/Grids/InventoryGridPrefab_12x4.prefab`

## What Gets Created

The tool creates a prefab with:

- **Root GameObject**: Named after your prefab name
  - Has `GridLayoutGroup` component configured
  - All settings match your specifications

- **Child Slots**: All slots as children
  - Named: `Slot_0_0`, `Slot_1_0`, ... `Slot_11_3`
  - In correct order (left-to-right, top-to-bottom)
  - All have your slot prefab component attached

## After Creation

1. **Verify the prefab**:
   - Select it in Project window
   - Check it has correct number of slots
   - Verify GridLayoutGroup settings

2. **Assign to component**:
   - Open EquipmentScreen scene
   - Select `InventoryGridUI` component
   - Drag prefab to **Grid Prefab** field
   - Leave **Slot Prefab** empty

3. **Test**:
   - Play the game
   - Load EquipmentScreen
   - Grid should load instantly!

## Tips

### For Different Slot Types

If you need different slot prefabs for different grids:
- Create each grid separately
- Use the appropriate slot prefab for each
- The tool will use whatever prefab you assign

### Custom Grid Sizes

To create custom-sized grids:
1. Don't use presets
2. Manually set Grid Width/Height
3. Adjust Cell Size and Spacing as needed
4. Set custom Prefab Name

### Batch Creation

To create all 4 grids quickly:
1. Use preset for InventoryGrid → Create
2. Use preset for StashGrid → Create
3. Use preset for EffigyGrid → Create
4. Use preset for AuraGrid → Create

Each takes ~10 seconds to create!

## Troubleshooting

### "Slot Prefab is required" Warning?

- Make sure you've assigned a prefab to the "Slot Prefab" field
- The prefab must be a prefab asset (not a scene object)

### Prefab Not Created?

- Check the Save Path exists
- Ensure you have write permissions
- Check Console for errors

### Wrong Number of Slots?

- Verify Grid Width × Grid Height matches expected total
- Check that all slots were created (expand prefab in Project window)

### Slots in Wrong Order?

- The tool creates slots in order: left-to-right, top-to-bottom
- If order is wrong, check GridLayoutGroup settings
- Verify slots are direct children of grid root

## Performance Comparison

**Manual Creation**:
- Time: ~5-10 minutes per grid
- Error-prone: Easy to miscount or misname slots
- Tedious: 60+ drag-and-drop operations

**Helper Script**:
- Time: ~10 seconds per grid
- Accurate: Always correct number and naming
- Easy: One click!

## Files

- **Script**: `Assets/Editor/CreateGridPrefab.cs`
- **Menu**: `FracturedConcord → Create Grid Prefab`

