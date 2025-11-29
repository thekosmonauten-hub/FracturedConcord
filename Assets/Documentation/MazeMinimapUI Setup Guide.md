# MazeMinimapUI Setup Guide

## Overview

This guide walks you through setting up the `MazeMinimapUI` component in MazeScene. The minimap displays where cards normally appear (bottom-center) and shows the floor layout as a grid of clickable nodes.

---

## Step-by-Step Setup

### Step 1: Locate Card Hand Container in MazeScene

1. **Open MazeScene** in Unity
2. **Find the Card Hand Container** (where cards are displayed during combat):
   - Look for GameObject named `CardHandSection`, `CardHandContainer`, `CardHand`, or similar
   - Or find the parent container that holds the card UI elements
   - Typically located at bottom-center of the screen

**Example Hierarchy:**
```
Canvas
├── CombatUI
│   ├── CardSection
│   │   ├── CardHandSection        ← This is what we need
│   │   │   ├── CardHand (ScrollView)
│   │   │   └── [Card elements...]
│   │   ├── DiscardPile
│   │   └── DrawPile
```

---

### Step 2: Create Minimap Container

1. **Select the Card Hand Container** (from Step 1)
2. **Create Empty Child GameObject**:
   - Right-click Card Hand Container → **Create Empty**
   - Name it `MinimapContainer`

3. **Configure RectTransform**:
   - Select `MinimapContainer`
   - In RectTransform:
     - **Anchor Presets**: Stretch both (Alt+Shift+Click the preset)
     - **Left/Right/Top/Bottom**: All set to 0
     - This makes it fill the entire card hand area

**Result Hierarchy:**
```
CardHandSection (or similar)
├── [Card UI elements...] (hidden during maze mode)
└── MinimapContainer          ← NEW: Our minimap container
```

---

### Step 3: Create Minimap Grid Layout

1. **Select MinimapContainer**
2. **Add GridLayoutGroup Component**:
   - Add Component → **UI** → **Layout** → **Grid Layout Group**

3. **Configure GridLayoutGroup**:
   - **Cell Size**: X = 80, Y = 80 (matches nodeSize in MazeMinimapUI)
   - **Spacing**: X = 10, Y = 10 (matches nodeSpacing)
   - **Start Corner**: Upper Left
   - **Start Axis**: Horizontal
   - **Child Alignment**: Upper Left
   - **Constraint**: Fixed Column Count
   - **Constraint Count**: 4 (matches gridSize from MazeConfig, e.g., 4x4 grid)

4. **Create Grid GameObject** (optional, for better organization):
   - Right-click MinimapContainer → **Create Empty**
   - Name it `MinimapGrid`
   - Move GridLayoutGroup to this object if you prefer separation
   - Configure RectTransform to stretch (fill parent)

**Result Hierarchy:**
```
MinimapContainer (RectTransform, stretches to fill parent)
└── MinimapGrid (RectTransform, GridLayoutGroup)
    └── [Node buttons will spawn here at runtime]
```

---

### Step 4: Add MazeMinimapUI Component

1. **Select MinimapContainer** (or MinimapGrid if you created it)
2. **Add Component** → `MazeMinimapUI`

3. **Assign Basic References**:
   - **Minimap Container**: Drag `MinimapContainer` GameObject here
   - **Minimap Grid**: Drag the GameObject with GridLayoutGroup here

---

### Step 5: Create Node Button Prefab (Optional but Recommended)

If you don't create a prefab, the system will generate basic buttons at runtime. Creating a prefab gives you better control over styling.

#### Step 5a: Create Prefab Structure

1. **Create Empty GameObject** in scene:
   - Right-click Hierarchy → **Create Empty**
   - Name it `MazeNodeButton`

2. **Add Components**:
   - Add Component → **UI** → **Image** (background)
   - Add Component → **UI** → **Button**

3. **Create Icon Child**:
   - Right-click `MazeNodeButton` → **UI** → **Image**
   - Name it `Icon`
   - Configure RectTransform:
     - Anchor Presets: Stretch both (Alt+Shift+Click)
     - All offsets: 0
     - This makes icon fill the button

**Result Hierarchy:**
```
MazeNodeButton (RectTransform, Image, Button)
└── Icon (RectTransform, Image)     ← Node type icon goes here
```

#### Step 5b: Configure Button Styles

1. **Select MazeNodeButton**
2. **Configure Image Component**:
   - **Color**: White (default, will be changed by code based on state)
   - **Image Type**: Simple
   - **Preserve Aspect**: Unchecked

3. **Configure Button Component**:
   - **Interactable**: True
   - **Transition**: Color Tint (default)
   - **Colors**:
     - **Normal Color**: White
     - **Highlighted Color**: Light gray (0.9, 0.9, 0.9, 1)
     - **Pressed Color**: Darker gray (0.8, 0.8, 0.8, 1)
     - **Selected Color**: White
     - **Disabled Color**: Gray (0.5, 0.5, 0.5, 0.5)

