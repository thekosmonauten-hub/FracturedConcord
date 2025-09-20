# ScriptableObject Board System Guide

## Overview

The ScriptableObject Board System replaces the shared CoreBoard asset approach with individual ScriptableObject assets for each passive tree board. This provides better organization, easier editing, and proper asset management.

## Key Benefits

- **Individual Board Assets**: Each board has its own ScriptableObject asset
- **Proper Asset Management**: No more shared assets causing conflicts
- **Easy Editing**: Edit each board independently in the Unity Inspector
- **Automatic Discovery**: Board manager automatically discovers and loads all board assets
- **Theme-Based Design**: Each board has appropriate colors and stats based on its theme

## System Architecture

### Core Components

1. **PassiveBoardScriptableObject**: ScriptableObject class that holds board data
2. **PassiveBoardAssetGenerator**: Editor utility to generate individual board assets
3. **PassiveTreeBoardManager**: Updated to discover and load ScriptableObject assets
4. **ScriptableObjectBoardTest**: Test script to verify the system works correctly

### File Structure

```
Assets/
├── Resources/
│   └── PassiveTree/
│       ├── core_board.asset
│       ├── fire_Board.asset
│       ├── cold_board.asset
│       ├── lightning_board.asset
│       ├── chaos_board.asset
│       ├── physical_board.asset
│       ├── life_board.asset
│       ├── energyshield_board.asset
│       ├── attack_board.asset
│       ├── armour_board.asset
│       ├── evasion_board.asset
│       ├── spelldamage_board.asset
│       ├── armourEvasion_board.asset
│       ├── armourEs_board.asset
│       ├── evasionES_board.asset
│       ├── cardMechanics_board.asset
│       └── discard_board.asset
├── Scripts/
│   ├── Editor/
│   │   └── PassiveBoardAssetGenerator.cs
│   ├── Test/
│   │   └── ScriptableObjectBoardTest.cs
│   └── Data/PassiveTree/
│       └── PassiveTreeBoardManager.cs (updated)
```

## Usage Guide

### Step 1: Generate Board Assets

1. **Open the Board Asset Generator**:
   - Go to `Tools > Passive Tree > Generate Board Assets`
   - This opens the editor window

2. **Generate All Boards**:
   - Click "Generate All Board Assets" to create all 17 board assets
   - Or generate individual boards using the specific buttons

3. **Verify Generation**:
   - Check the Console for generation logs
   - Assets will be created in `Assets/Resources/PassiveTree/`

### Step 2: Test the System

1. **Add Test Component**:
   - Add `ScriptableObjectBoardTest` to any GameObject in your scene
   - Enable "Run Test On Start" to test automatically

2. **Run Tests**:
   - Use the context menu "Run ScriptableObject Board Test"
   - Check the Console for detailed test results

3. **Verify Discovery**:
   - The board manager should discover all generated assets
   - Each board should have proper data and nodes

### Step 3: Integration

1. **Automatic Discovery**:
   - The `PassiveTreeBoardManager` automatically discovers ScriptableObject assets
   - No manual configuration required

2. **Board Assignment**:
   - Assignment rules are pre-configured for all 17 boards
   - Boards unlock based on character level (5-80)

3. **Character Integration**:
   - The `PassiveTreeCharacterIntegration` component works with the new system
   - Stat bonuses are calculated from discovered boards

## Board Themes and Properties

### Theme Colors
- **Fire**: Red-orange (1, 0.3, 0.1)
- **Cold**: Light blue (0.5, 0.8, 1)
- **Lightning**: Yellow (1, 1, 0.3)
- **Chaos**: Purple (0.8, 0.2, 0.8)
- **Physical**: Brown (0.8, 0.6, 0.4)
- **Life**: Green (0.3, 1, 0.3)
- **Armor**: Gray (0.7, 0.7, 0.7)
- **Evasion**: Dark green (0.3, 0.8, 0.3)
- **Critical**: Light red (1, 0.5, 0.5)
- **Utility**: Light blue (0.8, 0.8, 1)
- **Elemental**: Orange (1, 0.8, 0.5)

### Board Properties
- **Core Board**: 15 max points, pre-allocated starting node
- **Element Boards**: 20 max points, theme-appropriate starting stats
- **Defense Boards**: 20 max points, defensive starting stats
- **Utility Boards**: 20 max points, utility-focused starting stats

## Assignment Rules

### Level-Based Unlocking
- **Level 5**: Fire Board
- **Level 10**: Cold Board
- **Level 15**: Lightning Board
- **Level 20**: Chaos Board
- **Level 25**: Physical Board
- **Level 30**: Life Board
- **Level 35**: Energy Shield Board
- **Level 40**: Attack Board
- **Level 45**: Armour Board
- **Level 50**: Evasion Board
- **Level 55**: Spell Damage Board
- **Level 60**: Armour/Evasion Board
- **Level 65**: Armour/ES Board
- **Level 70**: Evasion/ES Board
- **Level 75**: Card Mechanics Board
- **Level 80**: Discard Board

## Troubleshooting

### Common Issues

1. **No Boards Discovered**:
   - Check if ScriptableObject assets exist in `Resources/PassiveTree/`
   - Use the Board Asset Generator to create missing assets
   - Verify asset names match the expected format

2. **Invalid Asset Errors**:
   - Use "Clean Up Old Assets" in the Board Asset Generator
   - Regenerate assets using "Generate All Board Assets"
   - Check Console for specific error messages

3. **Board Not Available**:
   - Check assignment rules in `PassiveTreeBoardManager`
   - Verify character level meets unlock requirements
   - Use "Validate All Board Assets" to check asset integrity

### Debug Tools

1. **Board Asset Generator**:
   - `Tools > Passive Tree > Generate Board Assets`
   - Generate, validate, and clean up board assets

2. **ScriptableObjectBoardTest**:
   - Test ScriptableObject discovery and loading
   - Verify board data integrity

3. **PassiveTreeBoardManager Debug**:
   - Context menu "Get Debug Info"
   - Shows discovered boards and assignment status

## Migration from Old System

### Before (Shared CoreBoard)
- All boards referenced the same `CoreBoard.asset`
- Manual configuration required for each board
- Difficult to edit individual boards

### After (Individual ScriptableObjects)
- Each board has its own `.asset` file
- Automatic discovery and loading
- Easy individual editing in Inspector
- Proper asset management and version control

### Migration Steps
1. **Backup existing data** (if any)
2. **Generate new board assets** using the Board Asset Generator
3. **Test the new system** using ScriptableObjectBoardTest
4. **Update any custom scripts** that referenced the old system
5. **Remove old CoreBoard references** once migration is complete

## Best Practices

1. **Asset Naming**: Use consistent naming convention (e.g., `boardname_board.asset`)
2. **Theme Consistency**: Ensure board themes match their intended purpose
3. **Node Design**: Design nodes that fit the board's theme and purpose
4. **Extension Points**: Configure extension points for logical board connections
5. **Testing**: Always test board discovery and assignment after changes

## Future Enhancements

1. **Visual Board Editor**: In-editor board design tool
2. **Node Templates**: Pre-built node templates for common effects
3. **Board Validation**: Automated validation of board connections and balance
4. **Import/Export**: Tools to import/export board designs
5. **Board Variants**: Support for different board variants (e.g., seasonal themes)

## Support

For issues or questions about the ScriptableObject Board System:
1. Check the Console for error messages
2. Run the ScriptableObjectBoardTest for diagnostics
3. Use the Board Asset Generator's validation tools
4. Review this documentation for troubleshooting steps
