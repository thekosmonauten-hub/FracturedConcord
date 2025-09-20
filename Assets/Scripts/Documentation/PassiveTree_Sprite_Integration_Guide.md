# Passive Tree Sprite Integration Guide

## Overview
This guide explains how to integrate your custom BoardContainer and Cell sprites into the modular passive tree system. The system supports theme-based sprite assignment and automatic visual updates.

## Quick Setup

### Step 1: Create the Sprite Manager Asset
1. **In Unity Project window**: Right-click → Create → Dexiled → Passive Tree → Sprite Manager
2. **Name it**: `PassiveTreeSpriteManager`
3. **Location**: Save it in `Assets/Resources/PassiveTree/` for easy access

### Step 2: Assign Your Sprites
1. **Select the Sprite Manager asset** you just created
2. **In the Inspector**, you'll see three main sections:
   - **Board Container Sprites**: For your BoardContainer sprites
   - **Cell Sprites**: For your Cell sprites  
   - **Node Type Sprites**: For different node types

3. **Assign your sprites**:
   - **Default Board Container**: Drag your main BoardContainer sprite here (used for CoreBoard and as fallback)
   - **Default Cell Sprite**: Drag your main Cell sprite here (used for CoreBoard and as fallback)
   - **Theme-Specific**: Add theme-specific variations using the "Add Theme" buttons

### Step 3: Connect to Your Boards
1. **Select your PassiveBoard assets** (or create new ones)
2. **In the Inspector**, find the "Sprite Management" section
3. **Assign the Sprite Manager**: Drag your `PassiveTreeSpriteManager` asset here
4. **Enable Custom Sprites**: Check "Use Custom Sprites"

### Step 4: Connect to UI Components
1. **Select your PassiveTreeBoardUI components** in the scene
2. **In the Inspector**, find the "Sprite Management" section
3. **Assign the Sprite Manager**: Drag your `PassiveTreeSpriteManager` asset here
4. **Enable Custom Sprites**: Check "Use Custom Sprites"

## Detailed Configuration

### CoreBoard Sprites (Basic Board/Cell)
The CoreBoard uses the **Default Board Container** and **Default Cell Sprite** as its primary sprites. These serve as the "Basic Board" and "Basic Cell" sprites for the foundation of your passive tree.

**To set up CoreBoard sprites:**
1. Assign your BoardContainer sprite to **Default Board Container**
2. Assign your Cell sprite to **Default Cell Sprite**
3. The CoreBoard will automatically use these sprites

**Note**: The CoreBoard currently uses the `Utility` theme, but it primarily relies on the default sprites rather than theme-specific ones.

### Board Container Sprites
Board container sprites are used as the background for each passive tree board.

**Default Board Container**: Used when no theme-specific sprite is available
**Theme-Specific Board Sprites**: Different sprites for different board themes (Fire, Cold, Lightning, etc.)

**How to Add Theme-Specific Board Sprites**:
1. Click "Add Theme Board Sprite" in the Sprite Manager
2. Select the theme (Fire, Cold, Lightning, etc.)
3. Drag your BoardContainer sprite for that theme

### Cell Sprites
Cell sprites are used for individual grid cells within the passive tree boards.

**Default Cell Sprite**: Used when no theme-specific sprite is available
**Theme-Specific Cell Sprites**: Different cell sprites for different board themes

**How to Add Theme-Specific Cell Sprites**:
1. Click "Add Theme Cell Sprite" in the Sprite Manager
2. Select the theme (Fire, Cold, Lightning, etc.)
3. Drag your Cell sprite for that theme

### Node Type Sprites
Node type sprites are used for different types of passive nodes.

**Available Node Types**:
- **Main Node**: Starting point nodes (0 cost)
- **Travel Node**: Attribute nodes (1 cost)
- **Extension Node**: Board connection points (0 cost)
- **Keystone Node**: Build-defining effects (1 cost, larger, more prominent)
- **Notable Node**: Powerful passives (2 cost, medium size)
- **Small Node**: Minor bonuses (1 cost, smaller size)

## Theme System

The sprite system supports the following board themes:
- **Fire**: Fire-themed passives
- **Cold**: Cold-themed passives  
- **Lightning**: Lightning-themed passives
- **Chaos**: Chaos-themed passives
- **Physical**: Physical-themed passives
- **Life**: Life-themed passives
- **Armor**: Armor-themed passives
- **Evasion**: Evasion-themed passives
- **Critical**: Critical-themed passives
- **Speed**: Speed-themed passives
- **Utility**: Utility-themed passives
- **Elemental**: General elemental passives
- **Keystone**: Keystone-themed passives

## Sprite Requirements

### Board Container Sprites
- **Format**: PNG, JPG, or other Unity-supported formats
- **Size**: Recommended 512x512 or larger for good quality
- **Style**: Should be designed to tile or scale well
- **Transparency**: Can use alpha channel for transparency

