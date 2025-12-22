# Card Embossing Slots System

## Overview

The embossing slot system provides visual indicators on cards showing how many embossing slots they have and which slots are filled. This gives players immediate feedback about embossing capacity before implementing the full embossing feature.

**Created:** November 2, 2025  
**Purpose:** Prepare card visuals for embossing system implementation

---

## Implementation

### Card Data Structures

#### CardData (Base)
```csharp
[Header("Embossing System")]
[Range(0, 5)]
public int embossingSlots = 1;
```

#### Card (Runtime)
```csharp
[Header("Embossing System")]
public int embossingSlots = 1;
public List<string> appliedEmbossings = new List<string>();
```

**Properties:**
- `embossingSlots` - Number of total slots (0-3)
- `appliedEmbossings` - List of applied embossing effect IDs

---

## Helper Methods

### Card.cs Methods

```csharp
// Check if card has any empty slots
bool HasEmptyEmbossingSlot()

// Get number of empty slots
int GetEmptySlotCount()

// Get number of filled slots
int GetFilledSlotCount()

// Check if card can be embossed
bool CanEmboss()
```

### Usage Example

```csharp
Card card = GetCard();

if (card.CanEmboss())
{
    int available = card.GetEmptySlotCount();
    int filled = card.GetFilledSlotCount();
    
    Debug.Log($"{card.cardName}: {filled}/{card.embossingSlots} slots filled");
    Debug.Log($"Can add {available} more embossings");
}
```

---

## Visual Display System

### CardDisplay Component

The `CardDisplay` component automatically displays embossing slots when a card is set:

```csharp
void DisplayEmbossingSlots(Card card)
{
    // Clears old slots
    // Creates slot indicators based on card.embossingSlots
    // Shows filled vs empty slots with different colors
}
```

### Slot Visual Styles

**Empty Slot:**
- Color: Dark gray (RGB: 0.3, 0.3, 0.3, Alpha: 0.5)
- Shape: Small circle (20x20 pixels)
- Purpose: Shows available embossing capacity

**Filled Slot:**
- Color: Gold/Amber (RGB: 1.0, 0.84, 0.0, Alpha: 0.9)
- Shape: Small circle (20x20 pixels)
- Purpose: Indicates an embossing is applied

**Outline:**
- Color: Black (Alpha: 0.8)
- Offset: (1, -1)
- Purpose: Improves visibility on any background

---

## Slot Count Guidelines

### By Rarity

| Rarity | Slots | Purpose |
|--------|-------|---------|
| Common | 1 | Basic cards, limited customization |
| Magic | 2 | Slightly better embossing capacity |
| Rare | 3 | Good embossing potential |
| Unique | 4-5 | Maximum embossing capacity |

### Design Philosophy

- **Don't overdo it:** Most cards should have 1-3 slots
- **Unique cards get more:** 4-5 slots reserved for special/legendary cards
- **0 slots possible:** Some cards may not be embossable
- **Balance consideration:** More slots = more powerful when fully embossed
- **Maximum capacity:** 5 slots allows for highly customizable endgame cards

---

## Unity Prefab Structure (Manual Setup)

Both `CardPrefab.prefab` and `CardPrefab_combat.prefab` should have the following structure:

### Required Hierarchy

```
CardPrefab (or VisualRoot in CardPrefab_combat)
└── EmbossingSlots (parent container)
    ├── Slot1Container
    │   ├── Slot1Embossing (sprite for empty slot)
    │   └── Slot1Filled (sprite for filled slot, disabled by default)
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

### How It Works

**SlotXContainer:**
- Active/inactive based on `card.embossingSlots`
- If `card.embossingSlots = 3`, only Slot1, Slot2, Slot3 containers are active

**SlotXEmbossing:**
- Always visible when container is active
- Shows the "empty slot" visual

**SlotXFilled:**
- Disabled by default
- Enabled when `card.appliedEmbossings[X-1]` contains an embossing
- Shows the "filled slot" visual (embossing sprite/effect)

### Visual Reference

```
Card with 3 slots, 2 filled:

