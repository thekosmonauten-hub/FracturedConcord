# Manual EncounterNode Setup Guide

## Overview

This guide walks you through manually adding EncounterNode buttons to your MainGameUI scene (without using WorldMapNodeGenerator). This gives you full control over positioning, layout, and configuration.

## Prerequisites

1. ✅ EncounterManager is set up (auto-created or manually added)
2. ✅ Encounter assets exist in `Resources/Encounters/Act1`, etc.
3. ✅ MainGameUI scene is open

## Step-by-Step Setup

### Step 1: Create Container for Encounter Nodes

1. **In MainGameUI scene, create a parent container:**
   - Right-click in Hierarchy → Create Empty
   - Name it: `EncounterNodesContainer` (or `Act1Nodes`, `Act2Nodes`, etc.)
   - Add a `RectTransform` component (if using UI)
   - Set it as a child of your Canvas or WorldMap panel

2. **Configure the container:**
   - Set anchor/stretch for layout
   - Position where you want encounter nodes to appear

### Step 2: Create EncounterButton GameObject

For each encounter you want to display:

1. **Create the button GameObject:**
   - Right-click on `EncounterNodesContainer` → UI → Button
   - Name it: `EncounterNode_1` (or `EncounterNode_2`, etc. matching the encounter ID)

2. **Add EncounterButton component:**
   - Select the button GameObject
   - Click "Add Component" in Inspector
   - Search for: `EncounterButton`
   - Add it

### Step 3: Configure EncounterButton Component

In the Inspector, configure the `EncounterButton` component:

#### **Encounter Settings**

- **Encounter Asset** (Optional):
  - Drag the `EncounterDataAsset` from `Resources/Encounters/Act1/1.WhereNightFirstFell.asset` (or your encounter asset)
  - This auto-populates encounter data if `autoSyncEncounterData` is enabled

- **Encounter ID**:
  - Set to the encounter ID (e.g., `1` for first encounter)
  - **Important**: Must match the ID in your EncounterDataAsset

- **Encounter Name**:
  - Set to the encounter name (e.g., `Where Night First Fell`)
  - Will be overridden by EncounterManager if `autoSyncEncounterData = true`

- **Auto Sync Encounter Data**: ✅ **Keep this CHECKED**
  - When checked, button syncs data from EncounterManager
  - Ensures button stays in sync with progression

#### **Requirements**

- **Require Unlocked Encounter**: ✅ Checked (default)
  - Button will be disabled if encounter is locked

- **Require Selected Character**: ✅ Checked (default)
  - Button requires a character to be loaded

- **Require Active Deck**: ✅ Checked (default)
  - Button requires an active deck

#### **UI References** (Auto-assigned, but verify)

- **Button**: Should auto-assign the Button component
- **Button Text TMP**: Assign your TextMeshProUGUI component for the button text
- **Area Level Label**: (Optional) TextMeshProUGUI for area level display
- **Status Label**: (Optional) TextMeshProUGUI for status messages
- **Wave Preview Label**: (Optional) TextMeshProUGUI for wave preview
- **Locked Overlay**: (Optional) CanvasGroup for locked state overlay
- **Lock Icon**: (Optional) GameObject for lock icon
- **Encounter Image**: (Optional) Image component for encounter sprite

### Step 4: Configure Button UI

1. **Set up button text:**
   - Create or assign a TextMeshProUGUI child
   - Assign it to `Button Text TMP` in EncounterButton
   - Text will auto-update from EncounterManager

2. **Set up area level display (optional):**
   - Create a TextMeshProUGUI for area level
   - Name it something with "area" in the name (e.g., `AreaLevelText`)
   - Assign to `Area Level Label`
   - Will show the encounter's area level

3. **Set up locked overlay (optional):**
   - Create a CanvasGroup child for the locked overlay
   - Assign to `Locked Overlay`
   - This will show/hide when encounter is locked/unlocked

4. **Set up lock icon (optional):**
   - Create a GameObject with an Image/Sprite for lock icon
   - Assign to `Lock Icon`
   - Will show when encounter is locked

### Step 5: Position the Button

1. **Set button position:**
   - Use RectTransform to position where you want
   - Can use anchors, or set absolute positions
   - Repeat for each encounter node

2. **Layout options:**
   - Use Unity's Layout Groups (Horizontal/Vertical/Grid) for automatic layout
   - Or manually position each button

### Step 6: Verify Configuration

For each EncounterButton, verify:

- ✅ `encounterID` matches your EncounterDataAsset ID
- ✅ `autoSyncEncounterData` is checked
- ✅ `requireUnlockedEncounter` is checked (if you want lock behavior)
- ✅ Button component is assigned
- ✅ Text component is assigned (if using text)

### Step 7: Test in Play Mode

1. **Play the game**
2. **Check console** for:
   ```
   [EncounterManager] System initialized
   [EncounterButton] EncounterButton: Wired onClick for 'Where Night First Fell' (ID 1)
   ```

3. **Verify button behavior:**
   - Button text shows encounter name
   - Area level displays (if configured)
   - Button is enabled/disabled based on unlock state
   - Clicking button starts the encounter

## Configuration Examples

### Example 1: Basic EncounterButton (Minimal Setup)

```
EncounterNode_1 (GameObject)
├── Button Component
├── EncounterButton Component
│   ├── Encounter ID: 1
│   ├── Encounter Name: "Where Night First Fell"
│   ├── Auto Sync Encounter Data: ✅
│   └── Button: (auto-assigned)
└── Text (TMP) (Child)
    └── Assigned to Button Text TMP
```