4. **Configure Icon Image**:
   - Select `Icon` child
   - **Image Type**: Simple
   - **Preserve Aspect**: Checked (to maintain icon proportions)
   - **Color**: White (default, will be tinted by code)

#### Step 5c: Create Prefab Asset

1. **Drag MazeNodeButton** from Hierarchy to `Assets/Prefabs/` folder
2. **Name it** `MazeNodeButton.prefab`
3. **Delete** the scene instance (keep the prefab)

4. **Assign to MazeMinimapUI**:
   - Select `MinimapContainer` (with MazeMinimapUI component)
   - In Inspector, find **Node Button Prefab** field
   - Drag `MazeNodeButton.prefab` here

---

### Step 6: Set Up Run Info Display

The run info shows floor number, seed, and active modifiers. Set this up near the top of the screen.

#### Step 6a: Create Run Info Container

1. **Find or Create Top Bar** in MazeScene:
   - Look for existing top bar UI (e.g., where "WAVE: X/Y" is shown)
   - Or create new container:
     - Right-click Canvas → **UI** → **Panel**
     - Name it `RunInfoPanel`

2. **Position RunInfoPanel**:
   - **Anchor Presets**: Top-Center
   - **Position**: Adjust as needed (e.g., Y = -30 to be below top bar)
   - **Width**: ~400 pixels
   - **Height**: ~60 pixels

#### Step 6b: Create Floor Text

1. **Inside RunInfoPanel**, create TextMeshPro:
   - Right-click RunInfoPanel → **UI** → **Text - TextMeshPro**
   - Name it `FloorText`

2. **Configure FloorText**:
   - **Text**: "Floor 1/8" (placeholder, will be updated by code)
   - **Font Size**: 24
   - **Alignment**: Center
   - **Color**: White or light color
   - **RectTransform**: 
     - Anchor: Top-Center
     - Position: Y = -10 (or as desired)

3. **Assign to MazeMinimapUI**:
   - Select `MinimapContainer` (with MazeMinimapUI component)
   - Drag `FloorText` to **Floor Text** field

#### Step 6c: Create Seed Text (Optional)

1. **Inside RunInfoPanel**, create TextMeshPro:
   - Right-click RunInfoPanel → **UI** → **Text - TextMeshPro**
   - Name it `SeedText`

2. **Configure SeedText**:
   - **Text**: "Seed: 12345" (placeholder)
   - **Font Size**: 16
   - **Alignment**: Center
   - **Color**: Gray (0.7, 0.7, 0.7, 1)
   - **Position**: Below FloorText (e.g., Y = -35)

3. **Assign to MazeMinimapUI**:
   - Drag `SeedText` to **Seed Text** field

#### Step 6d: Create Modifiers Container

1. **Inside RunInfoPanel** (or nearby), create container:
   - Right-click RunInfoPanel → **Create Empty**
   - Name it `ModifiersContainer`
   - Add **Horizontal Layout Group** component (to show modifiers side-by-side)

2. **Configure ModifiersContainer**:
   - **RectTransform**: Position below seed text or in corner
   - **Horizontal Layout Group**:
     - **Spacing**: 10
     - **Child Alignment**: Middle Left
     - **Child Force Expand**: Width = false, Height = false

3. **Assign to MazeMinimapUI**:
   - Drag `ModifiersContainer` to **Modifiers Container** field

---

### Step 7: Create Modifier Display Prefab (Optional)

This shows shrine buffs and curses as small icons/text.

#### Step 7a: Create Modifier Prefab Structure

1. **Create GameObject**:
   - Right-click Hierarchy → **UI** → **Image**
   - Name it `ModifierDisplay`

2. **Add Icon**:
   - Right-click ModifierDisplay → **UI** → **Image**
   - Name it `Icon`
   - **RectTransform**: 
     - Width: 32, Height: 32
     - Position: Left side

3. **Add Text**:
   - Right-click ModifierDisplay → **UI** → **Text - TextMeshPro**
   - Name it `Text`
   - **RectTransform**: 
     - Anchor: Stretch (fills remaining space)
     - Left: 40 (offset from icon)
   - **Text**: "Shrine of Cohesion" (placeholder)
   - **Font Size**: 14

**Result Hierarchy:**
```
ModifierDisplay (RectTransform, Image - background)
├── Icon (RectTransform, Image)    ← Modifier icon
└── Text (RectTransform, TextMeshPro)  ← Modifier name
```

#### Step 7b: Configure and Save Prefab

1. **Configure ModifierDisplay RectTransform**:
   - **Width**: ~200 pixels
   - **Height**: 32 pixels

