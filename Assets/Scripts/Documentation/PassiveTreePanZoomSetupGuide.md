# Passive Tree Pan & Zoom Setup Guide

## Overview
This guide explains how to implement pan and zoom functionality for your passive tree canvas, allowing users to navigate and explore the passive tree effectively.

## Components Created

### 1. PassiveTreePanZoom.cs
- **Purpose**: Handles all pan and zoom functionality
- **Features**:
  - Mouse drag to pan
  - Mouse wheel to zoom
  - Keyboard controls (WASD/Arrow keys for pan, +/- for zoom)
  - Touch support for mobile
  - Smooth movement with lerping
  - Bounds checking to prevent excessive panning
  - UI element blocking (won't pan/zoom when over buttons, etc.)

### 2. PassiveTreeUIController.cs
- **Purpose**: Manages UI controls for the passive tree
- **Features**:
  - Reset view button
  - Fit to view button
  - Zoom in/out buttons
  - Zoom level display
  - Zoom slider

## Setup Instructions

### Step 1: Add Pan/Zoom Component
1. **Select your Canvas GameObject** (the one containing the passive tree)
2. **Add Component**: `PassiveTreePanZoom`
3. **Configure Settings**:
   - **Pan Speed**: 1.0 (adjust for sensitivity)
   - **Zoom Speed**: 0.5 (adjust for zoom sensitivity)
   - **Min Zoom**: 0.5 (50% zoom)
   - **Max Zoom**: 3.0 (300% zoom)
   - **Smooth Movement**: Enabled (recommended)
   - **Smooth Speed**: 10.0 (adjust for smoothness)
   - **Enable Bounds**: Enabled (prevents excessive panning)
   - **Max Pan Distance**: 1000 (adjust based on your tree size)

### Step 2: Create UI Controls (Optional)
1. **Create a UI Panel** for controls (top-right corner recommended)
2. **Add Buttons**:
   - Reset View Button
   - Fit to View Button
   - Zoom In Button (+)
   - Zoom Out Button (-)
3. **Add UI Elements**:
   - Zoom Level Text (shows current zoom percentage)
   - Zoom Slider (for precise zoom control)
4. **Add Component**: `PassiveTreeUIController`
5. **Assign References**:
   - Drag your buttons to the respective fields
   - Assign the zoom level text and slider
   - Assign the `PassiveTreePanZoom` component

### Step 3: Configure Canvas Settings
1. **Ensure your Canvas has**:
   - **Graphic Raycaster** component
   - **Event System** in the scene
2. **BoardContainer Setup**:
   - The `PassiveTreePanZoom` will automatically find the `BoardContainer`
   - If it can't find it, it will search for any `RectTransform` child

## Usage

### Mouse Controls
- **Left Click + Drag**: Pan the view
- **Mouse Wheel**: Zoom in/out
- **Right Click + Drag**: Alternative pan method

### Keyboard Controls
- **WASD** or **Arrow Keys**: Pan the view
- **+/- Keys**: Zoom in/out
- **Space**: Reset view (if implemented)

### Touch Controls (Mobile)
- **Single Finger Drag**: Pan the view
- **Pinch Gesture**: Zoom in/out (requires additional setup)

### UI Controls
- **Reset View Button**: Centers the view and sets zoom to 100%
- **Fit to View Button**: Adjusts zoom to show the entire tree
- **Zoom In/Out Buttons**: Incremental zoom changes
- **Zoom Slider**: Precise zoom control

## Configuration Options

### Pan Settings
```csharp
[SerializeField] private float panSpeed = 1f;        // How fast panning moves
[SerializeField] private bool enablePan = true;      // Enable/disable panning
```

### Zoom Settings
```csharp
[SerializeField] private float zoomSpeed = 0.5f;     // How fast zooming changes
[SerializeField] private float minZoom = 0.5f;       // Minimum zoom level (50%)
[SerializeField] private float maxZoom = 3f;         // Maximum zoom level (300%)
[SerializeField] private bool enableZoom = true;     // Enable/disable zooming
```

### Smooth Movement
```csharp
[SerializeField] private bool smoothMovement = true; // Enable smooth transitions
[SerializeField] private float smoothSpeed = 10f;    // How fast smooth movement is
```

### Bounds
```csharp
[SerializeField] private bool enableBounds = true;           // Enable pan bounds
[SerializeField] private float maxPanDistance = 1000f;       // Maximum pan distance
```

## Troubleshooting

### Input System Conflicts
**Error**: `InvalidOperationException: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System package`

**Solution**: The script has been updated to handle both Input Systems:
1. **Try the updated `PassiveTreePanZoom`** - it now supports both old and new Input Systems
2. **Disable keyboard controls** - Set `Enable Keyboard Controls` to false in the inspector
3. **Use the simple version** - Replace with `PassiveTreePanZoomSimple` which only uses mouse/touch input
4. **Check Player Settings** - Ensure Input System is properly configured

### Pan/Zoom Not Working
1. **Check Canvas Setup**:
   - Ensure Canvas has `Graphic Raycaster`
   - Ensure `Event System` exists in scene
2. **Check BoardContainer**:
   - Verify `BoardContainer` exists as child of Canvas
   - Check console for error messages
3. **Check Component References**:
   - Ensure `PassiveTreePanZoom` is on the Canvas GameObject
   - Verify `BoardContainer` is found automatically

### Smooth Movement Issues
1. **Adjust Smooth Speed**: Lower values = smoother but slower
2. **Disable Smooth Movement**: Set to false for instant movement
3. **Check Frame Rate**: Smooth movement depends on consistent frame rate

### Zoom Issues
1. **Check Zoom Bounds**: Ensure min/max zoom values are reasonable
2. **Adjust Zoom Speed**: Lower values = more precise zooming
3. **Check Mouse Wheel**: Ensure mouse wheel events are being captured

### UI Blocking Issues
1. **Check UI Elements**: Ensure buttons, sliders, etc. are properly blocking
2. **Adjust Blocking Logic**: Modify `IsOverBlockingUI()` method if needed
3. **Test Interaction**: Try clicking on UI elements to ensure they work

## Advanced Features

### Custom Zoom Levels
```csharp
// Set specific zoom level
panZoom.SetTargetZoom(2.0f); // 200% zoom

// Get current zoom
float currentZoom = panZoom.CurrentZoom;
```

### Custom Pan Position
```csharp
// Set specific position
panZoom.SetTargetPosition(new Vector2(100, 50));

// Get current position
Vector2 currentPos = panZoom.CurrentPosition;
```

### Programmatic Control
```csharp
// Reset to default view
panZoom.ResetView();

// Fit content to view
panZoom.FitToView();

// Show current state (debug)
panZoom.ShowCurrentState();
```

## Performance Considerations

1. **Smooth Movement**: Uses `Update()` for lerping - ensure good frame rate
2. **Event Handling**: Mouse/touch events are processed efficiently
3. **Bounds Checking**: Minimal performance impact
4. **UI Raycasting**: Only when needed for blocking detection

## Mobile Considerations

1. **Touch Sensitivity**: May need to adjust `panSpeed` for touch devices
2. **Zoom Sensitivity**: Adjust `zoomSpeed` for touch zooming
3. **UI Scaling**: Ensure UI controls are appropriately sized for touch
4. **Performance**: Test on target devices for smooth performance

## Integration with Existing Systems

### With PassiveTreeBoardUI
- Works seamlessly with existing passive tree board
- No conflicts with node interactions
- Maintains proper hierarchy

### With Character Stats Panel
- No interference with other UI systems
- Proper event handling prevents conflicts

### With Scene Management
- Automatically initializes with scene
- Persists across scene changes if needed
- Clean shutdown and cleanup

## Future Enhancements

1. **Pinch-to-Zoom**: Add multi-touch support for mobile
2. **Double-Tap Reset**: Quick reset gesture
3. **Zoom to Node**: Programmatically zoom to specific nodes
4. **Animation Transitions**: Smooth transitions between zoom levels
5. **Save View State**: Remember user's preferred view settings
6. **Mini-Map**: Add a mini-map for navigation
7. **Keyboard Shortcuts**: Additional keyboard controls
8. **Mouse Sensitivity**: Adjustable mouse sensitivity settings