### Example 2: Full-Featured EncounterButton

```
EncounterNode_1 (GameObject)
├── Button Component
├── Image Component (for background)
├── EncounterButton Component
│   ├── Encounter Asset: [EncounterDataAsset]
│   ├── Encounter ID: 1
│   ├── Auto Sync Encounter Data: ✅
│   ├── Button: (auto-assigned)
│   ├── Button Text TMP: [TextMeshProUGUI]
│   ├── Area Level Label: [TextMeshProUGUI]
│   ├── Status Label: [TextMeshProUGUI]
│   ├── Wave Preview Label: [TextMeshProUGUI]
│   ├── Locked Overlay: [CanvasGroup]
│   └── Lock Icon: [GameObject]
├── Text (TMP) - Button Text
├── AreaLevelText (TMP) - Area Level
├── StatusText (TMP) - Status Messages
├── WavePreview (TMP) - Wave Info
├── LockedOverlay (CanvasGroup)
└── LockIcon (Image)
```

## Data Flow Verification

### How Data Flows from EncounterManager to Button

1. **OnEnable** → Button calls `RefreshVisuals()`
2. **RefreshVisuals** → Calls `EvaluateAvailability()`
3. **EvaluateAvailability** → Calls `EncounterManager.Instance.GetEncounterByID(encounterID)`
4. **If data found** → Calls `ApplyEncounterData(data)` to sync button fields
5. **UI Updates** → Button text, area level, sprite, etc. update

### Verify Data Sync

To verify data is syncing correctly:

1. **Set encounterID** in Inspector (e.g., `1`)
2. **Play the game**
3. **Check console** for:
   ```
   [EncounterButton] EncounterButton: Wired onClick for 'Where Night First Fell' (ID 1)
   ```
4. **Verify button text** shows the encounter name from EncounterManager
5. **Check area level** displays correctly (if configured)

## Common Issues & Solutions

### Issue: Button shows wrong encounter name

**Solution:**
- Verify `encounterID` matches the ID in EncounterManager
- Ensure `autoSyncEncounterData` is checked
- Check that EncounterManager has loaded encounters (check console)

### Issue: Button is always disabled

**Solution:**
- Check `requireUnlockedEncounter` - if checked, encounter must be unlocked
- Verify character is loaded (`requireSelectedCharacter`)
- Verify deck is active (`requireActiveDeck`)
- Check console for availability reason messages

### Issue: Button doesn't respond to clicks

**Solution:**
- Verify Button component is assigned
- Check that EventSystem exists in scene
- Verify Canvas has GraphicRaycaster
- Check `verboseDebug` is enabled to see click logs

### Issue: Data not syncing from EncounterManager

**Solution:**
- Ensure `autoSyncEncounterData` is checked
- Verify EncounterManager.Instance is not null
- Check that encounter exists in EncounterManager (verify ID)
- Look for errors in console

## Best Practices

1. **Use EncounterDataAsset references:**
   - Assign the asset in Inspector for easier management
   - Button will auto-sync from asset if `autoSyncEncounterData` is enabled

2. **Keep autoSyncEncounterData enabled:**
   - Ensures buttons stay in sync with EncounterManager
   - Prevents stale data issues

3. **Use consistent naming:**
   - Name buttons: `EncounterNode_1`, `EncounterNode_2`, etc.
   - Makes it easier to identify and manage

4. **Group by Act:**
   - Create separate containers: `Act1Nodes`, `Act2Nodes`, etc.
   - Easier to organize and manage

5. **Test incrementally:**
   - Add one button, test it
   - Then add more buttons
   - Easier to debug issues

## Quick Checklist

For each EncounterButton you create:

- [ ] GameObject created and named appropriately
- [ ] EncounterButton component added
- [ ] `encounterID` set correctly
- [ ] `autoSyncEncounterData` checked
- [ ] Button component assigned
- [ ] Text component assigned (if using)
- [ ] UI references assigned (optional features)
- [ ] Positioned correctly in scene
- [ ] Tested in Play mode
- [ ] Button text updates from EncounterManager
- [ ] Button enables/disables based on unlock state
- [ ] Clicking button starts encounter

## Example: Setting Up Encounter 1

Here's a complete example for setting up Encounter 1:

1. **Create GameObject:**
   ```
   Hierarchy:
   Canvas
   └── EncounterNodesContainer
       └── EncounterNode_1 (Button)
   ```

2. **Configure EncounterButton:**
   - Encounter ID: `1`
   - Encounter Name: `Where Night First Fell` (will be overridden)
   - Auto Sync Encounter Data: ✅ **Checked**
   - Require Unlocked Encounter: ✅ **Checked**
   - Require Selected Character: ✅ **Checked**
   - Require Active Deck: ✅ **Checked**

3. **Assign UI References:**
   - Button: (auto-assigned)
   - Button Text TMP: [Your TextMeshProUGUI]

4. **Test:**
   - Play the game
   - Verify button shows "Where Night First Fell"
   - Verify button is enabled (Encounter 1 is always unlocked)
   - Click button → Should start encounter

## Summary

Manual setup gives you full control but requires:
- Creating GameObjects for each button
- Configuring EncounterButton component
- Setting up UI references
- Positioning buttons

The key is ensuring:
- ✅ `encounterID` matches EncounterManager data
- ✅ `autoSyncEncounterData` is enabled
- ✅ UI references are assigned
- ✅ Button component is present

Once set up, the buttons will automatically sync with EncounterManager and respond to progression changes!


