# Core Board Initialization Guide

## Overview

The Passive Tree system now automatically initializes the core board when entering the PassiveTreeScene. This ensures that the core board is always available without requiring manual setup.

## How It Works

### **Automatic Initialization**

When the `PassiveTreeTestController` starts, it automatically:

1. **Tries to load the core board from Resources** (`Assets/Resources/PassiveTree/core_board.asset`)
2. **Falls back to programmatic creation** if the asset isn't found
3. **Sets up the grid manager** with the loaded board
4. **Initializes the UI** for interaction

### **Initialization Process**

```
Start() → SetupDataManager() → InitializeCoreBoard()
                                    ↓
                            LoadCoreBoardFromResources()
                                    ↓
                            [Success] → SetupBoardWithAsset()
                            [Failure] → CreateCoreBoardProgrammatically()
```

## Configuration Options

### **Inspector Settings**

In the `PassiveTreeTestController` component:

| Setting | Description | Default |
|---------|-------------|---------|
| `Auto Initialize Core Board` | Enable/disable automatic initialization | `true` |
| `Load Core Board From Resources` | Try to load from Resources folder first | `true` |
| `Test Board Data` | Manual override (optional) | `null` |

### **Resource Loading Paths**

The system tries to load the core board from these paths in order:

1. `PassiveTree/core_board`
2. `PassiveTree/CoreBoard`
3. `CoreBoard`

## Usage

### **Automatic Setup (Recommended)**

1. **Open PassiveTreeScene**
2. **The core board initializes automatically**
3. **No manual setup required**

### **Manual Override**

If you want to use a specific board asset:

1. **Assign a `CoreBoardScriptableObject`** to the `Test Board Data` field
2. **Disable `Auto Initialize Core Board`** if you want full manual control
3. **The system will use your assigned board**

### **Testing and Debugging**

Use the context menu options (right-click on PassiveTreeTestController):

- **"Reinitialize Core Board"** - Re-run the initialization process
- **"Force Create Core Board Programmatically"** - Skip Resources loading
- **"Test Load Core Board From Resources"** - Test Resources loading only
- **"Show Board Status"** - Display current board state
- **"Validate Board"** - Validate the current board structure

## Troubleshooting

### **Common Issues**

#### **"Could not load core board from Resources"**
- **Cause**: Core board asset not found in Resources folder
- **Solution**: 
  1. Use the Board Asset Generator to create the core board asset
  2. Ensure it's saved in `Assets/Resources/PassiveTree/core_board.asset`
  3. Or let the system create it programmatically

#### **"GridManager not assigned"**
- **Cause**: GridManager reference missing in inspector
- **Solution**: Assign the GridManager component in the inspector

#### **"Board validation failed"**
- **Cause**: Board structure is invalid
- **Solution**: Use "Force Create Core Board Programmatically" to recreate

### **Debug Steps**

1. **Check Console Logs** for initialization messages
2. **Use "Show Board Status"** to verify current state
3. **Try "Reinitialize Core Board"** to restart the process
4. **Use "Test Load Core Board From Resources"** to test asset loading

## Integration with Board Asset Generator

### **Creating Core Board Assets**

1. **Open Board Asset Generator**: `Tools > Passive Tree > Generate Board Assets`
2. **Click "Generate Core Board"** or "Generate All Board Assets"
3. **Assets are created in**: `Assets/Resources/PassiveTree/core_board.asset`

### **Asset Discovery**

The system automatically discovers and uses:
- **CoreBoardScriptableObject** assets in Resources
- **Generated assets** from the Board Asset Generator
- **Manually created** core board assets

## Best Practices

### **Development Workflow**

1. **Use automatic initialization** for most cases
2. **Generate core board assets** using the Board Asset Generator
3. **Test with context menu options** for debugging
4. **Validate boards** before committing changes

### **Asset Management**

1. **Keep core board assets** in `Assets/Resources/PassiveTree/`
2. **Use consistent naming**: `core_board.asset`
3. **Validate assets** after generation
4. **Test both loading paths** (Resources and programmatic)

### **Scene Setup**

1. **Ensure PassiveTreeTestController** is in the scene
2. **Assign GridManager** reference
3. **Enable auto initialization** (default)
4. **Test on scene load**

## Advanced Configuration

### **Custom Initialization**

If you need custom initialization logic:

```csharp
// Override the initialization process
public class CustomPassiveTreeController : PassiveTreeTestController
{
    protected override void InitializeCoreBoard()
    {
        // Custom initialization logic
        Debug.Log("Custom core board initialization...");
        
        // Call base method or implement custom logic
        base.InitializeCoreBoard();
    }
}
```

### **Multiple Board Support**

The system can be extended to support multiple boards:

```csharp
// Load additional boards
private void LoadAdditionalBoards()
{
    var fireBoard = Resources.Load<FireBoardScriptableObject>("PassiveTree/fire_board");
    var coldBoard = Resources.Load<ColdBoardScriptableObject>("PassiveTree/cold_board");
    
    // Set up additional boards...
}
```

## Extension Point Visualization

### **Visual Representation**

Extension points are now automatically rendered in the grid as special visual elements:

- **Yellow squares with orange borders** - Extension points are visually distinct from regular nodes
- **Slightly larger than regular nodes** - Makes them easy to identify
- **Interactive** - Click to view information and available connections

### **Extension Point Features**

#### **Visual Indicators**
- **Color**: Yellow with orange border
- **Size**: 90% of tile size (larger than regular nodes)
- **Position**: Based on `extensionPoint.position` coordinates

#### **Interaction**
- **Click**: Shows extension point information in the info panel
- **Hover**: Logs extension point details to console
- **Information Display**: Shows available boards and connection status

#### **Available Information**
When an extension point is selected, the info panel shows:
- Extension point ID
- Position coordinates
- Current connections vs. maximum connections
- List of available boards for connection

### **Testing Extension Points**

Use the context menu options to test extension points:

1. **Right-click on PassiveTreeTestController**
2. **Select "Test Extension Points"**
3. **Check console for detailed extension point information**

### **Extension Point Data**

The core board includes 4 extension points:

| Extension Point | Position | Available Boards |
|-----------------|----------|------------------|
| `core_ext_top` | (0, 3) | fire_board, cold_board, lightning_board |
| `core_ext_left` | (3, 0) | physical_board, chaos_board |
| `core_ext_right` | (3, 6) | life_board, armor_board |
| `core_ext_bottom` | (6, 3) | evasion_board, critical_board |

### **Future Enhancements**

The extension point system is designed for future expansion:

- **Board Connection UI**: Click extension points to connect to available boards
- **Visual Connections**: Show lines between connected boards
- **Board Selection**: Choose which board to connect to
- **Connection Validation**: Ensure valid board connections

## Conclusion

The new automatic initialization system ensures that the core board is always available when entering the Passive Tree scene. This eliminates the need for manual setup and provides a robust fallback system for both development and production use.

The system is designed to be:
- **Zero-configuration** for basic use
- **Flexible** for advanced customization
- **Robust** with multiple fallback options
- **Debuggable** with comprehensive logging and testing tools

---

*For more information about the Board Asset Generator, see `Dynamic_Board_Asset_Generator_Guide.md`*
