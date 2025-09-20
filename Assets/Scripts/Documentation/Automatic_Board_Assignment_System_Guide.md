# Automatic Board Assignment System Guide

## Overview

The **Automatic Board Assignment System** eliminates the need for manual board configuration in the passive tree integration. Instead of manually assigning boards to the integration component, the system automatically detects and assigns boards based on player progression and game conditions.

## Key Benefits

- **No Manual Configuration**: Boards are automatically detected and assigned
- **Scalable**: Works with 9-12 boards without performance issues
- **Dynamic**: Boards unlock based on player progression
- **Event-Driven**: Real-time updates when boards become available
- **Flexible**: Supports both automatic and manual assignment modes

## System Architecture

### Core Components

1. **PassiveTreeBoardManager** - Automatic board detection and assignment
2. **Updated PassiveTreeCharacterIntegration** - Automatic board integration
3. **BoardAssignmentRule** - Rules for when boards become available
4. **BoardCondition** - Conditions that trigger board availability

### Data Flow

```
Board Discovery → Board Assignment Rules → Available Boards → Character Integration
      ↑                                                              ↓
      └─────────────── Real-time Updates ←──────────────────────────┘
```

## Implementation Details

### 1. PassiveTreeBoardManager

The board manager automatically discovers and manages board availability:

#### Key Features
- **Automatic Discovery**: Scans Resources folder and ScriptableObjects for boards
- **Rule-Based Assignment**: Uses configurable rules to determine board availability
- **Event System**: Notifies when boards are unlocked/locked
- **Performance Optimized**: Efficient caching and update intervals

#### Core Methods
```csharp
// Board discovery
public void DiscoverAllBoards()
public void UpdateBoardAvailability()

// Board access
public List<PassiveBoard> GetAvailableBoards()
public List<PassiveBoard> GetAllBoards()
public PassiveBoard GetBoard(string boardId)

// Manual control
public void UnlockBoard(string boardId)
public bool IsBoardAvailable(string boardId)
```

#### Events
```csharp
public event Action<List<PassiveBoard>> OnBoardsDiscovered;
public event Action<string> OnBoardUnlocked;
public event Action<string> OnBoardLocked;
public event Action<List<PassiveBoard>> OnAvailableBoardsChanged;
```

### 2. Board Assignment Rules

Rules define when boards become available based on game conditions:

#### Default Rules
```csharp
// Core board is always available
new BoardAssignmentRule
{
    ruleName = "Core Board Always Available",
    boardId = "core_board",
    condition = BoardCondition.Always,
    priority = 0
}

// Fire board unlocked by level 5
new BoardAssignmentRule
{
    ruleName = "Fire Board Level 5",
    boardId = "fire_board",
    condition = BoardCondition.LevelReached,
    conditionValue = 5,
    priority = 1
}
```

#### Available Conditions
- **Always**: Board is always available
- **LevelReached**: Board unlocked at specific character level
- **PointsSpent**: Board unlocked after spending X points
- **NodesAllocated**: Board unlocked after allocating X nodes
- **BoardsUnlocked**: Board unlocked after unlocking X boards
- **CustomCondition**: Custom condition evaluation

### 3. Updated PassiveTreeCharacterIntegration

The integration component now supports automatic board assignment:

#### Key Changes
- **Automatic Mode**: Uses board manager for automatic assignment
- **Manual Override**: Still supports manual board assignment
- **Event-Driven**: Responds to board availability changes
- **Seamless Integration**: No changes needed to existing stat calculation

#### Configuration
```csharp
[Header("Board Management")]
[SerializeField] private bool useAutomaticBoardAssignment = true;
[SerializeField] private PassiveBoard[] manualBoards; // Fallback
```

## Setup Instructions

### Step 1: Create Board Manager

1. **Create GameObject** in your scene:
   ```
   GameObject → Create Empty → Name: "PassiveTreeBoardManager"
   ```

2. **Add Component**:
   ```
   Add Component → PassiveTree → PassiveTreeBoardManager
   ```

3. **Configure Settings**:
   - **Enable Auto Detection**: true (recommended)
   - **Detection Interval**: 1 second
   - **Enable Debug Logs**: true (for development)
   - **Scan Resources Folder**: true
   - **Scan ScriptableObjects**: true

### Step 2: Update Character Integration

1. **Enable Automatic Assignment**:
   ```
   PassiveTreeCharacterIntegration → Use Automatic Board Assignment: true
   ```

2. **Remove Manual Board Assignment**:
   - Clear the "Manual Boards" array
   - Or keep as fallback for testing

### Step 3: Configure Board Assignment Rules

The system includes default rules for 12 boards:

| Board | Unlock Condition | Level |
|-------|------------------|-------|
| Core Board | Always Available | - |
| Fire Board | Level 5 | 5 |
| Cold Board | Level 10 | 10 |
| Lightning Board | Level 15 | 15 |
| Chaos Board | Level 20 | 20 |
| Physical Board | Level 25 | 25 |
| Life Board | Level 30 | 30 |
| Energy Board | Level 35 | 35 |
| Attack Board | Level 40 | 40 |
| Defense Board | Level 45 | 45 |
| Utility Board | Level 50 | 50 |
| Movement Board | Level 55 | 55 |

### Step 4: Create Board Assets

1. **Create ScriptableObject**:
   ```
   Right-click → Create → Dexiled → Passive Tree → Passive Board
   ```

