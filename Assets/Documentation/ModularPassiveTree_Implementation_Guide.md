# Modular Passive Tree Implementation Guide

## Overview

The Modular Passive Tree System is a complete rewrite of the old grid-based passive tree approach, implementing a board-based architecture that provides better organization, extensibility, and maintainability. This system follows the existing CombatSceneManager integration approach (Option 1) as documented in the project preferences.

## System Architecture

### Core Components

#### 1. ModularPassiveTreeManager
- **Purpose**: Main interface for managing the modular passive tree system
- **Integration**: Follows CombatSceneManager approach with singleton pattern
- **Features**: Board management, auto-connections, theme-based organization
- **Location**: `Assets/Scripts/Data/PassiveTree/ModularPassiveTreeManager.cs`

#### 2. PassiveTreeDataManager (Updated)
- **Purpose**: Core data management with modular board support
- **Features**: Board creation, extension point management, connection handling
- **Backward Compatibility**: Maintains legacy system support
- **Location**: `Assets/Scripts/Data/PassiveTree/PassiveTreeDataManager.cs`

#### 3. ExtensionBoards Folder Structure
- **Organization**: Theme-based board organization
- **Structure**:
  ```
  ExtensionBoards/
  ├── Core/           # Core board implementations
  ├── Fire/           # Fire-themed boards
  ├── Cold/           # Cold-themed boards
  ├── Life/           # Life-themed boards
  └── Discard/        # Discard-themed boards
  ```

### Board System

#### Board Types
- **Core Board**: Starting point for the passive tree
- **Extension Boards**: Themed boards that connect to core or other extension boards
- **Theme Boards**: Specialized boards for specific character builds

#### Board Properties
```csharp
public class PassiveBoard
{
    public string id;                    // Unique identifier
    public string name;                  // Display name
    public string description;           // Board description
    public BoardTheme theme;             // Visual and functional theme
    public Vector2Int size;             // Grid dimensions (7x7, 9x9, etc.)
    public List<ExtensionPoint> extensionPoints; // Connection points
    public int maxPoints;                // Maximum skill points
    public Vector3Int worldPosition;     // Position in world space
}
```

#### Board Themes
- **General**: Basic passive skills
- **Fire**: Fire damage and burning effects
- **Cold**: Cold damage and freezing effects
- **Life**: Health and regeneration bonuses
- **Discard**: Card discard mechanics
- **Utility**: Movement and utility skills

## Implementation Steps

### Step 1: Setup ExtensionBoards Folder Structure

1. **Create ExtensionBoards folder** in `Assets/Scripts/Data/PassiveTree/`
2. **Create theme subfolders**:
   - Core
   - Fire
   - Cold
   - Life
   - Discard
3. **Move existing board ScriptableObjects** to appropriate theme folders

### Step 2: Configure ModularPassiveTreeManager

1. **Add ModularPassiveTreeManager component** to a GameObject in your scene
2. **Assign default core board** in the inspector
3. **Add available boards** to the available boards list
4. **Configure connection settings**:
   - Enable auto-connections
   - Set maximum boards per tree
   - Adjust connection check interval

### Step 3: Integration with CombatSceneManager

1. **Add ModularPassiveTreeManager** to your main game scene
2. **Reference the manager** in your CombatSceneManager or main game controller
3. **Subscribe to events** for board changes and tree updates

## Usage Examples

### Basic Setup

```csharp
// Get the modular passive tree manager
var treeManager = ModularPassiveTreeManager.Instance;

// Initialize the system
treeManager.InitializeModularSystem();

// Get all active boards
var activeBoards = treeManager.GetAllActiveBoards();

// Get boards by theme
var fireBoards = treeManager.GetBoardsByTheme(BoardTheme.Fire);
```

### Board Management

```csharp
// Add a new available board
treeManager.AddAvailableBoard(myFireBoard);

// Check if a board is active
bool isActive = treeManager.IsBoardActive("fire_board_01");

// Get tree structure information
string treeInfo = treeManager.GetTreeStructureInfo();
```

### Event Handling

```csharp
// Subscribe to board events
treeManager.OnBoardAdded += (board) => {
    Debug.Log($"Board added: {board.name}");
};

treeManager.OnTreeStructureChanged += () => {
    Debug.Log("Tree structure changed");
};

treeManager.OnBoardConnected += (connection) => {
    Debug.Log($"Boards connected: {connection.boardAId} -> {connection.boardBId}");
};
```

## Extension Board Development

### Creating New Extension Boards

#### 1. Create Board ScriptableObject
```csharp
[CreateAssetMenu(fileName = "New Fire Board", menuName = "Passive Tree/Fire Board")]
public class MyFireBoardScriptableObject : BaseBoardScriptableObject
{
    [Header("Fire-Specific Properties")]
    [SerializeField] private float fireDamageBonus = 25f;
    [SerializeField] private bool enableBurning = true;
    
    protected override void InitializeBoard()
    {
        base.InitializeBoard();
        
        // Set fire-specific properties
        boardData.theme = BoardTheme.Fire;
        boardData.name = "Infernal Board";
        boardData.description = "A board focused on fire damage and burning effects";
        
        // Add fire-themed nodes
        AddFireNodes();
    }
    
    private void AddFireNodes()
    {
        // Implementation for adding fire-themed passive nodes
    }
}
```

