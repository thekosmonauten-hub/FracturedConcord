# Extension Board Positioning System - Implementation Guide

## 🎯 **Overview**

The Extension Board Positioning System provides perfect orthographic alignment for extension boards using a hierarchical grid container approach. This system seamlessly integrates with your existing CellContainer.prefab and ExtensionPoint system.

## 🏗️ **System Architecture**

### **Hierarchical Grid Structure**
```
Boards Container (Parent Grid - Unity Grid Component)
├── CoreBoard (Grid Position: 0,0)
│   └── Grid (Cell Container) - 7x7 cells using CellContainer.prefab
├── ExtensionBoard_North (Grid Position: 0,1)
│   └── Grid (Cell Container) - 7x7 cells using CellContainer.prefab
├── ExtensionBoard_South (Grid Position: 0,-1)
│   └── Grid (Cell Container) - 7x7 cells using CellContainer.prefab
├── ExtensionBoard_East (Grid Position: 1,0)
│   └── Grid (Cell Container) - 7x7 cells using CellContainer.prefab
└── ExtensionBoard_West (Grid Position: -1,0)
    └── Grid (Cell Container) - 7x7 cells using CellContainer.prefab
```

### **Key Components**

1. **BoardPositioningManager** - Main positioning and alignment system
2. **ExtensionBoardGenerator** - Creates extension boards using CellContainer.prefab
3. **ExtensionBoardController** - Controls individual extension boards
4. **Integration with PassiveTreeManager** - Seamless integration with existing system

---

## 🚀 **Setup Instructions**

### **Step 1: Add Components to Scene**

1. **Add BoardPositioningManager** to your PassiveTreeManager GameObject:
   ```csharp
   // The PassiveTreeManager will automatically create these if they don't exist
   // Or manually add them in the inspector:
   // - BoardPositioningManager component
   // - ExtensionBoardGenerator component
   ```

2. **Configure Prefab References** in BoardPositioningManager:
   - **Core Board Prefab**: Assign your existing CoreBoard.prefab
   - **Extension Board Prefab**: Assign your CellContainer.prefab (or create a template)

3. **Configure Grid Settings**:
   - **Grid Cell Size**: (64, 64) - matches your existing system

### **Step 2: Automatic Cell Scaling & Collider Fix**

The system automatically handles cell scaling and collider sizing:

- **✅ Cell Scaling**: Automatically fixes all cells to 1.0x scale on start
- **✅ BoxCollider Size**: Automatically sets all cell colliders to 1.72x1.72
- **✅ Manual Fix Available**: Right-click BoardPositioningManager → "Fix All Board Cell Scaling & Colliders"

**What gets fixed automatically:**
- CoreBoard cells (from prefab instantiation)
- Extension board cells (during generation)
- Any existing cells with incorrect scaling or collider size

### **Step 3: Configure ExtensionBoardGenerator**

1. **Assign CellContainer Prefab**:
   - Set `cellContainerPrefab` to your existing CellContainer.prefab

2. **Configure Board Themes**:
   ```csharp
   // Add theme data for different board types
   BoardThemeData[] availableThemes = {
       new BoardThemeData { theme = BoardTheme.Fire, ... },
       new BoardThemeData { theme = BoardTheme.Cold, ... },
       new BoardThemeData { theme = BoardTheme.Life, ... }
   };
   ```

3. **Set Node Sprites**:
   - Assign appropriate sprites for different node types
   - Configure theme-specific sprites

### **Step 3: Test the System**

1. **Enable Extension Boards** in PassiveTreeManager:
   ```csharp
   // In inspector, check "Enable Extension Boards"
   // Or via code:
   passiveTreeManager.SetExtensionBoardsEnabled(true);
   ```

2. **Test Extension Board Spawning**:
   ```csharp
   // Use context menu: "Spawn Test Extension Board"
   // Or via code:
   passiveTreeManager.SpawnTestExtensionBoard();
   ```

