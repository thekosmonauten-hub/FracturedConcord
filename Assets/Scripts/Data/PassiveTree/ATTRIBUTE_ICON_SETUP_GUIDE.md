# Attribute Icon System Setup Guide

## Overview
The Attribute Icon System automatically displays appropriate icons on passive tree cells based on their JSON data attributes. This makes the passive tree more visually informative and easier to understand.

## Components Created

### 1. AttributeIconManager
- **Location**: `Assets/Scripts/Data/PassiveTree/AttributeIconManager.cs`
- **Purpose**: Manages all attribute-based icons and determines which icon to display
- **Features**:
  - Supports single attributes (STR, DEX, INT)
  - Supports attribute combinations (STR+DEX, STR+INT, DEX+INT, All)
  - Supports special stats (Health, Energy Shield, Evasion, etc.)
  - Automatic fallback to default icons

### 2. Enhanced CellController
- **Location**: `Assets/Scripts/Data/PassiveTree/CellController.cs`
- **New Features**:
  - `useAttributeIcons` toggle to enable/disable attribute icons
  - Automatic icon assignment based on JSON data
  - Fallback to node type sprites if no attribute icon found

### 3. Enhanced CellJsonData
- **Location**: `Assets/Scripts/Data/PassiveTree/CellJsonData.cs`
- **New Features**:
  - Automatically updates cell sprite when JSON data is loaded
  - Seamless integration with attribute icon system

### 4. AttributeIconSetup
- **Location**: `Assets/Scripts/Data/PassiveTree/AttributeIconSetup.cs`
- **Purpose**: Helper script to configure the entire system
- **Features**:
  - One-click setup for all cells
  - Validation of icon assignments
  - Testing functionality

## Setup Instructions

### Step 1: Create AttributeIconManager
1. Create an empty GameObject in your scene
2. Name it "AttributeIconManager"
3. Add the `AttributeIconManager` component
4. Assign your attribute icons to the appropriate fields:

#### Required Icons:
- **Strength Icon**: `Assets/Art/Player/Attribute_STR.aseprite`
- **Dexterity Icon**: `Assets/Art/Player/Attribute_DEX.aseprite`
- **Intelligence Icon**: `Assets/Art/Player/Attribute_INT.aseprite`
- **Default Icon**: `Assets/Art/Player/AttributeIcon.aseprite`

#### Optional Combination Icons:
- **Strength + Dexterity Icon**: For nodes with both STR and DEX
- **Strength + Intelligence Icon**: For nodes with both STR and INT
- **Dexterity + Intelligence Icon**: For nodes with both DEX and INT
- **All Attributes Icon**: For nodes with STR, DEX, and INT

#### Special Stat Icons:
- **Health Icon**: For health/armor focused nodes
- **Energy Shield Icon**: For energy shield focused nodes
- **Evasion Icon**: For evasion focused nodes
- **Spell Power Icon**: For spell power focused nodes
- **Accuracy Icon**: For accuracy focused nodes
- **Resistance Icon**: For elemental resistance nodes

### Step 2: Configure Cells
1. Add `AttributeIconSetup` component to any GameObject in your scene
2. Right-click on the component and select "Setup Attribute Icon System"
3. This will automatically:
   - Find or create the AttributeIconManager
   - Enable attribute icons on all cells
   - Update all cell sprites

### Step 3: Load JSON Data
1. Right-click on the `AttributeIconSetup` component
2. Select "Load JSON Data for All Cells"
3. This will load JSON data for all cells that don't have it yet

### Step 4: Test the System
1. Right-click on the `AttributeIconSetup` component
2. Select "Test Attribute Icon System"
3. Check the console for test results

## Icon Assignment Logic

The system uses the following priority order:

1. **Attribute Combinations** (highest priority):
   - All three attributes (STR + DEX + INT) → All Attributes Icon
   - Two attributes → Combination Icon (STR+DEX, STR+INT, DEX+INT)
   - Single attribute → Individual Attribute Icon (STR, DEX, INT)

2. **Special Stats**:
   - Health/Armor stats → Health Icon
   - Energy Shield stats → Energy Shield Icon
   - Evasion stats → Evasion Icon
   - Spell Power stats → Spell Power Icon
   - Accuracy stats → Accuracy Icon
   - Resistance stats → Resistance Icon

3. **Fallback**:
   - Node type sprite (from existing system)
   - Default icon

## JSON Data Requirements

The system reads the `stats` object from your JSON data:

```json
{
  "stats": {
    "strength": 5,
    "dexterity": 5,
    "intelligence": 0,
    "maxHealthIncrease": 8,
    "armor": 50
  }
}
```

### Supported Stats:
- **Core Attributes**: `strength`, `dexterity`, `intelligence`
- **Health**: `maxHealthIncrease`, `armor`, `armorIncrease`
- **Energy Shield**: `maxEnergyShield`, `maxEnergyShieldIncrease`
- **Evasion**: `evasion`, `evasionIncrease`, `increasedEvasion`
- **Spell Power**: `spellPowerIncrease`
- **Accuracy**: `accuracy`
- **Resistance**: `elementalResist`

## Manual Configuration

### Enable/Disable Attribute Icons per Cell:
1. Select a cell GameObject
2. In the `CellController` component, toggle `Use Attribute Icons`
3. The cell will immediately update its sprite

### Update Icons Manually:
1. Select a cell with `CellJsonData` component
2. Right-click and select "Load JSON Data For This Cell"
3. The icon will update automatically

## Troubleshooting

### No Icons Appearing:
1. Check that `AttributeIconManager` exists in the scene
2. Verify that icons are assigned to the manager
3. Ensure `Use Attribute Icons` is enabled on cells
4. Check that JSON data is loaded (use "Test JSON Parsing" on CellJsonData)

### Wrong Icons:
1. Check the JSON data stats for the cell
2. Verify icon assignments in `AttributeIconManager`
3. Use "Test Attribute Icon System" to debug

### Performance Issues:
1. Disable `Debug Mode` in `AttributeIconManager`
2. Disable `Show Debug Info` in `CellController` components
3. Only enable attribute icons on cells that need them

## Example Usage

```csharp
// Get icon for a specific cell
var cellJsonData = cell.GetComponent<CellJsonData>();
var iconManager = FindFirstObjectByType<AttributeIconManager>();
var icon = iconManager.GetIconForCellData(cellJsonData);

// Update a cell's sprite manually
var cellController = cell.GetComponent<CellController>();
cellController.UpdateSpriteForJsonData();
```

## Integration with Existing Systems

The attribute icon system is designed to work alongside your existing sprite system:

1. **Priority**: Attribute icons take priority over node type sprites
2. **Fallback**: If no attribute icon is found, falls back to node type sprites
3. **Compatibility**: Works with existing `autoAssignSprite` functionality
4. **Performance**: Minimal overhead, only updates when JSON data changes

## Future Enhancements

Potential improvements you could add:
- Custom icon sets for different themes
- Animated icons for special nodes
- Icon scaling based on node importance
- Color coding for different attribute values
- Icon overlays for node states (purchased, locked, etc.)

