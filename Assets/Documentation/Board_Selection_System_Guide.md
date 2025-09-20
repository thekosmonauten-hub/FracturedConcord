# Board Selection System Guide

## Overview

The Board Selection System allows players to choose which type of extension board to create when clicking on extension points. This provides flexibility and variety in the passive tree system.

## Components

### 1. BoardData ScriptableObject
- **Location**: `Assets/Scripts/Data/PassiveTree/BoardData.cs`
- **Purpose**: Defines board metadata including name, description, theme, JSON data path, and visual properties
- **Usage**: Create instances via `Create > Passive Tree > Board Data`

### 2. BoardSelectionUI
- **Location**: `Assets/Scripts/UI/PassiveTree/BoardSelectionUI.cs`
- **Purpose**: Manages the board selection UI panel and handles user interactions
- **Features**:
  - Shows available boards in a grid layout
  - Handles board selection and cancellation
  - Supports board preview images and descriptions

### 3. BoardSelectionButton
- **Location**: `Assets/Scripts/UI/PassiveTree/BoardSelectionButton.cs`
- **Purpose**: Individual button component for each board option
- **Features**:
  - Displays board preview image
  - Shows board name and description
  - Handles click events

### 4. BoardPositioningManager Integration
- **Location**: `Assets/Scripts/Data/PassiveTree/BoardPositioningManager.cs`
- **New Methods**:
  - `HandleExtensionPointClick()` - Shows board selection UI
  - `OnBoardSelected()` - Handles board selection from UI
  - `OnBoardSelectionCancelled()` - Handles selection cancellation

## Setup Instructions

### Step 1: Create Board Data Assets

1. Right-click in Project window
2. Select `Create > Passive Tree > Board Data`
3. Configure the board data:
   - **Board Name**: Display name (e.g., "Fire Board")
   - **Board Description**: Brief description
   - **Board Theme**: Visual theme
   - **Board Size**: Grid dimensions (default: 7x7)
   - **JSON Data Asset**: TextAsset reference to JSON file (auto-assignable)
   - **JSON Data Path**: Path to JSON file (auto-populated)
   - **Board Preview**: Sprite image for UI preview
   - **Board Color**: Color theme for the board
   - **Is Unlocked**: Whether board is available
   - **Required Level**: Minimum level to unlock

#### Automated JSON Assignment

The BoardData ScriptableObject includes automated JSON assignment features:

1. **Auto-Assign JSON Data**: Right-click on BoardData asset → "Auto-Assign JSON Data"
2. **Auto-Assign All Boards**: Use the custom inspector button to process all BoardData assets
3. **Create Sample JSON Files**: Generate sample JSON files for common board types
4. **Validation**: Validate that all required fields are properly configured

### Step 2: Create Board Selection UI

1. Create a Canvas in your scene
2. Add the BoardSelectionUI component to a GameObject
3. Assign the following references:
   - **Selection Panel**: The main UI panel GameObject
   - **Board Button Container**: Container for board buttons
   - **Close Button**: Cancel button
   - **Title Text**: Title label
   - **Description Text**: Description label
   - **Board Button Prefab**: Prefab for individual board buttons

### Step 3: Configure BoardPositioningManager

1. Select the BoardPositioningManager in your scene
2. In the Inspector, find the "Board Selection UI" section
3. Assign the BoardSelectionUI reference
4. Enable "Use Board Selection" checkbox

### Step 4: Create Board Button Prefab

1. Create a UI Button prefab with the following structure:
   ```
   BoardButton (Button)
   ├── PreviewImage (Image)
   ├── BoardName (TextMeshPro)
   └── BoardDescription (TextMeshPro)
   ```
2. Add the BoardSelectionButton component
3. Assign all UI references
4. Save as prefab

## Usage Flow

### Player Experience
1. Player clicks on an extension point (cell with NodeType.Extension)
2. Board selection UI appears with available board options
3. Player selects a board type
4. Extension board is created at the appropriate position
5. UI closes automatically

### Developer Flow
1. `PassiveTreeManager.OnCellClicked()` detects extension point click
2. `HandleExtensionNodeClick()` processes the extension node
3. `HandleExtensionBoardSpawn()` finds the corresponding extension point
4. `BoardPositioningManager.HandleExtensionPointClick()` shows selection UI
5. `BoardSelectionUI.ShowBoardSelection()` displays available boards
6. Player selects board → `OnBoardSelected()` is called
7. `SpawnExtensionBoard()` creates the selected board

## Configuration Options

### BoardPositioningManager Settings
- **Use Board Selection**: Enable/disable board selection UI
- **Board Selection UI**: Reference to BoardSelectionUI component

### BoardSelectionUI Settings
- **Available Boards**: List of BoardData assets to show
- **Show Debug Info**: Enable debug logging
- **Board Button Prefab**: Prefab for board selection buttons

## Creating New Board Types

### Automated Workflow (Recommended)

1. **Create BoardData Asset**:
   - Right-click → `Create > Passive Tree > Board Data`
   - Set board name (e.g., "Fire Board")
   - Configure other fields

2. **Auto-Assign JSON Data**:
   - Right-click on BoardData asset → "Auto-Assign JSON Data"
   - Or use "Create Sample JSON Files" button in inspector

3. **Add to Available Boards**:
   - Select BoardSelectionUI in scene
   - Add the new BoardData to the "Available Boards" list

### Manual Workflow

### 1. Create JSON Data
1. Create a JSON file with node data for your board
2. Place in appropriate folder (e.g., `Assets/Resources/PassiveTree/`)
3. Follow the existing JSON structure

### 2. Create BoardData Asset
1. Right-click → `Create > Passive Tree > Board Data`
2. Configure all fields:
   - Set unique board name
   - Add description
   - Assign JSON data asset manually
   - Create preview sprite
   - Set appropriate color theme

### 3. Add to Available Boards
1. Select BoardSelectionUI in scene
2. Add the new BoardData to the "Available Boards" list
3. Test the selection flow

## Troubleshooting

### Common Issues

**Board Selection UI doesn't appear**
- Check that `useBoardSelection` is enabled in BoardPositioningManager
- Verify BoardSelectionUI reference is assigned
- Ensure extension point is properly configured

**No boards show in selection**
- Check that BoardData assets are added to "Available Boards" list
- Verify boards are marked as "Is Unlocked"
- Check that required level is met

**Board buttons don't work**
- Verify BoardSelectionButton component is on button prefab
- Check that all UI references are assigned
- Ensure button prefab is assigned in BoardSelectionUI

**Selected board doesn't spawn**
- Check that extension board prefab is assigned
- Verify JSON data path is correct
- Check console for error messages

### Debug Information

Enable "Show Debug Info" in both BoardPositioningManager and BoardSelectionUI to see detailed logging of the selection process.

## Future Enhancements

- Board preview in 3D
- Board unlock requirements
- Board categories/filtering
- Board statistics display
- Board comparison tool
- Board favorites system