┌─────────────────────┐
│   Heavy Strike      │
│                     │
│   [Card Art]        │
│                     │
│   Description       │
│                     │
│   ◉ ◉ ○ [x] [x]    │
│   ^1 ^2 ^3 ^4  ^5  │
└─────────────────────┘

Slot1Container: Active, Slot1Filled: Active
Slot2Container: Active, Slot2Filled: Active  
Slot3Container: Active, Slot3Filled: Inactive
Slot4Container: Inactive
Slot5Container: Inactive
```

---

## Code Logic Flow

### When Card is Displayed

```csharp
CardDisplay.SetCard(card)
  ↓
DisplayEmbossingSlots(card)
  ↓
For each slot (1 to 5):
  ↓
  Find Slot{i}Container
  ↓
  If i <= card.embossingSlots:
    → Activate SlotXContainer
    → Ensure SlotXEmbossing is active (empty slot visual)
    → Check if card.appliedEmbossings[i-1] exists
      → If yes: Enable SlotXFilled (show embossing sprite)
      → If no: Disable SlotXFilled (show empty slot)
  Else:
    → Deactivate SlotXContainer (hide completely)
```

### Examples

**Example 1: Common Card (1 slot, empty)**
```
card.embossingSlots = 1
card.appliedEmbossings = []

Result:
Slot1Container: Active
  Slot1Embossing: Active ✓
  Slot1Filled: Inactive
Slot2-5Container: Inactive
```

**Example 2: Rare Card (3 slots, 2 filled)**
```
card.embossingSlots = 3
card.appliedEmbossings = ["fire_dmg", "strength_bonus"]

Result:
Slot1Container: Active
  Slot1Embossing: Active ✓
  Slot1Filled: Active ✓ (shows fire embossing)
Slot2Container: Active
  Slot2Embossing: Active ✓
  Slot2Filled: Active ✓ (shows strength embossing)
Slot3Container: Active
  Slot3Embossing: Active ✓
  Slot3Filled: Inactive (empty slot)
Slot4-5Container: Inactive
```

**Example 3: Unique Card (5 slots, fully embossed)**
```
card.embossingSlots = 5
card.appliedEmbossings = ["fire", "strength", "speed", "crit", "lifesteal"]

Result:
All 5 SlotXContainer: Active
All 5 SlotXEmbossing: Active ✓
All 5 SlotXFilled: Active ✓ (shows each embossing sprite)
```

---

## Testing the System

### Test Case 1: Empty Slots
```csharp
Card card = new Card();
card.cardName = "Heavy Strike";
card.embossingSlots = 2;
card.appliedEmbossings = new List<string>();

// Display card
cardDisplay.SetCard(card);

// Expected: 2 dark gray circles at bottom of card
```

### Test Case 2: Partially Filled
```csharp
Card card = new Card();
card.cardName = "Heavy Strike";
card.embossingSlots = 3;
card.appliedEmbossings = new List<string> { "emboss_fire", "emboss_strength" };

// Display card
cardDisplay.SetCard(card);

// Expected: 2 gold circles + 1 gray circle
```

### Test Case 3: Fully Filled
```csharp
Card card = new Card();
card.cardName = "Heavy Strike";
card.embossingSlots = 2;
card.appliedEmbossings = new List<string> { "emboss_fire", "emboss_strength" };

// Display card
cardDisplay.SetCard(card);

// Expected: 2 gold circles, no gray circles
```

### Test Case 5: Maximum Slots (Unique Card)
```csharp
Card card = new Card();
card.cardName = "Legendary Blade";
card.embossingSlots = 5;
card.appliedEmbossings = new List<string> { "emboss_fire", "emboss_strength", "emboss_speed" };

// Display card
cardDisplay.SetCard(card);

// Expected: ◉ ◉ ◉ ○ ○ (3 gold, 2 gray)
```

### Test Case 4: No Slots
```csharp
Card card = new Card();
card.cardName = "Basic Strike";
card.embossingSlots = 0;

