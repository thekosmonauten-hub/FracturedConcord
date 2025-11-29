# Warrant Fusion System Setup Guide

## Overview

The Peacekeeper 3→1 fusion system allows players to combine 3 warrants into 1 enhanced warrant with modifier locking support. This system uses Unity's legacy UI (uGUI) to match the existing Warrant UI components.

## Components Created

### 1. `WarrantFusionLogic.cs`
Core fusion logic that combines 3 warrants into 1:
- Validates input warrants (no Unique, no blueprints)
- Handles modifier locking (guaranteed preservation of locked modifiers)
- Determines fused rarity (upgrades by one tier, max Rare)
- Combines modifiers from all 3 warrants
- Returns a new `WarrantDefinition` instance

### 2. `WarrantFusionUI.cs`
Main UI controller for the fusion interface:
- Manages 3 input slots and 1 output slot
- Handles modifier locking UI
- Integrates with `WarrantLockerGrid` for drag-and-drop
- Provides fuse, clear, and close buttons
- Shows status messages and fusion results

### 3. `WarrantFusionSlot.cs`
Individual slot component for placing warrants:
- Supports drag-and-drop from `WarrantLockerItem`
- Displays warrant icon, name, and rarity
- Validates warrants (rejects Unique and blueprints)
- Returns warrants to locker when replaced

### 4. `WarrantModifierLockItem.cs`
UI item for toggling modifier locks:
- Displays modifier name, value, and source slot
- Toggle to lock/unlock modifiers
- Visual feedback (color change when locked)

## Unity Setup Instructions

### Step 1: Create Fusion Panel GameObject

1. In your TownScene or WarrantTree scene, create a new GameObject named `WarrantFusionPanel`
2. Add a `Canvas` component (or use existing Canvas)
3. Add a `CanvasGroup` component for show/hide functionality
4. Add the `WarrantFusionUI` component

### Step 2: Build the UI Hierarchy

Create the following hierarchy under `WarrantFusionPanel`:

```
WarrantFusionPanel (GameObject with WarrantFusionUI)
├── Background (Image)
│   └── Color: Semi-transparent dark (0, 0, 0, 0.8)
│
├── Header (HorizontalLayoutGroup)
│   ├── Title (TextMeshProUGUI) - "Warrant Fusion"
│   └── CloseButton (Button)
│
├── InputSlotsContainer (HorizontalLayoutGroup)
│   ├── Slot1 (GameObject with WarrantFusionSlot)
│   │   ├── SlotBackground (Image) - Raycast Target: true
│   │   ├── RarityBorder (Image)
│   │   ├── WarrantIcon (Image)
│   │   ├── WarrantNameText (TextMeshProUGUI)
│   │   └── SlotLabelText (TextMeshProUGUI) - "Slot 1"
│   │
│   ├── Slot2 (GameObject with WarrantFusionSlot)
│   │   └── [Same structure as Slot1]
│   │
│   └── Slot3 (GameObject with WarrantFusionSlot)
│       └── [Same structure as Slot1]
│
├── OutputSlotContainer
│   └── OutputSlot (GameObject with WarrantFusionSlot)
│       └── [Same structure as input slots, but set isInteractable = false]
│
├── ModifierLockSection
│   ├── ModifierLockHeader (TextMeshProUGUI) - "Lock Modifiers"
│   └── ModifierLockScrollView (ScrollRect)
│       ├── Viewport (Mask)
│       │   └── Content (VerticalLayoutGroup) - This is the modifierLockContainer
│       └── Scrollbar Vertical
│
├── ActionButtons (HorizontalLayoutGroup)
│   ├── FuseButton (Button) - "Fuse Warrants"
│   ├── ClearButton (Button) - "Clear"
│   └── CloseButton (Button) - "Close"
│
└── StatusText (TextMeshProUGUI)
```

### Step 3: Create Modifier Lock Item Prefab

1. Create a GameObject named `ModifierLockItem`
2. Add `WarrantModifierLockItem` component
3. Add `HorizontalLayoutGroup` for layout
4. Add child GameObjects:
   - `LockToggle` (Toggle) - Checkbox for locking
   - `ModifierNameText` (TextMeshProUGUI)
   - `ModifierValueText` (TextMeshProUGUI)
   - `SlotLabelText` (TextMeshProUGUI)
   - `BackgroundImage` (Image)
5. Save as prefab: `Assets/Prefabs/UI/WarrantModifierLockItem.prefab`

### Step 4: Wire Up WarrantFusionUI

Assign the following references in the `WarrantFusionUI` inspector:

