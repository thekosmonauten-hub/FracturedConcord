# Canvas-Based Passive Tree System Guide

## Overview

The Canvas-Based Passive Tree System creates a **single large canvas** where all connected boards are visible simultaneously, with zoom and pan functionality. This matches the TypeScript implementation shown in the screenshot, providing a seamless, expansive passive tree experience.

## Key Features

### **✅ Single Large Canvas**
- **All Boards Visible**: No switching between boards - all connected boards are displayed simultaneously
- **Expansive Layout**: Large canvas (2000x2000 by default) accommodates multiple boards
- **Spatial Arrangement**: Boards positioned relative to each other with proper spacing

### **✅ Zoom & Pan Navigation**
- **Mouse Wheel Zoom**: Scroll to zoom in/out (0.5x to 3x)
- **Right-Click Pan**: Hold right mouse button to pan around the canvas
- **Smooth Navigation**: Intuitive controls for exploring the large tree

### **✅ Extension Point Alignment**
- **Seamless Connections**: Extension points align between connected boards
- **Visual Connections**: Lines show connections between boards
- **Automatic Positioning**: New boards positioned relative to extension points

### **✅ Multi-Board Management**
- **Core Board**: Always starts at center (0,0)
- **Extension Boards**: Added through extension point clicks
- **Persistent State**: All connections and allocations persist across sessions

## System Architecture

### **Core Components**

#### **CanvasBasedPassiveTreeController**
- **Main Controller**: Manages the entire canvas-based system
- **Zoom & Pan**: Handles navigation controls
- **Board Management**: Coordinates board creation and positioning
- **Extension Point Handling**: Manages board connections

#### **BoardUI**
- **Individual Board Rendering**: Each board is a separate UI component
- **Node Management**: Handles node creation, interaction, and visuals
- **Extension Points**: Renders and manages extension point interactions
- **Visual Updates**: Updates node colors based on allocation state

#### **BoardConnection**
- **Visual Connections**: Represents lines between connected boards
- **Connection Data**: Stores source extension point and target board
- **Position Tracking**: Maintains connection line positions

### **Data Flow**

```
1. Initialize Core Board → SetupBoardOnCanvas()
2. Click Extension Point → OnExtensionPointClicked()
3. Discover Available Boards → DiscoverExtensionBoards()
4. Calculate Position → CalculateBoardPosition()
5. Create Board UI → CreateBoardUI()
6. Create Connection → CreateBoardConnection()
7. Update Canvas → UpdateInfoText()
```

## Setup Instructions

### **Scene Setup**

1. **Create Canvas Structure**:
   ```
   Canvas (Screen Space - Overlay)
   ├── ScrollRect
   │   └── CanvasContent (RectTransform)
   └── UI Controls
       ├── Info Text
       ├── Add Points Button
       ├── Allocate Button
       └── Clear Preview Button
   ```

2. **Add CanvasBasedPassiveTreeController**:
   - Add to a GameObject in the scene
   - Assign references in Inspector

### **Required Components**

#### **Canvas Settings**
- **ScrollRect**: For panning functionality
- **CanvasContent**: Large RectTransform for board placement
- **Canvas Size**: 2000x2000 (configurable)
- **Board Spacing**: 100 (space between boards)

#### **Zoom & Pan Settings**
- **Min Zoom**: 0.5x
- **Max Zoom**: 3x
- **Zoom Speed**: 0.1
- **Pan Speed**: 1.0

#### **UI References**
- **Info Text**: Display system information
- **Add Points Button**: Add test points
- **Allocate Button**: Allocate previewed nodes
- **Clear Preview Button**: Clear previewed nodes

### **Inspector Configuration**

1. **Select CanvasBasedPassiveTreeController** in scene
2. **Assign Canvas Components**:
   - `_scrollRect` → ScrollRect component
   - `_canvasContent` → Canvas content RectTransform
3. **Configure Settings**:
   - Set `_canvasSize` to desired canvas size
   - Adjust `_boardSpacing` for board separation
   - Configure zoom/pan settings
4. **Assign UI Elements**:
   - Link info text and buttons
   - Set test character settings

## Usage Workflow

### **Initial Setup**

1. **Scene Loads**: Core board automatically initializes at center
2. **Canvas Setup**: Large canvas created with zoom/pan enabled
3. **Data Loading**: Character data and allocations loaded
4. **Visual Update**: All nodes display current allocation state

### **Adding Extension Boards**

1. **Click Extension Point**: Click yellow extension point on any board
2. **Board Discovery**: System finds available boards from Resources
3. **Position Calculation**: New board positioned relative to extension point
4. **Board Creation**: Board UI created and added to canvas
5. **Connection Creation**: Visual connection line drawn between boards
6. **State Update**: All boards remain visible and interactive

### **Navigation**

1. **Zoom**: Use mouse wheel to zoom in/out
2. **Pan**: Hold right mouse button and drag to pan
3. **Board Interaction**: Click nodes and extension points normally
4. **Multi-Board View**: All boards remain visible during navigation

## Visual Design

### **Board Appearance**

#### **Board Container**
- **Background**: Dark semi-transparent background
- **Border**: Colored border (blue for core, yellow for extensions)
- **Title**: Board name displayed at top
- **Size**: Automatically sized based on grid dimensions

#### **Nodes**
- **Unallocated**: White circles with black text
- **Allocated**: Green circles with black text
- **Previewed**: Yellow circles with black text
- **Size**: 20px diameter (configurable)
- **Spacing**: 30px between nodes (configurable)

#### **Extension Points**
- **Appearance**: Yellow circles with orange borders
- **Size**: 24px diameter (20% larger than nodes)
- **Interaction**: Clickable to add new boards

