# CellPrefab Setup Guide

## Overview

The CellPrefab system separates cell background sprites from node content, which should help resolve sprite rendering issues. This creates a cleaner separation of concerns:

- **CellPrefab**: Handles cell background sprites (Basic Cell, etc.)
- **NodePrefab**: Handles node-specific content and sprites
- **ConnectionPrefab**: Handles connection lines

## Why CellPrefab Helps

1. **Clear Separation**: Cell sprites and node sprites are handled by different components
2. **Better Rendering**: Each prefab has its own Image component with specific settings
3. **Easier Debugging**: You can test cell sprites and node sprites independently
4. **Modular Design**: Easier to manage and modify each layer separately

## Setup Steps

### 1. Create CellPrefab

1. **Create GameObject**: Right-click in Hierarchy → Create Empty
2. **Name it**: `CellPrefab`
3. **Add Components**:
   - `RectTransform`
   - `Image` (for cell background)
   - `PassiveTreeCellUI` script

### 2. Configure CellPrefab

1. **Set RectTransform**:
   - Width: 60 (or your desired cell size)
   - Height: 60 (or your desired cell size)
   - Anchor: Center
   - Pivot: (0.5, 0.5)
   - **Important**: Make sure Width and Height are equal for square cells

2. **Configure Image Component**:
   - Source Image: Leave empty (will be set by script)
   - Color: White (or your desired base color)
   - Material: Default UI Material
   - Raycast Target: False (unless you need cell interactions)

3. **Configure PassiveTreeCellUI**:
   - Cell Background: Drag the Image component here
   - Cell Rect Transform: Drag the RectTransform here
   - Use Custom Cell Sprites: Checked
   - Log Sprite Assignments: Checked (for debugging)

### 3. Create Prefab

1. **Drag to Project**: Drag the configured CellPrefab to your Prefabs folder
2. **Name it**: `CellPrefab`
3. **Delete from Scene**: Remove the GameObject from the scene

### 4. Update PassiveTreeBoardUI

1. **Assign CellPrefab**: Drag your CellPrefab to the `Cell Prefab` field in PassiveTreeBoardUI
2. **Configure Cell Settings**:
   - **Cell Size**: Set the desired cell size (default: 60)
   - **Auto Calculate Cell Size**: Check this to automatically size cells to fit the container
   - **Cell Padding**: Set padding around the grid (default: 20)
3. **Quick Setup**: Right-click on PassiveTreeBoardUI → "Set Cell Size to Fit Node"
4. **Test**: The board should now create cells before creating nodes

## Testing the CellPrefab

### 1. Basic Test
1. **Press Play** in Unity
2. **Check Console** for cell creation messages
3. **Look for** cell GameObjects in the hierarchy under BoardContainer

### 2. Sprite Test
1. **Right-click on any Cell GameObject** → "Test Cell Sprite Rendering"
2. **Check Console** for detailed sprite information
3. **Look for** blue color test (should see blue squares briefly)

### 3. Debug Test
1. **Right-click on any Cell GameObject** → "Force Update Cell Sprite"
2. **Check Console** for sprite assignment messages

## Expected Hierarchy Structure

```
PassiveTreeCanvas
└── BoardContainer (with BoardContainer sprite)
    ├── Cell_0_0 (with Basic Cell sprite)
    │   └── NodePrefab (Clone) (with node content)
    ├── Cell_0_1 (with Basic Cell sprite)
    │   └── NodePrefab (Clone) (with node content)
    └── ... (more cells and nodes)
```

## Troubleshooting

### Cells Not Visible
1. **Check CellPrefab Image**: Ensure the Image component is properly configured
2. **Check CellPrefab Size**: Make sure RectTransform size is appropriate
3. **Check CellPrefab Position**: Ensure cells are positioned correctly
4. **Test with Color**: Use "Test Cell Sprite Rendering" to see if Image component works

### Cells Overlapping
1. **Check Cell Size**: Ensure all cells have the same Width and Height in RectTransform
2. **Check Cell Positioning**: The system now uses edge-to-edge positioning
3. **Check Board Size**: Ensure boardData.size matches your grid expectations
4. **Check Cell Prefab**: Ensure the CellPrefab RectTransform is properly configured

### Cells Not Covering Container
1. **Enable Auto Calculate**: Check "Auto Calculate Cell Size" in PassiveTreeBoardUI
2. **Increase Cell Size**: Set a larger value in the "Cell Size" field
3. **Check Container Size**: Ensure the BoardContainer has the correct size
4. **Use Debug Cell Positioning**: To see the calculated cell sizes

### Sprites Not Loading
1. **Check PassiveTreeSpriteManager**: Ensure cell sprites are assigned
2. **Check Sprite Import Settings**: Ensure sprites are imported as "Sprite (2D and UI)"
3. **Check Sprite Alpha**: Ensure sprites have proper alpha settings
4. **Check Image Color**: Ensure Image component color is not transparent

### Performance Issues
1. **Disable Logging**: Uncheck "Log Sprite Assignments" in production
2. **Optimize Prefabs**: Keep prefabs lightweight
3. **Use Object Pooling**: For large boards, consider object pooling

## Benefits of This Approach

1. **Clearer Debugging**: You can test cell sprites independently of node sprites
2. **Better Organization**: Each layer has its own responsibility
3. **Easier Customization**: You can modify cell appearance without affecting nodes
4. **Improved Performance**: Better separation allows for optimization
5. **Future-Proof**: Easier to add features like cell highlighting, animations, etc.

## Next Steps

1. **Test the CellPrefab** with your existing sprites
2. **Verify cell sprites are visible** in the scene
3. **Test node sprites** on top of the cells
4. **Optimize** based on your specific needs
5. **Add features** like cell highlighting, hover effects, etc.

## Integration with Existing System

The CellPrefab system is designed to work alongside your existing NodePrefab system:

- **Cells are created first** (background layer)
- **Nodes are created second** (content layer)
- **Both use the same sprite manager** for consistency
- **Both respect the useCustomSprites setting**

This should resolve the sprite rendering issues while maintaining compatibility with your existing system.
