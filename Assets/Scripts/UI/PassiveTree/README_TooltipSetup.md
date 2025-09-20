# Passive Tree Tooltip System Setup Guide

## Problem Solved
Your tooltip system wasn't working because:
1. **CellController** was looking for `PassiveTreeTooltip` but you're using JSON data which requires `JsonPassiveTreeTooltip`
2. **Missing tooltip prefab** - the JSON tooltip system needs a prefab or will create one dynamically
3. **Missing data manager connection** - tooltips need to be connected to the JSON data source
4. **Missing EventSystem** - required for pointer events to work

## Quick Setup (Recommended)

### Step 1: Add Quick Setup Script
1. Create an empty GameObject in your scene
2. Add the `QuickTooltipSetup` component to it
3. In the Inspector, check "Setup On Start" if you want automatic setup
4. Right-click the component and select "Quick Setup Tooltips"

### Step 2: Verify Setup
1. Right-click the `QuickTooltipSetup` component
2. Select "Debug Current State" to check for missing components
3. If any components are missing, select "Create Missing Components"

### Step 3: Test Tooltips
1. Play your scene
2. Hover over any passive tree cell
3. You should see a tooltip with the cell's information from your JSON data

## Manual Setup (Alternative)

### Step 1: Create Tooltip Setup
1. Create an empty GameObject named "PassiveTreeTooltipSetup"
2. Add the `PassiveTreeTooltipSetup` component
3. Right-click the component and select "Setup Tooltip System"

### Step 2: Verify Components
Make sure you have these components in your scene:
- **EventSystem** (required for pointer events)
- **Canvas** (required for UI)
- **GraphicRaycaster** (required for UI interaction)
- **JsonBoardDataManager** (manages JSON data)
- **JsonPassiveTreeTooltip** (handles tooltips)

### Step 3: Connect JSON Data
1. Find the `JsonBoardDataManager` in your scene
2. Make sure it has your `CoreBoardData.json` file assigned
3. Call `LoadBoardData()` on the manager

## How It Works

### CellController Changes
- Now supports both `PassiveTreeTooltip` and `JsonPassiveTreeTooltip`
- Automatically detects which tooltip system is available
- Prioritizes JSON tooltip system for JSON-based data

### JsonPassiveTreeTooltip Features
- **Dynamic Tooltip Creation**: Creates tooltips automatically if no prefab is assigned
- **Smart Positioning**: Positions tooltips to avoid screen edges
- **Rich Content**: Shows node name, description, stats, type, cost, and status
- **Mouse Following**: Tooltips follow the mouse cursor

### Tooltip Content
The tooltip displays:
- **Node Name** (from JSON data)
- **Description** (from JSON data)
- **Stats** (converted from JSON stats object)
- **Type** (small, notable, keystone, etc.)
- **Cost** (skill points required)
- **Status** (purchased, available, locked)

## Troubleshooting

### Tooltips Not Showing
1. Check Console for error messages
2. Verify EventSystem exists in scene
3. Verify Canvas and GraphicRaycaster exist
4. Check that JSON data is loaded
5. Ensure cells have Collider2D components

### Tooltips Showing But No Content
1. Verify JSON data is loaded in JsonBoardDataManager
2. Check that cell positions match JSON data positions
3. Verify the JSON structure matches expected format

### Tooltips Not Following Mouse
1. Check Input System is enabled
2. Verify Mouse.current is available
3. Check that followMouse is enabled in tooltip settings

## JSON Data Format
Your `CoreBoardData.json` should have this structure:
```json
{
  "nodes": [
    [
      {
        "id": "Cell_0_0",
        "name": "Node Name",
        "description": "Node description",
        "type": "small",
        "stats": { "strength": 5 },
        "cost": 1
      }
    ]
  ]
}
```

## Testing
1. Use the "Test Tooltip System" context menu option
2. Hover over cells to see tooltips
3. Check Console for debug messages
4. Use "Debug Current State" to verify setup

## Customization
- Modify tooltip appearance in `PassiveTreeTooltipSetup`
- Adjust positioning in `JsonPassiveTreeTooltip`
- Customize content formatting in the tooltip population methods

## Files Modified
- `CellController.cs` - Added support for both tooltip systems
- `JsonPassiveTreeTooltip.cs` - Added dynamic tooltip creation
- `PassiveTreeTooltipSetup.cs` - New comprehensive setup script
- `QuickTooltipSetup.cs` - New quick setup script

## Next Steps
1. Test the tooltip system with your existing cells
2. Customize tooltip appearance if needed
3. Add any additional tooltip features you require
4. Consider creating a tooltip prefab for better performance