#### 2. Define Extension Points
```csharp
// In your board initialization
var extensionPoint = new ExtensionPoint
{
    id = "fire_extension_01",
    position = new Vector2Int(6, 3), // Bottom-right corner
    availableBoards = new List<string> { "fire_board_02", "fire_board_03" },
    maxConnections = 2,
    currentConnections = 0
};

boardData.extensionPoints.Add(extensionPoint);
```

#### 3. Add to ExtensionBoards Folder
- Place your new board ScriptableObject in the appropriate theme folder
- Ensure the board follows the naming convention: `[Theme]BoardScriptableObject.cs`
- Update the board's metadata and properties

### Board Connection Rules

#### Compatibility Matrix
- **Core Board**: Can connect to any theme
- **Fire Board**: Can connect to Fire, General, and Life themes
- **Cold Board**: Can connect to Cold, General, and Life themes
- **Life Board**: Can connect to any theme
- **Discard Board**: Can connect to General and Utility themes

#### Connection Validation
```csharp
// Check if boards can connect
bool canConnect = extensionPoint.availableBoards.Contains(board.id) &&
                  extensionPoint.currentConnections < extensionPoint.maxConnections;

// Validate board limits
if (treeManager.GetAllActiveBoards().Count >= maxBoardsPerTree)
{
    Debug.LogWarning("Maximum boards reached");
    return false;
}
```

## Testing and Validation

### Automated Testing

The system includes a comprehensive test suite (`ModularPassiveTreeTest.cs`) that validates:

1. **System Initialization**: Core system setup and initialization
2. **Core Board Creation**: Default board creation and configuration
3. **Board Management**: Adding, removing, and querying boards
4. **Board Connections**: Connection creation and validation
5. **Board Themes**: Theme-based organization and filtering
6. **System Reset**: Tree reset and cleanup functionality
7. **Error Handling**: Null parameter handling and edge cases

### Manual Testing Steps

1. **Setup Test Environment**:
   - Create a test scene with ModularPassiveTreeManager
   - Assign test boards to the manager
   - Enable debug logging

2. **Run Test Sequence**:
   - Use the test script's context menu "Run Tests"
   - Monitor console output for test results
   - Verify board creation and connections

3. **Validate Functionality**:
   - Check board positioning and connections
   - Verify theme-based organization
   - Test board limits and connection rules

## Performance Considerations

### Optimization Strategies

1. **Lazy Loading**: Boards are only created when needed
2. **Connection Caching**: Connection information is cached for quick access
3. **Event-Driven Updates**: Tree changes only trigger necessary updates
4. **Board Limits**: Configurable maximum boards prevent performance issues

### Memory Management

- **Board Cleanup**: Boards are properly disposed when removed
- **Event Unsubscription**: Events are cleaned up to prevent memory leaks
- **Resource Pooling**: Consider implementing object pooling for frequently created/destroyed boards

## Troubleshooting

### Common Issues

#### 1. Boards Not Connecting
- **Check**: Extension point compatibility
- **Verify**: Board limits and connection rules
- **Debug**: Enable debug logging in ModularPassiveTreeManager

#### 2. System Not Initializing
- **Check**: Component references in inspector
- **Verify**: Required ScriptableObjects are assigned
- **Debug**: Check console for initialization errors

#### 3. Performance Issues
- **Check**: Number of active boards
- **Verify**: Connection check intervals
- **Optimize**: Reduce board complexity or increase limits

### Debug Tools

1. **Debug Logging**: Enable `_showDebugInfo` in managers
2. **Test Scripts**: Use `ModularPassiveTreeTest` for validation
3. **Inspector Debugging**: Check component states in Unity inspector
4. **Console Monitoring**: Watch for error messages and warnings

## Future Enhancements

### Planned Features

1. **Advanced Board Themes**: More specialized theme types
2. **Dynamic Board Generation**: Procedural board creation
3. **Board Templates**: Pre-made board layouts for common builds
4. **Visual Improvements**: Better connection lines and board visualization
5. **Save/Load System**: Persistent tree state management

### Extension Points

1. **Custom Board Types**: User-defined board themes
2. **Board Modifiers**: Dynamic board property changes
3. **Connection Animations**: Smooth board connection transitions
4. **Board Search**: Find specific boards or nodes
5. **Build Validation**: Validate optimal passive tree builds

## Development Guidelines

### Code Standards

1. **Naming Conventions**: Use descriptive names for boards and components
2. **Documentation**: Include XML documentation for all public methods
3. **Error Handling**: Implement proper validation and error messages
4. **Event Usage**: Use events for loose coupling between components
5. **Performance**: Consider performance impact of board operations

### Testing Requirements

1. **Unit Tests**: Test individual board and connection logic
2. **Integration Tests**: Test board manager interactions
3. **Performance Tests**: Validate system performance with many boards
4. **Edge Case Tests**: Test boundary conditions and error scenarios

### Maintenance

1. **Regular Testing**: Run test suite after system changes
2. **Performance Monitoring**: Watch for performance degradation
3. **Documentation Updates**: Keep implementation guide current
4. **Code Reviews**: Review board implementations for consistency

---

## Conclusion

The Modular Passive Tree System provides a robust, extensible foundation for passive skill trees that integrates seamlessly with the existing CombatSceneManager approach. By following the implementation guide and development guidelines, developers can create complex, theme-based passive trees that enhance gameplay while maintaining performance and maintainability.

For additional support or questions, refer to the test scripts, debug tools, and comprehensive logging system included with the implementation.



