# Grid Background Setup Guide

## ✅ **Unity Grid System Background Setup**

This guide shows you how to create a regular grid background for your passive tree using Unity's built-in Grid system.

## 🎯 **Quick Setup (5 Minutes)**

### **Step 1: Create the Grid Background**
1. **Create an empty GameObject** in your scene
2. **Name it**: "PassiveTreeGridBackground"
3. **Add the GridBackground script** to the GameObject
4. **Assign your grid tile sprite** to the Grid Tile field

### **Step 2: Configure Settings**
```
Grid Settings:
- Grid Tile: [Your 512x512 tile sprite]
- Cell Size: 1.0
- Grid Width: 10
- Grid Height: 10
- Show Debug Info: ✓

Sprite Scaling:
- Auto Scale Sprite: ✓ (automatically scales sprite to fit cell size)
- Sprite Scale: 1.0 (additional scaling multiplier)

Gap Removal:
- Remove Gaps: ✓ (removes gaps between cells for seamless tiling)
- Gap Offset: 0.0 (manual offset adjustment)

Runtime Persistence:
- Persist in Runtime: ✓ (keeps grid active during runtime)
- Prevent Destruction: ✓ (prevents GameObject from being destroyed)

Visual Settings:
- Sorting Order: -100
- Grid Color: White
```

### **Step 3: Generate the Grid**
1. **Right-click** on the GridBackground component
2. **Select "Create Grid Background"** from the context menu
3. **Wait for the grid to generate** (may take a moment for large grids)

## 🎯 **Detailed Setup**

### **Step 1: Prepare Your Grid Tile**
1. **Create a 512x512 tile** in your image editor
2. **Make it seamless** using the offset method
3. **Import into Unity** with these settings:
   - Texture Type: Sprite (2D and UI)
   - Wrap Mode: Repeat
   - Filter Mode: Bilinear
   - Max Size: 512
   - Compression: High Quality

### **Step 2: Create the Grid GameObject**
1. **Create Empty GameObject**: GameObject → Create Empty
2. **Name it**: "PassiveTreeGridBackground"
3. **Position it**: (0, 0, 0)
4. **Add GridBackground script**: Add Component → Scripts → GridBackground

### **Step 3: Configure the Grid**
```
Grid Settings:
- Grid Tile: [Drag your tile sprite here]
- Cell Size: 1.0 (adjust for your needs)
- Grid Width: 10 (adjust for coverage)
- Grid Height: 10 (adjust for coverage)
- Show Debug Info: ✓ (for troubleshooting)

Sprite Scaling:
- Auto Scale Sprite: ✓ (automatically scales sprite to fit cell size)
- Sprite Scale: 1.0 (additional scaling multiplier)

Gap Removal:
- Remove Gaps: ✓ (removes gaps between cells for seamless tiling)
- Gap Offset: 0.0 (manual offset adjustment)

Runtime Persistence:
- Persist in Runtime: ✓ (keeps grid active during runtime)
- Prevent Destruction: ✓ (prevents GameObject from being destroyed)

Visual Settings:
- Sorting Order: -100 (behind everything)
- Grid Color: White (or your preferred color)
```

### **Step 4: Generate the Grid**
1. **Right-click** on the GridBackground component
2. **Select "Create Grid Background"** from the context menu
3. **Check the console** for generation progress
4. **Verify the grid appears** in the scene

## 🎯 **Customization Options**

### **Grid Size Settings**
- **Cell Size**: Controls the size of each grid cell
  - `1.0` = Standard size
  - `0.5` = Smaller cells
  - `2.0` = Larger cells

- **Grid Width/Height**: Controls how many cells to create
  - `10x10` = 100 cells (perfect for most cases)
  - `15x15` = 225 cells (medium coverage)
  - `20x20` = 400 cells (large coverage)

### **Sprite Scaling Settings**
- **Auto Scale Sprite**: Automatically scales sprites to fit cell size
  - `✓` = Sprite scales to match cell size (recommended)
  - `✗` = Sprite uses original size (may be too small/large)

- **Sprite Scale**: Additional scaling multiplier
  - `1.0` = No additional scaling
  - `0.5` = Half size
  - `2.0` = Double size

### **Gap Removal Settings**
- **Remove Gaps**: Removes gaps between cells for seamless tiling
  - `✓` = Gaps removed for seamless background (recommended)
  - `✗` = Gaps remain between cells

- **Gap Offset**: Manual offset adjustment for fine-tuning
  - `0.0` = Auto-calculated offset (recommended)
  - `0.5` = Half cell size offset
  - `1.0` = Full cell size offset

### **Runtime Persistence Settings**
- **Persist in Runtime**: Keeps grid active during runtime
  - `✓` = Grid persists during runtime (recommended)
  - `✗` = Grid may be destroyed during runtime

- **Prevent Destruction**: Prevents GameObject from being destroyed
  - `✓` = GameObject won't be destroyed (recommended)
  - `✗` = GameObject can be destroyed

### **Visual Settings**
- **Sorting Order**: Controls depth rendering
  - `-100` = Behind everything
  - `0` = Default layer
  - `100` = In front of most things

- **Grid Color**: Controls the color of all grid cells
  - `White` = Default
  - `Light Gray` = Subtle background
  - `Custom Color` = Match your theme

## 🎯 **Context Menu Commands**

