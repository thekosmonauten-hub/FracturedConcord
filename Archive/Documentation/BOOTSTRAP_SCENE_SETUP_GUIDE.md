# Bootstrap Scene Setup Guide

## Overview

A Bootstrap scene is a persistent scene that loads once at game start and never unloads. It contains all persistent managers and systems that need to exist throughout the game's lifetime. This enables additive scene loading, which dramatically improves scene transition performance.

## Benefits

- ✅ **Faster Scene Transitions**: Only content scenes load/unload, not managers
- ✅ **No Manager Recreation**: Managers persist across all scenes
- ✅ **Better Memory Management**: Assets loaded once, reused everywhere
- ✅ **Smoother Performance**: No initialization overhead on scene changes
- ✅ **Cleaner Architecture**: Clear separation between persistent systems and content

## Architecture

```
Bootstrap Scene (Persistent - Never Unloads)
├── CharacterManager
├── EquipmentManager
├── EncounterManager
├── WarrantsManager
├── DeckManager
├── LootManager
├── StashManager
├── AssetPreloader
├── SceneInitializationManager
├── TransitionManager
└── Other Persistent Managers

+ Content Scenes (Load/Unload Additively)
├── MainMenu
├── MainGameUI
├── EquipmentScreen
├── CombatScene
├── WarrantTree
└── Other Content Scenes
```

---

## Step-by-Step Setup

### Step 1: Create Bootstrap Scene

1. **Create New Scene**:
   - File → New Scene → Basic (Scene)
   - Name it `BootstrapScene.unity`
   - Save to: `Assets/Scenes/BootstrapScene.unity`

2. **Set as First Scene in Build**:
   - File → Build Settings
   - Drag `BootstrapScene.unity` to the top of the scene list
   - This ensures it loads first when the game starts

3. **Keep Scene Minimal**:
   - The scene should be mostly empty (just managers)
   - No UI, no game objects, just the essential managers
   - You can add a simple camera if needed (or use a UI-only camera)

---

### Step 2: Identify Persistent Managers

These managers should be moved to Bootstrap scene (they already use `DontDestroyOnLoad`):

**Core Managers:**
- ✅ `CharacterManager`
- ✅ `EquipmentManager`
- ✅ `EncounterManager`
- ✅ `WarrantsManager`
- ✅ `DeckManager`
- ✅ `LootManager`
- ✅ `StashManager`
- ✅ `AutoSaveManager`
- ✅ `TutorialManager`
- ✅ `MazeRunManager`
- ✅ `StarterDeckManager`

**Scene Management:**
- ✅ `AssetPreloader` (new)
- ✅ `SceneInitializationManager` (new)
- ✅ `TransitionManager`

**Optional (if they use DontDestroyOnLoad):**
- Check other managers in your project

---

### Step 3: Create Bootstrap Manager Script

This script will initialize all managers and handle the initial scene load.

```csharp
// Assets/Scripts/SceneManagement/BootstrapManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the Bootstrap scene and initial game setup.
/// Loads once at game start and never unloads.
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    [Header("Initial Scene")]
    [Tooltip("Scene to load after bootstrap initialization")]
    [SerializeField] private string initialSceneName = "MainMenu";
    
    [Header("Bootstrap Settings")]
    [Tooltip("Wait for asset preloading before loading initial scene")]
    [SerializeField] private bool waitForAssetPreload = true;
    
    private void Awake()
    {
        // Ensure this is the only BootstrapManager
        if (FindObjectsOfType<BootstrapManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        StartCoroutine(InitializeBootstrap());
    }
    
    /// <summary>
    /// Initialize bootstrap systems and load initial scene
    /// </summary>
    private IEnumerator InitializeBootstrap()
    {
        Debug.Log("[BootstrapManager] Starting bootstrap initialization...");
        
        // Wait for asset preloader to finish (if enabled)
        if (waitForAssetPreload && AssetPreloader.Instance != null)
        {
            // Wait a few frames for preloading to start
            yield return new WaitForSeconds(0.1f);
            
            // Wait for preloading to complete
            while (AssetPreloader.Instance != null)
            {
                // Check if preloading is complete (you may need to add a public property)
                yield return null;
            }
        }
        
        // Wait one more frame to ensure all managers are ready
        yield return null;
        
        Debug.Log($"[BootstrapManager] Bootstrap ready, loading initial scene: {initialSceneName}");
        
        // Load initial scene additively
        yield return ImprovedSceneLoader.LoadSceneAdditive(initialSceneName, true);
        
        Debug.Log("[BootstrapManager] ✅ Bootstrap initialization complete!");
    }
}
```

---

### Step 4: Set Up Bootstrap Scene

1. **Open BootstrapScene.unity**

2. **Create BootstrapManager GameObject**:
   - Right-click Hierarchy → Create Empty
   - Name it `BootstrapManager`
   - Add Component → `BootstrapManager`
   - Set **Initial Scene Name** to `"MainMenu"` (or your starting scene)

3. **Add Persistent Managers**:
   For each manager, you have two options:

   **Option A: Auto-Create (Recommended)**
   - Managers auto-create themselves if not found
   - Just ensure the scripts exist
   - They'll be created automatically when accessed

   **Option B: Manual Setup**
   - Create empty GameObjects for each manager
   - Add the manager components
   - Configure settings as needed