2. **Configure Board**:
   - **Board ID**: Must match assignment rules (e.g., "fire_board")
   - **Board Name**: Display name
   - **Theme**: Visual theme
   - **Size**: Grid dimensions
   - **Nodes**: Add passive nodes

3. **Place in Resources**:
   ```
   Assets/Resources/PassiveTree/YourBoard.asset
   ```

## Usage Examples

### Basic Automatic Assignment

```csharp
// Get the integration component
var integration = FindObjectOfType<PassiveTreeCharacterIntegration>();

// Enable automatic assignment (default)
integration.EnableAutomaticBoardAssignment();

// Set character stats
integration.SetCharacterStats(characterStats);

// Boards are automatically assigned and updated
```

### Manual Override (if needed)

```csharp
// Override with manual boards
PassiveBoard[] manualBoards = { fireBoard, coldBoard };
integration.SetAvailableBoards(manualBoards);

// Switch back to automatic
integration.EnableAutomaticBoardAssignment();
```

### Board Manager Access

```csharp
// Get board manager
var boardManager = PassiveTreeBoardManager.Instance;

// Get available boards
var availableBoards = boardManager.GetAvailableBoards();

// Check specific board
bool isFireBoardAvailable = boardManager.IsBoardAvailable("fire_board");

// Manually unlock board (for testing)
boardManager.UnlockBoard("fire_board");
```

### Custom Board Assignment Rules

```csharp
// Create custom rule
var customRule = new BoardAssignmentRule
{
    ruleName = "Custom Fire Board Rule",
    boardId = "fire_board",
    condition = BoardCondition.PointsSpent,
    conditionValue = 20,
    priority = 1
};

// Add to board manager
boardManager.assignmentRules = new BoardAssignmentRule[] { customRule };
```

## Event Handling

### Subscribe to Board Events

```csharp
// Subscribe to board manager events
var boardManager = PassiveTreeBoardManager.Instance;

boardManager.OnBoardUnlocked += (boardId) => {
    Debug.Log($"Board unlocked: {boardId}");
    // Update UI, show notification, etc.
};

boardManager.OnAvailableBoardsChanged += (boards) => {
    Debug.Log($"Available boards changed: {boards.Count} boards");
    // Update UI, recalculate stats, etc.
};
```

### Integration Events

```csharp
// The integration component automatically handles board changes
// No additional event handling needed for stat calculations
```

## Performance Considerations

### Optimization Features

1. **Caching**: Boards are cached after discovery
2. **Update Intervals**: Configurable detection intervals
3. **Event-Driven**: Only updates when necessary
4. **Efficient Discovery**: Optimized board scanning

### Best Practices

1. **Use Default Rules**: Default rules are optimized for performance
2. **Limit Custom Rules**: Too many custom rules can impact performance
3. **Appropriate Intervals**: Set detection interval based on needs
4. **Disable Debug Logs**: Disable in production for better performance

## Debugging

### Debug Methods

```csharp
// Get board manager debug info
boardManager.GetDebugInfo();

// Force board discovery
boardManager.ForceBoardDiscovery();

// Force availability update
boardManager.ForceBoardAvailabilityUpdate();

// Get integration debug info
integration.GetDebugInfo();
```

### Common Issues

1. **Boards Not Discovered**
   - Check if boards are in Resources folder
   - Verify board IDs match assignment rules
   - Check debug logs for discovery errors

2. **Boards Not Available**
   - Check assignment rules and conditions
   - Verify character level integration
   - Check if boards are properly configured

3. **Performance Issues**
   - Increase detection interval
   - Reduce number of custom rules
   - Disable debug logs in production

## Migration from Manual Assignment

### Before (Manual Assignment)
```csharp
// Old way - manual assignment
integration.SetAvailableBoards(new PassiveBoard[] { 
    coreBoard, fireBoard, coldBoard, lightningBoard 
});
```

### After (Automatic Assignment)
```csharp
// New way - automatic assignment
integration.EnableAutomaticBoardAssignment();
// Boards are automatically assigned based on player progression
```

### Migration Steps

1. **Enable Automatic Assignment**:
   ```csharp
   integration.EnableAutomaticBoardAssignment();
   ```

2. **Remove Manual Board Arrays**:
   - Clear manual board assignments
   - Remove hardcoded board references

3. **Update Board Assets**:
   - Ensure board IDs match assignment rules
   - Place boards in Resources folder

4. **Test Board Progression**:
   - Verify boards unlock at correct levels
   - Test stat calculations with different board combinations

## Future Enhancements

### Planned Features

1. **Advanced Conditions**: More complex unlock conditions
2. **Board Dependencies**: Boards that require other boards
3. **Progressive Unlocking**: Boards that unlock in stages
4. **Custom Events**: Integration with game events
5. **Analytics**: Track board usage and progression

### Technical Improvements

1. **Async Discovery**: Background board discovery
2. **Compression**: Optimize board data storage
3. **Caching**: More sophisticated caching strategies
4. **Validation**: Board configuration validation

## Conclusion

The Automatic Board Assignment System provides a robust, scalable solution for managing passive tree boards without manual configuration. It automatically adapts to player progression and ensures optimal performance even with 9-12 boards.

Key benefits:
- **Zero Configuration**: Works out of the box
- **Scalable**: Handles any number of boards efficiently
- **Dynamic**: Responds to player progression
- **Maintainable**: Easy to add new boards and rules
- **Performance Optimized**: Efficient caching and updates

This system eliminates the need for manual board assignment while providing the flexibility to override when needed, making it perfect for both development and production use.
