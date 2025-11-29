# Maze Hub System - Setup Guide

**Created:** Based on user request  
**Purpose:** Centralized UI/Hub for maze-related activities (Start Run, Vendor, Forge, etc.)  
**Location:** `Assets/Scripts/MazeSystem/`

---

## Overview

The Maze Hub is a centralized location for all maze-related activities, replacing scattered encounter nodes with a cohesive hub interface. This system allows players to:

1. **Start Maze Runs** with difficulty selection
2. **Spend Maze Currency** on various services
3. **Access Vendor/Shop** for maze-specific items
4. **Use Forge** for crafting/upgrading
5. **View Statistics** and progress

---

## Architecture

### Core Components

```
MazeHubController (Main Controller)
    ├── MazeDifficultyConfig (ScriptableObject) - Difficulty settings
    ├── MazeCurrencyManager (Singleton) - Currency tracking
    │
    ├── Main Hub Panel - Navigation center
    ├── Start Run Panel - Difficulty selection
    ├── Vendor Panel - Shop interface (TODO)
    ├── Forge Panel - Crafting interface (TODO)
    └── Statistics Panel - Progress tracking (TODO)
```

### Files Created

1. **MazeDifficultyConfig.cs** - ScriptableObject for difficulty configurations
2. **MazeHubController.cs** - Main controller for hub navigation and run starting
3. **MazeDifficultyButton.cs** - UI component for difficulty selection buttons
4. **MazeCurrencyManager.cs** - Manager for maze-specific currency

---

## Setup Instructions

### Step 1: Create Difficulty Configurations

1. Right-click in Project window → `Create > Dexiled > Maze Difficulty Config`
2. Create difficulty levels (e.g., "Standard", "Veteran", "Elite", "Master")
3. Configure each difficulty:
   - **Difficulty Name**: Display name (e.g., "Standard")
   - **Description**: Flavor text describing the difficulty
   - **Maze Config**: Reference to `MazeConfig` asset to use
   - **Enemy Level Multiplier**: 1.0 = normal, 1.5 = +50% levels
   - **Experience/Loot/Currency Multipliers**: Reward scaling
   - **Required Level**: Minimum player level
   - **Entry Cost**: Currency cost to start (0 = free)
   - **Entry Currency Type**: Which currency to use
   - **Difficulty Color**: UI theme color
   - **Is Recommended**: Show badge for recommended difficulty

### Step 2: Create Maze Hub Scene

1. Create new scene: `File > New Scene`
2. Save as `Assets/Scenes/MazeHub.unity`
3. Add Canvas (Screen Space - Overlay)
4. Add `MazeHubController` component to a GameObject

### Step 3: Setup Hub UI Layout

#### Main Hub Panel
- **Panel Container** (GameObject) - **ALWAYS ACTIVE** (never deactivated)
  - **Start Run Button** (Button) → Calls `startRunButton.onClick`
  - **Vendor Button** (Button) → Calls `vendorButton.onClick`
  - **Forge Button** (Button) → Calls `forgeButton.onClick`
  - **Statistics Button** (Button) → Calls `statisticsButton.onClick`
  - **Exit Button** (Button) → Calls `exitButton.onClick`
  - **Currency Display** (TextMeshProUGUI) → Shows current maze currency

#### Overlay Panels (Start Run, Vendor, Forge, Statistics)
These panels open as **overlays on top of the Main Hub Panel**. They should be:
- **Centered on screen** (use RectTransform anchors/anchoredPosition)
- **Sibling order**: Overlay panels should be children of the Canvas (same level as Main Hub Panel)
- **Z-order**: Overlay panels appear above Main Hub Panel when active

#### Start Run Panel (Overlay)
- **Panel Container** (GameObject) - Starts inactive, opens as overlay
  - **Title** (TextMeshProUGUI) - "Select Difficulty"
  - **Difficulty Container** (Transform) - Vertical/Horizontal Layout Group
    - Spawns difficulty buttons dynamically
  - **Back Button** (Button) → Calls `backToHubButton.onClick` (closes overlay)

#### Deadzone Backdrop
- **Automatically created** by `MazeHubController` if not assigned
- **Full-screen semi-transparent backdrop** that appears behind overlay panels
- **Clicking outside overlay panels** (on the deadzone) closes the overlay
- **Color**: Configurable via `deadzoneColor` (default: semi-transparent black)
- **Manual setup** (optional): Create a GameObject with Image + Button components, assign to `deadzoneBackdrop` field

