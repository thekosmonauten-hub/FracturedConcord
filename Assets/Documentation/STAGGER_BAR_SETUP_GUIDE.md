# Stagger Bar Setup Guide

This guide explains how to add stagger bars to both enemies and the player in combat.

## Overview

Stagger bars show how close an enemy or player is to being stunned. When the stagger meter fills to 100%, the target is stunned for 1 turn.

## Enemy Stagger Bar (Overlay on Health Bar)

The enemy stagger bar appears as a **transparent overlay** on top of the health bar, showing the stagger progress.

### Unity Setup Steps:

1. **Open your Enemy prefab** (or the EnemyCombatDisplay GameObject in the scene)

2. **Locate the HealthBar GameObject** (usually a child of the enemy panel)
   - This is typically a `Slider` component

3. **Create a Stagger Bar Overlay:**
   - Right-click on the **HealthBar GameObject** → **UI** → **Image**
   - Name it "StaggerBarOverlay"
   - Make sure it's a **child of the HealthBar** (same level as the Fill Area)

4. **Configure the Stagger Bar Image:**
   - Select the StaggerBarOverlay Image
   - In the **Image** component:
     - Set **Image Type** to "Filled"
     - Set **Fill Method** to "Horizontal" (match your health bar direction)
     - Set **Fill Origin** to "Left"
     - Set **Color** to a semi-transparent yellow/orange:
       - R: 255, G: 200, B: 0, A: 150 (60% opacity)
       - Or use: `#FFC800` with alpha 150
   - In the **RectTransform**:
     - Set **Anchors** to stretch-stretch (hold Alt+Shift and click the stretch anchor preset)
     - Set **Left, Right, Top, Bottom** all to 0 (so it matches the health bar size)
     - Make sure it's positioned **above** the health fill in the hierarchy (renders on top)

5. **Optional: Add Stagger Text:**
   - Right-click on the enemy panel → **UI** → **Text - TextMeshPro**
   - Name it "StaggerText"
   - Position it near the health bar
   - Set text color to yellow/orange
   - Set font size appropriately

6. **Assign in EnemyCombatDisplay:**
   - Select the **EnemyCombatDisplay** component
   - Drag the **StaggerBarOverlay Image** into the "Stagger Bar Overlay" field
   - Drag the **StaggerText** (if created) into the "Stagger Text" field

### Visual Design Tips:
- Use a **yellow/orange color** to distinguish from health (red)
- Set **alpha to ~150-200** (60-80% opacity) so health bar shows through
- Consider adding a subtle **glow effect** when stagger is high (use Outline or Shadow component)
- The bar automatically fills from 0% to 100% based on `currentStagger / staggerThreshold`
- Position the overlay so it renders **on top** of the health bar (higher in hierarchy or use Canvas sorting)

## Player Stagger Bar

The player stagger bar should be a separate vertical bar, similar to the Guard or Energy Shield bars.

### Unity Setup Steps:

1. **Open your PlayerCombatDisplay prefab** (or GameObject in scene)

2. **Create a Stagger Bar Container:**
   - Right-click on the player UI container (where Guard/Energy Shield bars are)
   - Create Empty → Name it "StaggerBarContainer"
   - Add a `RectTransform` component
   - Position it near your other resource bars

3. **Create the Bar Background (Optional but Recommended):**
   - Right-click on StaggerBarContainer → **UI** → **Image**
   - Name it "StaggerBarBackground"
   - Set Color to dark gray/black with low alpha (e.g., RGBA: 0, 0, 0, 100)
   - This creates a background frame for the bar

4. **Create the Bar Fill:**
   - Right-click on StaggerBarContainer → **UI** → **Image**
   - Name it "StaggerBarFill"
   - Make it a child of StaggerBarBackground (if you created one) or StaggerBarContainer
   - In the **Image** component:
     - Set **Image Type** to "Filled"
     - Set **Fill Method** to "Vertical" (to match other player bars)
     - Set **Fill Origin** to "Bottom"
     - Set **Color** to yellow/orange (e.g., RGBA: 255, 200, 0, 255)
   - In the **RectTransform**:
     - Set anchors to stretch-stretch
     - Set Left, Right, Top, Bottom to 0 (or small padding if you have a background)

5. **Add the VerticalHealthBar Component:**
   - Select the **StaggerBarFill** GameObject
   - Add Component → **VerticalHealthBar**
   - This will handle the fill animation and effects
   - Configure colors in the VerticalHealthBar component

6. **Add Text Display (Optional):**
   - Right-click on StaggerBarContainer → **UI** → **Text - TextMeshPro**
   - Name it "StaggerText"
   - Position it over or next to the bar
   - Set text color to yellow/orange
   - Set font size appropriately

7. **Assign in PlayerCombatDisplay:**
   - Select the **PlayerCombatDisplay** component
   - Drag the **StaggerBarFill** GameObject (with VerticalHealthBar) into the "Stagger Bar" field
   - Drag the **StaggerBarContainer** into the "Stagger Bar Game Object" field
   - Drag the **StaggerText** (if created) into the "Stagger Text" field

### Visual Design Tips:
- Match the style of your Guard/Energy Shield bars
- Use **yellow/orange** color scheme (RGB: 255, 200, 0)
- Consider showing **"STAGGERED"** text when the bar is full (100%)
- Position it near other resource bars (Guard, Energy Shield, etc.)
- Use the same width/height as your other vertical bars for consistency

## Code Integration

The code has been updated to support stagger bars. The components will automatically update when:
- Stagger is applied (via `AddStagger()`)
- Stagger decays (via `DecayStagger()`)
- Stagger threshold is reached (bar should show 100%)

### Update Methods:

- **EnemyCombatDisplay.UpdateStaggerDisplay()** - Updates enemy stagger bar
- **PlayerCombatDisplay.UpdateStaggerDisplay()** - Updates player stagger bar

These are called automatically when stagger values change.

## Testing

1. Start combat
2. Attack enemies to build stagger
3. Watch the stagger bar fill up
4. When it reaches 100%, the enemy should be stunned
5. Test player stagger by having enemies attack the player

## Troubleshooting

**Stagger bar not showing:**
- Check that the Image component is assigned
- Verify the GameObject is active
- Check that `staggerThreshold > 0` for the enemy/player

**Stagger bar not updating:**
- Ensure `UpdateStaggerDisplay()` is being called
- Check that stagger values are changing (use Debug.Log)
- Verify the fill amount calculation is correct

**Bar appears in wrong position:**
- Check RectTransform anchors and positioning
- Ensure it's a child of the correct parent
- Verify the fill method matches the health bar direction

