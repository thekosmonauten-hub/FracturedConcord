# MainMenu Controller Setup Instructions

## Quick Setup Guide

### Step 1: Add Controller Script to Scene

1. In your **MainMenu** scene, create an empty GameObject
2. Rename it to **MainMenuController**
3. Add the `MainMenuController` script component to it

### Step 2: Assign References in Inspector

Select the **MainMenuController** GameObject and assign these references:

#### Center Panel Buttons (4 references)
- **Main Continue Button** ← Drag `MainMenuCanvas/CenterPanel/MainContinueButton`
- **Start Journey Button** ← Drag `MainMenuCanvas/CenterPanel/StartJourneyButton`
- **Settings Button** ← Drag `MainMenuCanvas/CenterPanel/SettingsButton`
- **Exit Button** ← Drag `MainMenuCanvas/CenterPanel/ExitButton`

#### Character Slot Panel (3 references)
- **Character Slot Panel** ← Drag `MainMenuCanvas/TogglePanel/CharacterSlotPanel`
- **Character Slot Container** ← Drag `MainMenuCanvas/TogglePanel/CharacterSlotPanel/CharacterSlotContainer`
- **Sidebar Toggle Button** ← Drag `MainMenuCanvas/TogglePanel/SidebarToggleButton`

#### Character Slot Prefab (1 reference)
- **Character Slot Prefab** ← Drag your `CharacterSlotPrefab` from Project window

#### Scene Names (2 text fields)
- **Character Creation Scene Name**: `CharacterCreation` (or your scene name)
- **Game Scene Name**: `GameScene` (or your scene name)

#### Animation Settings (3 fields)
- **Panel Anim Duration**: `0.3` (seconds)
- **Panel Anim Ease**: `OutCubic` (dropdown)
- **Panel Offset X**: `600` (pixels - how far off-screen the panel slides)

---

## Step 3: Setup Character Slot Prefab Structure

Make sure your **CharacterSlotPrefab** has the correct child names (case-sensitive!):

```
CharacterSlotPrefab
├─ Background              ← Image component
├─ CharacterName           ← TextMeshProUGUI component
├─ CharacterLevel          ← TextMeshProUGUI component
├─ CharacterProgression    ← TextMeshProUGUI component
├─ ContinueButton          ← Button component
│  └─ Text                 ← TextMeshProUGUI (child)
└─ DeleteButton            ← Button component
   └─ Text                 ← TextMeshProUGUI (child)
```

**Critical**: The script looks for these exact names using `transform.Find()`. If your names don't match, the script won't find the components!

---

## Step 4: Setup CharacterSlotPanel for Animation

The panel needs to be positioned correctly for the slide animation:

1. Select **CharacterSlotPanel** in Hierarchy
2. In **RectTransform**:
   - **Anchors**: Set to **Top-Right** or **Stretch Right** (so it anchors to the right side)
   - **Pivot**: `(1, 0.5)` or `(1, 1)` depending on your design
   - Initial **Anchored Position X**: Should be positive (e.g., 600) to start off-screen
3. The script will animate it to `X = 0` (visible) and back to `X = 600` (hidden)

---

## Step 5: Verify Existing Managers

Make sure these GameObjects are in your MainMenu scene:

- ✅ **CharacterManager** (with CharacterManager component)
- ✅ **CharacterSaveSystem** (with CharacterSaveSystem component)
- ✅ **TransitionManager** (with TransitionManager component - optional)
- ✅ **EventSystem** (required for all UI interaction)

The script will **auto-find** these using `FindObjectOfType<>()`, so no manual assignment needed!

---

## Step 6: Add LayoutGroup to CharacterSlotContainer (Recommended)

For automatic spacing of character slots:

1. Select **CharacterSlotContainer**
2. Add **Vertical Layout Group** component:
   - **Child Alignment**: Upper Center
   - **Control Child Size**: ✅ Width only
   - **Child Force Expand**: ✅ Width only
   - **Spacing**: 10-15 pixels
   - **Padding**: 10 all sides
3. Add **Content Size Fitter** component:
   - **Vertical Fit**: Preferred Size

This makes the container grow automatically as character slots are added!

---

## Testing Checklist

After setup, test these features:

### Main Menu Buttons
- [ ] **MainContinueButton** loads most recent character (or opens panel if none exist)
- [ ] **StartJourneyButton** opens character slot panel
- [ ] **SettingsButton** logs to console (not implemented yet)
- [ ] **ExitButton** quits application (or stops Play mode in Editor)

### Character Panel
- [ ] **SidebarToggleButton** slides panel in from right
- [ ] Panel animates smoothly
- [ ] Clicking button again closes panel

### Character Slots
- [ ] Character slots appear in panel (if you have saved characters)
- [ ] Character name, level, and progression display correctly
- [ ] **ContinueButton** on slot loads that character
- [ ] **DeleteButton** removes character and refreshes list
- [ ] After deleting all characters, **MainContinueButton** becomes disabled

### Scene Flow
- [ ] Loading a character transitions to game scene
- [ ] TransitionManager fade effect works (if present)
- [ ] No console errors

---

## Common Issues & Fixes

### "ContinueButton not found on slot"
**Problem**: Script can't find the button on the prefab.  
**Fix**: Check prefab structure - button must be named exactly `ContinueButton` (case-sensitive).

### "CharacterSaveSystem is null"
**Problem**: CharacterSaveSystem not in scene.  
**Fix**: Add the CharacterSaveSystem GameObject from your prefabs or previous scene.

### Panel doesn't animate
**Problem**: CharacterSlotPanel reference not assigned or DOTween not working.  
**Fix**: 
1. Verify CharacterSlotPanel is assigned in Inspector
2. Check DOTween is installed (Package Manager)
3. Check console for errors

### Character slots overlap or don't layout properly
**Problem**: CharacterSlotContainer needs layout components.  
**Fix**: Add Vertical Layout Group + Content Size Fitter (see Step 6).

### Main Continue Button always disabled
**Problem**: No saved characters exist.  
**Fix**: Create a character first, or manually test by calling `DebugPrintCharacterCount()` context menu.

---

## Debug Features

The script includes debug context menu items. Right-click the script in Inspector:

- **Debug: Print Character Count** - Shows all saved characters in console
- **Debug: Toggle Panel** - Manually toggle the panel for testing

---

## Optional Enhancements

### 1. Add Close Button to Panel
Add a close button (X) inside CharacterSlotPanel:

```csharp
// In Inspector, add an OnClick event:
// MainMenuController.CloseCharacterPanel()
```

### 2. Add "Create New Character" Button
Add a button in CharacterSlotPanel that calls:

```csharp
// In Inspector, add an OnClick event:
// MainMenuController.GoToCharacterCreation()
```

### 3. Add Confirmation Dialog for Delete
Update the `OnCharacterDeleteClicked` method to show a confirmation dialog before deleting.

---

## Summary

**Required Steps:**
1. ✅ Add MainMenuController script to scene
2. ✅ Assign 10 references in Inspector
3. ✅ Verify prefab structure matches expected names
4. ✅ Setup CharacterSlotPanel positioning
5. ✅ Add layout components to CharacterSlotContainer
6. ✅ Test all buttons and features

**Total Setup Time:** ~5 minutes

You're done! 🎉


