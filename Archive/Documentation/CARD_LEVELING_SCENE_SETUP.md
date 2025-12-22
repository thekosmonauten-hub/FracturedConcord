# Card Leveling System - Scene Setup Guide

## Overview

This guide explains how to set up the card leveling system in your Unity scenes and prefabs.

**Created:** November 2, 2025  
**For:** Card & Embossing leveling system integration

---

## Scene Setup

### Required Manager (Automatic)

The `CardExperienceManager` is a **singleton** that auto-creates itself when needed. No manual scene setup required!

```
Scene Hierarchy:
└── CardExperienceManager (auto-created at runtime)
```

**How it works:**
- First call to `CardExperienceManager.Instance` creates the GameObject
- Uses `DontDestroyOnLoad` to persist across scenes
- Automatically saves/loads with character data

**No action needed** - The system handles this automatically.

---

## Prefab Setup

### CardPrefab.prefab & CardPrefab_combat.prefab

Both prefabs need the same components for level tracking:

### Component 1: CardLevel Text (Already Created ✓)

**What You Have:**
```
VisualRoot (or card root)
└── CardLevelContainer
    └── Text (TMP) ← Shows "Lv. 5", "Lv. 20", etc.
```

**How CardDisplay Finds It:**
1. Searches for `"CardLevel"` (direct child)
2. If not found, searches `"CardLevelContainer/Text (TMP)"`
3. Auto-assigns to `cardLevelText` field

**Configuration:**
- **Text Content:** Will be set dynamically to `"Lv. {level}"`
- **Font Size:** 10-12pt recommended
- **Position:** Top-right corner of card (above card art)
- **Color:** White or yellow for visibility
- **Auto-Hide:** Hidden for Level 1 cards (to reduce clutter)

**Example:**
```
Position: Anchored to top-right
Offset: X = -10, Y = -10 (from top-right corner)
Size: 40×20
```

### Component 2: XP Slider (Already Created ✓)

**What You Have:**
```
VisualRoot (or card root)
└── Slider (UI Slider component)
    ├── Background
    └── Fill Area
        └── Fill (changes based on XP)
```

**OR (Alternative Location):**
```
VisualRoot
└── CardLevelContainer
    └── Slider
```

**How CardDisplay Finds It:**
1. Searches for `"Slider"` (direct child of VisualRoot)
2. If not found, searches `"CardLevelContainer/Slider"`
3. Auto-assigns to `cardXPSlider` field

**Slider Configuration:**

**1. Slider Component Settings:**
- **Direction:** Left to Right (Horizontal)
- **Min Value:** 0
- **Max Value:** 1
- **Whole Numbers:** Unchecked
- **Interactable:** Unchecked (read-only, visual only)

**2. Visual Settings:**
- **Background:** Dark color (semi-transparent)
- **Fill Color:** Gold/Yellow (RGB: 1.0, 0.84, 0.0)
- **Height:** 4-8 pixels (thin bar)
- **Width:** Match card width minus padding

**3. Position:**
- **Location:** Bottom of CardLevelContainer or below card name
- **Anchor:** Stretch horizontally
- **Offset:** 5px padding on left/right

**Example Layout:**
```
┌─────────────────────┐
│ Heavy Strike  Lv.5  │ ← CardLevel text
│ ████░░░░░░░░░░░     │ ← XP Slider (60% filled)
│                     │
│   [Card Art]        │
└─────────────────────┘
```

**Auto-Hide:** Hidden when card reaches level 20 (max level)

---

## Prefab Hierarchy Reference

### CardPrefab.prefab Structure

```
CardPrefab
├── VisualRoot (or direct children)
│   ├── CardLevelContainer (your custom container)
│   │   ├── Text (TMP) ← CardLevel text
│   │   └── Slider ← XP progress bar
│   ├── CardName
│   ├── CardImage
│   ├── DescriptionText
│   ├── AdditionalEffectText
│   └── EmbossingSlots
│       ├── Slot1Container
│       │   ├── Slot1Embossing
│       │   └── Slot1Filled
│       └── ... (Slot2-5)
```

### CardPrefab_combat.prefab Structure

