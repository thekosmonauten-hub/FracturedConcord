# Passive Tree Persistent Data Integration Guide

## Overview

This guide covers the implementation of a **persistent data system** for the passive tree that integrates seamlessly with the character stats system. The system ensures that passive tree allocations persist across game sessions and provide real-time stat bonuses to characters.

## System Architecture

### Core Components

1. **PassiveTreeData** - Persistent data structure for storing tree state
2. **PassiveTreeDataManager** - Manager for saving/loading and data operations
3. **PassiveTreeCharacterIntegration** - Integration with character stats system
4. **Updated PassiveTreeTestController** - Test controller with persistent data support

### Data Flow

```
Passive Tree UI → PassiveTreeDataManager → PassiveTreeData (JSON) → Character Stats
     ↑                                                                    ↓
     └─────────────── Real-time Updates ←────────────────────────────────┘
```

## Implementation Details

### 1. PassiveTreeData Structure

The `PassiveTreeData` class stores all persistent information about a character's passive tree:

```csharp
[System.Serializable]
public class PassiveTreeData
{
    // Character reference
    public string characterId;
    public string characterName;
    
    // Allocated nodes with ranks
    public List<AllocatedNodeData> allocatedNodes = new List<AllocatedNodeData>();
    
    // Board connections
    public List<BoardConnectionData> boardConnections = new List<BoardConnectionData>();
    
    // Point management
    public int availablePoints = 0;
    public int totalPointsEarned = 0;
    
    // Board state
    public List<string> unlockedBoards = new List<string>();
    
    // Metadata
    public DateTime lastModified;
    public int version = 1;
}
```

#### AllocatedNodeData
```csharp
[System.Serializable]
public class AllocatedNodeData
{
    public string nodeId;
    public int currentRank;
    public int maxRank;
    public DateTime allocatedAt;
    public DateTime lastModified;
}
```

#### BoardConnectionData
```csharp
[System.Serializable]
public class BoardConnectionData
{
    public string sourceBoardId;
    public string targetBoardId;
    public string extensionPointId;
    public DateTime connectedAt;
}
```

### 2. PassiveTreeDataManager

The manager handles all data persistence operations:

#### Key Features
- **Singleton Pattern** - Single instance across the game
- **Auto-save** - Automatic saving every 30 seconds
- **Event System** - Notifies when data changes
- **JSON Persistence** - Saves to `Application.persistentDataPath/PassiveTreeData/`

#### Core Methods
```csharp
// Load or create tree data for a character
public PassiveTreeData LoadOrCreateTreeData(string characterId, string characterName)

// Allocate/deallocate nodes
public bool AllocateNode(string nodeId, int rank = 1)
public bool DeallocateNode(string nodeId, int rank = 1)

// Point management
public void AddPoints(int points)
public bool SpendPoints(int points)

// Get total passive stats
public Dictionary<string, float> GetTotalPassiveStats(PassiveBoard[] boards)

// Apply stats to character
public void ApplyPassiveStatsToCharacter(CharacterStatsData characterStats, PassiveBoard[] boards)
```

#### Events
```csharp
public event Action<PassiveTreeData> OnTreeDataChanged;
public event Action<string> OnNodeAllocated;
public event Action<string> OnNodeDeallocated;
public event Action OnPointsChanged;
```

### 3. PassiveTreeCharacterIntegration

This component provides real-time integration between passive tree data and character stats:

#### Key Features
- **Real-time Updates** - Automatically updates character stats when passives change
- **Change Detection** - Only updates when stats actually change
- **Performance Optimized** - Caches stats to avoid unnecessary calculations
- **Event-driven** - Responds to passive tree changes

#### Core Methods
```csharp
// Set character stats to integrate with
public void SetCharacterStats(CharacterStatsData characterStats)

// Set available boards for stat calculation
public void SetAvailableBoards(PassiveBoard[] boards)

// Update passive stats and apply to character
public void UpdatePassiveStats()

// Get passive stat information
public float GetPassiveStat(string statName)
public Dictionary<string, float> GetAllPassiveStats()
public string GetPassiveStatsSummary()
```