### Cell Sprites
- **Format**: PNG, JPG, or other Unity-supported formats
- **Size**: Recommended 64x64 to 128x128 pixels
- **Style**: Should work well as individual grid cells
- **Transparency**: Can use alpha channel for transparency

## Advanced Features

### Automatic Sprite Updates
The system automatically updates sprites when:
- Board theme changes
- Node type changes
- Sprite manager assignments change

### Fallback System
If a theme-specific sprite is not available, the system falls back to:
1. Default sprite for the category
2. Previous theme's sprite
3. System default (if no sprites are assigned)

### Validation Tools
Use the "Validate Sprites" button in the Sprite Manager to check for:
- Missing sprite assignments
- Invalid sprite references
- Configuration issues

## Troubleshooting

### Sprites Not Showing
1. **Check Sprite Manager Assignment**: Ensure the Sprite Manager is assigned to both PassiveBoard and PassiveTreeBoardUI
2. **Enable Custom Sprites**: Make sure "Use Custom Sprites" is checked
3. **Check Sprite References**: Use "Validate Sprites" to find missing assignments
4. **Check Console**: Look for error messages in the Unity Console

### Performance Issues
1. **Optimize Sprite Sizes**: Use appropriately sized sprites
2. **Enable Compression**: Use Unity's sprite compression settings
3. **Limit Theme Variations**: Don't create unnecessary theme-specific sprites

### Visual Issues
1. **Check Sprite Import Settings**: Ensure sprites are imported as "Sprite (2D and UI)"
2. **Check Pivot Points**: Ensure sprites have appropriate pivot points
3. **Check Transparency**: Verify alpha channel settings

## Best Practices

### Organization
- **Naming Convention**: Use descriptive names like "FireBoardContainer", "ColdCellSprite"
- **Folder Structure**: Organize sprites by theme in separate folders
- **Asset Management**: Use Unity's asset organization features

### Performance
- **Sprite Atlasing**: Consider using Unity's sprite atlasing for better performance
- **Compression**: Use appropriate compression settings for your target platform
- **Memory Management**: Monitor memory usage with large sprite collections

### Workflow
- **Iterative Design**: Start with basic sprites and refine over time
- **Theme Consistency**: Maintain visual consistency within themes
- **Testing**: Test sprites at different scales and resolutions

## Example Setup

### Basic Setup
1. Create `PassiveTreeSpriteManager` asset
2. Assign your main BoardContainer sprite as "Default Board Container"
3. Assign your main Cell sprite as "Default Cell Sprite"
4. Assign the Sprite Manager to your PassiveBoard assets
5. Assign the Sprite Manager to your PassiveTreeBoardUI components

### Theme-Specific Setup
1. Create theme-specific BoardContainer sprites (Fire, Cold, Lightning, etc.)
2. Create theme-specific Cell sprites for each theme
3. Add theme-specific sprites in the Sprite Manager
4. Set board themes in your PassiveBoard assets
5. The system will automatically use the appropriate sprites

## Integration with Existing Systems

The sprite system integrates seamlessly with:
- **Existing PassiveBoard assets**: No changes needed to existing boards
- **PassiveTreeManager**: Automatic sprite updates when boards are loaded
- **UI System**: Works with both UI Toolkit and Legacy UI
- **Theme System**: Respects existing board themes

## Future Enhancements

Potential future features:
- **Animated Sprites**: Support for animated sprite sequences
- **Dynamic Themes**: Runtime theme switching
- **Custom Shaders**: Theme-specific shader effects
- **Sprite Variants**: Multiple sprite variations per theme
- **Auto-Generation**: Automatic sprite generation based on themes

## Testing and Debugging

### Using the Test Script
The `PassiveTreeSpriteTest` component provides several useful debugging tools:

1. **Add to Scene**: Add the `PassiveTreeSpriteTest` component to any GameObject in your scene
2. **Configure**: Assign your `PassiveTreeSpriteManager` and any test boards
3. **Use Context Menu**: Right-click the component and select from:
   - **Test Sprite Assignments**: Comprehensive sprite verification
   - **Test CoreBoard Sprites**: Specifically test CoreBoard sprite usage
   - **Find All Sprite Managers**: Locate all sprite managers in the project
   - **Find All Passive Boards**: Check all boards and their sprite managers

### Debugging Tips
- Use the "Debug Sprite Configuration" button in the Sprite Manager inspector
- Check the Unity Console for detailed logging messages
- Verify that all sprite references are properly assigned

## Support

If you encounter issues:
1. Check this guide for troubleshooting steps
2. Use the "Validate Sprites" tool in the Sprite Manager
3. Check the Unity Console for error messages
4. Review the sprite import settings
5. Ensure all components are properly assigned
6. Use the `PassiveTreeSpriteTest` component for debugging