```
CardPrefab_combat
└── VisualRoot 
    ├── CardLevelContainer (your custom container)
    │   ├── Text (TMP) ← CardLevel text
    │   └── Slider ← XP progress bar
    ├── CardName
    ├── CardImage
    ├── DescriptionText
    ├── AdditionalEffectText
    └── EmbossingSlots
        └── ... (Slot1-5 containers)
```

---

## Component Assignment

### Option 1: Automatic (Recommended)

The `CardDisplay` component will **automatically find and wire up** these components:

**When CardDisplay is attached to a prefab:**
1. Awake() runs
2. Searches for "CardLevelContainer/Text (TMP)"
3. Searches for "Slider" or "CardLevelContainer/Slider"
4. Auto-assigns references

**No manual assignment needed!** Just ensure naming is correct.

### Option 2: Manual Assignment

If auto-assignment doesn't work:

1. **Select** CardPrefab in Project window
2. **Expand** to see root GameObject
3. **Find** `CardDisplay` component in Inspector
4. **Drag** components to fields:
   - `Card Level Text` → Drag "Text (TMP)" from CardLevelContainer
   - `Card XP Slider` → Drag "Slider" from CardLevelContainer

### Option 3: Context Menu (Editor Only)

1. **Select** CardPrefab root in prefab editing mode
2. **Find** `CardDisplay` component
3. **Right-click** component header → **"Auto-Assign References"**
4. **Check** console for confirmation logs

---

## Verification Checklist

### CardPrefab.prefab
- [ ] Has `CardLevelContainer` GameObject
- [ ] `CardLevelContainer` has `Text (TMP)` child
- [ ] `CardLevelContainer` has `Slider` child
- [ ] Slider has `Fill` GameObject with Image component
- [ ] Slider Min=0, Max=1, Interactable=off
- [ ] Text (TMP) positioned at top of card
- [ ] Slider positioned below level text or at card top

### CardPrefab_combat.prefab
- [ ] Same structure as CardPrefab
- [ ] Components are children of `VisualRoot `
- [ ] Naming matches exactly (case-sensitive)

### Testing Auto-Assignment

**Test in Play Mode:**
1. Start game
2. Navigate to EquipmentScreen with a character
3. Check Console for logs:
   - `"[CardDisplay] Auto-assigned cardLevelText"`
   - `"[CardDisplay] Auto-assigned cardXPSlider"`
4. Look at card in carousel - should show level and XP bar

---

## Visual Design Recommendations

### CardLevel Text

**Recommended Settings:**
```
Font: Your card title font
Size: 10-14pt
Color: White or Gold
Alignment: Center
Position: Top-right or top-left of card
Background: Optional semi-transparent panel
```

**Visual Examples:**

**Option 1: Top-Right Badge**
```
┌─────────────────────┐
│ Heavy Strike  [Lv.5]│ ← Badge style
│                     │
```

**Option 2: Top-Left Corner**
```
┌─────────────────────┐
│[Lv.5]  Heavy Strike │ ← Corner style
│                     │
```

**Option 3: Below Name**
```
┌─────────────────────┐
│   Heavy Strike      │
│      Lv. 5          │ ← Centered below name
```

### XP Slider

**Recommended Settings:**
```
Direction: Horizontal (left to right)
Height: 4-6 pixels (thin bar)
Background Color: Dark gray (0.2, 0.2, 0.2, 0.5)
Fill Color: Gold (1.0, 0.84, 0.0, 1.0)
Border: 1px black outline (optional)
```

**Visual Examples:**

**Style 1: Under Level Text**
```
┌─────────────────────┐
│ Heavy Strike  Lv.5  │
│ ████████░░░░░░░     │ ← 60% to level 6
│                     │
│   [Card Art]        │
```

**Style 2: Integrated with Level Container**
```
┌─────────────────────┐
│  ┌─────────────┐    │
│  │ Lv. 5       │    │ ← Container with text
│  │ ████░░░░░░  │    │ ← and XP bar
│  └─────────────┘    │
│   [Card Art]        │
```

**Style 3: Bottom of Card**
```
┌─────────────────────┐
│   Heavy Strike      │
│   [Card Art]        │
│   Description       │
│ Lv.5 ████░░░░░░░    │ ← Level + bar at bottom
└─────────────────────┘
```

