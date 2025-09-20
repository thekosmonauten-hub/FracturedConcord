# Connecting ScriptableObjects to Boards Guide

## Overview

This guide explains how to connect the new individual ScriptableObject classes (`CoreBoardScriptableObject`, `FireBoardScriptableObject`, etc.) to their respective board assets in Unity.

## Prerequisites

- Individual board ScriptableObject classes have been created
- Board Asset Updater utility is available
- Unity Editor is open

## Method 1: Using the Board Asset Updater (Recommended)

### Step 1: Open the Board Asset Updater
1. In Unity, go to **Tools** → **Passive Tree** → **Update Board Assets**
2. This opens the Board Asset Updater window

### Step 2: Find Existing Assets
1. Click **"Find and List Existing Board Assets"**
2. Check the Console for a list of existing assets
3. This helps you understand what needs to be updated

### Step 3: Create New Individual Board Assets
1. Click **"Create New Individual Board Assets"**
2. This automatically creates new assets in `Assets/Resources/PassiveTree/`:
   - `CoreBoard.asset` (CoreBoardScriptableObject)
   - `FireBoard.asset` (FireBoardScriptableObject)
   - `ColdBoard.asset` (ColdBoardScriptableObject)
   - `DiscardBoard.asset` (DiscardBoardScriptableObject)
   - `LifeBoard.asset` (LifeBoardScriptableObject)

### Step 4: Initialize Each Board
1. Select each new asset in the Project window
2. In the Inspector, click the **"Setup [Board Name]"** button (Context Menu)
3. This initializes the board with its specific configuration

## Method 2: Manual Creation

### Step 1: Create Assets via Create Menu
1. Right-click in the Project window
2. Go to **Create** → **Passive Tree**
3. Select the specific board type:
   - **Core Board** (CoreBoardScriptableObject)
   - **Fire Board** (FireBoardScriptableObject)
   - **Cold Board** (ColdBoardScriptableObject)
   - **Discard Board** (DiscardBoardScriptableObject)
   - **Life Board** (LifeBoardScriptableObject)

### Step 2: Name and Organize
1. Name each asset appropriately (e.g., "FireBoard", "ColdBoard")
2. Move them to `Assets/Resources/PassiveTree/` for automatic discovery
3. Select each asset and click the setup button in the Inspector

## Method 3: Update Existing Assets

If you have existing board assets using the old `PassiveBoardScriptableObject`:

### Step 1: Backup Existing Assets
1. Create a backup of your existing board assets
2. Note their current configurations

### Step 2: Create New Assets
1. Use Method 1 or 2 to create new individual board assets
2. Copy the configuration from old assets to new ones if needed

### Step 3: Update References
1. Find all scripts that reference the old assets
2. Update them to reference the new individual board assets
3. Test to ensure everything works correctly

## Verification Steps

### Step 1: Check Asset Creation
1. Verify assets exist in `Assets/Resources/PassiveTree/`
2. Confirm each asset has the correct ScriptableObject type
3. Check that setup buttons are available in the Inspector

### Step 2: Test Board Discovery
1. Open the **Board Asset Updater**
2. Click **"Find and List Existing Board Assets"**
3. Verify all new assets are listed correctly

### Step 3: Test Board Manager Integration
1. The `PassiveTreeBoardManager` should automatically discover the new assets
2. Check the Console for discovery messages
3. Verify boards are available in the automatic assignment system

## Troubleshooting

### Issue: Assets Not Found
**Problem**: Board assets are not being discovered by the system
**Solution**: 
- Ensure assets are in `Assets/Resources/PassiveTree/`
- Check that assets have the correct ScriptableObject type
- Verify assets have been initialized with the setup button

### Issue: Setup Button Not Available
**Problem**: Context menu setup button is missing
**Solution**:
- Ensure the ScriptableObject class inherits from `BaseBoardScriptableObject`
- Check that the `[ContextMenu]` attribute is present
- Recompile scripts if needed

### Issue: Board Data Not Initializing
**Problem**: Board data remains empty after setup
**Solution**:
- Check the Console for error messages during setup
- Verify all required methods are implemented in the board class
- Ensure the `SetupBoard()` method is working correctly

## Best Practices

### Asset Organization
- Keep all board assets in `Assets/Resources/PassiveTree/`
- Use consistent naming conventions
- Group related assets together

### Asset Management
- Always backup before making changes
- Test each board individually before integration
- Use the Board Asset Updater for bulk operations

### Performance Considerations
- Board assets are loaded at runtime from Resources
- Consider lazy loading for large numbers of boards
- Monitor memory usage with many board assets

## Integration with Existing Systems

### PassiveTreeBoardManager
The `PassiveTreeBoardManager` automatically discovers board assets from the Resources folder. No additional configuration is needed.

### PassiveTreeDataManager
Board data is integrated through the existing data management system. Individual board bonuses are calculated through the `GetBoardBonuses()` method.

### UI Integration
Board assets can be referenced in UI components for display and interaction. Use the board's `boardData` property to access node information.

## Next Steps

1. **Create Remaining Boards**: Implement the remaining board classes (Lightning, Chaos, Physical, etc.)
2. **Test Integration**: Verify all boards work with the passive tree system
3. **Optimize Performance**: Monitor and optimize board loading and processing
4. **Add Custom Logic**: Implement board-specific mechanics and interactions

## Support

If you encounter issues:
1. Check the Console for error messages
2. Verify all ScriptableObject classes are properly implemented
3. Ensure assets are in the correct location
4. Test with a single board first before adding more
