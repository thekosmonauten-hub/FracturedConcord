# Individual Board Scripts System Guide

## Overview

The Individual Board Scripts System replaces the shared `PassiveBoardScriptableObject` approach with dedicated ScriptableObject classes for each board type. This provides better organization, board-specific functionality, and easier maintenance.

## Key Benefits

- **Board-Specific Logic**: Each board has its own class with specialized behavior
- **Better Organization**: Clear separation of concerns for each board type
- **Easier Maintenance**: Changes to one board don't affect others
- **Type Safety**: Compile-time checking for board-specific operations
- **Extensibility**: Easy to add new board types with custom functionality

## System Architecture

### Core Components

1. **`BaseBoardScriptableObject`**: Abstract base class providing common functionality
2. **Individual Board Classes**: Specific implementations for each board type
3. **Board Manager Integration**: Updated to work with the new system
4. **Asset Generation**: Tools to create board-specific assets

### Class Hierarchy

```
BaseBoardScriptableObject (abstract)
├── CoreBoardScriptableObject
├── FireBoardScriptableObject
├── ColdBoardScriptableObject
├── LightningBoardScriptableObject
├── LifeBoardScriptableObject
├── DiscardBoardScriptableObject
└── [Other Board Classes...]
```

## File Structure

```
Assets/
├── Scripts/
│   └── Data/PassiveTree/
│       ├── BaseBoardScriptableObject.cs
│       ├── CoreBoardScriptableObject.cs
│       ├── FireBoardScriptableObject.cs
│       ├── ColdBoardScriptableObject.cs
│       ├── LightningBoardScriptableObject.cs
│       ├── LifeBoardScriptableObject.cs
│       ├── DiscardBoardScriptableObject.cs
│       └── [Other Board Classes...]
├── Resources/PassiveTree/
│   ├── core_board.asset
│   ├── fire_board.asset
│   ├── cold_board.asset
│   └── [Other Board Assets...]
└── Documentation/
    └── Individual_Board_Scripts_System_Guide.md
```

## Base Class Features

### `BaseBoardScriptableObject`

The abstract base class provides:

- **Common Properties**: `boardData`, `isCoreBoard`, `isKeystoneBoard`
- **Setup Methods**: `SetupBoard()`, `AddStartingNode()`, `AddBoardNodes()`
- **Node Creation Helpers**: `CreateSmallNode()`, `CreateNotableNode()`, `CreateTravelNode()`
- **Extension Point Management**: `AddExtensionPoints()`, connection methods
- **Event Handlers**: `OnNodeAllocated()`, `OnBoardUnlocked()`
- **Bonus Calculation**: `GetBoardBonuses()`

### Key Methods to Override

```csharp
// Override these in derived classes
protected virtual void AddBoardNodes() { }
protected virtual System.Collections.Generic.Dictionary<string, float> GetStartingStats() { }
protected virtual System.Collections.Generic.List<string> GetNorthConnections() { }
protected virtual System.Collections.Generic.List<string> GetSouthConnections() { }
protected virtual System.Collections.Generic.List<string> GetEastConnections() { }
protected virtual System.Collections.Generic.List<string> GetWestConnections() { }
public virtual void OnNodeAllocated(PassiveNode node) { }
public virtual void OnBoardUnlocked() { }
public virtual System.Collections.Generic.Dictionary<string, float> GetBoardBonuses() { }
```

## Board Implementations

### Core Board (`CoreBoardScriptableObject`)

- **Theme**: Utility
- **Starting Stats**: Health +10, Mana +10
- **Special Features**: Pre-allocated starting node
- **Connections**: All element boards, physical boards, utility boards

### Fire Board (`FireBoardScriptableObject`)

- **Theme**: Fire
- **Starting Stats**: Fire Damage +10, Burn Chance +5%
- **Special Features**: Fire damage bonuses, ignite mechanics
- **Notable Nodes**: Infernal Power, Burning Fury, Flame Conversion
- **Connections**: Fire keystone, chaos board

### Cold Board (`ColdBoardScriptableObject`)

- **Theme**: Cold
- **Starting Stats**: Cold Damage +10, Freeze Chance +5%
- **Special Features**: Cold damage bonuses, freeze mechanics
- **Notable Nodes**: Frozen Heart, Ice Mastery, Frost Conversion
- **Connections**: Cold keystone, fire board

### Life Board (`LifeBoardScriptableObject`)

- **Theme**: Life
- **Starting Stats**: Health +20, Health Regen +2
- **Special Features**: Life regeneration, vitality bonuses
- **Notable Nodes**: Vitality, Life Force, Life Conversion
- **Connections**: Physical board, armor/evasion boards

### Discard Board (`DiscardBoardScriptableObject`)

- **Theme**: Utility (Card Mechanics)
- **Starting Stats**: Discard Power +5, Mana +10
- **Special Features**: Discard mechanics, card system integration
- **Notable Nodes**: Discard Mastery, Discard Fury, Discard Conversion
- **Connections**: Card mechanics board

## Usage Guide

### Creating a New Board

1. **Create the Class**:
   ```csharp
   [CreateAssetMenu(fileName = "NewBoard", menuName = "Passive Tree/New Board")]
   public class NewBoardScriptableObject : BaseBoardScriptableObject
   {
       public override void SetupBoard()
       {
           // Set board properties
           boardData.id = "new_board";
           boardData.name = "New Board";
           boardData.theme = BoardTheme.YourTheme;
           
           // Initialize and add nodes
           boardData.InitializeBoard();
           AddStartingNode();
           AddBoardNodes();
           AddExtensionPoints();
       }
       
       protected override void AddBoardNodes()
       {
           // Add your board-specific nodes
       }
   }
   ```