#### Difficulty Button Prefab
- **Button Root** (GameObject with Button component)
  - **Icon** (Image) - Difficulty icon
  - **Name Text** (TextMeshProUGUI) - Difficulty name
  - **Description Text** (TextMeshProUGUI) - Description
  - **Requirements Text** (TextMeshProUGUI) - Level requirement
  - **Cost Text** (TextMeshProUGUI) - Entry cost
  - **First Clear Bonus Text** (TextMeshProUGUI) - Shows first clear bonus info
  - **Recommended Badge** (GameObject) - Shows if recommended
  - **First Clear Badge** (GameObject) - Shows when first clear bonus is available (not cleared yet)
    - Typically positioned in corner (e.g., top-right)
    - Should have an Image component with your "First Clear Available" sprite
    - Example: Gold star, "NEW" badge, or "First Clear" icon
  - **Cleared Badge** (GameObject, optional) - Shows when difficulty has been completed
    - Typically positioned in corner (e.g., top-right, opposite of First Clear Badge)
    - Should have an Image component with your "Cleared" sprite
    - Example: Checkmark, "COMPLETED" badge, or completion icon
  - Add `MazeDifficultyButton` component to root
  - **In Inspector**: Assign all UI references to the `MazeDifficultyButton` component fields

### Step 4: Configure MazeHubController

1. Assign scene references:
   - **Maze Scene Name**: `"MazeScene"`
   - **Main Hub Panel**: Main navigation panel (always visible)
   - **Start Run Panel**: Difficulty selection overlay panel
   - **Vendor/Forge/Statistics Panels**: Overlay panels (can be empty for now)
   - **Deadzone Backdrop**: (Optional) Manually created backdrop, or leave null to auto-create

2. Assign button references:
   - **Start Run Button**, **Vendor Button**, **Forge Button**, etc.
   - **Back To Hub Button**: Closes current overlay (can be in each overlay panel)

3. Configure deadzone (optional):
   - **Deadzone Color**: Color of the backdrop (default: semi-transparent black)
   - If `deadzoneBackdrop` is null, it will be created automatically

4. Assign difficulty selection:
   - **Difficulty Container**: Transform where buttons spawn (inside Start Run Panel)
   - **Difficulty Button Prefab**: Prefab for difficulty buttons
   - **Available Difficulties**: Drag difficulty configs here

5. Assign currency display:
   - **Maze Currency Text**: TextMeshProUGUI for currency display (in Main Hub Panel)
   - **Maze Currency Type**: Which currency to display (default: `OrbOfGeneration`)

**Important UI Hierarchy:**
- Main Hub Panel: Always active, base layer
- Deadzone Backdrop: Appears when overlay is open, blocks interaction with Main Hub
- Overlay Panels: Appear on top when opened, can be closed by clicking deadzone or back button

### Step 5: Integration with MazeRunManager

The `MazeHubController` automatically:
- Sets difficulty multipliers in `PlayerPrefs` before starting run
- Assigns `MazeConfig` to `MazeRunManager.mazeConfig`
- Checks entry requirements and costs
- Deducts entry cost before starting

**Modifiers are stored in PlayerPrefs:**
- `MazeDifficulty_EnemyMultiplier`
- `MazeDifficulty_ExperienceMultiplier`
- `MazeDifficulty_LootMultiplier`
- `MazeDifficulty_CurrencyMultiplier`
- `MazeDifficulty_Name`

**To use in combat/loot generation:**
```csharp
float enemyMultiplier = PlayerPrefs.GetFloat("MazeDifficulty_EnemyMultiplier", 1.0f);
float experienceMultiplier = PlayerPrefs.GetFloat("MazeDifficulty_ExperienceMultiplier", 1.0f);
// Apply multipliers when generating enemies/loot
```

---

## Lore Integration

The Maze Hub represents a **centralized staging area** for maze expeditions. Instead of random encounter nodes, players gather at a hub where they can:

- **Plan expeditions** by selecting difficulty
- **Prepare** by visiting vendor for supplies
- **Upgrade equipment** at the forge before entering
- **Track progress** through statistics

This fits the lore better as a structured, strategic approach to maze exploration rather than random encounters.

---

## Currency System

The `MazeCurrencyManager` tracks maze-specific currency separate from main game currency. Currently uses `PlayerPrefs` for storage, but can be integrated with:

- `CharacterManager` for character-specific storage
- `LootManager` for currency rewards
- Save system for persistence

**Currency flow:**
1. Earn currency during maze runs (with difficulty multiplier)
2. Spend at hub for entry costs, vendor purchases, forge upgrades
3. Currency persists across sessions via `PlayerPrefs`

---

