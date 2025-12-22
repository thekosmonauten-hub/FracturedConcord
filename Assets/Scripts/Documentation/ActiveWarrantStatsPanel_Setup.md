# Active Warrant Stats Panel Setup Guide

This guide explains how to set up the Active Warrant Stats panel that displays aggregated warrant modifiers grouped by category.

## Overview

The Active Warrant Stats panel shows:
- All active warrant modifiers from equipped warrants
- Stats grouped by category (Damage, Defense, Attributes, etc.)
- The statKey for each modifier (for verification)
- The aggregated total value (e.g., if 2 warrants give +8% each, shows +16%)

## Components Created

1. **WarrantStatsCollector.cs** - Collects and aggregates warrant modifiers by category
2. **ActiveWarrantStatsPanel.cs** - UI panel script that displays the stats
3. **WarrantStatsButton.cs** - Button controller to open/close the panel

## Setup Instructions

### Step 1: Create the UI Panel

1. In the WarrantTree scene, create a new GameObject named "ActiveWarrantStatsPanel"
2. Add a Canvas component (or make it a child of an existing Canvas)
3. Add the following UI structure:
   ```
   ActiveWarrantStatsPanel (GameObject)
   ├── Background (Image - dark panel background)
   ├── TitleLabel (TextMeshProUGUI - "Active Warrant Stats")
   ├── CloseButton (Button)
   ├── ScrollView (ScrollRect)
   │   └── Viewport
   │       └── Content (VerticalLayoutGroup)
   └── [CategoryHeaderPrefab] (optional - for category headers)
   └── [StatRowPrefab] (optional - for stat rows)
   ```

### Step 2: Configure the Panel Script

1. Add the `ActiveWarrantStatsPanel` component to the panel GameObject
2. Assign references:
   - **Panel Root**: The main panel GameObject
   - **Close Button**: The close button
   - **Title Label**: The title text (optional)
   - **Scroll View**: The ScrollRect component
   - **Content Container**: The Content transform (child of ScrollView/Viewport)
   - **Category Header Prefab**: (Optional) Prefab for category headers
   - **Stat Row Prefab**: (Optional) Prefab for stat rows
   - **Board State**: WarrantBoardStateController (will auto-find if not assigned)
   - **Locker Grid**: WarrantLockerGrid (will auto-find if not assigned)
   - **Graph Builder**: WarrantBoardGraphBuilder (will auto-find if not assigned - used to get RuntimeGraph)

### Step 3: Create Prefabs (Optional but Recommended)

#### Category Header Prefab
1. Create a GameObject with TextMeshProUGUI
2. Style it as a header (larger font, bold, different color)
3. Save as prefab: "CategoryHeaderPrefab"

#### Stat Row Prefab
1. Create a GameObject with TextMeshProUGUI
2. Style it as a stat row (normal font, white text)
3. Save as prefab: "StatRowPrefab"

**Note**: If prefabs are not assigned, the script will create simple text objects as fallback.

### Step 4: Add the Button

1. In the WarrantTree scene, find or create a button (e.g., in the top bar)
2. Set the button text to "Active Warrant Stats" or "Warrant Stats"
3. Add the `WarrantStatsButton` component to the button
4. Assign the **Stats Panel** reference (or it will auto-find)

### Step 5: Test

1. Enter Play mode
2. Open the WarrantTree scene
3. Click the "Active Warrant Stats" button
4. Verify that:
   - Panel opens and shows warrant stats
   - Stats are grouped by category
   - Each stat shows: "Display Name (statKey): +X%" or "+X"
   - Values are correctly aggregated

## Categories

Stats are automatically categorized as:
- **Damage**: All damage-related modifiers
- **Defense**: Armour, Evasion, Energy Shield, Health, Mana
- **Attributes**: Strength, Dexterity, Intelligence
- **Ailments**: Ignite, Shock, Chill, Freeze, Bleed, Poison, etc.
- **Utility**: Speed, Cast Speed, Regeneration, etc.
- **Resistances**: All resistance modifiers
- **Other**: Any unclassified modifiers

## Troubleshooting

### Panel doesn't open
- Check that `ActiveWarrantStatsPanel` component is assigned
- Verify button has `WarrantStatsButton` component
- Check console for error messages

### No stats displayed
- Verify warrants are equipped in the tree
- Check that WarrantBoardStateController, WarrantLockerGrid, and WarrantBoardGraphBuilder are in the scene
- Check console for warnings about missing references

### Stats show incorrectly
- Verify that warrant modifiers have proper `modifierId` values
- Check that statKeys match the expected format (e.g., "increasedElementalDamage")

## Example Display

```
--- Damage ---
Increased Elemental Damage (increasedElementalDamage): +16%
Increased Fire Damage (increasedFireDamage): +8%
--- Defense ---
Increased Armour (armourIncreased): +12%
Increased Max Health (maxHealthIncreased): +10%
```