2. **Override Required Methods**:
   - `AddBoardNodes()`: Add board-specific nodes
   - `GetStartingStats()`: Define starting bonuses
   - Connection methods: Define board connections
   - `OnNodeAllocated()`: Board-specific behavior

3. **Create the Asset**:
   - Right-click in Project window
   - Create > Passive Tree > New Board
   - Configure in Inspector

### Board-Specific Behavior

Each board can implement custom behavior:

```csharp
public override void OnNodeAllocated(PassiveNode node)
{
    base.OnNodeAllocated(node);
    
    // Board-specific logic
    if (node.stats.ContainsKey("fireIncrease"))
    {
        // Fire board specific behavior
        ApplyFireEffects(node.stats["fireIncrease"]);
    }
}

public override System.Collections.Generic.Dictionary<string, float> GetBoardBonuses()
{
    var bonuses = new System.Collections.Generic.Dictionary<string, float>();
    
    // Calculate board-specific bonuses
    var allocatedNodes = boardData.GetAllocatedNodes();
    foreach (var node in allocatedNodes)
    {
        // Aggregate bonuses from allocated nodes
    }
    
    return bonuses;
}
```

## Integration with Board Manager

The `PassiveTreeBoardManager` automatically discovers and loads board assets:

```csharp
// Board manager discovers all board types
var boardAssets = Resources.LoadAll<BaseBoardScriptableObject>("PassiveTree");

foreach (var boardAsset in boardAssets)
{
    // Each board asset is now properly typed
    if (boardAsset is FireBoardScriptableObject fireBoard)
    {
        // Fire board specific handling
    }
    else if (boardAsset is ColdBoardScriptableObject coldBoard)
    {
        // Cold board specific handling
    }
}
```

## Migration from Old System

### Before (Shared ScriptableObject)
- All boards used the same `PassiveBoardScriptableObject` class
- Limited to data-driven behavior
- Difficult to add board-specific logic

### After (Individual ScriptableObjects)
- Each board has its own class
- Board-specific behavior and logic
- Type-safe operations
- Easy to extend and maintain

### Migration Steps

1. **Backup existing data**
2. **Create new board classes** for each board type
3. **Update board manager** to work with new system
4. **Test each board** individually
5. **Remove old shared assets** once migration is complete

## Best Practices

### Board Design

1. **Consistent Naming**: Use clear, descriptive names for nodes and stats
2. **Theme Consistency**: Ensure board themes match their intended purpose
3. **Balanced Stats**: Design nodes with appropriate stat values
4. **Logical Connections**: Connect boards that make thematic sense

### Code Organization

1. **Single Responsibility**: Each board class handles only its own logic
2. **DRY Principle**: Use base class methods to avoid code duplication
3. **Clear Documentation**: Document board-specific behavior and requirements
4. **Type Safety**: Use strongly-typed operations where possible

### Performance

1. **Efficient Node Creation**: Use helper methods for common node types
2. **Cached Calculations**: Cache board bonuses when possible
3. **Event Optimization**: Only trigger events when necessary
4. **Memory Management**: Properly dispose of board resources

## Troubleshooting

### Common Issues

1. **Board Not Loading**:
   - Check asset is in `Resources/PassiveTree/` folder
   - Verify board class extends `BaseBoardScriptableObject`
   - Ensure `SetupBoard()` is called

2. **Nodes Not Appearing**:
   - Check `AddBoardNodes()` implementation
   - Verify node positions are within board bounds
   - Ensure nodes are properly added to board data

3. **Connections Not Working**:
   - Check connection method implementations
   - Verify board IDs match between connected boards
   - Ensure extension points are properly configured

### Debug Tools

1. **Board Validation**:
   ```csharp
   [ContextMenu("Validate Board")]
   public virtual void ValidateBoard()
   {
       // Validation logic
   }
   ```

2. **Debug Logging**:
   ```csharp
   Debug.Log($"[{GetType().Name}] Board-specific message");
   ```

3. **Inspector Debugging**:
   - Use `[Header]` and `[SerializeField]` for organization
   - Add debug fields for testing

## Future Enhancements

### Planned Features

1. **Visual Board Editor**: In-editor board design tool
2. **Node Templates**: Pre-built node templates for common effects
3. **Board Validation**: Automated validation of board connections
4. **Board Variants**: Support for different board variants
5. **Dynamic Board Loading**: Runtime board loading and modification

### Extension Points

1. **Custom Node Types**: Support for board-specific node types
2. **Advanced Effects**: Complex board-specific effects and mechanics
3. **Board Interactions**: Cross-board effects and synergies
4. **Conditional Logic**: Board behavior based on game state

## Support

For issues or questions about the Individual Board Scripts System:

1. Check the Console for error messages
2. Validate board configuration using debug tools
3. Review this documentation for troubleshooting steps
4. Test board functionality in isolation
5. Check board manager integration

## Conclusion

The Individual Board Scripts System provides a robust, extensible foundation for the passive tree system. Each board can now have its own unique behavior, making the system more maintainable and easier to extend with new board types and features.
