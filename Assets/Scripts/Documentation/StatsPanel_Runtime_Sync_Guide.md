# StatsPanel Runtime Synchronization Guide

## Overview
The StatsPanelRuntime component has been synchronized with the Editor Window to provide **exact** functionality in-game. All features, data structures, and logic from the Editor Window are now available in the runtime version.

## Changes Applied

### 1. Enhanced Character Data Handling
- ✅ **Character Validation**: Checks for empty/invalid character data
- ✅ **PlayerPrefs Fallback**: Attempts to load character from PlayerPrefs if CharacterManager data is empty
- ✅ **Comprehensive Debug Logging**: Detailed logging for troubleshooting
- ✅ **Test Data Fallback**: Uses test data if no valid character is found

### 2. Updated Attribute Data Structure
- ✅ **5-Row Display**: Matches Editor Window's comprehensive attribute display
- ✅ **Row 1**: Current total values (STR/DEX/INT)
- ✅ **Row 2**: Expected base values at current level
- ✅ **Row 3**: Bonuses from items/equipment
- ✅ **Row 4**: Health/Mana/Energy Shield values
- ✅ **Row 5**: Combat stats (Attack/Defense/Crit)

### 3. Enhanced Class System Support
- ✅ **Primary Classes**: Marauder, Ranger, Witch (single attribute focus)
- ✅ **Hybrid Classes**: Brawler, Thief, Apostle (dual attribute focus)
- ✅ **Base Stats Calculation**: Proper level-up progression
- ✅ **Bonus Calculation**: Accurate item/equipment bonus display

### 4. Complete Data Section Support
- ✅ **Attributes**: 5 rows with comprehensive stat breakdown
- ✅ **Resources**: Health/Mana/Energy Shield/Reliance with bonuses
- ✅ **Damage**: All 5 damage types (Physical, Fire, Cold, Lightning, Chaos)
- ✅ **Resistances**: Current/Max format with bonuses

## Runtime Setup Instructions

### Step 1: Create Test Scene
1. Create a new scene called "StatsPanelRuntimeTest"
2. Save it in `Assets/Scenes/`

### Step 2: Set Up StatsPanel GameObject
1. Create an empty GameObject named "StatsPanel"
2. Add the following components:
   - `UIDocument` component
   - `StatsPanelRuntime` component

### Step 3: Configure UIDocument
1. Select the StatsPanel GameObject
2. In the UIDocument component inspector:
   - **Panel Settings**: Create new or use existing PanelSettings
   - **Visual Tree Asset**: Leave empty (will auto-assign)
   - **Style Sheet**: Leave empty (will auto-assign)

### Step 4: Test Character Setup (Optional)
1. Create an empty GameObject named "CharacterManager"
2. Add the `CharacterManager` component
3. Create a test character or load existing character data

### Step 5: Test the Runtime Panel
1. Press Play in Unity
2. The StatsPanel should automatically display data
3. Check the Console for debug messages

## Expected Runtime Behavior

### Debug Console Messages
Look for these successful messages:
```
[StatsPanelRuntime] Awake() called
[StatsPanelRuntime] Auto-assigned UIDocument: True
[StatsPanelRuntime] Auto-assigned VisualTreeAsset: True
[StatsPanelRuntime] Auto-assigned StyleSheet: True
[StatsPanelRuntime] InitializeUI() started
[StatsPanelRuntime] visualTreeAsset set successfully
[StatsPanelRuntime] Style sheet loaded: StatsPanel
[StatsPanelRuntime] UI References - Name: True, Class: True, Level: True, Exp: True
[StatsPanelRuntime] ListView References - Attributes: True, Resources: True, Damage: True, Resistances: True
[StatsPanelRuntime] CharacterManager.Instance: CharacterManager
[StatsPanelRuntime] CharacterManager found, HasCharacter: True/False
[StatsPanelRuntime] Created test data - Attributes: 5, Resources: 2, Damage: 5, Resistances: 2
[StatsPanelRuntime] Setting up AttributeListView with 5 items
[StatsPanelRuntime] Found 3 columns for Attribute
[StatsPanelRuntime] AttributeListView setup complete
[StatsPanelRuntime] Setting up ResourceListView with 2 items
[StatsPanelRuntime] Found 4 columns for Resource
[StatsPanelRuntime] ResourceListView setup complete
[StatsPanelRuntime] Setting up DamageListView with 5 items
[StatsPanelRuntime] Found 4 columns for Damage
[StatsPanelRuntime] DamageListView setup complete
[StatsPanelRuntime] Setting up ResistanceListView with 2 items
[StatsPanelRuntime] Found 4 columns for Resistance
[StatsPanelRuntime] ResistanceListView setup complete
[StatsPanelRuntime] All list views refreshed
```

