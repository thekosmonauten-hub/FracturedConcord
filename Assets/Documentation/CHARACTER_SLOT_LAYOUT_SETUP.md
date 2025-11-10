# Character Slot Container Layout Setup

## Quick Setup for Top-Aligned Character Slots with Padding

### Step 1: Select CharacterSlotContainer

In Unity Hierarchy:
```
MainMenuCanvas
└─ TogglePanel
   └─ CharacterSlotPanel
      └─ CharacterSlotContainer  ← Select this
```

---

### Step 2: Add Vertical Layout Group

1. With **CharacterSlotContainer** selected, click **Add Component**
2. Search for **Vertical Layout Group**
3. Click to add it

---

### Step 3: Configure Vertical Layout Group

Set these properties in the Inspector:

#### **Padding**
- **Left**: `10`
- **Right**: `10`
- **Top**: `10`
- **Bottom**: `10`

#### **Spacing**
- **Spacing**: `10` (space between character slots)

#### **Child Alignment**
- **Child Alignment**: **Upper Center** (starts from top)

#### **Control Child Size**
- ✅ **Width** (checked)
- ❌ **Height** (unchecked - let slots control their own height)

#### **Use Child Scale**
- ❌ Unchecked

#### **Child Force Expand**
- ✅ **Width** (checked - slots fill container width)
- ❌ **Height** (unchecked - slots use preferred height)

---

### Step 4: Add Content Size Fitter (Optional but Recommended)

This makes the container grow/shrink based on how many character slots are inside:

1. With **CharacterSlotContainer** still selected, click **Add Component**
2. Search for **Content Size Fitter**
3. Click to add it
4. Set:
   - **Horizontal Fit**: Unconstrained
   - **Vertical Fit**: **Preferred Size**

---

### Step 5: Setup CharacterSlotContainer RectTransform

Make sure the RectTransform is configured to fill the CharacterSlotPanel:

- **Anchors**: Stretch / Stretch (fill parent)
- **Left**: `0`
- **Right**: `0`
- **Top**: `0`
- **Bottom**: `0`

---

## Expected Result

Character slots will:
- ✅ Start from the **top**
- ✅ Stack **vertically**
- ✅ Have **10px padding** around the edges
- ✅ Have **10px spacing** between each slot
- ✅ **Fill the width** of the container
- ✅ Container **auto-grows** as slots are added

---

## Visual Example

```
┌─────────────────────────────────────┐
│ CharacterSlotContainer              │
│ ┌─────────────────────────────────┐ │ ← 10px top padding
│ │ Character Slot 1                │ │
│ │ TESTER123                       │ │
│ │ Level 1 Marauder - Act 1       │ │
│ │ [Continue] [Delete]             │ │
│ └─────────────────────────────────┘ │
│          ↕ 10px spacing              │
│ ┌─────────────────────────────────┐ │
│ │ Character Slot 2                │ │
│ │ HERO456                         │ │
│ │ Level 5 Witch - Act 2           │ │
│ │ [Continue] [Delete]             │ │
│ └─────────────────────────────────┘ │
│          ↕ 10px spacing              │
│ ┌─────────────────────────────────┐ │
│ │ Character Slot 3                │ │
│ └─────────────────────────────────┘ │
│                                     │
│ ← 10px left/right padding           │
└─────────────────────────────────────┘
```

---

## Troubleshooting

### Slots not starting from top?
- Check **Child Alignment** is set to **Upper Center** (not Middle Center)

### Slots overlapping?
- Ensure **Spacing** is set (e.g., 10)
- Check **Control Child Size** → Height is UNCHECKED

### Slots not filling width?
- **Control Child Size** → Width should be CHECKED
- **Child Force Expand** → Width should be CHECKED

### Container not growing with slots?
- Add **Content Size Fitter** component
- Set **Vertical Fit** to **Preferred Size**

### Slots cut off at bottom?
- Make sure CharacterSlotPanel has a **Scroll Rect** component
- Verify CharacterSlotContainer is set as the **Content** of the Scroll Rect

---

## Optional: Setup Scroll Rect (if not already done)

If you have many characters and want scrolling:

1. Select **CharacterSlotPanel** (parent of CharacterSlotContainer)
2. Add **Scroll Rect** component:
   - **Content**: Drag CharacterSlotContainer here
   - **Viewport**: Leave empty (or create a Viewport child)
   - **Horizontal**: ❌ Unchecked
   - **Vertical**: ✅ Checked
   - **Movement Type**: Clamped
   - **Scroll Sensitivity**: 20

---

## Summary

**Required Components on CharacterSlotContainer:**
1. ✅ **Vertical Layout Group** (spacing: 10, alignment: Upper Center)
2. ✅ **Content Size Fitter** (vertical: Preferred Size)
3. ✅ **RectTransform** (anchors: Stretch/Stretch)

That's it! Character slots will now align perfectly from the top with consistent spacing! 🎯