---

## 🎮 **How It Works**

### **Perfect Orthographic Positioning**

The system uses Unity's Grid component for pixel-perfect alignment:

```csharp
// Board positioning calculation
Vector3 worldPosition = boardsContainer.TransformPoint(
    new Vector3(gridPosition.x, gridPosition.y, 0)
);
```

**Grid Coordinates:**
- **Core Board**: (0, 0)
- **North Extension**: (0, 1)
- **South Extension**: (0, -1)
- **East Extension**: (1, 0)
- **West Extension**: (-1, 0)

### **Extension Point Integration**

Extension points are automatically created at board edges:

```csharp
// Extension point creation
ExtensionPoint extensionPoint = new ExtensionPoint
{
    id = $"core_extension_{direction.x}_{direction.y}",
    position = GetBoardEdgePosition(direction),
    worldPosition = new Vector3Int(extensionGridPosition.x, extensionGridPosition.y, 0),
    availableBoards = new List<string> { "fire_board", "cold_board", "life_board" },
    maxConnections = 1,
    currentConnections = 0
};
```

### **CellContainer.prefab Integration**

Each extension board uses your existing CellContainer.prefab:

```csharp
// Cell generation using existing prefab
GameObject cell = Instantiate(cellContainerPrefab, board.transform);
cell.name = $"Cell_{x}_{y}";
cell.transform.position = board.transform.TransformPoint(new Vector3(x, y, 0));
```

---

## 🔧 **Usage Examples**

### **Basic Extension Board Spawning**

```csharp
// Get the board positioning manager
var positioningManager = passiveTreeManager.GetBoardPositioningManager();

// Get available extension points
var availablePoints = positioningManager.GetAvailableExtensionPoints();

// Spawn an extension board
if (availablePoints.Count > 0)
{
    var extensionPoint = availablePoints[0];
    bool success = positioningManager.SpawnExtensionBoard(extensionPoint, "fire_board");
    
    if (success)
    {
        Debug.Log("Extension board spawned successfully!");
    }
}
```

### **Custom Board Generation**

```csharp
// Get the board generator
var boardGenerator = passiveTreeManager.GetBoardGenerator();

// Generate a custom extension board
GameObject customBoard = boardGenerator.GenerateExtensionBoard(
    BoardTheme.Fire, 
    "CustomFireBoard"
);
```

### **Board Management**

```csharp
// Get all positioned boards
var allBoards = positioningManager.GetAllPositionedBoards();

// Get board at specific position
GameObject northBoard = positioningManager.GetBoardAtPosition(new Vector2Int(0, 1));

// Remove a board
bool removed = positioningManager.RemoveBoard(new Vector2Int(0, 1));
```

---

## 🎨 **Visual Debugging**

### **Grid Gizmos**

Enable grid visualization in the inspector:
- Check "Show Grid Gizmos" in BoardPositioningManager
- Grid lines will be drawn in the Scene view
- Positioned boards will be highlighted in green
- Extension points will be shown as yellow spheres

### **Debug Methods**

Use context menu methods for testing:
- **"Spawn Test Extension Boards"** - Spawns test boards in all directions
- **"Debug Board Positions"** - Shows all board positions in console
- **"Debug Board Info"** - Shows detailed board information

---

## ⚙️ **Configuration Options**

### **BoardPositioningManager Settings**

```csharp
[Header("Board Container Settings")]
public Transform boardsContainer;           // Parent container for all boards
public GameObject coreBoardPrefab;          // Core board prefab
public GameObject extensionBoardPrefab;     // Extension board template
public float boardSpacing = 7f;            // Distance between board centers

[Header("Grid Settings")]
public Vector2Int gridCellSize = new Vector2Int(900, 900); // Unity Grid cell size (128px × 7 cells = 896px + spacing = 900px per board)
public Vector2Int boardSize = new Vector2Int(7, 7);        // Board dimensions
public int cellPixelSize = 128;                            // Individual cell size in pixels
```

