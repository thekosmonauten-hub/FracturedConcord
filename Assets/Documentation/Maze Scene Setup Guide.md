# Maze Scene Setup Guide

## Overview

The maze system uses a **separate MazeScene** instead of modifying CombatScene. This keeps CombatScene untouched and allows both scenes to coexist.

**MazeScene** is a copy of CombatScene with maze-specific components added. It looks identical but has the minimap instead of cards.

---

## Setup Steps

### 1. Create MazeScene

1. **Copy CombatScene**:
   - In Unity, open `Assets/Scenes/CombatScene.unity`
   - File → Save As → Name it `MazeScene`
   - Save to: `Assets/Scenes/MazeScene.unity`

2. **Add Scene to Build Settings**:
   - File → Build Settings
   - Click "Add Open Scenes" (or drag MazeScene into build list)

### 2. Set Up MazeScene Components

#### Step 1: Add Minimap Container

1. In MazeScene, locate the **CardHandSection** or card container (where cards are displayed)
2. Create an empty GameObject as child of this area
3. Name it `MinimapContainer`
4. Add `RectTransform` component if needed
5. Set up layout (GridLayoutGroup recommended)

#### Step 2: Add Maze Components

1. **Add MazeMinimapUI**:
   - Select `MinimapContainer`
   - Add Component → `MazeMinimapUI`
   - In Inspector, assign:
     - **Minimap Container**: Reference to itself (RectTransform)
     - **Minimap Grid**: Create/add GridLayoutGroup component
     - **Node Size**: (80, 80) - adjust to fit
     - **Node Spacing**: (10, 10)
     - Assign node icons/sprites (optional)

2. **Add MazeSceneController**:
   - Find root Canvas or a parent GameObject in MazeScene
   - Add Component → `MazeSceneController`
   - In Inspector, assign:
     - **Card Hand Container**: The card hand UI element (to hide)
     - **Enemy Panels**: Array of enemy panel GameObjects (to hide)
     - **End Turn Button**: The end turn button (to hide)
     - **Wave Indicator Text**: TextMeshPro showing "WAVE: X/Y" (will change to "FLOOR: X/Y")
     - **Minimap UI**: Reference to the MazeMinimapUI component
     - **Minimap Container**: Reference to MinimapContainer
     - **Player Display**: Player UI panel (to keep visible)
     - **Player Resources Panel**: Player health/mana bars (to keep visible)

3. **Add MazeRunManager** (if not already in scene):
   - Create empty GameObject
   - Name it `MazeRunManager`
   - Add Component → `MazeRunManager`
   - Assign `DefaultMazeConfig` in Inspector (or leave empty to auto-load from Resources)

#### Step 3: Initial State

- Set `MinimapContainer` to **active** (default for maze mode)
- Set card hand container to **inactive** (hidden in maze mode)
- Set enemy panels to **inactive** (hidden in maze mode)
- Keep player display **active** (always visible)

---

### 3. Create EncounterDataAsset for Maze Entry

To route players to MazeScene using the existing encounter system:

1. **Create EncounterDataAsset**:
   - Right-click Project → **Create** → **Dexiled** → **Encounter Data**
   - Name it `MazeEntryEncounter` (or similar)
   - Configure:
     - **Encounter ID**: Unique ID (e.g., 100)
     - **Encounter Name**: "The Weave of Broken Clauses" (or your maze name)
     - **Scene Name**: `MazeScene` ← **IMPORTANT: Must be "MazeScene"**
     - **Act Number**: 2 (or wherever maze is unlocked)
     - **Area Level**: 1
     - Configure prerequisites, sprites, etc. as needed

2. **Save to Resources**:
   - Place in: `Assets/Resources/Encounters/Act2/MazeEntryEncounter.asset`
   - (Or appropriate act folder based on when maze unlocks)

3. **Use with EncounterButton**:
   - Find or create EncounterButton in MainGameUI
   - Assign the `MazeEntryEncounter` asset
   - The button will load MazeScene when clicked (via EncounterManager)

---

