# Board System Migration Guide

## Overview

This guide explains the migration from the old shared `PassiveBoardScriptableObject` system to the new individual board ScriptableObject system. The new system provides better organization, type safety, and scalability.

## What Changed

### ❌ **Old System (Deprecated)**
```csharp
// Single shared class for all boards
public class PassiveBoardScriptableObject : ScriptableObject
{
    public PassiveBoard boardData;
}

// All boards used the same ScriptableObject type
var coreBoard = ScriptableObject.CreateInstance<PassiveBoardScriptableObject>();
var fireBoard = ScriptableObject.CreateInstance<PassiveBoardScriptableObject>();
```

### ✅ **New System (Current)**
```csharp
// Base class for common functionality
public abstract class BaseBoardScriptableObject : ScriptableObject
{
    public PassiveBoard boardData;
    public abstract void SetupBoard();
}

// Individual classes for each board type
public class CoreBoardScriptableObject : BaseBoardScriptableObject
public class FireBoardScriptableObject : BaseBoardScriptableObject
public class ColdBoardScriptableObject : BaseBoardScriptableObject
```

## Migration Steps

### Step 1: Update References

#### **In Scripts**
```csharp
// OLD
[SerializeField] private PassiveBoardScriptableObject _boardData;

// NEW
[SerializeField] private BaseBoardScriptableObject _boardData;
```

#### **In Methods**
```csharp
// OLD
public void SetBoardData(PassiveBoardScriptableObject boardData)

// NEW
public void SetBoardData(BaseBoardScriptableObject boardData)
```

### Step 2: Update Asset Creation

#### **Old Way**
```csharp
// OLD - Manual creation
var boardAsset = ScriptableObject.CreateInstance<PassiveBoardScriptableObject>();
boardAsset.boardData = new PassiveBoard();
// ... manual setup
```

#### **New Way**
```csharp
// NEW - Use the generator
// 1. Create board class inheriting from BaseBoardScriptableObject
// 2. Use Tools > Passive Tree > Generate Board Assets
// 3. Assets are created automatically
```

### Step 3: Update Asset Loading

#### **Old Way**
```csharp
// OLD - Load specific asset type
var coreBoard = Resources.Load<PassiveBoardScriptableObject>("PassiveTree/CoreBoard");
```

#### **New Way**
```csharp
// NEW - Load specific board type
var coreBoard = Resources.Load<CoreBoardScriptableObject>("PassiveTree/core_board");
var fireBoard = Resources.Load<FireBoardScriptableObject>("PassiveTree/fire_board");
```

## File Changes

### **Updated Files**

| File | Change | Reason |
|------|--------|--------|
| `PassiveTreeTestController.cs` | `PassiveBoardScriptableObject` → `BaseBoardScriptableObject` | Use new base class |
| `GridManager.cs` | `PassiveBoardScriptableObject` → `BaseBoardScriptableObject` | Use new base class |
| `CoreBoardSetupEditor.cs` | `PassiveBoardScriptableObject` → `CoreBoardScriptableObject` | Use specific board type |
| `PassiveBoardAssetGenerator.cs` | Removed old fallback references | Clean up old system |

### **New Files**

| File | Purpose |
|------|---------|
| `BaseBoardScriptableObject.cs` | Base class for all board ScriptableObjects |
| `CoreBoardScriptableObject.cs` | Core board implementation |
| `FireBoardScriptableObject.cs` | Fire board implementation |
| `ColdBoardScriptableObject.cs` | Cold board implementation |
| `DiscardBoardScriptableObject.cs` | Discard board implementation |
| `LifeBoardScriptableObject.cs` | Life board implementation |

## Asset Migration

### **Old Assets**
- Location: `Assets/Resources/PassiveTree/CoreBoard.asset`
- Type: `PassiveBoardScriptableObject`
- Status: **Deprecated**

### **New Assets**
- Location: `Assets/Resources/PassiveTree/core_board.asset`
- Type: `CoreBoardScriptableObject`
- Status: **Current**