## Setup Instructions

### Step 1: Create Data Manager

1. **Create GameObject** in your scene:
   ```
   GameObject → Create Empty → Name: "PassiveTreeDataManager"
   ```

2. **Add Component**:
   ```
   Add Component → PassiveTree → PassiveTreeDataManager
   ```

3. **Configure Settings**:
   - **Save Directory**: "PassiveTreeData" (default)
   - **Auto Save**: true (recommended)
   - **Auto Save Interval**: 30 seconds
   - **Enable Debug Logs**: true (for development)

### Step 2: Create Character Integration

1. **Create GameObject** in your scene:
   ```
   GameObject → Create Empty → Name: "PassiveTreeCharacterIntegration"
   ```

2. **Add Component**:
   ```
   Add Component → PassiveTree → PassiveTreeCharacterIntegration
   ```

3. **Configure Settings**:
   - **Enable Auto Integration**: true
   - **Update Interval**: 0.1 seconds
   - **Enable Debug Logs**: true

4. **Assign Boards**:
   - Drag your `PassiveBoard` assets to the "Available Boards" array

### Step 3: Connect to Character Stats

In your character initialization script:

```csharp
// Get the integration component
var integration = FindObjectOfType<PassiveTreeCharacterIntegration>();

// Set character stats
integration.SetCharacterStats(characterStats);

// Set available boards
PassiveBoard[] boards = { coreBoard, fireBoard, coldBoard }; // Your boards
integration.SetAvailableBoards(boards);
```

### Step 4: Update Your Test Controller

The `PassiveTreeTestController` has been updated to use persistent data:

1. **Add UI Buttons** (optional):
   - Save Button → `SaveTreeData()`
   - Load Button → `LoadTreeData()`

2. **Configure Test Settings**:
   - **Test Character ID**: "test_character"
   - **Test Character Name**: "Test Character"

## Usage Examples

### Basic Node Allocation

```csharp
// Get the data manager
var dataManager = PassiveTreeDataManager.Instance;

// Load character data
dataManager.LoadOrCreateTreeData("player_001", "My Character");

// Add points
dataManager.AddPoints(5);

// Allocate a node
bool success = dataManager.AllocateNode("strength_node", 1);
if (success)
{
    dataManager.SpendPoints(1);
}
```

### Real-time Stat Integration

```csharp
// Get the integration component
var integration = FindObjectOfType<PassiveTreeCharacterIntegration>();

// Set up integration
integration.SetCharacterStats(characterStats);
integration.SetAvailableBoards(availableBoards);

// Stats are automatically updated when nodes are allocated/deallocated
```

### Manual Stat Updates

```csharp
// Force a stat update
integration.UpdatePassiveStats();

// Get current passive stats
var passiveStats = integration.GetAllPassiveStats();

// Get specific stat
float strengthBonus = integration.GetPassiveStat("strength");
```

### Save/Load Operations

```csharp
// Manual save
dataManager.SaveCurrentTreeData();

// Manual load
dataManager.LoadOrCreateTreeData(characterId, characterName);

// Reset tree
dataManager.ResetCurrentTree();

// Delete character data
dataManager.DeleteCharacterTreeData(characterId);
```

## File Structure

### Save Files
```
Application.persistentDataPath/
└── PassiveTreeData/
    ├── passive_tree_player_001.json
    ├── passive_tree_player_002.json
    └── passive_tree_test_character.json
```

### JSON Format Example
```json
{
  "characterId": "player_001",
  "characterName": "My Character",
  "allocatedNodes": [
    {
      "nodeId": "core_main",
      "currentRank": 1,
      "maxRank": 1,
      "allocatedAt": "2024-12-19T10:30:00",
      "lastModified": "2024-12-19T10:30:00"
    },
    {
      "nodeId": "strength_node",
      "currentRank": 2,
      "maxRank": 3,
      "allocatedAt": "2024-12-19T10:31:00",
      "lastModified": "2024-12-19T10:32:00"
    }
  ],
  "boardConnections": [],
  "availablePoints": 3,
  "totalPointsEarned": 10,
  "unlockedBoards": ["core_board"],
  "lastModified": "2024-12-19T10:32:00",
  "version": 1
}
```