### Visual Display (Test Data)
The runtime panel should display exactly the same as the Editor Window:

#### **Attributes Section** (5 rows)
```
Strength    | Dexterity  | Intelligence
32          | 14         | 14
32          | 14         | 14
+0          | +0         | +0
Health: 420/420 | Mana: 3/3 | ES: 28/28
Attack: 78 | Defense: 28 | Crit: 7.0%
```

#### **Resources Section** (2 rows)
```
Health      | Mana       | Energy Shield | Reliance
420/420     | 3/3        | 28/28         | 75%
+50         | +10        | +15           | +5%
```

#### **Damage Section** (5 rows)
```
Type        | Flat       | Increased     | More
Physical    | 50         | 110%          | 30%
Fire        | 50         | 110%          | 30%
Cold        | 50         | 110%          | 30%
Lightning   | 50         | 110%          | 30%
Chaos       | 50         | 110%          | 30%
```

#### **Resistances Section** (2 rows)
```
Fire        | Cold       | Lightning     | Chaos
75/75       | 60/60      | 45/45         | 25/25
+10         | +15        | +20           | +5
```

## Character Data Integration

### With Valid Character Data
If a valid character is loaded:
1. **Character Info**: Displays actual character name, class, level
2. **Experience Bar**: Shows current experience progress
3. **Attributes**: Calculates base stats, bonuses, and combat stats
4. **Resources**: Shows actual health/mana/energy shield values
5. **Damage**: Displays character's damage stats
6. **Resistances**: Shows character's resistance values

### With Invalid/Empty Character Data
If no valid character is found:
1. **PlayerPrefs Fallback**: Attempts to load from PlayerPrefs
2. **Test Data**: Falls back to comprehensive test data
3. **Debug Logging**: Provides detailed information about the fallback process

## Troubleshooting

### If Panel Doesn't Display
1. Check that UIDocument component is attached
2. Verify StatsPanelRuntime component is attached
3. Check Console for error messages
4. Ensure UXML and USS files are in the correct location

### If Data Doesn't Match Editor Window
1. Check that all setup methods are being called
2. Verify data lists are being populated correctly
3. Check Console for debug messages
4. Ensure character data is valid (if using real character)

### If Character Data Not Loading
1. Check CharacterManager.Instance exists
2. Verify character data is properly initialized
3. Check PlayerPrefs for character name
4. Look for fallback to test data messages

## Success Criteria
The runtime synchronization is successful when:
- ✅ Runtime panel displays identical data to Editor Window
- ✅ All four sections show correct data
- ✅ Character data integration works (if character available)
- ✅ Test data fallback works (if no character)
- ✅ Debug console shows all expected messages
- ✅ No compilation or runtime errors
- ✅ Panel responds to character data changes

## Integration with Game Systems

### CharacterManager Integration
The runtime panel automatically integrates with:
- CharacterManager.Instance for character data
- PlayerPrefs for character persistence
- Character class for detailed stat calculations

### Refresh Capability
The panel can be refreshed at runtime:
```csharp
// Get reference to StatsPanelRuntime component
StatsPanelRuntime statsPanel = FindObjectOfType<StatsPanelRuntime>();

// Refresh the panel
statsPanel.RefreshPanel();
```

This ensures the runtime StatsPanel provides **exactly** the same functionality and data display as the Editor Window!
