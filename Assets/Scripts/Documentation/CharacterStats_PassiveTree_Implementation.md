# Character Stats & Passive Tree Implementation

## Overview

This document describes the implementation of two core systems for the Dexiled game:

1. **Character Stats System** - A comprehensive data handler for managing hundreds of character statistics
2. **Passive Tree System** - A modular board-based passive skill tree system

Both systems are designed to be highly extensible, compatible with existing affix/stat systems, and provide rich UI experiences.

## Character Stats System

### Core Components

#### CharacterStatsData.cs
The central data structure that manages all character statistics.

**Key Features:**
- **Comprehensive Stat Coverage**: Manages 100+ different statistics across multiple categories
- **Affix System Compatibility**: Uses string-based stat names for easy integration with existing affix systems
- **Dynamic Stat Management**: Supports flat, increased, more, and set modifiers
- **Equipment Integration**: Tracks equipment stats and applies them to character calculations
- **Temporary Buffs**: Supports temporary stat modifications with duration tracking

**Stat Categories:**
- Core Attributes (Strength, Dexterity, Intelligence)
- Combat Resources (Health, Energy Shield, Mana, Reliance, Guard)
- Combat Stats (Attack Power, Defense, Critical Chance, etc.)
- Damage Modifiers (Increased/Added damage for all damage types)
- Resistances (Physical, Elemental, Chaos)
- Defense Stats (Armour, Evasion, Block/Dodge chances)
- Recovery Stats (Regeneration, Leech)
- Combat Mechanics (Attack Speed, Cast Speed, etc.)
- Card System Stats (Cards drawn, Hand size, etc.)
- Passive Tree Stats (Points, Allocated nodes)

**Key Methods:**
```csharp
// Get/Set stat values by name
float GetStatValue(string statName)
void SetStatValue(string statName, float value)
void AddToStat(string statName, float value)
void MultiplyStat(string statName, float multiplier)

// Apply equipment and temporary effects
void ApplyEquipmentStats(Dictionary<string, float> equipmentStats)
void ApplyTemporaryBuff(string statName, float value, float duration)
void RemoveTemporaryBuff(string statName)

// Formatting and display
string GetStatDisplayString(string statName)
Dictionary<string, float> GetAllStats()
```

#### CharacterStatsController.cs
UI controller for displaying character statistics in a comprehensive, categorized interface.

**Key Features:**
- **Categorized Display**: Organizes stats into logical sections (Core, Combat, Damage, etc.)
- **Dynamic Updates**: Real-time updates when stats change
- **Resource Visualization**: Visual bars for Health, Energy Shield, Mana, Reliance
- **Color Coding**: Positive/negative stat values are color-coded
- **Event Integration**: Subscribes to CharacterManager and PassiveTreeManager events

**UI Sections:**
- Character Info (Name, Class, Level, Experience)
- Resource Bars (Health, Energy Shield, Mana, Reliance)
- Core Attributes
- Combat Stats
- Damage Modifiers
- Resistances
- Defense Stats
- Recovery Stats
- Combat Mechanics
- Card System Stats
- Equipment Summary

## Passive Tree System

### Core Components

#### PassiveNode.cs (ScriptableObject)
Defines individual passive skill nodes with effects and requirements.

**Key Features:**
- **ScriptableObject Design**: Easy to create and manage in Unity Editor
- **Flexible Effects**: Supports multiple stat modifiers per node
- **Requirement System**: Level, class, and prerequisite node requirements
- **Visual Customization**: Icons, colors, sizes based on node type
- **Grid Positioning**: Uses Vector2 for flexible positioning

**Node Types:**
- Normal: Basic stat nodes
- Notable: Medium-sized nodes with significant effects
- Keystone: Large nodes with build-defining effects
- Mastery: Small utility nodes
- Starting: Character starting points
- Extension: Connection points between boards

**Effect Types:**
- Flat: Direct stat additions
- Increased: Percentage-based additions
- More: Multiplicative modifiers
- Set: Override stat values

#### PassiveBoard.cs (ScriptableObject)
Defines modular passive tree boards with nodes, connections, and themes.