### 4. Alternative: Direct Entry (MazeRunEntry)

If you don't want to use EncounterData:

1. **Create Button in MainGameUI**:
   - Right-click Hierarchy → **UI** → **Button**
   - Name it `StartMazeButton`

2. **Add MazeRunEntry Component**:
   - Add Component → `MazeRunEntry`
   - Set `useEncounterSystem` to **false**
   - (Optional) Configure seed and config override

3. **Button will directly load MazeScene** when clicked

---

## How It Works

### Scene Flow

1. **Player clicks maze entry** (EncounterButton or MazeRunEntry):
   - MazeRunManager starts a new run
   - Loads **MazeScene** (via EncounterData.sceneName or directly)

2. **MazeScene loads**:
   - MazeSceneController detects maze run active
   - Shows minimap, hides cards/enemies
   - Player display remains visible

3. **Player clicks combat node**:
   - MazeRunManager stores maze context
   - Loads **CombatScene** (for combat)

4. **Combat ends**:
   - If victory: Returns to **MazeScene** (minimap visible)
   - If defeat: Returns to MainGameUI (run failed)

### Key Differences from CombatScene

| Feature | CombatScene | MazeScene |
|---------|-------------|-----------|
| Cards | Visible | Hidden |
| Minimap | Hidden | Visible |
| Enemies | Visible (during combat) | Hidden |
| Player Display | Visible | Visible |
| Top Bar | "WAVE: X/Y" | "FLOOR: X/Y" |

---

## Testing

1. **Test Entry Point**:
   - Press Play
   - Click EncounterButton or button with MazeRunEntry
   - Should load MazeScene

2. **Test Minimap Display**:
   - MazeScene should show minimap instead of cards
   - Player display should be visible
   - Floor number should show in top bar

3. **Test Node Selection**:
   - Click nodes to move
   - Adjacent nodes should be selectable (highlighted)
   - Visited nodes should be grayed out

4. **Test Combat Flow**:
   - Click combat node
   - Should load CombatScene
   - After victory, should return to MazeScene

---

## Troubleshooting

**"MazeScene loads but minimap doesn't show"**
- Check MazeSceneController has correct references
- Ensure MinimapContainer is active
- Verify MazeRunManager has active run

**"Cards still showing in MazeScene"**
- Check card hand container is inactive
- Verify MazeSceneController is switching to maze mode
- Check that MazeSceneController references are assigned

**"Scene loads but no run started"**
- Check MazeRunManager.StartRun() was called
- Verify MazeRunManager.Instance.HasActiveRun() returns true
- Check logs for errors

**"EncounterButton loads wrong scene"**
- Verify EncounterDataAsset has `sceneName = "MazeScene"`
- Check EncounterManager loaded the encounter correctly
- Ensure encounter is unlocked

**"Combat doesn't return to MazeScene"**
- Check MazeRunManager.OnCombatVictory() is called
- Verify it loads mazeSceneName correctly
- Check CombatSceneManager returns to correct scene

---

## File Structure

```
Assets/
├── Scenes/
│   ├── CombatScene.unity        ← Untouched
│   └── MazeScene.unity          ← New (copy of CombatScene + maze components)
├── Scripts/
│   └── MazeSystem/
│       ├── MazeMinimapUI.cs
│       ├── MazeSceneController.cs
│       ├── MazeRunManager.cs
│       └── MazeRunEntry.cs
└── Resources/
    ├── Encounters/
    │   └── Act2/
    │       └── MazeEntryEncounter.asset  ← EncounterDataAsset with sceneName="MazeScene"
    └── Maze/
        └── DefaultMazeConfig.asset
```

---

## Summary

✅ **MazeScene** = Copy of CombatScene with maze components
✅ **CombatScene** = Untouched, still works normally
✅ **EncounterData.sceneName** = Routes to MazeScene
✅ **Minimap** = Replaces cards in MazeScene
✅ **Combat** = Still uses CombatScene
✅ **Flow** = MazeScene ↔ CombatScene ↔ MainGameUI

The system keeps both scenes separate while maintaining the same visual layout!