### **ExtensionBoardGenerator Settings**

```csharp
[Header("Prefab References")]
public GameObject cellContainerPrefab;      // Your existing CellContainer.prefab
public Sprite[] nodeSprites;               // Default node sprites
public Sprite extensionPointSprite;        // Extension point sprite

[Header("Board Configuration")]
public Vector2Int boardSize = new Vector2Int(7, 7);  // Board dimensions
public float cellSpacing = 1f;                       // Cell spacing
public Vector2 cellSize = new Vector2(1f, 1f);       // Cell size
```

---

## 🧪 **Testing Checklist**

### **Basic Functionality**
- [ ] Core board spawns at position (0, 0)
- [ ] Extension boards spawn in correct orthographic positions
- [ ] All boards use CellContainer.prefab correctly
- [ ] Extension points are created at board edges
- [ ] Grid alignment is pixel-perfect

### **Extension Board Spawning**
- [ ] North extension spawns at (0, 1)
- [ ] South extension spawns at (0, -1)
- [ ] East extension spawns at (1, 0)
- [ ] West extension spawns at (-1, 0)
- [ ] Multiple extensions can be spawned simultaneously

### **Integration Testing**
- [ ] Extension nodes in core board trigger board spawning
- [ ] New extension boards create their own extension points
- [ ] Board removal works correctly
- [ ] System integrates with existing PassiveTreeManager

### **Visual Testing**
- [ ] Grid gizmos display correctly
- [ ] Board positioning is visually perfect
- [ ] Extension points are visible and clickable
- [ ] No overlapping or misaligned boards

---

## 🚨 **Troubleshooting**

### **Common Issues**

#### **Boards Not Aligning Properly**
- **Check**: Grid cell size matches your existing system (64x64)
- **Check**: Board spacing is set correctly (7f)
- **Check**: Unity Grid component is properly configured

#### **Extension Boards Not Spawning**
- **Check**: CellContainer.prefab is assigned
- **Check**: Extension boards are enabled in PassiveTreeManager
- **Check**: Available extension points exist

#### **Extension Points Not Working**
- **Check**: ExtensionPoint.cs is properly integrated
- **Check**: Board edge positions are calculated correctly
- **Check**: Extension point sprites are assigned

### **Debug Steps**

1. **Enable Debug Logging**:
   ```csharp
   // Set showDebugInfo = true in all components
   ```

2. **Use Context Menu Methods**:
   - Right-click on components in inspector
   - Use debug methods to test functionality

3. **Check Console Output**:
   - Look for error messages
   - Verify successful operations

---

## 🎉 **Success Indicators**

### **Perfect Alignment** ✅
- All boards are positioned exactly where they should be
- No visual gaps or overlaps between boards
- Grid lines align perfectly with board edges

### **Seamless Integration** ✅
- Extension boards use existing CellContainer.prefab
- Extension points work with existing system
- No breaking changes to existing functionality

### **Scalable System** ✅
- Can spawn unlimited extension boards
- System handles multiple boards efficiently
- Easy to add new board types and themes

---

## 🚀 **Next Steps**

### **Immediate**
1. **Test the system** with your existing CoreBoard.prefab
2. **Configure board themes** for different extension types
3. **Add custom sprites** for theme-specific nodes

### **Future Enhancements**
1. **Save/Load system** for board configurations
2. **Visual connection lines** between boards
3. **Board templates** for common configurations
4. **Advanced positioning** with custom layouts

---

## Step 3: Camera Centering

The `BoardPositioningManager` automatically centers the camera on the core board's center cell (3,3) at runtime:

### Automatic Camera Centering
- **Enabled by default**: `autoCenterCameraOnCoreBoard = true`
- **Target position**: Core board center cell (3,3)
- **Camera controller**: Automatically finds `SimpleCameraController` in the scene
- **Smooth movement**: Uses the camera controller's smooth focusing system

