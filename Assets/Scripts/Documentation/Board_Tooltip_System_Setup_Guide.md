# Board Tooltip System Setup Guide

## Overview
This guide explains how to set up and use the board tooltip system for the passive tree dropdown menus. The system now uses **selection-based tooltips** that appear when you select a board from a dropdown, rather than hover-based tooltips.

## Setup Instructions

### 1. Basic Setup
1. **Add BoardTooltipManager Component**: Add the `BoardTooltipManager` script to a GameObject in your scene
2. **Assign Tooltip Canvas**: Set the `tooltipCanvas` field to your UI Canvas
3. **Connect to TierBasedBoardManager**: Assign the `BoardTooltipManager` to the `tooltipManager` field in `TierBasedBoardManager`

### 2. Tooltip Prefab Setup (IMPORTANT!)
**You need to create and assign a tooltip prefab:**

1. **Create Tooltip Prefab**:
   - Create a new UI Panel in your scene
   - Add a TextMeshPro - Text (UI) component to it
   - Style it as a tooltip (background, border, etc.)
   - Make it a prefab by dragging it to your Prefabs folder
   - Delete the scene instance

2. **Assign to BoardTooltipManager**:
   - Select the GameObject with `BoardTooltipManager`
   - Drag your tooltip prefab to the `Tooltip Prefab` field
   - Make sure the `Tooltip Canvas` field is assigned to your UI Canvas

### 3. How It Works
- **Selection-Based**: Tooltips appear when you select a board from any dropdown
- **Static Display**: Tooltips appear at the center of the screen for better visibility
- **Easy Dismissal**: Press ESC or click elsewhere to dismiss tooltips
- **Content Loading**: Content is loaded from ScriptableObjects or fallback data
- **Screen Bounds**: Tooltips are positioned to stay within screen bounds

### 4. Troubleshooting

#### Tooltips Not Appearing
1. **Check Tooltip Prefab**: Make sure the `tooltipPrefab` field is assigned in `BoardTooltipManager`
2. **Check Canvas**: Ensure the `tooltipCanvas` field is assigned to a UI Canvas
3. **Check Connection**: Verify `BoardTooltipManager` is assigned to `TierBasedBoardManager.tooltipManager`

#### Tooltips Not Appearing on Selection
1. **Check Show Tooltip Setting**: Make sure `showTooltipOnSelection` is enabled in `TierBasedBoardManager`
2. **Test Tooltip System**: Use the "Test Tooltip Integration" context menu in `TierBasedBoardManager`
3. **Check Console Logs**: Look for debug messages about board selection and tooltip display
4. **Verify Dropdown Population**: Use "Test Dropdown Options" to check if boards are loaded correctly

#### Tooltip Won't Dismiss
1. **Press ESC**: The tooltip should dismiss when you press the Escape key
2. **Click Elsewhere**: Click outside of dropdown areas to dismiss the tooltip
3. **Check Input System**: Make sure your input system is working correctly

#### Input System Errors
If you see errors about `UnityEngine.Input` not being available:
- The system now supports both old and new Input Systems
- Make sure your project's Input System settings are configured correctly
- The code automatically detects and uses the appropriate input system

### 5. Testing
1. **Test Tooltip Integration**: Right-click `TierBasedBoardManager` → "Test Tooltip Integration"
2. **Test Board Selection Tooltip**: Right-click `TierBasedBoardManager` → "Test Board Selection Tooltip"
3. **Test Board Loading**: Right-click `TierBasedBoardManager` → "Test Board Loading"
4. **Test Dropdown Options**: Right-click `TierBasedBoardManager` → "Test Dropdown Options"
5. **Test in Game**: Select a board from any dropdown to see the tooltip appear

### 6. Customization

#### Positioning Options
You can now control tooltip positioning through the inspector:

1. **In TierBasedBoardManager**:
   - `Tooltip Position`: Choose from Center, TopRight, BottomLeft, TopLeft, BottomRight, MousePosition, or Custom
   - `Custom Offset`: Set custom X,Y coordinates when using Custom position

2. **Quick Code Changes**:
   - Edit `CalculateTooltipPosition()` method in `TierBasedBoardManager.cs`
   - Modify the position calculation in `ShowTooltipForSelectedBoard()`

#### Styling Options
You can customize tooltip appearance through the inspector:

1. **In BoardTooltipManager**:
   - `Background Color`: Change the tooltip background color
   - `Text Color`: Change the text color
   - `Font Size`: Adjust the text size
   - `Padding`: Set the padding around the text
   - `Add Border`: Enable/disable border
   - `Border Color`: Set border color
   - `Border Width`: Set border thickness

2. **Custom Prefab**:
   - Set `Use Custom Prefab` to true
   - Create a custom tooltip prefab with your own styling
   - Assign it to the `Tooltip Prefab` field

#### Content Customization
- Modify tooltip content in `BoardTooltipManager.InitializeFallbackTooltips()`
- Add new board tooltips by editing the fallback content
- Customize the stat summaries for each board

### 7. Alternative System
If the dynamic tooltip system doesn't work, you can use the simpler `SimpleBoardTooltip` system:
1. Add `SimpleBoardTooltip` component to a GameObject
2. Assign a UI Text component to display tooltips
3. Use the `StartHover()` and `StopHover()` methods

## Recent Fixes
- **Switched to Selection-Based Tooltips**: Replaced complex hover detection with simple selection-based tooltips
- **Simplified Implementation**: Removed all complex dropdown hover detection code
- **Added Easy Dismissal**: Tooltips can be dismissed with ESC key or clicking elsewhere
- **Enhanced User Experience**: Tooltips appear at screen center for better visibility
- **Fixed Input System Compatibility**: Updated all mouse position references to work with both old and new Input Systems

## Next Steps
1. Create a tooltip prefab if you haven't already
2. Assign it to the `BoardTooltipManager.tooltipPrefab` field
3. Test the system by selecting boards from dropdowns
4. Add more board tooltips as you implement additional boards
5. Customize tooltip appearance and positioning as needed