2. **Create Prefab**:
   - Drag to `Assets/Prefabs/`
   - Name it `ModifierDisplay.prefab`
   - Delete scene instance

3. **Assign to MazeMinimapUI**:
   - Drag prefab to **Modifier Display Prefab** field

---

### Step 8: Configure Node Visual Settings

In MazeMinimapUI component Inspector:

1. **Node Visual Settings**:
   - **Node Size**: (80, 80) - Should match GridLayoutGroup Cell Size
   - **Node Spacing**: (10, 10) - Should match GridLayoutGroup Spacing

2. **Node State Colors** (optional, can use defaults):
   - **Unrevealed Color**: Dark gray, semi-transparent (0.2, 0.2, 0.2, 0.5)
   - **Revealed Color**: White (1, 1, 1, 1)
   - **Visited Color**: Gray (0.5, 0.5, 0.5, 0.8)
   - **Selectable Color**: Yellow (1, 1, 0, 1) - Highlights selectable nodes
   - **Current Position Color**: Green (0, 1, 0, 1) - Shows player's current node

---

### Step 9: Configure Grid Tiles (Tier-Based Backgrounds)

The maze system supports different background tiles for each tier of floors. This gives visual variety as players progress through the maze.

1. **Open MazeConfig Asset**:
   - Find your `DefaultMazeConfig` asset (or the config you're using)
   - Select it in Project window

2. **Configure Grid Visual Tiles**:
   - In Inspector, find **Grid Visual Tiles** section
   - **Floors Per Tier**: Set to 2 (default) - This means:
     - Tier 1: Floors 1-2
     - Tier 2: Floors 3-4
     - Tier 3: Floors 5-6
     - Tier 4: Floors 7-8
   - Adjust if you want different tier distribution

3. **Assign Grid Tile Sprites**:
   - **Tier 1 Grid Tile**: Sprite for floors 1-2 (e.g., "maze_tile_tier1")
   - **Tier 2 Grid Tile**: Sprite for floors 3-4 (e.g., "maze_tile_tier2")
   - **Tier 3 Grid Tile**: Sprite for floors 5-6 (e.g., "maze_tile_tier3")
   - **Tier 4 Grid Tile**: Sprite for floors 7-8 (e.g., "maze_tile_tier4")

4. **Grid Tile Sprite Requirements**:
   - Should be sized to match node size (e.g., 80x80 pixels)
   - Can use 9-slice sprites (Image Type: Sliced) for better scaling
   - These tiles appear as backgrounds for all grid cells (including empty spaces)
   - Node icons appear on top of these tiles

**Example:**
- Tier 1: Light stone tiles (early floors)
- Tier 2: Darker stone tiles (mid floors)
- Tier 3: Cracked/weathered tiles (late floors)
- Tier 4: Glowing/entropic tiles (final floors)

**Note**: Grid tiles are automatically applied based on the current floor's tier. No additional setup needed in MazeMinimapUI - it reads from MazeConfig automatically.

---

### Step 10: Assign Node Icons (Optional but Recommended)

Create or assign sprite icons for each node type:

1. **Create/Import Node Icons**:
   - Create sprite assets for each node type:
     - `start_node_icon` - Start node (entry point)
     - `combat_node_icon` - Combat encounters (sword icon)
     - `chest_node_icon` - Treasure (chest icon)
     - `shrine_node_icon` - Shrines (glowing icon)
     - `trap_node_icon` - Traps (warning icon)
     - `forge_node_icon` - Forge (anvil/hammer icon)
     - `boss_node_icon` - Boss encounters (skull/crown icon)
     - `stairs_node_icon` - Stairs (stairs icon)
     - `special_node_icon` - Special nodes (star icon)
     - `hidden_node_icon` - Unrevealed nodes (question mark or fog)

2. **Assign Icons in MazeMinimapUI**:
   - Select `MinimapContainer` (with MazeMinimapUI component)
   - Drag each sprite to the corresponding field:
     - **Start Node Icon** → start_node_icon sprite
     - **Combat Node Icon** → combat_node_icon sprite
     - **Chest Node Icon** → chest_node_icon sprite
     - etc.

**Note**: If icons are not assigned, the system will use colored squares (default button appearance).

---

## Complete Setup Example

Here's what your final MazeScene hierarchy should look like:

```
MazeScene
└── Canvas
    ├── CombatUI (or similar main container)
    │   ├── TopBar
    │   │   └── RunInfoPanel                     ← Run info display
    │   │       ├── FloorText (TextMeshPro)      ← Floor number
    │   │       ├── SeedText (TextMeshPro)       ← Seed display
    │   │       └── ModifiersContainer           ← Active modifiers
    │   │           └── [ModifierDisplay instances spawn here]
    │   ├── PlayerArea
    │   │   └── PlayerDisplay (always visible)
    │   ├── EnemyArea (hidden during maze mode)
    │   │   └── EnemyPanels...
    │   └── CardHandSection (card container)
    │       ├── [Card UI elements...] (hidden during maze mode)
    │       └── MinimapContainer (RectTransform) ← Minimap here
    │           ├── MazeMinimapUI (Component)
    │           └── MinimapGrid (GridLayoutGroup)
    │               └── [Node buttons spawn here at runtime]
    └── MazeRunManager (GameObject, DontDestroyOnLoad)
```

---

## Field Assignment Summary

### Required Fields in MazeMinimapUI:

| Field | What to Assign | Example |
|-------|----------------|---------|
| **Minimap Container** | RectTransform of container | `MinimapContainer` GameObject |
| **Minimap Grid** | GridLayoutGroup component | `MinimapGrid` GameObject (or same as container) |
| **Floor Text** | TextMeshPro for floor display | `FloorText` TextMeshPro |
| **Seed Text** | TextMeshPro for seed (optional) | `SeedText` TextMeshPro (can be null) |
| **Modifiers Container** | Transform for modifier UI | `ModifiersContainer` GameObject (can be null) |

### Optional Fields:

| Field | What to Assign | Example |
|-------|----------------|---------|
| **Node Button Prefab** | Prefab for node buttons | `MazeNodeButton.prefab` (auto-created if null) |
| **Modifier Display Prefab** | Prefab for modifiers | `ModifierDisplay.prefab` (auto-created if null) |
| **Node Icons** | Sprite assets (9 types) | `start_node_icon`, `combat_node_icon`, etc. |

### Grid Tile Configuration (in MazeConfig):

| Field | What to Assign | Example |
|-------|----------------|---------|
| **Tier 1 Grid Tile** | Sprite for floors 1-2 | `maze_tile_tier1.sprite` |
| **Tier 2 Grid Tile** | Sprite for floors 3-4 | `maze_tile_tier2.sprite` |
| **Tier 3 Grid Tile** | Sprite for floors 5-6 | `maze_tile_tier3.sprite` |
| **Tier 4 Grid Tile** | Sprite for floors 7-8 | `maze_tile_tier4.sprite` |
| **Floors Per Tier** | Number (default: 2) | 2 (means 2 floors per tier) |

---

## Testing Checklist

After setup, verify:

- [ ] MinimapContainer is positioned where cards normally are
- [ ] GridLayoutGroup has correct Cell Size (80x80) and Spacing (10x10)
- [ ] Constraint Count matches MazeConfig gridSize (e.g., 4 for 4x4 grid)
- [ ] FloorText and SeedText are visible in RunInfoPanel
- [ ] ModifiersContainer is positioned (even if empty)
- [ ] Node Button Prefab is assigned (or will be auto-created)
- [ ] Node icons are assigned (optional, but recommended)

---

## Troubleshooting

**"Minimap doesn't appear"**
- Check MinimapContainer is active in hierarchy
- Verify MinimapContainer RectTransform stretches to fill card area
- Check GridLayoutGroup is assigned correctly

**"Nodes appear too large/small"**
- Verify Node Size in MazeMinimapUI matches GridLayoutGroup Cell Size
- Check grid Constraint Count matches MazeConfig gridSize

**"Can't click nodes"**
- Verify Button component exists on node prefab
- Check node buttons are being created (look in hierarchy at runtime)
- Ensure Button.interactable is true

**"Floor/Seed text doesn't update"**
- Check TextMeshPro components are assigned
- Verify MazeRunManager has active run
- Check console for errors

---

## Quick Reference: Complete Field Assignment

```csharp
MazeMinimapUI Component:

[Header("UI Container")]
minimapContainer = MinimapContainer (RectTransform)

[Header("Minimap Grid")]
minimapGrid = MinimapGrid (GridLayoutGroup) // or same as container
nodeButtonPrefab = MazeNodeButton.prefab (optional)

[Header("Run Info Display")]
floorText = FloorText (TextMeshPro)
seedText = SeedText (TextMeshPro) // optional
modifiersContainer = ModifiersContainer (Transform) // optional
modifierDisplayPrefab = ModifierDisplay.prefab // optional

[Header("Node Visual Settings")]
nodeSize = (80, 80) // must match GridLayoutGroup Cell Size
nodeSpacing = (10, 10) // must match GridLayoutGroup Spacing

[Header("Node Sprites/Icons")]
startNodeIcon = start_node_icon.sprite // optional
combatNodeIcon = combat_node_icon.sprite // optional
// ... (assign all 9 icons if desired)

[Header("Node State Colors")]
// Use defaults or customize colors for each state
```

---

This completes the MazeMinimapUI setup! The minimap should now display when you start a maze run.

