# Character Screen Card Hover Preview Setup

This guide explains how to set up the full card preview that appears when hovering over simplified card rows in CharacterDisplayUI.

---

## 🎯 Overview

**Feature:** When you hover over a simplified card row (CharacterScreenDeckCard), a full detailed card preview appears in the FullCardPreview container.

**Components:**
1. `CharacterScreenCardHover` - Detects mouse hover on simplified cards
2. `CharacterDisplayController` - Spawns/hides full card preview
3. `DeckBuilderCardUI` - Displays the full card details

---

## ✅ Setup Steps

### **1. Assign Full Card Preview Prefab**

1. **Select `CharacterDisplayController` GameObject** in the scene
2. **In Inspector → Full Card Preview (Hover):**
   ```
   Full Card Preview Prefab: [CardPrefab]
   Full Card Preview Container: [FullCardPreview]
   Preview Scale: 1.0
   Show Hover Debug Logs: ☐ (optional, for testing)
   ```

**Prefab to Use:**
- **CardPrefab.prefab** (`Assets/Art/CardArt/CardPrefab.prefab`)
- Must have `DeckBuilderCardUI` component
- Shows full card details (art, name, cost, description, effects, embossing)

### **2. Assign Full Card Preview Container**

The container already exists in the scene:
- **GameObject name:** `FullCardPreview`
- **Path:** `CharacterDisplayUI/Background/LeftPage/FullCardPreview`
- **FileID:** 1944753816

**To assign:**
1. **In Hierarchy**, expand: `CharacterDisplayUI → Background → LeftPage`
2. Find `FullCardPreview` GameObject
3. **Drag it** to `CharacterDisplayController → Full Card Preview Container` field

---

## 🔧 How It Works

### **Automatic Hover System**

When you spawn cards using `DisplayStarterDeck()`:

1. **For each card with `DeckCardListUI`:**
   - Automatically adds `CharacterScreenCardHover` component
   - Sets the card data
   - Subscribes to hover events

2. **On Mouse Enter:**
   - `CharacterScreenCardHover.OnPointerEnter()` fires
   - Triggers `OnCardHoverEnter` event
   - `CharacterDisplayController` spawns full card preview

3. **On Mouse Exit:**
   - `CharacterScreenCardHover.OnPointerExit()` fires
   - Triggers `OnCardHoverExit` event
   - `CharacterDisplayController` destroys preview

### **Code Flow**

```
User hovers card
    ↓
CharacterScreenCardHover.OnPointerEnter()
    ↓
OnCardHoverEnter event → CharacterDisplayController
    ↓
Instantiate fullCardPreviewPrefab
    ↓
Initialize with DeckBuilderCardUI
    ↓
Display in fullCardPreviewContainer
    ↓
User moves mouse away
    ↓
CharacterScreenCardHover.OnPointerExit()
    ↓
OnCardHoverExit event → CharacterDisplayController
    ↓
Destroy preview card
```

---

## 📐 Container Setup (Already Done)

The `FullCardPreview` container in the scene:
- **Location:** Inside `LeftPage` (angled book page)
- **Transform:**
  - Rotation: -1.616° (inherits from LeftPage rotation)
  - Position: Positioned on left page
  - Size: 301.7 × 514.9
- **Child:** `Image` component (optional background)

---

## 🎨 Preview Card Display

**The preview card:**
- Uses `DeckBuilderCardUI` (full card display)
- Shows all card details:
  - ✅ High-res card artwork (`cardImage`)
  - ✅ Card name
  - ✅ Mana cost
  - ✅ Full description
  - ✅ Card effects
  - ✅ Embossing slots
  - ✅ Card level/XP (if applicable)
  - ✅ Combo descriptions
  - ✅ Category icons