---

## Slider Fill Colors

### Recommended Color Schemes

**1. Gold (Default):**
- Empty: Dark gray (0.2, 0.2, 0.2)
- Fill: Gold (1.0, 0.84, 0.0)
- Good for general leveling

**2. Level-Based Gradient:**
```csharp
// In CardDisplay.SetCard() - advanced option
if (cardXPSlider != null && cardXPSlider.fillRect != null)
{
    Image fill = cardXPSlider.fillRect.GetComponent<Image>();
    if (fill != null)
    {
        // Color by level tier
        if (card.cardLevel <= 5)
            fill.color = Color.white; // White (beginner)
        else if (card.cardLevel <= 10)
            fill.color = Color.green; // Green (intermediate)
        else if (card.cardLevel <= 15)
            fill.color = Color.cyan; // Cyan (advanced)
        else
            fill.color = new Color(1f, 0.84f, 0f); // Gold (expert)
    }
}
```

**3. Rarity-Based:**
- Common cards: Gray fill
- Magic cards: Blue fill
- Rare cards: Purple fill
- Unique cards: Orange/gold fill

---

## Testing the Setup

### Test 1: Visual Components Exist

**In Unity Editor:**
1. Open `CardPrefab` in Prefab mode
2. Expand hierarchy
3. Verify:
   - ✅ CardLevelContainer exists
   - ✅ Text (TMP) child exists
   - ✅ Slider child exists
   - ✅ Slider has Fill GameObject

### Test 2: Auto-Assignment Works

**In Play Mode:**
1. Start game
2. Create a new character
3. Navigate to EquipmentScreen
4. **Check Console** for:
```
[CardDisplay] Auto-assigned cardLevelText
[CardDisplay] Auto-assigned cardXPSlider
```

If you see warnings instead, check naming and hierarchy.

### Test 3: Level Display Works

**Runtime Test:**
```csharp
// In Unity Console or test script:
Card testCard = new Card();
testCard.cardName = "Heavy Strike";
testCard.cardLevel = 5;
testCard.cardExperience = 75;

// Get required XP for level 6
int required = testCard.GetRequiredExperienceForNextLevel();
Debug.Log($"Progress: {testCard.cardExperience}/{required}");

// Display the card
cardDisplay.SetCard(testCard);

// Expected:
// - CardLevel text shows: "Lv. 5"
// - XP slider shows: 75/required (e.g., 75/175 = 43%)
```

### Test 4: XP Slider Updates

**Create Test Cards at Different Levels:**

```csharp
// Level 1, no XP
Card lvl1 = CreateTestCard();
lvl1.cardLevel = 1;
lvl1.cardExperience = 0;
cardDisplay.SetCard(lvl1);
// Expected: Slider = 0% filled, Level text hidden

// Level 5, half to next level
Card lvl5 = CreateTestCard();
lvl5.cardLevel = 5;
lvl5.cardExperience = 88; // Half of 175 required
cardDisplay.SetCard(lvl5);
// Expected: Slider = 50% filled, "Lv. 5" shown

// Level 20 (max)
Card lvl20 = CreateTestCard();
lvl20.cardLevel = 20;
lvl20.cardExperience = 0;
cardDisplay.SetCard(lvl20);
// Expected: Slider hidden, "Lv. 20" shown
```

---

## Scene Integration

### No Scene Changes Needed!

The `CardExperienceManager` is auto-created when needed. However, you can optionally add it manually for easier debugging:

### Optional: Manual Manager Setup

**Create Manager in Scene:**
1. Create Empty GameObject: `CardExperienceManager`
2. Attach `CardExperienceManager.cs` component
3. The singleton will use this instance instead of creating a new one

**Benefits of Manual Setup:**
- Easier to view in Hierarchy during debugging
- Can add to DontDestroyOnLoad scene for organization
- Can set breakpoints on the component

**Location:**
Any scene works, but recommended:
- **MainGameUI** scene (main hub)
- Or **DontDestroyOnLoad** pseudo-scene

---

## Card Prefab Component Reference

### CardDisplay Component Fields

