# Dynamic Board Asset Generator System

## Overview

The Dynamic Board Asset Generator is a Unity Editor utility that automatically discovers and generates ScriptableObject assets for passive tree boards. It replaces the old manual approach with a fully automated system that scales with your codebase.

## Key Features

### 🔍 **Automatic Discovery**
- Automatically detects all ScriptableObject classes that inherit from `BaseBoardScriptableObject`
- No need to manually register new board types
- Scales automatically as you add more board classes

### 🎯 **Dynamic UI**
- Generates buttons dynamically for all available board classes
- UI automatically updates when new board classes are added
- No hardcoded board lists to maintain

### 🏷️ **Smart Naming Convention**
- Automatically extracts board IDs from class names
- Converts PascalCase to snake_case (e.g., `FireBoardScriptableObject` → `fire_board`)
- Generates appropriate display names and descriptions

### ⚙️ **Automatic Configuration**
- Determines board themes based on class names
- Sets appropriate max points for different board types
- Creates sensible default descriptions

## How It Works

### 1. Discovery Process

The system uses reflection to find all available board ScriptableObject classes:

```csharp
// Finds all classes that inherit from BaseBoardScriptableObject
var boardTypes = GetBoardScriptableObjectTypes();

// For each class, creates a BoardDefinition
foreach (var boardType in boardTypes)
{
    var boardDef = CreateBoardDefinitionFromType(boardType);
}
```

### 2. Naming Convention

The system follows a consistent naming convention:

| Class Name | Extracted ID | Display Name | Asset Path |
|------------|--------------|--------------|------------|
| `CoreBoardScriptableObject` | `core_board` | `Core Board` | `core_board.asset` |
| `FireBoardScriptableObject` | `fire_board` | `Fire Board` | `fire_board.asset` |
| `ColdBoardScriptableObject` | `cold_board` | `Cold Board` | `cold_board.asset` |

**Process:**
1. Remove "ScriptableObject" suffix
2. Remove "Board" suffix (if present)
3. Convert PascalCase to snake_case
4. Add "_board" suffix

### 3. Dynamic Asset Creation

Uses reflection to create the correct ScriptableObject type:

```csharp
// Find the appropriate ScriptableObject type
Type boardType = FindBoardScriptableObjectType(boardId);

// Create instance using reflection
BaseBoardScriptableObject boardAsset = (BaseBoardScriptableObject)ScriptableObject.CreateInstance(boardType);
```

## Usage

### Accessing the Generator

1. **Unity Menu**: `Tools > Passive Tree > Generate Board Assets`
2. **Window Title**: "Board Asset Generator"

### Available Functions

#### **Generate All Board Assets**
- Automatically generates assets for all detected board classes
- Creates assets in `Assets/Resources/PassiveTree/`
- Logs progress and results

#### **Show Available Board Classes**
- Lists all detected board ScriptableObject classes
- Shows the mapping from class names to board IDs
- Useful for debugging and verification

#### **Individual Board Generation**
- Dynamically generated buttons for each available board class
- Click to generate a specific board asset
- Useful for selective generation

#### **Utility Functions**
- **Clean Up Old Assets**: Removes invalid or duplicate assets
- **Validate All Board Assets**: Checks the validity of existing assets

## Creating New Board Classes

### Step 1: Create the ScriptableObject Class

Create a new class that inherits from `BaseBoardScriptableObject`:

```csharp
using UnityEngine;
using PassiveTree;

[CreateAssetMenu(fileName = "LightningBoard", menuName = "Passive Tree/Lightning Board")]
public class LightningBoardScriptableObject : BaseBoardScriptableObject
{
    [ContextMenu("Setup Lightning Board")]
    public override void SetupBoard()
    {
        Debug.Log("[LightningBoardScriptableObject] Setting up lightning board...");
        
        // Set basic board properties
        boardData.id = "lightning_board";
        boardData.name = "Lightning Board";
        boardData.description = "Board focused on lightning damage and shock effects";
        boardData.theme = BoardTheme.Lightning;
        boardData.size = new Vector2Int(7, 7);
        boardData.maxPoints = 20;
        boardData.boardColor = new Color(1f, 1f, 0.3f); // Yellow for lightning
        isCoreBoard = false;
        
        // Initialize the board
        boardData.InitializeBoard();
        
        // Add starting node
        AddStartingNode();
        
        // Add board-specific nodes
        AddBoardNodes();
        
        // Add extension points
        AddExtensionPoints();
        
        Debug.Log("[LightningBoardScriptableObject] Lightning board setup complete!");
    }
    
    protected override System.Collections.Generic.Dictionary<string, float> GetStartingStats()
    {
        return new System.Collections.Generic.Dictionary<string, float>
        {
            { "lightningDamage", 10f },
            { "shockChance", 5f }
        };
    }
    
    protected override void AddBoardNodes()
    {
        // Add lightning-specific nodes here
        // Example:
        AddNode(CreateSmallNode("lightning_1_1", "Lightning Strike", "+10 Lightning Damage", 1, 1, 
            new System.Collections.Generic.Dictionary<string, float> { { "lightningDamage", 10f } }), 1, 1);
    }
}
```

### Step 2: Follow Naming Convention

Ensure your class follows the naming convention:
- **Format**: `[BoardName]BoardScriptableObject`
- **Examples**: 
  - `LightningBoardScriptableObject`
  - `ChaosBoardScriptableObject`
  - `PhysicalBoardScriptableObject`

### Step 3: Generate Assets

