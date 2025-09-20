# Extension Board System Guide

## Overview

The Extension Board System allows players to connect additional passive tree boards through extension points, creating a modular and expandable passive tree experience. Players can discover, connect, and navigate between multiple boards seamlessly.

## How It Works

### **Extension Point Discovery**
1. **Click an extension point** (yellow squares with orange borders)
2. **System discovers** available boards from `Resources/PassiveTree/`
3. **Automatically connects** the first available board
4. **Board becomes available** for navigation

### **Board Navigation**
- **Core Board**: Always available as the starting point
- **Extension Boards**: Connected through extension points
- **Seamless Switching**: Navigate between boards using UI buttons

## Features

### **✅ Automatic Board Discovery**
- **Resource Loading**: Automatically loads board assets from `Resources/PassiveTree/`
- **ID Matching**: Matches extension point board IDs with available assets
- **Error Handling**: Graceful handling of missing board assets

### **✅ Board Connection System**
- **Extension Point Validation**: Checks available boards for each extension point
- **Connection Tracking**: Maintains connection status and count
- **Persistent Connections**: Saves connection data across sessions

### **✅ Multi-Board Navigation**
- **Core Board Access**: Always return to the core board
- **Extension Board Cycling**: Navigate through connected extension boards
- **Visual Feedback**: Clear indication of current board and navigation options

### **✅ Persistent State**
- **Connection Persistence**: Connected boards remain available on restart
- **Allocation Persistence**: Node allocations persist across all boards
- **Navigation State**: Current board selection is maintained

## UI Components

### **Required Buttons**
1. **Return to Core Board** - Navigate back to the core board
2. **Next Board** - Cycle to the next connected extension board
3. **Previous Board** - Cycle to the previous connected extension board

### **Button Configuration**
1. **Select PassiveTreeTestController** in the scene
2. **In Inspector**, find the "Board Navigation UI" section
3. **Drag and drop** your UI buttons to the corresponding fields:
   - `_returnToCoreBoardButton` → "Return to Core" button
   - `_nextBoardButton` → "Next Board" button
   - `_previousBoardButton` → "Previous Board" button

## Usage Workflow

### **Connecting Extension Boards**

1. **Navigate to Extension Point**: Click on a yellow extension point
2. **Automatic Discovery**: System finds available boards
3. **Board Connection**: First available board is automatically connected
4. **Board Switch**: View switches to the newly connected board

### **Navigating Between Boards**

1. **Return to Core**: Use "Return to Core Board" button
2. **Cycle Extension Boards**: Use "Next Board" / "Previous Board" buttons
3. **Visual Feedback**: Info panel shows current board and navigation status

### **Managing Connections**

- **Automatic Connection**: Extension points automatically connect available boards
- **Connection Tracking**: System tracks which boards are connected
- **Persistent State**: Connections remain active across game sessions

## Technical Implementation

### **Core Components**

#### **PassiveTreeTestController**
- **`HandleExtensionBoards()`**: Manages extension point interactions
- **`DiscoverExtensionBoards()`**: Loads board assets from Resources
- **`ConnectExtensionBoard()`**: Establishes board connections
- **`SwitchToBoard()`**: Handles board navigation

#### **Board Navigation State**
- **`_coreBoardAsset`**: Reference to the core board
- **`_connectedBoards`**: List of connected extension boards
- **`_currentBoardIndex`**: Current board position (-1 = core, 0+ = extension)

#### **Extension Point Integration**
- **`OnExtensionPointClicked()`**: Handles extension point interactions
- **`UpdateExtensionPointInfo()`**: Displays extension point details

### **Data Flow**

```
1. Click Extension Point → HandleExtensionBoards()
2. Discover Available Boards → DiscoverExtensionBoards()
3. Connect Board → ConnectExtensionBoard()
4. Switch View → SwitchToBoard()
5. Update UI → UpdateInfoText()
```

