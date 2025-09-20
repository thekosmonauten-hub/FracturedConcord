# Old Passive Tree System - Archived

## Overview
This directory contains the archived files from the old passive tree system that had become too complex and problematic.

## What Was Archived

### UI Components (Moved to UI/)
- `PassiveTreeNodeUI.cs` - Complex node UI with tooltip issues
- `PassiveTreeBoardUI.cs` - Complex board UI system
- `PassiveTreeCellUI.cs` - Old cell system
- `ConnectionLineUI.cs` - Connection line rendering
- `PassiveTreeUIController.cs` - UI controller
- `PassiveTreePanZoom.cs` - Pan and zoom functionality
- `PassiveTreePanZoomSimple.cs` - Simple pan and zoom

### Data Components (Moved to Data/)
- `PassiveBoard.cs` - Old board data structure
- `BoardConnection.cs` - Connection data
- `ExtensionPoint.cs` - Extension point data
- `CoreBoardSetup.cs` - Board setup logic
- `PassiveTreeSpriteManager.cs` - Old sprite management system

### Management (Moved to Managers/)
- `PassiveTreeManager.cs` - Old management system

### Testing (Moved to Test/)
- `PassiveTreeSpriteTest.cs` - Sprite testing
- `PassiveTreeNodeVisibilityDebugger.cs` - Node visibility debugging

## What Was Kept (Essential for New System)

### Data Structures (Kept in Assets/Scripts/Data/PassiveTree/)
- `PassiveNode.cs` - Core node data structure (may be reused)
- `PlayerPassiveState.cs` - Player progress tracking (keep for new system)

## Issues with Old System
- Complex tooltip layering problems
- Prefab management issues
- Hierarchy rendering complications
- Difficult to maintain and debug
- Multiple conflicting UI approaches

## New System Approach
The new system will use a GridManager approach with:
- Centralized grid management
- Simple tile-based rendering
- ScriptableObject-driven data
- Cleaner separation of concerns
- Easier to maintain and extend

## Migration Notes
- Node data structure from `PassiveNode.cs` may be adapted for new system
- Player state tracking from `PlayerPassiveState.cs` will be preserved
- All UI complexity has been removed for fresh start
- New system will be built from scratch with clean architecture
