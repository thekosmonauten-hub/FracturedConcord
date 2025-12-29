# Scene Loading Integration Guide

## Quick Start - Getting Started with Improved Loading

This guide will walk you through integrating the new scene loading optimizations into your project.

## Step 1: Add Asset Preloader (Recommended - Do This First)

The `AssetPreloader` preloads common assets at game startup, preventing loading delays during scene transitions.

### Setup:

1. **Open your MainMenu scene** (or the first scene that loads when the game starts)

2. **Create a GameObject for AssetPreloader**:
   - Right-click in Hierarchy → Create Empty
   - Name it `AssetPreloader`
   - Add Component → `AssetPreloader`

3. **Configure Settings** (optional):
   - **Preload On Start**: ✅ Checked (default)
   - **Max Load Time Per Frame**: 0.016 (default - ~1 frame at 60fps)

4. **Add More Assets to Preload** (optional):
   - Open `Assets/Scripts/SceneManagement/AssetPreloader.cs`
   - Find the `commonAssets` array in `PreloadCommonAssets()`
   - Add any frequently-used asset names:
     ```csharp
     string[] commonAssets = new string[]
     {
         "WarrantDatabase",
         "WarrantNotableDatabase",
         "WarrantAffixDatabase",
         "ForgeMaterialDatabase",
         "CardDatabase",  // Add if you have one
         "ItemDatabase", // Add if you have one
         // Add more common assets here
     };
     ```

5. **Save the scene**

**Result**: Common assets will now be preloaded at game start, making scene transitions faster.

---

## Step 2: Add Scene Initialization Manager (Recommended)

The `SceneInitializationManager` automatically finds and initializes components that implement `ISceneInitializable`, spreading heavy work across frames.

### Setup:

1. **Open your MainMenu scene** (or Bootstrap scene if you create one)

2. **Create a GameObject for SceneInitializationManager**:
   - Right-click in Hierarchy → Create Empty
   - Name it `SceneInitializationManager`
   - Add Component → `SceneInitializationManager`

3. **Configure Settings** (optional):
   - **Max Time Per Frame**: 0.016 (default - ~1 frame at 60fps)
   - **Components Per Frame**: 1 (default - lower = smoother, higher = faster)

4. **Save the scene**

**Result**: The manager will automatically initialize any `ISceneInitializable` components in loaded scenes.

---

## Step 3: Convert Heavy Components to Use ISceneInitializable (Optional but Recommended)

For components that do heavy initialization in `Awake()` or `Start()`, convert them to use the deferred initialization pattern.

### Example: Converting a Component

**Before** (blocks scene load):
```csharp
public class MyHeavyComponent : MonoBehaviour
{
    private void Start()
    {
        // Heavy work that blocks
        LoadLargeData();
        BuildComplexUI();
        InitializeManyObjects();
    }
}
```

**After** (deferred, non-blocking):
```csharp
using System.Collections;

public class MyHeavyComponent : MonoBehaviour, ISceneInitializable
{
    private bool isInitialized = false;
    
    public bool IsInitialized => isInitialized;
    
    private void Start()
    {
        // Lightweight setup only
        // Heavy work moved to Initialize()
    }
    
    public IEnumerator Initialize()
    {
        // Heavy work spread across frames
        LoadLargeData();
        yield return null; // Wait one frame
        
        BuildComplexUI();
        yield return null; // Wait one frame
        
        InitializeManyObjects();
        yield return null; // Wait one frame
        
        isInitialized = true;
    }
}
```

### Components Already Optimized:
- ✅ `EquipmentScreenUI` - Uses deferred initialization
- ✅ `InventoryGridUI` - Uses deferred refresh
- ✅ `EncounterManager` - Uses deferred initialization
- ✅ `WarrantBoardStateController` - Uses deferred initialization

---

## Step 4: Using Improved Scene Loader (Advanced - Optional)

The `ImprovedSceneLoader` provides additive scene loading, which is faster but requires more setup.

### Current Status:
- ✅ Framework created (`ImprovedSceneLoader.cs`)
- ⚠️ Not yet integrated into `TransitionManager`
- ⚠️ Requires Bootstrap scene pattern for full benefits

### When to Use:
- **Now**: You can use it manually for specific scene transitions
- **Future**: Full integration would require Bootstrap scene setup

### Manual Usage Example:
```csharp
// Instead of:
SceneManager.LoadScene("EquipmentScreen");

// Use:
StartCoroutine(ImprovedSceneLoader.LoadSceneAdditive("EquipmentScreen", true));
```

### Full Integration (Future):
This would require:
1. Creating a Bootstrap scene with persistent managers
2. Separating UI into its own persistent scene
3. Updating all scene transition code
4. Testing thoroughly

**Recommendation**: Start with Steps 1-3 first, then consider Step 4 later if you need even more performance.

---

## Step 5: Verify It's Working

### Check Console Logs:
When you run the game, you should see:
```
[AssetPreloader] Starting asset preload...
[AssetPreloader] Preloaded: WarrantDatabase
[AssetPreloader] Preloaded: WarrantNotableDatabase
[AssetPreloader] ✅ Preload complete! Loaded X assets.
```

When scenes load:
```
[SceneInitializationManager] Found X components to initialize in scene: EquipmentScreen
[SceneInitializationManager] Initializing: EquipmentScreenUI
[SceneInitializationManager] ✅ Initialized: EquipmentScreenUI
```

### Performance Check:
1. **Before**: Scene loads → Freeze → Scene appears
2. **After**: Scene loads → Smooth transition → Scene appears progressively

---

## Quick Integration Checklist

- [ ] **Step 1**: Add `AssetPreloader` to MainMenu scene
- [ ] **Step 2**: Add `SceneInitializationManager` to MainMenu scene
- [ ] **Step 3**: (Optional) Convert heavy components to `ISceneInitializable`
- [ ] **Step 4**: (Advanced) Consider additive loading for specific scenes
- [ ] **Test**: Verify faster loading times and smoother transitions

---

## Troubleshooting

### Assets Not Preloading?
- Check that asset names in `AssetPreloader` match your Resources folder structure
- Verify assets exist in `Resources/` folder
- Check console for warnings about missing assets

### Initialization Not Happening?
- Ensure `SceneInitializationManager` is in a scene that persists (MainMenu or Bootstrap)
- Check that components implement `ISceneInitializable` correctly
- Verify `IsInitialized` property returns `true` after initialization

### Still Seeing Freezing?
- Check Unity Profiler for remaining bottlenecks
- Look for heavy work still in `Awake()`/`Start()` methods
- Consider converting more components to deferred initialization

---

## Next Steps

1. **Start Simple**: Just add `AssetPreloader` and `SceneInitializationManager` to MainMenu
2. **Test**: See immediate improvements in scene loading
3. **Iterate**: Convert more components to `ISceneInitializable` as needed
4. **Advanced**: Consider Bootstrap scene pattern for maximum performance

---

## Files Reference

- **AssetPreloader**: `Assets/Scripts/SceneManagement/AssetPreloader.cs`
- **SceneInitializationManager**: `Assets/Scripts/SceneManagement/SceneInitializationManager.cs`
- **ISceneInitializable**: `Assets/Scripts/SceneManagement/ISceneInitializable.cs`
- **ImprovedSceneLoader**: `Assets/Scripts/SceneManagement/ImprovedSceneLoader.cs`