**Key Features:**
- **Modular Design**: 7x7 grid-based boards that can be connected
- **Theme System**: Different visual themes (Fire, Cold, Lightning, etc.)
- **Connection System**: Nodes can be connected with different line types
- **Extension Points**: Boards can connect to other boards
- **Size Variants**: Small (5x5), Medium (7x7), Large (9x9), ExtraLarge (11x11)

**Board Themes:**
- General, Fire, Cold, Lightning, Chaos
- Physical, Life, Energy, Attack, Defense
- Utility, Movement, CardSystem

**Connection Types:**
- Straight: Direct line connections
- Curved: Smooth curved connections
- Zigzag: Angular connections

#### PassiveTreeManager.cs
Singleton manager that orchestrates the entire passive tree system.

**Key Features:**
- **Board Management**: Tracks unlocked boards and connections
- **Node Allocation**: Handles point spending and node allocation
- **Tree Validation**: Ensures tree connectivity when deallocating nodes
- **Character Integration**: Applies passive effects to character stats
- **Persistence**: Saves/loads tree state per character

**Core Methods:**
```csharp
// Node management
bool AllocateNode(string nodeId)
bool DeallocateNode(string nodeId)
bool CanAllocateNode(string nodeId)

// Board management
void UnlockBoard(string boardId)
bool ConnectBoards(string boardAId, string boardBId, string extensionPointId)

// Tree validation
bool WouldBreakTree(string nodeId)
bool IsTreeConnected(List<string> nodeIds)

// State management
void SavePassiveTreeState()
void LoadPassiveTreeState()
```

#### PassiveTreeController.cs
UI controller for the passive tree interface.

**Key Features:**
- **Board Selection**: Visual board selection with theme colors
- **Node Interaction**: Click to select, allocate, or deallocate nodes
- **Connection Visualization**: Displays node connections with proper positioning
- **Node Information Panel**: Shows detailed node information and requirements
- **Tree Statistics**: Displays point allocation and board information

**Visual Features:**
- Color-coded nodes based on type and allocation status
- Dynamic node sizing based on node type
- Tooltip system for node information
- Board navigation with previous/next buttons

## Integration with Existing Systems

### Character Manager Integration
Both systems integrate seamlessly with the existing CharacterManager:

```csharp
// CharacterStatsData automatically syncs with Character
characterStats = new CharacterStatsData(character);

// PassiveTreeManager subscribes to character events
CharacterManager.Instance.OnCharacterLoaded += OnCharacterLoaded;
CharacterManager.Instance.OnCharacterLevelUp += OnCharacterLevelUp;
```

### Equipment System Integration
Character stats automatically include equipment bonuses:

```csharp
// Equipment stats are applied to character stats
characterStats.ApplyEquipmentStats(equipmentStats);

// Equipment changes trigger stat updates
EquipmentManager.Instance.OnEquipmentChanged += RefreshStats;
```

### Affix System Compatibility
The stat system uses string-based stat names for easy affix integration:

```csharp
// Affixes can directly modify stats
characterStats.AddToStat("increasedPhysicalDamage", 25f);
characterStats.MultiplyStat("criticalMultiplier", 1.2f);
```

## Usage Examples

### Creating a Passive Node
```csharp
// In Unity Editor: Right-click > Create > Dexiled > Passive Tree > Passive Node
// Or programmatically:
var node = ScriptableObject.CreateInstance<PassiveNode>();
node.nodeId = "strength_node";
node.nodeName = "Strength";
node.description = "Increases your strength by {strength}";
node.pointCost = 1;
node.nodeType = PassiveNode.NodeType.Normal;
node.gridPosition = new Vector2(3, 4);

// Add stat modifier
var modifier = new PassiveNode.StatModifier();
modifier.statName = "strength";
modifier.modifierType = PassiveNode.StatModifier.ModifierType.Flat;
modifier.value = 5f;
node.statModifiers.Add(modifier);
```

