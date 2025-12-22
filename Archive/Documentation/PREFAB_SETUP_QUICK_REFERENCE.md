# Card Prefab Setup - Quick Reference

## Your Current Prefab Structure

Based on your setup with **separate containers** for level and XP:

```
CardPrefab (or CardPrefab_combat)
└── VisualRoot  (or root for CardPrefab)
    ├── CardLevelContainer ← Level text container
    │   └── Text (TMP) ← Shows "Lv. 5", "Lv. 20"
    │
    ├── CardXpSlider ← XP progress container
    │   └── (Slider component attached to this GameObject)
    │       ├── Background
    │       └── Fill Area
    │           └── Fill
    │
    ├── CardName
    ├── CardImage
    ├── DescriptionText
    ├── AdditionalEffectText
    │
    └── EmbossingSlots ← Embossing system
        ├── Slot1Container
        │   ├── Slot1Embossing
        │   └── Slot1Filled
        ├── Slot2Container
        │   ├── Slot2Embossing
        │   └── Slot2Filled
        ├── Slot3Container
        │   ├── Slot3Embossing
        │   └── Slot3Filled
        ├── Slot4Container
        │   ├── Slot4Embossing
        │   └── Slot4Filled
        └── Slot5Container
            ├── Slot5Embossing
            └── Slot5Filled
```

---

## Auto-Assignment Rules

### CardDisplay Component Searches For:

| Field | Searches For | Type |
|-------|-------------|------|
| `cardLevelText` | `CardLevelContainer/Text (TMP)` | TextMeshProUGUI |
| `cardXPSlider` | `CardXpSlider` (with Slider component) | UI.Slider |
| `embossingSlotContainer` | `EmbossingSlots` | Transform |

**All automatic - no manual assignment needed!**

---

## Component Configuration

### 1. CardLevelContainer

**GameObject Settings:**
- Name: `CardLevelContainer`
- Position: Top-right or top-left of card
- Size: 40×20 or similar

**Child: Text (TMP)**
- Name: Exactly `Text (TMP)`
- Component: TextMeshProUGUI
- Text: `"Lv.\n20"` (placeholder, will be overwritten)
- Font Size: 10-12
- Alignment: Center
- Color: White or Gold

### 2. CardXpSlider

**GameObject Settings:**
- Name: `CardXpSlider`
- **Has Slider Component** attached directly to this GameObject
- Position: Below CardLevelContainer or at card top
- Width: Stretch across card (with padding)
- Height: 4-8 pixels

**Slider Component:**
- Direction: Left to Right
- Min Value: 0
- Max Value: 1
- Whole Numbers: Off
- Interactable: Off
- Fill Rect: Assigned to `Fill` GameObject

**Background (Image):**
- Color: Dark gray (0.2, 0.2, 0.2, 0.5)
- Sprite: Optional

**Fill (Image):**
- Color: Gold (1.0, 0.84, 0.0, 1.0)
- Sprite: Unity default or custom bar

---

## Naming Requirements (Case-Sensitive)

**Must be exact:**
- ✅ `CardLevelContainer` (not CardLevelcontainer or card_level_container)
- ✅ `Text (TMP)` (not Text or CardLevelText)
- ✅ `CardXpSlider` (not CardXPSlider or CardExpSlider)
- ✅ `EmbossingSlots` (not embossingSlots or EmbossSlots)
- ✅ `Slot1Container`, `Slot2Container`, etc. (not Slot1 or SlotContainer1)

---

## Visual Layout Example

```
┌─────────────────────────┐
│ Heavy Strike      Lv. 5 │ ← CardLevelContainer (top-right)
│ ████████░░░░░░░░░░░     │ ← CardXpSlider (below, full width)
│                         │
│     [Card Art]          │
│                         │
│ Deal 8 physical damage  │
│                         │
│     ◉ ◉ ○              │ ← EmbossingSlots (bottom)
└─────────────────────────┘

Legend:
Lv. 5 = Card is level 5 (+2.6% bonus)
████████░ = 60% XP progress to level 6
◉ = Filled embossing slot
○ = Empty embossing slot
```

---

## Runtime Behavior

### Level 1 Card (New/Starter)
```
Card: Heavy Strike
Level: 1
XP: 0/100

Display:
┌─────────────────────┐
│ Heavy Strike        │ ← No level text shown
│ ░░░░░░░░░░░░░░░     │ ← XP bar empty
│   [Card Art]        │
└─────────────────────┘
```