- **Panel References:**
  - `fusionPanel`: The root GameObject (WarrantFusionPanel)
  - `panelCanvasGroup`: The CanvasGroup component

- **Input Slots:**
  - `slot1`: Slot1 GameObject
  - `slot2`: Slot2 GameObject
  - `slot3`: Slot3 GameObject

- **Output Slot:**
  - `outputSlot`: OutputSlot GameObject

- **Modifier Locking UI:**
  - `modifierLockContainer`: The Content GameObject inside the ScrollRect
  - `modifierLockItemPrefab`: The ModifierLockItem prefab
  - `modifierScrollRect`: The ScrollRect component

- **Action Buttons:**
  - `fuseButton`: FuseButton
  - `clearButton`: ClearButton
  - `closeButton`: CloseButton

- **Status/Info Text:**
  - `statusText`: StatusText GameObject
  - `infoText`: (Optional) Additional info text

- **References:**
  - `warrantDatabase`: Your WarrantDatabase asset
  - `lockerGrid`: The WarrantLockerGrid component from your scene

### Step 5: Wire Up WarrantFusionSlot Components

For each slot (Slot1, Slot2, Slot3, OutputSlot):

1. Assign visual references:
   - `slotBackground`: Background Image
   - `warrantIcon`: WarrantIcon Image
   - `warrantNameText`: WarrantNameText
   - `slotLabelText`: SlotLabelText
   - `rarityBorder`: RarityBorder Image

2. Configure slot:
   - `slotLabel`: "Slot 1", "Slot 2", "Slot 3", or "Result"
   - `isInteractable`: true for input slots, false for output slot

3. Set rarity colors (optional, uses defaults if not set)

### Step 6: Wire Up WarrantModifierLockItem Prefab

In the prefab inspector:

1. Assign visual references:
   - `lockToggle`: LockToggle Toggle component
   - `modifierNameText`: ModifierNameText
   - `modifierValueText`: ModifierValueText
   - `slotLabelText`: SlotLabelText
   - `backgroundImage`: BackgroundImage

2. Configure colors:
   - `lockedColor`: Yellow (default)
   - `unlockedColor`: White (default)

### Step 7: Enable Drag-and-Drop

Ensure the slots can receive drops:

1. Add `Image` component to slot backgrounds if not present
2. Set `raycastTarget = true` on slot background Images
3. Ensure `EventSystem` exists in the scene (Unity usually adds this automatically)

## Usage

### Opening the Fusion Panel

```csharp
// From any script with access to WarrantFusionUI
WarrantFusionUI fusionUI = FindObjectOfType<WarrantFusionUI>();
fusionUI.ShowPanel();
```

### Integration with Peacekeeper Faction Panel

The fusion UI should be integrated into the Peacekeeper faction panel in TownScene:

1. Add a button in the Peacekeeper panel: "Fuse Warrants"
2. On click, show the `WarrantFusionPanel`
3. The panel can be shown/hidden via `ShowPanel()`, `HidePanel()`, or `TogglePanel()`

## Fusion Rules

1. **Input Requirements:**
   - All 3 slots must have warrants
   - Cannot fuse Unique warrants
   - Cannot fuse blueprint warrants

2. **Rarity Upgrade:**
   - Common → Magic
   - Magic → Rare
   - Rare → Rare (stays Rare)

3. **Modifier Locking:**
   - Locked modifiers are guaranteed to appear in the fused warrant
   - Unlocked modifiers may or may not appear (random selection)
   - Maximum modifiers based on fused rarity:
     - Common: 1 modifiers
     - Magic: 2 modifiers
     - Rare: 4 modifiers

4. **Notable Handling:**
   - If any input warrant has a Notable, one Notable is randomly selected for the fused warrant

5. **Result:**
   - Fused warrant is added to the WarrantLockerGrid
   - Input warrants are consumed (removed from locker)
   - Output slot displays the result

## Testing

1. Ensure you have at least 3 non-Unique, non-blueprint warrants in your locker
2. Open the fusion panel
3. Drag warrants into the 3 input slots
4. Lock desired modifiers using the toggle checkboxes
5. Click "Fuse Warrants"
6. Verify the fused warrant appears in the output slot and in the locker

## Notes

- The fusion system uses runtime `WarrantDefinition` instances (ScriptableObject.CreateInstance)
- Fused warrants are not persisted as assets, but are runtime instances
- The system integrates with the existing `WarrantLockerGrid` for inventory management
- All UI uses Unity's legacy UI system (uGUI) to match existing Warrant UI components