### **Resource Loading**

```csharp
// Load all board assets from Resources
var allBoardAssets = Resources.LoadAll<BaseBoardScriptableObject>("PassiveTree");

// Match board IDs with extension point requirements
var boardAsset = allBoardAssets.FirstOrDefault(asset => 
    asset.GetBoardData()?.id == boardId);
```

## Board Asset Requirements

### **File Structure**
```
Assets/Resources/PassiveTree/
├── CoreBoardScriptableObject.asset
├── FireBoardScriptableObject.asset
├── ColdBoardScriptableObject.asset
└── [Other Board Assets].asset
```

### **Board ID Matching**
- **Extension Point IDs**: Must match board asset IDs exactly
- **Case Sensitivity**: Board IDs are case-sensitive
- **Asset Loading**: Boards must be in `Resources/PassiveTree/` folder

### **Extension Point Configuration**
```csharp
// Example extension point configuration
extensionPoint.availableBoards = new List<string> { "fire_board", "cold_board" };
extensionPoint.currentConnections = 0;
extensionPoint.maxConnections = 2;
```

## Info Panel Display

When extension boards are connected, the info panel shows:

```
Board: Fire Board
Board Type: Extension Board (1)
Connected Boards: 2
Total Nodes: 15
Extension Points: 3

Character: Test Character
Available Points: 8
Allocated Nodes: 5
```

## Console Logging

The system provides detailed logging:

```
[PassiveTreeTestController] Selected extension point: core_ext_1
[PassiveTreeTestController] Discovering 2 available boards for extension point: core_ext_1
[PassiveTreeTestController] Discovered board: Fire Board (ID: fire_board)
[PassiveTreeTestController] Connecting extension board: Fire Board via extension point: core_ext_1
[PassiveTreeTestController] Added Fire Board to connected boards list (total: 1)
[PassiveTreeTestController] Successfully connected extension board: Fire Board
[PassiveTreeTestController] Switched to board: Fire Board (index: 0)
```

## Best Practices

### **Development**

1. **Board Asset Organization**: Keep all board assets in `Resources/PassiveTree/`
2. **ID Consistency**: Ensure extension point IDs match board asset IDs
3. **Error Handling**: Implement graceful fallbacks for missing assets
4. **Performance**: Load boards on-demand rather than all at once

### **User Experience**

1. **Clear Navigation**: Provide obvious navigation controls
2. **Visual Feedback**: Show current board and available options
3. **Persistent State**: Maintain user's board selection
4. **Seamless Transitions**: Smooth switching between boards

## Future Enhancements

### **Planned Features**

- **Board Selection UI**: Choose which board to connect from multiple options
- **Visual Connections**: Show connections between boards graphically
- **Board Requirements**: Require specific conditions to unlock boards
- **Board Categories**: Organize boards by themes or requirements

### **Advanced Features**

- **Dynamic Board Loading**: Load boards based on game progression
- **Board Merging**: Combine multiple boards into larger trees
- **Custom Connections**: Allow manual board connections
- **Board Templates**: Reusable board configurations

## Troubleshooting

### **Common Issues**

#### **"No board assets found for extension point"**
- **Cause**: Board assets not in `Resources/PassiveTree/` folder
- **Solution**: Move board assets to correct folder

#### **"Board asset not found for ID"**
- **Cause**: Extension point ID doesn't match board asset ID
- **Solution**: Check ID consistency between extension points and board assets

#### **"Cannot switch to null board"**
- **Cause**: Board asset reference is null
- **Solution**: Verify board asset setup and loading

### **Debug Steps**

1. **Check Console Logs** for extension board messages
2. **Verify Resource Path** for board assets
3. **Check ID Matching** between extension points and boards
4. **Test Board Loading** with individual board assets
5. **Verify Navigation** between connected boards

---

*For more information about the Passive Tree system, see `Core_Board_Initialization_Guide.md`*
