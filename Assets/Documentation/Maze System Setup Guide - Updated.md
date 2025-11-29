# Maze System Setup Guide - CombatScene Integration

## Vision

The maze minimap will be displayed **in place of cards** during maze mode, while keeping:
- ✅ Player character visible (same as combat scene)
- ✅ Player resources visible (health/mana bars - bottom-left)
- ✅ Resources visible (focus/aggro - bottom-right)
- ✅ Top bar visible (Floor indicator instead of Wave)

The cards area (bottom-center) will be replaced with the minimap grid.

---

## Unity Setup Steps

### 1. Create Maze Config Asset

1. In Unity, right-click in Project window → **Create** → **Dexiled** → **Maze Config**
2. Name it `DefaultMazeConfig`
3. Configure settings:
   - **Total Floors**: 8
   - **Min Nodes Per Floor**: 12
   - **Max Nodes Per Floor**: 16
   - **Grid Size**: (4, 4) - 4x4 grid
   - **Node Type Weights**: Adjust as needed
   - **Boss Floors**: [2, 4, 6]
   - **Boss Retreat Threshold**: 0.45

4. Save to: `Assets/Resources/Maze/DefaultMazeConfig.asset`

   **OR** assign it directly to MazeRunManager in the scene.

---

### 2. Set Up CombatScene for Maze Mode

**The maze system uses the existing CombatScene!** No separate scene needed.

#### Step 1: Open CombatScene

1. Open `Assets/Scenes/CombatScene.unity`

#### Step 2: Add Maze Components

1. **Find or Create Minimap Container**:
   - Locate the `CardHandSection` or card container in the scene
   - This is where cards are normally displayed (bottom-center)
   - Create an empty GameObject as child of this area
   - Name it `MinimapContainer`
   - Add `RectTransform` component if needed
   - Set up layout (GridLayoutGroup recommended)

2. **Add Minimap UI Component**:
   - Add Component → `MazeMinimapUI` to `MinimapContainer`
   - In Inspector, assign:
     - **Minimap Container**: Reference to itself (RectTransform)
     - **Minimap Grid**: Create/add GridLayoutGroup component
     - **Node Size**: (80, 80) - adjust to fit your layout
     - **Node Spacing**: (10, 10)
     - Assign node icons/sprites (optional - will use default if not assigned)

3. **Add Maze Scene Controller**:
   - Find root Canvas or a parent GameObject in CombatScene
   - Add Component → `MazeSceneController`
   - In Inspector, assign references:
     - **Card Hand Container**: The card hand UI element (to hide in maze mode)
     - **Enemy Panels**: Array of enemy panel GameObjects (to hide in maze mode)
     - **End Turn Button**: The end turn button (to hide in maze mode)
     - **Wave Indicator Text**: TextMeshPro showing "WAVE: X/Y" (will change to "FLOOR: X/Y")
     - **Minimap UI**: Reference to the MazeMinimapUI component
     - **Minimap Container**: Reference to MinimapContainer
     - **Player Display**: Player UI panel (to keep visible)
     - **Player Resources Panel**: Player health/mana bars (to keep visible)

#### Step 3: Initial State

- Set `MinimapContainer` to **inactive** by default (combat mode)
- Set card hand container to **active** by default (combat mode)
- MazeSceneController will handle switching automatically

---

### 3. Add MazeRunManager to Scene

1. In CombatScene (or any scene that persists), create empty GameObject
2. Name it `MazeRunManager`
3. Add Component → `MazeRunManager`
4. Assign `DefaultMazeConfig` in Inspector (or leave empty to auto-load from Resources)

**Note**: MazeRunManager uses `DontDestroyOnLoad`, so it persists across scenes.

---

### 4. Create Entry Point (Test Button)

**Option A: Add to MainGameUI Scene**

1. Open MainGameUI scene
2. Find or create a button (e.g., "Start Maze Run")
3. Add Component → `MazeRunEntry`
4. Button will start maze run when clicked

**Option B: Add to EncounterButton**

1. Find an EncounterButton in MainGameUI
2. Add Component → `MazeRunEntry`
3. (Optional) Override config if needed

---

### 5. Optional: Create Node Icon Sprites

Create sprite assets for different node types:

- `start_node_icon` - Start node
- `combat_node_icon` - Combat encounters
- `chest_node_icon` - Treasure
- `shrine_node_icon` - Shrines
- `trap_node_icon` - Traps
- `forge_node_icon` - Forge
- `boss_node_icon` - Boss
- `stairs_node_icon` - Stairs
- `special_node_icon` - Special nodes
- `hidden_node_icon` - Unrevealed nodes

Assign these in MazeMinimapUI component Inspector.

---

## How It Works

### Mode Switching

1. **Combat Mode** (default):
   - Cards visible (bottom-center)
   - Enemies visible
   - End Turn button visible
   - Minimap hidden

2. **Maze Mode** (when maze run active):
   - Cards hidden
   - Enemies hidden
   - End Turn button hidden
   - Minimap visible (in place of cards)
   - Player display visible
   - Top bar shows "FLOOR: X/Y" instead of "WAVE: X/Y"

### Flow

1. Player clicks button with `MazeRunEntry`
2. `MazeRunManager.StartRun()` generates first floor
3. Loads CombatScene (or scene already loaded)
4. `MazeSceneController` detects maze run active
5. Automatically switches to maze mode
6. `MazeMinimapUI` displays floor minimap
7. Player clicks nodes → combat → returns to maze mode

---

## Testing

1. Press Play
2. Click button with `MazeRunEntry`
3. CombatScene should load (or already be loaded)
4. Cards should hide, minimap should appear
5. Player display and resources should remain visible
6. Click nodes to move through the maze

---

## Troubleshooting

**"Minimap not showing"**
- Check MazeSceneController has correct references
- Ensure MinimapContainer is active
- Check MazeRunManager has active run

**"Cards still showing in maze mode"**
- Check MazeSceneController has Card Hand Container assigned
- Verify autoDetectMode is enabled
- Check that MazeRunManager.HasActiveRun() returns true

**"Player display hidden"**
- Ensure Player Display and Player Resources Panel are assigned
- They should always be active (MazeSceneController keeps them visible)

**"Nodes not clickable"**
- Check MinimapGrid is set up correctly
- Verify node buttons are being created
- Check MazeRunManager.SelectNode() is being called

**"Scene doesn't switch modes"**
- Check MazeSceneController.autoDetectMode is enabled
- Verify MazeRunManager exists and has active run
- Call MazeSceneController.CheckAndSwitchMode() manually if needed

---

## File Structure

```
Assets/
├── Scripts/
│   └── MazeSystem/
│       ├── MazeNode.cs
│       ├── MazeFloor.cs
│       ├── MazeRunData.cs
│       ├── MazeConfig.cs
│       ├── MazeRunManager.cs
│       ├── MazeRunEntry.cs
│       ├── MazeMinimapUI.cs          ← New: Minimap display
│       └── MazeSceneController.cs    ← New: Mode switching
├── Resources/
│   └── Maze/
│       └── DefaultMazeConfig.asset
└── Scenes/
    └── CombatScene.unity             ← Modified: Add maze components
```

---

## Next Steps

Once setup is complete, you should have:
1. ✅ Minimap displaying in place of cards
2. ✅ Player character and resources visible
3. ✅ Node clicking working
4. ✅ Automatic mode switching

**Still needed:**
- Combat integration (node click → combat → return)
- Visual polish (node icons, animations)
- Node interaction UI (shrine effects, chest loot, etc.)