## Integration with Existing Systems

### Character Stats System

The passive tree stats are automatically applied to the `CharacterStatsData` system:

```csharp
// Passive stats are added to character stats
characterStats.AddToStat("strength", passiveStrengthBonus);
characterStats.AddToStat("health", passiveHealthBonus);
```

### Combat System

Since passive stats are applied to character stats, they automatically affect combat calculations:

```csharp
// Combat system uses character stats (which include passive bonuses)
float damage = CalculateDamage(characterStats, card, enemy);
```

### Equipment System

Passive tree stats work alongside equipment stats:

```csharp
// Equipment stats are applied first
characterStats.ApplyEquipmentStats(equipmentStats);

// Passive tree stats are applied on top
integration.UpdatePassiveStats();
```

## Performance Considerations

### Optimization Features

1. **Change Detection** - Only updates when stats actually change
2. **Caching** - Caches applied stats to avoid unnecessary calculations
3. **Event-driven Updates** - Only recalculates when nodes are allocated/deallocated
4. **Configurable Update Intervals** - Adjust update frequency as needed

### Best Practices

1. **Use Auto-save** - Enable auto-save for automatic persistence
2. **Set Appropriate Update Intervals** - Balance responsiveness with performance
3. **Limit Board References** - Only include boards that are actually available
4. **Use Debug Logs** - Enable during development, disable in production

## Debugging

### Debug Methods

```csharp
// Get debug information
integration.GetDebugInfo();

// Manual stat update
integration.ManualUpdatePassiveStats();

// Get stats summary
string summary = integration.GetPassiveStatsSummary();
Debug.Log(summary);
```

### Common Issues

1. **Stats Not Updating**
   - Check if `SetCharacterStats()` was called
   - Verify boards are assigned in integration component
   - Check if auto-integration is enabled

2. **Data Not Saving**
   - Verify data manager exists in scene
   - Check auto-save is enabled
   - Look for file permission issues

3. **Performance Issues**
   - Increase update interval
   - Reduce number of available boards
   - Disable debug logs in production

## Testing

### Test Scripts

The updated `PassiveTreeTestController` provides comprehensive testing:

1. **Node Allocation** - Test persistent node allocation
2. **Point Management** - Test point spending and earning
3. **Save/Load** - Test data persistence
4. **Stat Integration** - Test real-time stat updates

### Test Scenarios

1. **New Character** - Verify initial state creation
2. **Node Allocation** - Test node allocation and stat updates
3. **Game Restart** - Verify data persistence across sessions
4. **Multiple Characters** - Test separate data for different characters
5. **Stat Calculation** - Verify correct stat bonuses

## Future Enhancements

### Planned Features

1. **Cloud Save Integration** - Save data to cloud services
2. **Backup/Restore** - Manual backup and restore functionality
3. **Data Migration** - Version migration for data format changes
4. **Analytics** - Track passive tree usage and popular builds
5. **Build Sharing** - Share passive tree builds between players

### Technical Improvements

1. **Binary Serialization** - Faster save/load with binary format
2. **Compression** - Compress save files for smaller storage
3. **Incremental Updates** - Only save changed data
4. **Background Saving** - Save data in background threads

## Conclusion

The persistent passive tree data integration system provides a robust foundation for character progression that persists across game sessions while maintaining real-time integration with the character stats system. The modular design ensures easy maintenance and future expansion.

Key benefits:
- **Persistent Data** - Allocations survive game restarts
- **Real-time Integration** - Stats update immediately
- **Performance Optimized** - Efficient change detection and caching
- **Event-driven** - Clean separation of concerns
- **Comprehensive Testing** - Full test coverage and debugging tools

This system serves as the foundation for a complete character progression system that can grow with your game while maintaining excellent performance and user experience.