### Manual Camera Control
- **Context menu**: Right-click on `BoardPositioningManager` → "Center Camera on Core Board"
- **Public method**: `CenterCameraOnCoreBoardManual()` for script access
- **Debug logging**: Shows camera centering actions when `showDebugInfo = true`

### Camera Settings
```csharp
[Header("Camera Settings")]
[SerializeField] private bool autoCenterCameraOnCoreBoard = true;
[SerializeField] private SimpleCameraController cameraController;
```

The camera will automatically focus on the center of your core board when it's created, providing the perfect starting view for your passive tree system!

## Troubleshooting Extension Board Placement

If extension boards are not positioning correctly (overlapping, wrong orientation, or disjointed placement):

### **Step 1: Check Grid Cell Size**
- **Problem**: `gridCellSize` set to individual cell size (e.g., 64x64)
- **Solution**: Set `gridCellSize` to match your board pixel size (128px × 7 cells = 896px + spacing = 900px)
- **Why**: Grid cell size should represent entire board spacing in pixels, not individual cell size
- **Formula**: `gridCellSize = cellPixelSize × boardSize + spacing` (e.g., 128 × 7 + 4 = 900)

### **Step 1.5: Verify Grid Positioning Logic**
- **Problem**: Extension boards spawning at same location instead of proper grid positions
- **Root Cause**: Extension points using `direction` instead of `direction * boardSize` for grid positioning
- **Solution**: Fixed to use `coreBoardGridPosition + (direction * boardSize)` for proper spacing
- **Result**: Extension boards now spawn at correct distances (7 units apart) around core board

### **Step 1.6: Fix Core Board World Position Reference** ✅
- **Problem**: CoreBoard not positioned at world (0,0), causing extension boards to spawn in wrong locations
- **Root Cause**: Extension board positioning assumed CoreBoard was at world (0,0)
- **Solution**: Detect CoreBoard's actual world position and use it as reference for extension board positioning
- **Result**: Extension boards now spawn relative to CoreBoard's actual position, not assumed (0,0)
- **Status**: ✅ **WORKING** - CoreBoard detected at (384.63, 384.63, 0.00)
- **Fix**: Changed from `boardsContainer.TransformPoint(worldPos) + coreBoardWorldPosition` to `coreBoardWorldPosition + worldPos`

### **Step 1.7: Configurable Extension Board Offset** ✅
- **Problem**: Extension boards positioned too far from CoreBoard
- **Solution**: Added configurable `extensionBoardOffset` field in inspector
- **Usage**: Adjust X, Y, Z values in "Extension Board Positioning" section
- **Default**: (-768, -768, 0) for closer spacing
- **Debug**: Use "Debug Extension Board Offset" to see current settings and expected positions

### **Step 1.8: Extension Board Coordinate Swapping** ✅
- **Problem**: Extension boards needed to be created at swapped coordinates for proper positioning
- **Solution**: Swapped extension board creation coordinates in both core and extension board setup
- **Mapping**: 
  - North extension point → creates board at East position
  - South extension point → creates board at West position
  - East extension point → creates board at North position
  - West extension point → creates board at South position
- **Result**: Extension boards now spawn in the correct positions relative to their extension points

### **Step 1.9: Automatic Extension Point Allocation** ✅
- **Problem**: When extension boards are created, corresponding extension points need to be automatically allocated
- **Solution**: Added automatic allocation of corresponding extension points on newly created boards
- **Cell-Based Logic**: 
  - Cell (3,6) allocated on CoreBoard → Cell (3,0) automatically allocated on ExtensionBoard
  - Cell (3,0) allocated on CoreBoard → Cell (3,6) automatically allocated on ExtensionBoard
  - Cell (0,3) allocated on CoreBoard → Cell (6,3) automatically allocated on ExtensionBoard
  - Cell (6,3) allocated on CoreBoard → Cell (0,3) automatically allocated on ExtensionBoard