### **Available Commands**
- **Create Grid Background**: Generate the grid
- **Clear Grid**: Remove all grid cells
- **Update Grid**: Recreate with current settings
- **Update Sprite Scaling**: Update sprite scaling for existing grid
- **Auto Calculate Gap Offset**: Automatically calculate optimal gap offset
- **Update Gap Removal**: Update gap removal for existing grid
- **Check Grid Validity**: Check if grid is still valid and recreate if needed
- **Force Recreate Grid**: Force recreate grid if it's missing or corrupted
- **Verify Grid Creation**: Verify that all grid cells were created properly
- **Protect All Children**: Apply DontDestroyOnLoad to all existing children
- **Set All Cells Color**: Change color of all cells
- **Show Grid Stats**: Display grid information (includes all settings and status)
- **Validate Grid Settings**: Check for errors

## 🎯 **Troubleshooting**

### **Common Issues**

**Issue: Grid not appearing**
- **Solution**: Check that Grid Tile is assigned
- **Solution**: Verify Sorting Order is set correctly
- **Solution**: Check that Grid Width/Height are greater than 0

**Issue: Performance problems**
- **Solution**: Reduce Grid Width/Height
- **Solution**: Use a smaller Cell Size
- **Solution**: Consider using a single tiled texture instead

**Issue: Grid cells not aligned**
- **Solution**: Check Cell Size setting
- **Solution**: Verify Grid component is present
- **Solution**: Use "Update Grid" to recreate

**Issue: Sprites too small or too large**
- **Solution**: Enable "Auto Scale Sprite" to automatically fit cell size
- **Solution**: Adjust "Sprite Scale" for additional scaling
- **Solution**: Use "Update Sprite Scaling" to apply changes to existing grid
- **Solution**: Check "Show Grid Stats" for scaling information

**Issue: 512x512 sprite appears tiny**
- **Solution**: Enable "Auto Scale Sprite" (this is the main fix!)
- **Solution**: Set "Cell Size" to match your desired grid cell size
- **Solution**: Use "Update Sprite Scaling" to apply changes

**Issue: Gaps between grid cells**
- **Solution**: Enable "Remove Gaps" to remove gaps between cells
- **Solution**: Use "Auto Calculate Gap Offset" for automatic gap removal
- **Solution**: Adjust "Gap Offset" manually if needed
- **Solution**: Use "Update Gap Removal" to apply changes to existing grid

**Issue: Grid cells not perfectly aligned**
- **Solution**: Use "Auto Calculate Gap Offset" to get optimal alignment
- **Solution**: Fine-tune "Gap Offset" for perfect positioning
- **Solution**: Check "Show Grid Stats" for gap removal information

**Issue: Grid disappears during runtime**
- **Solution**: Enable "Persist in Runtime" to keep grid active
- **Solution**: Enable "Prevent Destruction" to prevent GameObject destruction
- **Solution**: Use "Check Grid Validity" to detect and fix issues
- **Solution**: Use "Force Recreate Grid" to recreate missing grid

**Issue: GridBackground GameObject removed from hierarchy**
- **Solution**: Enable "Prevent Destruction" to prevent GameObject destruction
- **Solution**: Check if other scripts are destroying the GameObject
- **Solution**: Use "Check Grid Validity" to detect and fix issues
- **Solution**: Use "Force Recreate Grid" to recreate missing grid

**Issue: Grid children are removed but parent remains**
- **Solution**: Enable "Prevent Destruction" to protect all children
- **Solution**: Use "Protect All Children" to apply protection to existing children
- **Solution**: Check "Show Grid Stats" for child protection status
- **Solution**: Use "Force Recreate Grid" to recreate missing children

**Issue: Grid cells disappear during scene changes**
- **Solution**: Enable "Prevent Destruction" to protect all GameObjects
- **Solution**: Use "Protect All Children" to ensure all children are protected
- **Solution**: Check "Show Grid Stats" for protection status
- **Solution**: Use "Check Grid Validity" to detect and fix issues

**Issue: Only one grid cell is created**
- **Solution**: Use "Verify Grid Creation" to check if all cells were created
- **Solution**: Check if "Prevent Destruction" is interfering with creation
- **Solution**: Use "Force Recreate Grid" to recreate the entire grid
- **Solution**: Check console for any error messages during creation

### **Debug Information**
- **Enable "Show Debug Info"** for detailed logging
- **Use "Show Grid Stats"** to check grid status
- **Use "Validate Grid Settings"** to check for errors

## 🎯 **Performance Tips**

### **Optimization Strategies**
- **Start with smaller grids** (10x10) and increase as needed
- **Use appropriate cell size** for your passive tree scale
- **Consider using a single tiled texture** for very large grids
- **Test performance** on your target devices

### **Memory Usage**
- **10x10 grid**: ~100 GameObjects (very efficient)
- **15x15 grid**: ~225 GameObjects (efficient)
- **20x20 grid**: ~400 GameObjects (moderate memory)

## 🎯 **Alternative Approaches**

### **For Large Grids**
If you need a very large grid, consider:
1. **Single tiled texture** with material tiling
2. **Procedural grid generation** using shaders
3. **Chunked grid system** that loads/unloads sections

### **For Performance**
If performance is an issue:
1. **Reduce grid size** to minimum needed
2. **Use simpler sprites** for grid tiles
3. **Consider using a single large texture** instead

## 🎯 **Result**

**Your passive tree now has a beautiful, regular grid background!**

- ✅ **Perfect grid alignment** using Unity's Grid system
- ✅ **Easy customization** of grid size and appearance
- ✅ **Context menu tools** for easy management
- ✅ **Debug information** for troubleshooting
- ✅ **Performance optimized** for most use cases

**Your passive tree background is now ready!** 🎉
