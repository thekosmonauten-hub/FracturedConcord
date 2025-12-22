# Maze System Setup Guide

## What's Been Created

The core maze system has been implemented:

1. **Data Structures** (`Assets/Scripts/MazeSystem/`):
   - `MazeNode.cs` - Individual nodes on the floor
   - `MazeFloor.cs` - Floor/minimap data structure
   - `MazeRunData.cs` - Run state and modifiers
   - `MazeConfig.cs` - ScriptableObject configuration

2. **MazeRunManager** (`Assets/Scripts/MazeSystem/MazeRunManager.cs`):
   - Singleton manager for maze runs
   - Floor generation (12-16 nodes, connectivity validation)
   - Node interaction handling
   - Combat integration hooks

3. **Entry Point** (`Assets/Scripts/MazeSystem/MazeRunEntry.cs`):
   - Component to start maze runs from buttons/encounters

---

## Unity Setup Steps

### 1. Create Maze Config Asset

1. In Unity, right-click in Project window → **Create** → **Dexiled** → **Maze Config**
2. Name it `DefaultMazeConfig` (or any name)
3. Configure settings:
   - **Total Floors**: 8 (or your desired count)
   - **Min Nodes Per Floor**: 12
   - **Max Nodes Per Floor**: 16
   - **Grid Size**: (4, 4) - 4x4 grid
   - **Node Type Weights**: Adjust probabilities as needed
   - **Boss Floors**: [2, 4, 6] - Bosses spawn on these floors
   - **Boss Retreat Threshold**: 0.45 (boss retreats at 45% HP)

4. Place it in: `Assets/Resources/Maze/DefaultMazeConfig.asset`

   **OR** manually assign it to MazeRunManager in the scene.

---

### 2. Create MazeScene

1. **Create New Scene**:
   - File → New Scene → Basic (Scene)
   - Name it `MazeScene`
   - Save to: `Assets/Scenes/MazeScene.unity`

2. **Add UI Canvas**:
   - Right-click Hierarchy → **UI** → **Canvas**
   - Name it `MazeCanvas`

3. **Create Minimap UI Structure**:
   ```
   MazeCanvas
   ├── MinimapPanel (Panel)
   │   ├── GridLayout (GridLayoutGroup or manual layout)
   │   │   └── [Node buttons will be spawned here]
   ├── RunInfoPanel (Panel)
   │   ├── FloorText (TextMeshPro or Text)
   │   ├── SeedText (TextMeshPro or Text)
   │   └── ModifiersText (TextMeshPro or Text)
   └── ExitButton (Button)
   ```

4. **Add MazeRunManager to Scene**:
   - Right-click Hierarchy → **Create Empty**
   - Name it `MazeRunManager`
   - Add Component → `MazeRunManager`
   - In Inspector, assign `DefaultMazeConfig` (if you created it)

5. **Create Minimap UI Script** (Next step - I'll provide this):
   - Create a script `MazeSceneUI.cs` to handle minimap display and node clicks

---

### 3. Add Scene to Build Settings

1. File → Build Settings
2. Click "Add Open Scenes" (or drag MazeScene into build list)
3. Ensure `MazeScene` is in the build list

---

### 4. Create Entry Point (Test Button)

**Option A: Add to Existing EncounterButton**

1. Find an EncounterButton in MainGameUI scene
2. Add Component → `MazeRunEntry`
3. (Optional) Assign override config if needed

**Option B: Create Test Button**

1. In MainGameUI scene, create a button:
   - Right-click Hierarchy → **UI** → **Button**
   - Name it `StartMazeButton`
2. Add Component → `MazeRunEntry`
3. Position button where you want it

---

### 5. Optional: Create Default Config in Resources

If you want the system to auto-load config:

1. Create folder: `Assets/Resources/Maze/`
2. Place `DefaultMazeConfig.asset` in that folder
3. MazeRunManager will auto-load it if not assigned in scene

---

## Next Steps (To Be Implemented)

1. **MazeSceneUI Script** - Minimap display and interaction
   - Spawn node buttons from floor data
   - Handle node clicks
   - Update UI (floor info, modifiers, etc.)

2. **Combat Integration**
   - Modify CombatSceneManager to check for maze context
   - Return to MazeScene on victory
   - Handle boss retreat logic

3. **Node Type Implementations**
   - Chest loot
   - Shrine effects (run modifiers)
   - Trap effects
   - Forge UI (card fusion)

---

## Testing

1. Press Play
2. Click the button with `MazeRunEntry` component
3. Should load `MazeScene`
4. MazeRunManager should generate Floor 1 with 12-16 nodes
5. (Once UI is implemented) Should see minimap with nodes

---

## Troubleshooting

**"MazeRunManager.Instance is null"**
- Ensure MazeRunManager GameObject exists in scene
- Check that DontDestroyOnLoad is working

**"No maze config found"**
- Assign config to MazeRunManager in Inspector, OR
- Place config in `Resources/Maze/DefaultMazeConfig.asset`

**"Cannot generate floor"**
- Check MazeConfig has valid settings (min/max nodes, grid size)
- Ensure grid size can fit max nodes (e.g., 4x4 = 16 cells max)

**Scene doesn't load**
- Check scene name matches `MazeScene`
- Ensure scene is in Build Settings

---

## File Structure

```
Assets/
├── Scripts/
│   └── MazeSystem/
│       ├── MazeNode.cs
│       ├── MazeFloor.cs
│       ├── MazeRunData.cs
│       ├── MazeConfig.cs (ScriptableObject)
│       ├── MazeRunManager.cs
│       └── MazeRunEntry.cs
├── Resources/
│   └── Maze/
│       └── DefaultMazeConfig.asset (create this)
└── Scenes/
    └── MazeScene.unity (create this)
```