When you select CardPrefab and view the CardDisplay component, you should see:

```
CardDisplay (Script)
├── [References]
│   ├── Card Image: (auto-assigned)
│   ├── Category Icon: (auto-assigned)
│   ├── Card Name Text: (auto-assigned)
│   ├── Card Description Text: (auto-assigned)
│   ├── Additional Effect Text: (auto-assigned)
│   ├── Embossing Slot Container: (auto-assigned)
│   ├── Card Level Text: (auto-assigned) ← NEW
│   └── Card XP Slider: (auto-assigned) ← NEW
└── [Visual Settings]
    ├── Default Card Sprite
    └── Visual Assets
```

**If "auto-assigned" didn't work:**
- Manually drag components from hierarchy
- Verify naming matches exactly
- Check console for warnings

---

## CardLevelContainer Setup Details

### Hierarchy

Your current structure (already created):
```
VisualRoot (or card root)
├── CardLevelContainer (GameObject)
│   └── Text (TMP) (TextMeshProUGUI) ← Shows "Lv. 5"
│
└── CardXpSlider (GameObject with Slider component)
    ├── Background (Image)
    └── Fill Area (RectTransform)
        └── Fill (Image)
```

**Separate containers for visual clarity:**
- `CardLevelContainer` - Just the level text
- `CardXpSlider` - Just the XP progress bar

### Text (TMP) Settings

**Component:** TextMeshProUGUI