// Display card
cardDisplay.SetCard(card);

// Expected: No embossing indicators visible
```

---

## CardCarouselUI Integration

The carousel already works with the embossing system. When selecting a card:

```csharp
Card selectedCard = cardCarousel.GetSelectedCard();

if (selectedCard.CanEmboss())
{
    string groupKey = selectedCard.GetGroupKey();
    int copies = cardCarousel.GetSelectedCardCount();
    int availableSlots = selectedCard.GetEmptySlotCount();
    
    Debug.Log($"Card: {selectedCard.cardName}");
    Debug.Log($"Copies in deck: {copies}");
    Debug.Log($"Available embossing slots: {availableSlots}");
    Debug.Log($"Embossing will affect {copies} cards with groupKey: {groupKey}");
}
```

---

## Future Embossing System Integration

### When Implementing Embossing

The system is ready for embossing implementation:

**1. Apply Embossing:**
```csharp
void ApplyEmbossing(string groupKey, string embossingId)
{
    // Find all cards in deck with matching groupKey
    var cards = deck.GetCardsByGroupKey(groupKey);
    
    foreach (var card in cards)
    {
        if (card.CanEmboss())
        {
            card.appliedEmbossings.Add(embossingId);
            
            // Refresh card display
            RefreshCardDisplay(card);
        }
    }
}
```

**2. Remove Embossing:**
```csharp
void RemoveEmbossing(string groupKey, int slotIndex)
{
    var cards = deck.GetCardsByGroupKey(groupKey);
    
    foreach (var card in cards)
    {
        if (slotIndex < card.appliedEmbossings.Count)
        {
            card.appliedEmbossings.RemoveAt(slotIndex);
            RefreshCardDisplay(card);
        }
    }
}
```

**3. Check Embossing:**
```csharp
bool HasEmbossing(Card card, string embossingId)
{
    return card.appliedEmbossings != null && 
           card.appliedEmbossings.Contains(embossingId);
}
```

---

## Design Considerations

### Slot Position

**Recommended Placement:**
- Bottom of card (10-20px from edge)
- Centered horizontally
- Always visible (not covered by other elements)

### Slot Size

**Recommended Dimensions:**
- 20x20 pixels (for normal card size)
- Scale with card size if needed
- Spacing: 5px between slots

### Color Choices

**Empty Slot:**
- Low contrast to not distract
- Semi-transparent to blend with card
- Still visible enough to see slot count

**Filled Slot:**
- High contrast gold/amber
- Clearly indicates "something is here"
- Matches typical RPG enhancement visuals

### Alternative Visual Styles

If you want different visuals:

```csharp
// Option 1: Square slots
slotRect.sizeDelta = new Vector2(15, 15);
// Don't use circle sprite

// Option 2: Gem-like appearance
slotImage.sprite = Resources.Load<Sprite>("UI/GemSlot");

// Option 3: Rarity-based colors
Color slotColor = GetColorByRarity(card.rarity);
slotImage.color = slotColor;
```

---

## Configuration Guide

### Setting Slot Count in Inspector

**For CardDataExtended Assets:**
1. Select card asset in Project window
2. Find "Embossing System" section
3. Adjust "Embossing Slots" slider (0-5)
4. Save asset

**Default Values:**
- Common cards: 1 slot
- Magic cards: 2 slots
- Rare cards: 3 slots
- Unique cards: 4-5 slots

### Runtime Slot Modification

```csharp
// Upgrade card to add slot
card.embossingSlots++;

// Remove slot (if downgrading)
card.embossingSlots = Mathf.Max(0, card.embossingSlots - 1);