### **Migration Process**

1. **Backup old assets** (optional)
2. **Use "Clean Up Old Assets"** in the Board Asset Generator
3. **Generate new assets** using the Board Asset Generator
4. **Update scene references** to point to new assets
5. **Test functionality** to ensure everything works

## Code Migration Examples

### **Example 1: Test Controller**

#### **Before**
```csharp
[SerializeField] private PassiveBoardScriptableObject _testBoardData;

public void CreateTestBoard()
{
    _testBoardData = ScriptableObject.CreateInstance<PassiveBoardScriptableObject>();
    _currentBoard = CoreBoardSetup.CreateCompleteCoreBoard();
    _testBoardData.boardData = _currentBoard;
}
```

#### **After**
```csharp
[SerializeField] private BaseBoardScriptableObject _testBoardData;

public void CreateTestBoard()
{
    if (_testBoardData != null)
    {
        _testBoardData.SetupBoard();
        _currentBoard = _testBoardData.GetBoardData();
    }
}
```

### **Example 2: Grid Manager**

#### **Before**
```csharp
[SerializeField] private PassiveBoardScriptableObject _boardData;

public void SetBoardData(PassiveBoardScriptableObject boardData)
{
    _boardData = boardData;
    _board = _boardData.GetBoardData();
}
```

#### **After**
```csharp
[SerializeField] private BaseBoardScriptableObject _boardData;

public void SetBoardData(BaseBoardScriptableObject boardData)
{
    _boardData = boardData;
    _board = _boardData.GetBoardData();
}
```

## Benefits of Migration

### **✅ Type Safety**
- Each board has its own specific ScriptableObject type
- Compile-time error checking for board-specific operations
- Better IntelliSense support

### **✅ Organization**
- Clear separation between different board types
- Easier to find and modify board-specific code
- Better code organization and maintainability

### **✅ Scalability**
- Easy to add new board types
- No need to modify existing code when adding boards
- Automatic discovery and generation

### **✅ Extensibility**
- Each board can have its own specific functionality
- Override methods for board-specific behavior
- Custom validation and setup per board type

## Troubleshooting Migration

### **Common Issues**

#### **Compilation Errors**
- **Problem**: `PassiveBoardScriptableObject` not found
- **Solution**: Update references to use `BaseBoardScriptableObject` or specific board types

#### **Missing Assets**
- **Problem**: Old assets not loading
- **Solution**: Generate new assets using the Board Asset Generator

#### **Scene References Broken**
- **Problem**: Scene objects referencing old assets
- **Solution**: Reassign references to new assets in the Inspector

#### **Functionality Not Working**
- **Problem**: Board setup not working correctly
- **Solution**: Ensure new board classes override `SetupBoard()` method

### **Verification Steps**

1. **Check Compilation**: Ensure no compilation errors
2. **Validate Assets**: Use "Validate All Board Assets" in the generator
3. **Test Functionality**: Verify boards load and work correctly
4. **Check References**: Ensure all scene references are updated

## Rollback Plan

If migration issues occur, you can rollback:

1. **Restore old files** from version control
2. **Keep old assets** in `Assets/Resources/PassiveTree/`
3. **Revert code changes** to use old system
4. **Test functionality** to ensure everything works

## Future Considerations

### **Adding New Boards**
1. Create new class inheriting from `BaseBoardScriptableObject`
2. Follow naming convention: `[BoardName]BoardScriptableObject`
3. Override required methods
4. Use Board Asset Generator to create assets

### **Maintenance**
- Regular validation of assets
- Clean up old assets when no longer needed
- Update documentation as system evolves

## Conclusion

The migration to the new individual board ScriptableObject system provides significant benefits in terms of type safety, organization, and scalability. While the migration requires some initial effort, the long-term benefits make it worthwhile.

The new system is designed to be backward-compatible where possible, and the Board Asset Generator provides tools to automate much of the migration process.

---

*For detailed information about the new system, see `Dynamic_Board_Asset_Generator_Guide.md`*