4. **Add Scene Management Components**:
   - Create `AssetPreloader` GameObject → Add `AssetPreloader` component
   - Create `SceneInitializationManager` GameObject → Add `SceneInitializationManager` component
   - Create `TransitionManager` GameObject → Add `TransitionManager` component (if not auto-created)

5. **Save the Scene**

---

### Step 5: Update TransitionManager for Additive Loading

Modify `TransitionManager` to use additive loading when Bootstrap is present.

```csharp
// In TransitionManager.cs, update TransitionCoroutine:

private IEnumerator TransitionCoroutine(string sceneName)
{
    // ... existing fade-in code ...
    
    // Check if Bootstrap scene is loaded (means we're using additive loading)
    Scene bootstrapScene = SceneManager.GetSceneByName("BootstrapScene");
    bool useAdditive = bootstrapScene.IsValid();
    
    if (useAdditive)
    {
        // Get current active scene name
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        
        // Don't unload Bootstrap scene
        if (currentSceneName != "BootstrapScene")
        {
            // Load new scene additively
            yield return ImprovedSceneLoader.LoadSceneAdditive(sceneName, true);
            
            // Unload old scene
            yield return ImprovedSceneLoader.UnloadSceneAsync(currentSceneName);
        }
        else
        {
            // First scene load - just load additively
            yield return ImprovedSceneLoader.LoadSceneAdditive(sceneName, true);
        }
    }
    else
    {
        // Fallback to single scene loading (backward compatibility)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // ... existing async loading code ...
    }
    
    // ... existing fade-out code ...
}
```

---

### Step 6: Update Build Settings

1. **File → Build Settings**
2. **Scene Order**:
   ```
   0. BootstrapScene (must be first!)
   1. MainMenu
   2. CharacterCreation
   3. MainGameUI
   4. EquipmentScreen
   5. CombatScene
   ... (other scenes)
   ```
3. **Save Build Settings**

---

### Step 7: Update Scene Transition Code

Update any code that uses `SceneManager.LoadScene()` to use the new additive loading.

**Find and Replace Pattern:**
```csharp
// OLD:
SceneManager.LoadScene("EquipmentScreen");

// NEW:
if (TransitionManager.Instance != null)
{
    TransitionManager.Instance.TransitionToScene("EquipmentScreen");
}
else
{
    StartCoroutine(ImprovedSceneLoader.LoadSceneAdditive("EquipmentScreen", true));
}
```

**Files to Update:**
- `EncounterManager.cs` - Already uses TransitionManager ✅
- `MainMenuController.cs` - Check if it needs updates
- `DialogueManager.cs` - Check scene loading
- Any other direct `SceneManager.LoadScene()` calls

---

### Step 8: Test and Verify

1. **Run the Game**:
   - Bootstrap scene should load first
   - Then MainMenu should load additively
   - Check console for initialization logs

2. **Verify Managers Persist**:
   - Navigate between scenes
   - Managers should not be recreated
   - Check that `DontDestroyOnLoad` objects persist

3. **Check Performance**:
   - Scene transitions should be faster
   - No manager initialization on scene changes
   - Smoother frame rates

---

## Troubleshooting

### Bootstrap Scene Not Loading First?
- Check Build Settings - BootstrapScene must be at index 0
- Verify scene is enabled in Build Settings

### Managers Being Destroyed?
- Ensure managers use `DontDestroyOnLoad` in their `Awake()` methods
- Check that managers are in Bootstrap scene, not content scenes

### Scenes Loading Twice?
- Make sure you're not loading Bootstrap scene again
- Check that `TransitionManager` checks for Bootstrap before loading

### Missing Managers?
- Managers should auto-create if using singleton pattern
- Or manually add them to Bootstrap scene

---

## Advanced: Separate UI Scene (Optional)

For even better performance, you can separate UI into its own persistent scene:

1. **Create UIScene.unity**
2. **Move all UI elements** to this scene
3. **Load additively** and keep loaded
4. **Content scenes** become UI-free, loading even faster

**Structure:**
```
Bootstrap Scene (Managers)
+ UIScene (Persistent UI)
+ ContentScene (Current content - swaps in/out)
```

---

## Migration Checklist

- [ ] Create `BootstrapScene.unity`
- [ ] Create `BootstrapManager.cs` script
- [ ] Add managers to Bootstrap scene
- [ ] Set BootstrapScene as first in Build Settings
- [ ] Update `TransitionManager` for additive loading
- [ ] Update all `SceneManager.LoadScene()` calls
- [ ] Test scene transitions
- [ ] Verify managers persist
- [ ] Check performance improvements

---

## Expected Results

**Before Bootstrap:**
- Scene load: ~2-5 seconds (depends on scene complexity)
- Managers recreated on each scene
- Heavy initialization on every transition

**After Bootstrap:**
- Scene load: ~0.5-2 seconds (only content loads)
- Managers persist (no recreation)
- Minimal initialization overhead

---

## Files to Create/Modify

### New Files:
- `Assets/Scenes/BootstrapScene.unity`
- `Assets/Scripts/SceneManagement/BootstrapManager.cs`

### Files to Modify:
- `Assets/Scripts/UI/TransitionManager.cs` - Add additive loading support
- Any files using `SceneManager.LoadScene()` directly

---

## Next Steps After Setup

1. **Profile Performance**: Use Unity Profiler to measure improvements
2. **Optimize Further**: Convert more components to `ISceneInitializable`
3. **Consider UI Separation**: Move UI to separate persistent scene
4. **Expand Asset Preloading**: Add more assets to preload list

