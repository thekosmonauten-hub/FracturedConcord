# Board Asset Generator - Quick Reference

## 🚀 Quick Start

### 1. Access the Generator
- **Menu**: `Tools > Passive Tree > Generate Board Assets`
- **Window**: "Board Asset Generator"

### 2. Generate Assets
- Click **"Generate All Board Assets"** for all boards
- Or click individual board buttons for specific boards

### 3. Verify Results
- Click **"Show Available Board Classes"** to see detected classes
- Click **"Validate All Board Assets"** to check existing assets

## 📋 Naming Convention

| Class Name | Board ID | Asset Name |
|------------|----------|------------|
| `CoreBoardScriptableObject` | `core_board` | `core_board.asset` |
| `FireBoardScriptableObject` | `fire_board` | `fire_board.asset` |
| `ColdBoardScriptableObject` | `cold_board` | `cold_board.asset` |

**Rule**: `[BoardName]BoardScriptableObject` → `[boardname]_board`

## 🛠️ Creating New Board Classes

### Template
```csharp
[CreateAssetMenu(fileName = "NewBoard", menuName = "Passive Tree/New Board")]
public class NewBoardScriptableObject : BaseBoardScriptableObject
{
    [ContextMenu("Setup New Board")]
    public override void SetupBoard()
    {
        boardData.id = "new_board";
        boardData.name = "New Board";
        boardData.theme = BoardTheme.YourTheme;
        // ... rest of setup
    }
}
```

### Required Overrides
- `SetupBoard()` - Main setup method
- `GetStartingStats()` - Starting node stats
- `AddBoardNodes()` - Board-specific nodes

## 🔧 Common Commands

### Generate Assets
```csharp
// In Unity Editor
Tools > Passive Tree > Generate Board Assets
```

### Show Available Classes
```csharp
// Lists all detected board classes
ShowAvailableBoardClasses();
```

### Clean Up Old Assets
```csharp
// Removes invalid/duplicate assets
CleanUpOldAssets();
```

### Validate Assets
```csharp
// Checks existing asset validity
ValidateAllBoardAssets();
```

## 🎨 Theme Mapping

| Board Type | Theme | Color | Max Points |
|------------|-------|-------|------------|
| Core | Utility | Light Blue | 15 |
| Fire | Fire | Orange/Red | 20 |
| Cold | Cold | Blue | 20 |
| Lightning | Lightning | Yellow | 20 |
| Life | Life | Green | 20 |
| Discard | Utility | Purple | 20 |

## 🐛 Troubleshooting

### No Classes Detected
- ✅ Inherit from `BaseBoardScriptableObject`
- ✅ Follow naming convention
- ✅ Check compilation errors

### Assets Not Generated
- ✅ Class name ends with `BoardScriptableObject`
- ✅ No compilation errors
- ✅ Check console for warnings

### Invalid Assets
- ✅ Use "Clean Up Old Assets"
- ✅ Regenerate with "Generate All Board Assets"
- ✅ Check asset paths in `Assets/Resources/PassiveTree/`

## 📁 File Locations

### Scripts
```
Assets/Scripts/Data/PassiveTree/
├── BaseBoardScriptableObject.cs
├── CoreBoardScriptableObject.cs
├── FireBoardScriptableObject.cs
└── [YourBoard]ScriptableObject.cs
```

### Generated Assets
```
Assets/Resources/PassiveTree/
├── core_board.asset
├── fire_board.asset
├── cold_board.asset
└── [your_board].asset
```

## 🔄 Integration

### With PassiveTreeBoardManager
```csharp
// Automatically discovers new assets
var boards = PassiveTreeBoardManager.Instance.GetAvailableBoards();
```

### With UI Components
```csharp
// Updated to use BaseBoardScriptableObject
[SerializeField] private BaseBoardScriptableObject _boardData;
```

## ⚡ Quick Tips

1. **Always inherit** from `BaseBoardScriptableObject`
2. **Follow naming** convention exactly
3. **Use "Show Available Board Classes"** to debug
4. **Clean up old assets** when migrating
5. **Validate assets** regularly

## 📞 Support

- **Console Logs**: Check for detailed error messages
- **Validation**: Use "Validate All Board Assets" for diagnostics
- **Documentation**: See `Dynamic_Board_Asset_Generator_Guide.md` for full details

---

*This quick reference covers the most common use cases. For detailed information, see the full documentation.*