## Vendor Panel Setup

The Vendor Panel has been implemented! See `MazeVendorUI.cs` for details.

### Prerequisites:
**ItemTooltipManager Setup:**
1. Add `ItemTooltipManager` component to a GameObject in your scene (can be a child of Canvas)
2. Assign tooltip prefabs in the Inspector:
   - **Weapon Tooltip Prefab**: `Assets/Prefab/EquipmentScreen/WeaponTooltips.prefab`
   - **Equipment Tooltip Prefab**: `Assets/Prefab/EquipmentScreen/EquipmentTooltips.prefab`
3. Assign **Target Canvas** (the canvas where tooltips should appear)
4. Assign **Tooltip Container** (RectTransform where tooltips will be instantiated - can be same as Canvas)

The vendor slots will automatically use `ItemTooltipManager.Instance` to display tooltips on hover.

### Quick Setup:
1. **Add `MazeVendorUI` component** to your vendor panel GameObject
2. **Assign UI references:**
   - Item Grid Container (Transform with Grid Layout Group)
   - Vendor Item Slot Prefab (prefab for each item)
   - Currency Display Text
   - Refresh Button
   - Purchase Button (outside the item grid)
   - Selected Item Text (optional - displays selected item info)
3. **Configure settings:**
   - Vendor Item Count: 12 (default)
   - Level Range: 5 (±5 levels from player)
   - Rare Chance: 0.3 (30% chance for Rare items)
   - Base Cost Multiplier: 2 (price = level × 2)
   - Refresh Cost: 10 Mandate Fragments

### Vendor Item Slot Prefab (Simple):
Create a simple prefab with:
- **Background** (Image) - Optional: can be colored by rarity
- **Item Icon** (Image) - Shows item sprite
- **Item Price Text** (TextMeshProUGUI) - Shows price (e.g., "68 MandateFragment")
- **Sold Out Overlay** (GameObject) - Shows when item is purchased
- **Selection Indicator** (GameObject, optional) - Visual highlight when selected
- **MazeVendorItemSlot** component

**Tooltip System:**
- The vendor slot uses the centralized `ItemTooltipManager` singleton to display tooltips.
- **IMPORTANT**: You must add `ItemTooltipManager` component to a GameObject in your scene and assign the tooltip prefabs:
  - `WeaponTooltips.prefab`
  - `EquipmentTooltips.prefab`
- The tooltip manager handles all positioning, prefab selection, and tooltip lifecycle automatically.

**Note:** Item details (name, level, rarity, affixes, stats) are shown in the tooltip on hover, not always visible on the slot.

### Vendor Panel Purchase Button:
Add a **Purchase Button** to the Vendor Panel (outside the grid):
- Button is disabled until an item is selected
- Shows selected item info in `selectedItemText` (optional)
- Handles purchase when clicked

The vendor automatically:
- Generates items around player level (±5 levels)
- Creates Magic or Rare items with rolled affixes
- Calculates prices based on level and rarity
- Handles purchases with maze currency
- Removes purchased items from the grid

## Next Steps / TODO

1. **Create Maze Hub Scene** with UI layout
2. ✅ **Implement Vendor Panel** - COMPLETED
3. **Implement Forge Panel** for pre-run upgrades
4. **Implement Statistics Panel** for progress tracking
5. **Integrate currency rewards** into maze combat completion
6. **Add difficulty multipliers** to enemy spawning and loot generation
7. **Create difficulty config assets** for each difficulty level
8. **Add visual polish** (animations, transitions, effects)

---

## Usage Example

```csharp
// In your main menu or world map
public void OpenMazeHub()
{
    SceneManager.LoadScene("MazeHub");
}

// In MazeHubController (already implemented)
// Player clicks "Start Run" → Shows difficulty selection
// Player selects difficulty → Checks requirements/cost → Starts maze run
// Maze run uses difficulty multipliers for enemies/loot
```

---

## Troubleshooting

**Q: Difficulty buttons not appearing?**  
A: Ensure `difficultyContainer` and `difficultyButtonPrefab` are assigned in `MazeHubController`.

**Q: Entry cost not deducted?**  
A: Check that `MazeCurrencyManager` is initialized and currency is properly tracked.

**Q: Difficulty multipliers not applying?**  
A: Ensure combat/loot systems read from `PlayerPrefs` keys set by `MazeHubController`.

---

## Notes

- The hub system is designed to be extensible - add more panels as needed
- Currency system can be expanded to support multiple currency types
- Difficulty configurations are flexible - create as many as needed
- All panels can be initially empty/stubbed out and implemented later