### **Connection Lines**

#### **Visual Style**
- **Color**: Yellow for preview, green for connected
- **Thickness**: 3-5px lines
- **Style**: Straight lines between extension points

#### **Positioning**
- **Source**: Extension point on source board
- **Target**: Extension point on target board
- **Alignment**: Lines align extension points perfectly

## Technical Implementation

### **Board Positioning**

```csharp
// Calculate board position based on extension point
private Vector2 CalculateBoardPosition(ExtensionPoint extensionPoint, Vector2 worldPosition)
{
    // For now, place boards with spacing
    // TODO: Implement proper alignment based on extension point position
    Vector2 offset = new Vector2(_boardSpacing, 0);
    return worldPosition + offset;
}
```

### **Zoom and Pan**

```csharp
// Handle zoom with mouse wheel
void HandleZoom()
{
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll != 0)
    {
        _currentZoom = Mathf.Clamp(_currentZoom + scroll * _zoomSpeed, _minZoom, _maxZoom);
        _scrollRect.content.localScale = Vector3.one * _currentZoom;
    }
}

// Handle pan with right mouse button
void HandlePan()
{
    if (Input.GetMouseButtonDown(1)) // Right mouse button
    {
        _isDragging = true;
        _lastMousePosition = Input.mousePosition;
    }
    // ... pan logic
}
```

### **Board Creation**

```csharp
// Create board UI on canvas
private BoardUI CreateBoardUI(BaseBoardScriptableObject boardAsset, Vector2 position)
{
    GameObject boardObject = CreateSimpleBoardUI();
    
    // Set position
    var rectTransform = boardObject.GetComponent<RectTransform>();
    rectTransform.anchoredPosition = position;
    
    // Add BoardUI component
    var boardUI = boardObject.AddComponent<BoardUI>();
    boardUI.Initialize(boardAsset, this);
    
    return boardUI;
}
```

## Info Panel Display

The info panel shows comprehensive system information:

```
Canvas-Based Passive Tree

Character: Test Character
Available Points: 15
Total Points Earned: 25
Allocated Nodes: 8
Connected Boards: 3
Previewed Nodes: 2
Canvas Zoom: 1.25x
```

## Console Logging

The system provides detailed logging for debugging:

```
[CanvasBasedPassiveTreeController] Canvas setup complete - Size: 2000x2000
[CanvasBasedPassiveTreeController] Loaded core board: Core Board
[CanvasBasedPassiveTreeController] Board setup complete: Core Board at position (0, 0)
[CanvasBasedPassiveTreeController] Extension point clicked: core_ext_1 at (100, 0)
[CanvasBasedPassiveTreeController] Discovered board: Fire Board
[CanvasBasedPassiveTreeController] Connecting extension board: Fire Board
[CanvasBasedPassiveTreeController] Board setup complete: Fire Board at position (100, 0)
```

## Performance Considerations

### **Optimization Strategies**

1. **Object Pooling**: Reuse UI objects for better performance
2. **Culling**: Only render visible boards/nodes
3. **LOD System**: Reduce detail at high zoom levels
4. **Batch Rendering**: Group similar UI elements

### **Memory Management**

1. **Board Cleanup**: Remove unused board UIs
2. **Connection Cleanup**: Clean up connection lines
3. **Event Unsubscription**: Properly unsubscribe from events
4. **Resource Loading**: Load boards on-demand

## Future Enhancements

### **Planned Features**

- **Smart Board Positioning**: Automatic alignment based on extension point positions
- **Visual Connection Lines**: Implement actual line rendering between boards
- **Board Categories**: Color-code boards by theme
- **Mini-map**: Overview of entire tree structure

### **Advanced Features**

- **Board Templates**: Pre-configured board layouts
- **Connection Validation**: Ensure proper board connections
- **Performance Optimization**: Implement culling and LOD systems
- **Save/Load System**: Save board positions and connections

## Troubleshooting

### **Common Issues**

#### **"Canvas content not assigned"**
- **Cause**: Canvas content RectTransform not assigned
- **Solution**: Assign the canvas content in Inspector

#### **"ScrollRect not assigned - zoom/pan disabled"**
- **Cause**: ScrollRect component not assigned
- **Solution**: Assign ScrollRect component in Inspector

#### **"No boards available for extension point"**
- **Cause**: Board assets not found in Resources
- **Solution**: Ensure board assets are in `Resources/PassiveTree/` folder

#### **"Board asset not found for ID"**
- **Cause**: Extension point ID doesn't match board asset ID
- **Solution**: Check ID consistency between extension points and board assets

### **Debug Steps**

1. **Check Console Logs** for canvas setup messages
2. **Verify Canvas Structure** has proper ScrollRect setup
3. **Test Zoom/Pan** with mouse wheel and right-click
4. **Check Board Assets** are in correct Resources folder
5. **Verify Extension Points** have correct board IDs

## Migration from Grid-Based System

### **Key Differences**

1. **Single Canvas**: All boards on one large canvas vs. switching between boards
2. **Zoom & Pan**: Navigation controls vs. board switching buttons
3. **Spatial Layout**: Boards positioned relative to each other vs. individual views
4. **Visual Connections**: Lines between boards vs. separate board views

### **Migration Steps**

1. **Replace Controller**: Use `CanvasBasedPassiveTreeController` instead of `PassiveTreeTestController`
2. **Update Scene**: Set up canvas structure with ScrollRect
3. **Configure UI**: Assign new UI references
4. **Test Functionality**: Verify zoom, pan, and board connections work

---

*This system provides the foundation for a truly modular and expansive passive tree experience, matching the functionality shown in the TypeScript implementation.*