**Preview settings:**
- Centered in `FullCardPreview` container
- Scaled by `previewScale` (default 1.0)
- All raycasting disabled (doesn't block UI)
- Interaction disabled (display only)

---

## 🧪 Testing

### **Setup Test Mode:**

1. **Select `CharacterDisplayController`** in scene
2. **Enable Test Mode:**
   ```
   Test Mode: ✅
   Test Class: "Marauder"
   ```
3. **Verify Full Card Preview settings:**
   ```
   Full Card Preview Prefab: CardPrefab
   Full Card Preview Container: FullCardPreview
   Preview Scale: 1.0
   Show Hover Debug Logs: ✅ (for testing)
   ```

### **Test Hover:**

1. **Press Play**
2. **Hover over a simplified card** in the starter deck grid
3. **Verify:**
   - Full card preview appears in `FullCardPreview` container
   - Shows all card details
   - Preview disappears when mouse leaves

### **Expected Console Output:**

With `showHoverDebugLogs = true`:
```
[CharacterScreenCardHover] Hovering over: Strike
[CharacterDisplayController] Showing full preview for: Strike
[CharacterScreenCardHover] Stopped hovering
[CharacterDisplayController] Hid card preview
```

---

## 🔧 Adjusting Preview Position/Scale

### **Scale Adjustment:**

1. **Select `CharacterDisplayController`**
2. **Adjust `Preview Scale`:**
   - `0.8` - Smaller preview (80%)
   - `1.0` - Normal size (default)
   - `1.2` - Larger preview (120%)

### **Position Adjustment:**

The preview is centered in `FullCardPreview` container. To adjust:

1. **In Hierarchy**, select `FullCardPreview`
2. **Move/resize the container** using Rect Transform
3. Preview will automatically center within it

**Tips:**
- Position on the left or right side of the page
- Make it larger for more detail visibility
- Rotate slightly to match book angle if desired

---

## 🎨 Visual Polish (Optional)

### **Add Background/Frame to Preview:**

1. **Select `FullCardPreview` container**
2. **Add background image:**
   - Child → UI → Image
   - Assign a frame/background sprite
   - Adjust color/alpha

### **Add Shadow/Glow:**

1. **Select `FullCardPreview` container**
2. **Add Outline or Shadow component:**
   - Add Component → UI → Effects → Outline
   - Effect Distance: 3-5
   - Color: Black with alpha

---

## 📝 Customization Options

### **Disable Hover for Specific Cards:**

In the future, you could add:
```csharp
if (cardEntry.card.cardName == "BasicStrike")
{
    // Don't add hover component for basic cards
    continue;
}
```

### **Change Preview Animation:**

Currently instant show/hide. You could add:
```csharp
// In OnCardHoverEnter()
LeanTween.scale(currentPreviewCard, Vector3.one * previewScale, 0.2f)
    .setEase(LeanTweenType.easeOutBack);
```

### **Different Preview Prefab Per Card Type:**

```csharp
GameObject prefab = cardEntry.card.cardType == "Attack" 
    ? attackCardPreviewPrefab 
    : fullCardPreviewPrefab;
```

---

## 🐛 Troubleshooting

### ❌ "Preview not appearing"

**Fix:**
1. Check `Full Card Preview Prefab` is assigned
2. Check `Full Card Preview Container` is assigned
3. Enable `Show Hover Debug Logs` and check Console
4. Verify prefab has `DeckBuilderCardUI` component

### ❌ "Preview blocking UI clicks"

**Fix:**
- The code automatically disables raycasting on preview
- If still blocking, check if preview container has `CanvasGroup` with `blocksRaycasts = true`

### ❌ "Preview position wrong"

**Fix:**
1. Select `FullCardPreview` container in Hierarchy
2. Adjust position in Rect Transform
3. Preview will center within the container

### ❌ "Hover not detecting"

**Fix:**
1. Check if `CharacterScreenDeckCard` prefab has raycasting enabled
2. Verify `DeckCardListUI` or card background has `RaycastTarget = true`
3. Check if EventSystem exists in scene (should auto-exist)

---

## 🔄 Scene Hierarchy Reference

```
CharacterDisplayUI (Canvas)
└── Background (Image)
    ├── RightPage (rotated -16.38°)
    │   └── ContentSection
    │       └── ... (class info, attributes)
    └── LeftPage (rotated -16.38°)
        ├── StartingDeckContainer
        │   ├── Card_0_Strike (CharacterScreenDeckCard)
        │   │   └── CharacterScreenCardHover ← Added automatically
        │   ├── Card_1_Bash (CharacterScreenDeckCard)
        │   │   └── CharacterScreenCardHover ← Added automatically
        │   └── ... (more cards)
        ├── FullCardPreview ← Preview displays here
        │   └── Image (optional background)
        └── AscendancyView
```

---

## ✨ Benefits

**User Experience:**
- ✅ Hover over simple card row → see full details
- ✅ No clicking required
- ✅ Smooth experience like Hearthstone/deck builders
- ✅ Clean layout (simplified cards in grid, full card on demand)

**Performance:**
- ✅ Only one preview card at a time
- ✅ Preview destroyed when not hovering
- ✅ Simplified cards use `cardThumbnail` (smaller sprites)
- ✅ Preview uses `cardImage` (full detail)

---

## 📋 Quick Setup Checklist

- [ ] Assign `CardPrefab.prefab` to Full Card Preview Prefab
- [ ] Assign `FullCardPreview` GameObject to Full Card Preview Container
- [ ] Set Preview Scale (default: 1.0)
- [ ] Enable Test Mode
- [ ] Press Play and hover over cards
- [ ] Verify preview appears/disappears correctly

---

## 🎯 Current Status

**✅ Implemented:**
- CharacterScreenCardHover component (auto-added to cards)
- Hover event system
- Full card preview spawning/destruction
- Raycast prevention (preview doesn't block UI)
- Proper cleanup on scene exit

**⚠️ Needs Setup:**
- Assign full card preview prefab in Inspector
- Assign full card preview container in Inspector

**🧪 Ready to Test:**
- Enable test mode and hover over cards to verify

---

**Last Updated:** 2024-12-19
**Status:** ✅ Code Complete - Ready for Unity Setup