### Creating a Passive Board
```csharp
// In Unity Editor: Right-click > Create > Dexiled > Passive Tree > Passive Board
// Or programmatically:
var board = ScriptableObject.CreateInstance<PassiveBoard>();
board.boardId = "strength_board";
board.boardName = "Strength Board";
board.boardTheme = PassiveBoard.BoardTheme.Physical;
board.gridRows = 7;
board.gridColumns = 7;
board.maxPoints = 20;

// Add nodes to board
board.AddNode(strengthNode);

// Add connections
board.AddConnection("node1", "node2");
```

### Using Character Stats
```csharp
// Get character stats
var stats = new CharacterStatsData(character);

// Modify stats
stats.AddToStat("strength", 10f);
stats.MultiplyStat("criticalMultiplier", 1.5f);

// Apply temporary buff
stats.ApplyTemporaryBuff("attackSpeed", 0.5f, 10f);

// Get formatted display string
string displayValue = stats.GetStatDisplayString("strength"); // "25"
```

### Using Passive Tree Manager
```csharp
// Get manager instance
var treeManager = PassiveTreeManager.Instance;

// Allocate a node
bool success = treeManager.AllocateNode("strength_node");

// Get available nodes
var availableNodes = treeManager.GetAvailableNodes();

// Get tree statistics
var stats = treeManager.GetPassiveTreeStatistics();
Debug.Log($"Allocated {stats.allocatedPassivePoints} of {stats.totalPassivePoints} points");
```

## UI Setup Guide

### Character Stats UI Setup
1. Create a Canvas with ScrollRect for the stats display
2. Add CharacterStatsController component
3. Create stat row prefab with "StatName" and "StatValue" TextMeshPro components
4. Set up resource bars (Health, Energy Shield, Mana, Reliance)
5. Create section GameObjects for each stat category
6. Assign all UI references in the inspector

### Passive Tree UI Setup
1. Create a Canvas with ScrollRect for the tree display
2. Add PassiveTreeController component
3. Create prefabs for:
   - Board (with Image component)
   - Node (with Button and Image components)
   - Connection Line (with Image component)
   - Board Selection Button (with Button and TextMeshPro components)
4. Set up node information panel with TextMeshPro components
5. Create board selection panel
6. Assign all UI references in the inspector

## File Structure
```
Assets/Scripts/
├── Data/
│   ├── CharacterStatsData.cs
│   └── PassiveTree/
│       ├── PassiveNode.cs
│       ├── PassiveBoard.cs
│       └── PassiveTreeManager.cs
├── UI/
│   ├── CharacterStats/
│   │   └── CharacterStatsController.cs
│   ├── PassiveTree/
│   │   └── PassiveTreeController.cs
│   └── TooltipTrigger.cs
└── Documentation/
    └── CharacterStats_PassiveTree_Implementation.md
```

## Future Enhancements

### Character Stats System
- **Stat History**: Track stat changes over time
- **Stat Comparison**: Compare before/after equipment changes
- **Custom Stat Categories**: Allow user-defined stat groupings
- **Stat Export**: Export character stats to external formats

### Passive Tree System
- **Board Templates**: Pre-made board layouts for common builds
- **Build Sharing**: Share passive tree builds between players
- **Advanced Connections**: Curved and animated connection lines
- **Board Themes**: More visual themes and customization options
- **Node Search**: Search functionality for finding specific nodes
- **Build Validation**: Validate builds for optimal stat distribution

## Performance Considerations

### Character Stats System
- **Lazy Calculation**: Only recalculate stats when needed
- **Event-Driven Updates**: Use events to trigger updates instead of polling
- **Caching**: Cache frequently accessed stat values

### Passive Tree System
- **Object Pooling**: Reuse UI objects for better performance
- **Lazy Loading**: Load board data only when needed
- **Spatial Partitioning**: Optimize node lookup for large trees

## Testing

### Unit Tests
- Test stat calculations and modifications
- Test node allocation and deallocation
- Test tree connectivity validation
- Test board connection logic

### Integration Tests
- Test character stats with equipment
- Test passive tree with character progression
- Test UI updates with stat changes

### Performance Tests
- Test with large numbers of stats
- Test with complex passive trees
- Test UI responsiveness with many nodes

This implementation provides a solid foundation for character progression and customization while maintaining flexibility for future enhancements.