// Ensure applied embossings fit
if (card.appliedEmbossings.Count > card.embossingSlots)
{
    card.appliedEmbossings.RemoveRange(
        card.embossingSlots, 
        card.appliedEmbossings.Count - card.embossingSlots
    );
}
```

---

## Troubleshooting

### Slots Not Showing

**Check:**
1. `embossingSlotContainer` is assigned in CardDisplay (should reference the `EmbossingSlots` GameObject)
2. All 5 `SlotXContainer` GameObjects exist in the hierarchy
3. Each SlotContainer has `SlotXEmbossing` and `SlotXFilled` children
4. Card has `embossingSlots > 0`

**Debug Log:**
The system will log: `"[CardDisplay] Could not find Slot1Container..."` if the structure is incorrect.

### Filled Slots Not Appearing

**Check:**
1. `card.appliedEmbossings` is not null
2. List has entries (e.g., `["fire_emboss", "strength_emboss"]`)
3. `SlotXFilled` GameObjects exist in the prefab
4. `SlotXFilled` has an Image component with a sprite assigned

**Testing:**
```csharp
Card testCard = new Card();
testCard.embossingSlots = 3;
testCard.appliedEmbossings = new List<string> { "test_emboss_1", "test_emboss_2" };
cardDisplay.SetCard(testCard);
// Should show: ◉ ◉ ○
```

### Wrong Slots Activated

**Problem:** Slot4 and Slot5 are always active even for common cards

**Solution:**
- Ensure all `SlotXContainer` GameObjects start **enabled** in the prefab
- The code will deactivate them based on `card.embossingSlots`
- Check that slot names match exactly: `"Slot1Container"`, `"Slot2Container"`, etc.

### SlotXFilled Not Found Warning

**Problem:** Console shows "Could not find SlotXFilled"

**Solution:**
- Verify naming is exact: `Slot1Filled`, `Slot2Filled`, etc. (case-sensitive)
- Ensure `SlotXFilled` is a direct child of `SlotXContainer`
- Check the GameObject hierarchy matches the required structure

---

## Prefab Setup Checklist

Use this checklist to verify your prefab setup is correct:

### CardPrefab.prefab
- [ ] Has `EmbossingSlots` GameObject (child of card root)
- [ ] `EmbossingSlots` assigned to CardDisplay's `embossingSlotContainer` field
- [ ] Has all 5 SlotXContainer GameObjects (Slot1Container to Slot5Container)
- [ ] Each SlotXContainer has `SlotXEmbossing` child (visible sprite)
- [ ] Each SlotXContainer has `SlotXFilled` child (disabled by default)
- [ ] All `SlotXFilled` have Image components with embossing sprites assigned

### CardPrefab_combat.prefab
- [ ] Has `EmbossingSlots` GameObject (child of VisualRoot)
- [ ] `EmbossingSlots` assigned to CardDisplay's `embossingSlotContainer` field
- [ ] Has all 5 SlotXContainer GameObjects (Slot1Container to Slot5Container)
- [ ] Each SlotXContainer has `SlotXEmbossing` child (visible sprite)
- [ ] Each SlotXContainer has `SlotXFilled` child (disabled by default)
- [ ] All `SlotXFilled` have Image components with embossing sprites assigned

### Naming Must Be Exact (Case-Sensitive)
- ✅ `EmbossingSlots`
- ✅ `Slot1Container`, `Slot2Container`, ... `Slot5Container`
- ✅ `Slot1Embossing`, `Slot2Embossing`, ... `Slot5Embossing`
- ✅ `Slot1Filled`, `Slot2Filled`, ... `Slot5Filled`

---

## Summary

✅ **Embossing slots added** to CardData, Card, CardDataExtended (0-5 range)  
✅ **Helper methods** for checking slot availability  
✅ **Visual display system** working with manual prefab structure  
✅ **CardDisplay integration** automatically shows/hides slots  
✅ **Supports up to 5 embossings** per card  
✅ **Ready for embossing implementation**  

**Next Step:** Implement the actual embossing system that fills these slots with effects!

---

## Related Documentation

- `CARD_GROUPKEY_SYSTEM.md` - How cards are grouped for embossing
- `CardCarouselUI.cs` - Carousel implementation for card selection
- `CardDisplay.cs` - Card visual display component