**Properties:**
- **Text:** `"Lv.\n20"` (default/placeholder, will be overwritten)
- **Font Size:** 10-12
- **Alignment:** Center/Middle
- **Color:** White (#FFFFFF)
- **Auto-Size:** Enabled (optional)
- **Overflow:** Truncate or Ellipsis

**RectTransform:**
- **Anchors:** Depends on your design
- **Size:** 30×30 or 40×20
- **Position:** Top of CardLevelContainer

### Slider Settings

**Component:** UI Slider

**Properties:**
- **Interactable:** ☐ Unchecked (visual only)
- **Transition:** None
- **Direction:** Left to Right
- **Min Value:** 0
- **Max Value:** 1
- **Whole Numbers:** ☐ Unchecked
- **Value:** 0 (will be set dynamically)

**Fill Rect:**
- Must be assigned to `Fill` GameObject
- Fill image color: Gold (1.0, 0.84, 0.0, 1.0)

**Background:**
- Optional background image
- Color: Dark gray (0.2, 0.2, 0.2, 0.5)

**RectTransform:**
- **Anchors:** Stretch horizontally
- **Height:** 4-6 pixels
- **Padding:** 5px left/right (use Fill Area settings)

---

## Runtime Behavior

### When Card is Displayed

```csharp
CardDisplay.SetCard(card)
    ↓
1. Set CardLevel Text:
   - If level > 1: Show "Lv. {level}"
   - If level = 1: Hide text
    ↓
2. Set XP Slider:
   - If level < 20: Show progress (0.0 - 1.0)
   - If level = 20: Hide slider (max level)
    ↓
3. Display embossing slots
4. Display card data
```

### XP Slider Value Calculation

```csharp
// Example: Level 5 card with 88 XP
card.cardLevel = 5;
card.cardExperience = 88;
int required = card.GetRequiredExperienceForNextLevel(); // = 175

float progress = 88 / 175 = 0.503
slider.value = 0.503 // 50.3% filled
```

### Visual States

**State 1: Level 1, 0 XP**
```
[        ] ← Empty bar, no level text
```

**State 2: Level 5, 50% to Level 6**
```
Lv. 5
[████░░░░] ← Half filled
```

**State 3: Level 5, 90% to Level 6**
```
Lv. 5
[████████░] ← Almost full
```

**State 4: Level 20 (Max)**
```
Lv. 20
(no bar shown) ← Slider hidden at max level
```

---

## Troubleshooting

### Level Text Not Showing

**Check:**
1. `cardLevelText` is assigned in CardDisplay
2. Card has `cardLevel > 1` (Level 1 hides text)
3. Text component is enabled
4. Text color is visible against background

**Debug:**
```csharp
Debug.Log($"Card Level: {card.cardLevel}");
Debug.Log($"Level Text Assigned: {cardLevelText != null}");
```

### XP Slider Not Showing

**Check:**
1. `cardXPSlider` is assigned in CardDisplay
2. Card has `cardLevel < 20` (Level 20 hides slider)
3. Slider GameObject is enabled
4. Fill Rect is assigned in Slider component

**Debug:**
```csharp
Debug.Log($"Card Level: {card.cardLevel}");
Debug.Log($"XP: {card.cardExperience}/{card.GetRequiredExperienceForNextLevel()}");
Debug.Log($"Slider Assigned: {cardXPSlider != null}");
```

### Slider Not Filling

**Check:**
1. Slider's `Fill Rect` field is assigned
2. Fill Rect references the `Fill` GameObject
3. Fill image has a sprite or color
4. Slider value is being set (0.0 - 1.0)

**Fix:**
- Select Slider component
- Assign `Fill Rect` to `Fill Area/Fill`
- Ensure Fill has Image component with color

### Auto-Assignment Failed

**Check Console for:**
- `"[CardDisplay] Could not find..."`
- Warnings about missing components

**Solution:**
1. Verify naming is exact: `"Slider"`, `"CardLevelContainer"`, `"Text (TMP)"`
2. Check hierarchy structure matches guide
3. Manually assign in Inspector if needed

---

## Advanced: Syncing Card Levels from Manager

### Load Card Level from CardExperienceManager

If you want cards to display their saved level from the manager:

```csharp
// When creating card display
void SetCard(Card card)
{
    // Get saved level from manager (per groupKey)
    string groupKey = card.GetGroupKey();
    int savedLevel = CardExperienceManager.Instance.GetCardLevel(groupKey);
    int savedXP = CardExperienceManager.Instance.GetCardExperience(groupKey);
    
    // Update card instance with saved data
    card.cardLevel = savedLevel;
    card.cardExperience = savedXP;
    
    // Display normally
    // ... rest of SetCard code ...
}
```

**Note:** The system already syncs levels automatically through `CardExperienceManager.SyncLevelToActiveCards()`.

---

## Summary

### What You've Already Done ✓
- ✅ Created `CardLevelContainer` in both prefabs
- ✅ Added `Text (TMP)` for level display
- ✅ Added `Slider` for XP progress

### What Happens Automatically ✓
- ✅ `CardDisplay` finds and wires up components
- ✅ Level text updates when card is displayed
- ✅ XP slider fills based on progress
- ✅ Components hide/show based on level

### What You Need to Do
- ✅ **Nothing!** The system is ready to use.

### Optional Enhancements
- Customize slider colors
- Adjust text positioning
- Add level text background panel
- Color-code by level tier

---

## Quick Reference

### Component Names (Must Match)

| Component | Name in Hierarchy | Type |
|-----------|------------------|------|
| Level Text | `CardLevel` or `CardLevelContainer/Text (TMP)` | TextMeshProUGUI |
| XP Slider | `Slider` or `CardLevelContainer/Slider` | UI Slider |
| Embossing Container | `EmbossingSlots` | Transform |

### Slider Configuration

```
Min Value: 0
Max Value: 1
Whole Numbers: Off
Interactable: Off
Direction: Left to Right
```

### Auto-Assignment Paths

CardDisplay searches in this order:
1. Direct child: `transform.Find("ComponentName")`
2. In container: `transform.Find("Container/ComponentName")`
3. In VisualRoot: `visualRoot.Find("ComponentName")`

---

## Next Steps

With prefabs set up, the system will:

1. ✅ **Display card levels** on all cards (when level > 1)
2. ✅ **Show XP progress** visually with slider
3. ✅ **Auto-update** as cards gain XP in combat
4. ✅ **Hide components** when not needed (Level 1 or Level 20)

**You're ready to test the leveling system in-game!**

When cards gain XP in combat, you'll see the slider fill up, and when they level up, the level text will update automatically.

---

## Related Documentation

- `CARD_LEVELING_SYSTEM.md` - Complete leveling system documentation
- `CardExperienceManager.cs` - Manager implementation
- `CardDisplay.cs` - Display component with level/XP support