- **Result**: Extension boards maintain clean paths between boards with proper cell-to-cell connections

### **Step 2: Debug Board Positions**
Use the context menu options to debug positioning:
- **Right-click BoardPositioningManager** → "Debug Board Positions"
- **Right-click BoardPositioningManager** → "Debug Extension Points"
- **Right-click BoardPositioningManager** → "Debug Extension Point Allocation"
- **Right-click BoardPositioningManager** → "Debug Complete Allocation Process"
- **Right-click BoardPositioningManager** → "Manual Test Extension Board Creation"
- **Right-click BoardPositioningManager** → "Test Automatic Allocation"
- **Right-click BoardPositioningManager** → "Debug Grid Layout"
- **Right-click BoardPositioningManager** → "Debug Grid Component"
- **Right-click BoardPositioningManager** → "Detect Core Board Position"
- **Right-click BoardPositioningManager** → "Debug Extension Board Offset"
- **Right-click BoardPositioningManager** → "Update Grid Cell Size"
- **Right-click BoardPositioningManager** → "Spawn Single Extension Board (North)"
- **Right-click BoardPositioningManager** → "Spawn Test Extension Boards"

### **Step 3: Verify Extension Points**
Check the console for extension point creation:
- Extension points should be created at edge cell positions: (3,6), (3,0), (6,3), (0,3)
- Each extension point should have a unique world position

### **Step 4: Expected Extension Board Positions** ✅
Based on your CoreBoard position at (384.63, 384.63, 0.00) with configurable offset and coordinate swapping:
- **North Extension Point** → **East Position**: Grid (1, 0) → World (516.63, 384.63, 0.00) [with default -768 offset]
- **South Extension Point** → **West Position**: Grid (-1, 0) → World (-1151.37, 384.63, 0.00) [with default -768 offset]
- **East Extension Point** → **North Position**: Grid (0, 1) → World (384.63, 516.63, 0.00) [with default -768 offset]
- **West Extension Point** → **South Position**: Grid (0, -1) → World (384.63, -1151.37, 0.00) [with default -768 offset]

**Note**: Extension board creation coordinates are swapped for proper positioning. The extension point location determines which coordinate the board is created at.
- No overlapping extension points should exist
- Edge cells should be marked as extension points (use "Debug Extension Points" to verify)

### **Step 4: Check Board Prefabs**
- Ensure `coreBoardPrefab` and `extensionBoardPrefab` are assigned
- Verify prefabs have proper scaling (1.0x)
- Check that prefabs contain the expected cell structure

### Mouse Drag Controls
The `SimpleCameraController` now supports intuitive mouse drag controls using the new Input System:

- **Left-click hold and drag**: Move the camera around the scene
- **Mouse wheel**: Zoom in/out
- **Keyboard controls**: Still available for precise movement
- **Zoom-based sensitivity**: Camera movement sensitivity adjusts based on zoom level
- **Input System compatible**: Uses `Mouse.current` device for reliable input handling

### Mouse Drag Settings
```csharp
[Header("Mouse Drag Settings")]
[SerializeField] private bool enableMouseDrag = true;
[SerializeField] private float dragSensitivity = 2f;
[SerializeField] private float dragResponseSpeed = 15f; // Higher speed for more responsive dragging
```

### Control Methods
- **Enable/Disable**: `SetMouseDragEnabled(bool enabled)`
- **Adjust sensitivity**: `SetDragSensitivity(float sensitivity)`
- **Adjust response speed**: `SetDragResponseSpeed(float speed)`
- **Debug logging**: Shows drag start/end events when `showDebugInfo = true`

### Responsive Dragging
The camera now uses **direct position updates** during mouse dragging for immediate response, eliminating the "resistance" feeling. When not dragging, it uses smooth interpolation for other camera movements.

---

*This system provides the perfect foundation for your extension board positioning needs, using your existing CellContainer.prefab and integrating seamlessly with your current passive tree system!* 🎯
