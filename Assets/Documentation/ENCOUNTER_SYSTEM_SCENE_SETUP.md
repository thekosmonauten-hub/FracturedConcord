# EncounterManager Scene Setup - MainGameUI

## Quick Answer

**You don't NEED to add anything** - the system will auto-create itself. However, **it's RECOMMENDED** to add an EncounterManager GameObject to MainGameUI for better control and configuration.

## Auto-Creation Behavior

The refactored `EncounterManager` uses a singleton pattern that **auto-creates itself** if it doesn't exist:

```15:32:Assets/Scripts/EncounterSystem/EncounterManager.cs
    private static EncounterManager _instance;
    public static EncounterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EncounterManager>();
                if (_instance == null)
                {
                    var go = new GameObject("EncounterManager");
                    _instance = go.AddComponent<EncounterManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
```

This means:
- ✅ If no EncounterManager exists, it creates one automatically
- ✅ It uses `DontDestroyOnLoad`, so it persists across scenes
- ✅ It initializes itself in `Awake()`

## Recommended Setup (Optional but Better)

While auto-creation works, **adding it manually to MainGameUI is recommended** because:

1. **Configuration Access** - You can set Resources paths in the Inspector
2. **Editor Tools** - You can use context menu tools (Validate, Import, etc.)
3. **EnemyDatabase Reference** - You can assign the EnemyDatabase prefab
4. **Initialization Control** - Better control over when it initializes
5. **Debugging** - Easier to see and inspect in the scene

### Steps to Add EncounterManager to MainGameUI

1. **Open MainGameUI scene**

2. **Create GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name it: `EncounterManager`

3. **Add Component:**
   - Select the `EncounterManager` GameObject
   - Click "Add Component" in Inspector
   - Search for and add: `EncounterManager`

4. **Configure Settings:**
   - **Load From Resources**: ✅ Checked (default)
   - **Act 1 Resources Path**: `Encounters/Act1`
   - **Act 2 Resources Path**: `Encounters/Act2`
   - **Act 3 Resources Path**: `Encounters/Act3`
   - **Act 4 Resources Path**: `Encounters/Act4`
   - **Enemy Database Ref**: (Optional) Drag your EnemyDatabase prefab here if you have one

5. **Save the Scene**

## What Happens on Scene Load

When MainGameUI loads, the EncounterManager:

```444:450:Assets/Scripts/EncounterSystem/EncounterManager.cs
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGameUI")
        {
            Debug.Log("[EncounterManager] Scene loaded: MainGameUI. Refreshing encounter system.");
            InitializeEncounterGraph();
        }
    }
```

This refreshes the encounter graph and applies character progression, ensuring everything is up-to-date.

## Verification

To verify everything is working:

1. **Play the game** and check the console for:
   ```
   [EncounterManager] System initialized
   [EncounterManager] Loaded X encounters from Resources/Encounters/Act1
   [EncounterManager] Graph built with X encounters
   [EncounterManager] All encounters validated successfully.
   ```

2. **Check Hierarchy** - You should see an `EncounterManager` GameObject (either manually added or auto-created)

3. **Test Validation** - Right-click EncounterManager in Inspector → "Encounters > Validate Encounters"

## Troubleshooting

### If Encounters Don't Load

1. Check that `loadFromResources` is enabled
2. Verify Resources paths are correct
3. Ensure encounter assets exist in `Resources/Encounters/Act1`, etc.
4. Check console for errors

### If EncounterManager Auto-Creates

If you see a new `EncounterManager` GameObject created at runtime:
- This is normal if you didn't add one manually
- It will use default settings
- Consider adding one manually to the scene for better control

### If Validation Fails

1. Right-click EncounterManager → "Encounters > Validate Encounters"
2. Fix any errors reported
3. Re-validate

## Summary

| Setup Method | Pros | Cons |
|-------------|------|------|
| **Auto-Create** | No setup needed, works automatically | Can't configure in Inspector, harder to debug |
| **Manual Add** | Full control, Inspector config, editor tools | Requires one-time setup |

**Recommendation**: Add it manually to MainGameUI for the best experience, but the system will work either way.