1. Open the Board Asset Generator (`Tools > Passive Tree > Generate Board Assets`)
2. Click "Show Available Board Classes" to verify your class is detected
3. Click "Generate All Board Assets" or the specific board button
4. Assets will be created in `Assets/Resources/PassiveTree/`

## System Architecture

### Core Components

#### **PassiveBoardAssetGenerator**
- Main EditorWindow class
- Handles UI and user interactions
- Coordinates asset generation process

#### **BoardDefinition**
- Data structure for board metadata
- Contains ID, name, description, theme, max points, and ScriptableObject type
- Used for dynamic UI generation

#### **Discovery Methods**
- `GetBoardScriptableObjectTypes()`: Finds all board classes using reflection
- `CreateBoardDefinitionFromType()`: Creates BoardDefinition from class type
- `ExtractBoardIdFromClassName()`: Converts class names to board IDs

#### **Generation Methods**
- `GenerateBoardAsset()`: Creates individual board assets
- `FindBoardScriptableObjectType()`: Maps board IDs to class types
- `GetThemeColor()` and `GetThemeStats()`: Provides theme-specific defaults

### File Structure

```
Assets/
├── Scripts/
│   ├── Editor/
│   │   └── PassiveBoardAssetGenerator.cs          # Main generator
│   └── Data/PassiveTree/
│       ├── BaseBoardScriptableObject.cs           # Base class
│       ├── CoreBoardScriptableObject.cs           # Core board
│       ├── FireBoardScriptableObject.cs           # Fire board
│       ├── ColdBoardScriptableObject.cs           # Cold board
│       ├── DiscardBoardScriptableObject.cs        # Discard board
│       └── LifeBoardScriptableObject.cs           # Life board
└── Resources/PassiveTree/
    ├── core_board.asset                           # Generated assets
    ├── fire_board.asset
    ├── cold_board.asset
    ├── discard_board.asset
    └── life_board.asset
```

## Benefits

### ✅ **Zero Maintenance**
- Add new board classes → They automatically appear in the generator
- No need to update hardcoded lists or switch statements
- System scales automatically

### ✅ **Consistent Naming**
- Follows established naming conventions
- Automatically generates appropriate IDs and display names
- Reduces naming errors and inconsistencies

### ✅ **Future-Proof**
- Works with any number of board classes
- No limits on board types or themes
- Easy to extend with new features

### ✅ **Developer-Friendly**
- Clear feedback about available classes
- Helpful error messages when classes are missing
- Intuitive UI that updates dynamically

## Troubleshooting

### Common Issues

#### **No Board Classes Detected**
- **Cause**: Classes don't inherit from `BaseBoardScriptableObject`
- **Solution**: Ensure all board classes inherit from `BaseBoardScriptableObject`

#### **Compilation Errors**
- **Cause**: Missing using directives or incorrect inheritance
- **Solution**: Check that classes have proper using statements and inheritance

#### **Assets Not Generated**
- **Cause**: Class naming doesn't follow convention
- **Solution**: Ensure class names follow `[BoardName]BoardScriptableObject` pattern

#### **Invalid Assets**
- **Cause**: Old assets using deprecated system
- **Solution**: Use "Clean Up Old Assets" to remove invalid assets

### Debugging Tips

1. **Use "Show Available Board Classes"** to verify detection
2. **Check Console Logs** for detailed error messages
3. **Validate Assets** to check existing asset status
4. **Clean Up Old Assets** to remove problematic files

## Integration with Existing Systems

### **PassiveTreeBoardManager**
- Automatically discovers new board assets
- Loads assets using the new individual ScriptableObject system
- Maintains backward compatibility

### **PassiveTreeDataManager**
- Works with the new board system
- Saves and loads board data correctly
- Integrates with character progression

### **UI Components**
- `GridManager` and `PassiveTreeTestController` updated to use `BaseBoardScriptableObject`
- Maintains compatibility with existing UI
- Supports new board types automatically

## Best Practices

### **Class Creation**
1. Always inherit from `BaseBoardScriptableObject`
2. Follow the naming convention: `[BoardName]BoardScriptableObject`
3. Override `SetupBoard()` method for board-specific logic
4. Use `[CreateAssetMenu]` attribute for manual asset creation

### **Asset Management**
1. Use the generator for bulk asset creation
2. Validate assets regularly
3. Clean up old assets when migrating systems
4. Keep board classes in the `Assets/Scripts/Data/PassiveTree/` folder

### **Development Workflow**
1. Create new board ScriptableObject class
2. Test with "Show Available Board Classes"
3. Generate assets using the generator
4. Validate assets are created correctly
5. Test integration with existing systems

## Future Enhancements

### **Planned Features**
- **Custom Themes**: Support for custom board themes
- **Advanced Validation**: More sophisticated asset validation
- **Batch Operations**: Support for batch asset operations
- **Template System**: Predefined board templates

### **Extension Points**
- **Custom Generators**: Support for custom asset generation logic
- **Plugin System**: Allow third-party board type extensions
- **Advanced Naming**: Support for custom naming conventions

## Conclusion

The Dynamic Board Asset Generator provides a robust, scalable solution for managing passive tree board assets. It eliminates manual maintenance, ensures consistency, and makes the system future-proof. By following the established patterns and best practices, developers can easily extend the system with new board types while maintaining the integrity of the overall architecture.

---

*This documentation should be updated as the system evolves. For questions or issues, refer to the troubleshooting section or check the console logs for detailed error messages.*