### Level 5 Card (Mid-Level)
```
Card: Heavy Strike
Level: 5
XP: 88/175

Display:
┌─────────────────────┐
│ Heavy Strike  Lv. 5 │ ← Level text visible
│ ████████░░░░░░░     │ ← XP bar 50% filled
│   [Card Art]        │
└─────────────────────┘
```

### Level 20 Card (Max Level)
```
Card: Heavy Strike
Level: 20
XP: n/a (max)

Display:
┌─────────────────────┐
│ Heavy Strike Lv. 20 │ ← Level text visible (max)
│ (no XP bar)         │ ← Slider hidden
│   [Card Art]        │
└─────────────────────┘
```

---

## Verification Steps

### Step 1: Check Prefab Structure
- [ ] Open CardPrefab in Prefab mode
- [ ] Verify `CardLevelContainer` exists in VisualRoot
- [ ] Verify `CardXpSlider` exists in VisualRoot (separate from CardLevelContainer)
- [ ] Verify `Text (TMP)` is child of CardLevelContainer
- [ ] Verify Slider component is on CardXpSlider GameObject

### Step 2: Check Slider Configuration
- [ ] Select CardXpSlider
- [ ] Verify Slider component exists
- [ ] Check Min=0, Max=1
- [ ] Check Interactable is OFF
- [ ] Verify Fill Rect is assigned

### Step 3: Test Auto-Assignment
- [ ] Start Play Mode
- [ ] Navigate to EquipmentScreen
- [ ] Check Console for auto-assignment logs
- [ ] Should see: "Auto-assigned cardLevelText"
- [ ] Should see: "Auto-assigned cardXPSlider"

### Step 4: Visual Test
- [ ] Look at cards in carousel
- [ ] Should see level text (if cards are leveled)
- [ ] Should see XP bars (if not max level)
- [ ] Bars should fill as cards gain XP

---

## Common Issues & Solutions

### Issue: "Could not find CardLevelContainer"

**Solution:**
- Check spelling exactly: `CardLevelContainer` (capital C, capital L, capital C)
- Ensure it's a child of VisualRoot (for CardPrefab_combat)
- Or direct child of root (for CardPrefab)

### Issue: "Could not find CardXpSlider"

**Solution:**
- Check spelling exactly: `CardXpSlider` (capital C, capital X, lowercase p, capital S)
- Ensure it's a child of VisualRoot (for CardPrefab_combat)
- Verify Slider component is attached to this GameObject

### Issue: XP Bar Not Filling

**Solution:**
- Verify Slider's Fill Rect is assigned
- Check Fill GameObject has Image component
- Ensure Image has a color or sprite
- Test with `slider.value = 0.5f;` manually

### Issue: Level Text Shows "Lv.\n20" Placeholder

**Solution:**
- CardDisplay.SetCard() should overwrite this
- Check if SetCard() is being called
- Verify cardLevelText reference is assigned

---

## Alternative Slider Configurations

### Option 1: Slider on CardXpSlider GameObject (Your Setup)

```
CardXpSlider (has Slider component) ← Recommended
└── Fill Area
    └── Fill
```

**Code searches:**
```csharp
sliderContainer.GetComponent<Slider>() // Finds it directly
```

### Option 2: Slider as Child

```
CardXpSlider (container)
└── Slider (has Slider component)
    └── Fill Area
        └── Fill
```

**Code searches:**
```csharp
sliderContainer.Find("Slider").GetComponent<Slider>() // Also supported
```

Both work! The code handles both cases automatically.

---

## Summary

### Your Setup (Confirmed):
✅ `CardLevelContainer` - Contains level text  
✅ `CardXpSlider` - Contains XP slider  
✅ Separate GameObjects for visual control  
✅ Auto-assignment supports your structure  

### What Works Automatically:
✅ Component auto-detection  
✅ Level text updates  
✅ XP bar fills based on progress  
✅ Components hide/show as needed  

### What You Need to Do:
✅ **Nothing!** Your prefabs are set up correctly.

Just make sure the naming is exact (case-sensitive) and the Slider component is configured with Min=0, Max=1.

**Ready to test in-game!** 🎮

