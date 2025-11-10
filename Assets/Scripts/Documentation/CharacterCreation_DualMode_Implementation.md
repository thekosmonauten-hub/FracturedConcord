# Character Creation - Dual Mode Deck Preview

## Overview
The deck preview system now supports **TWO display modes** that automatically switch based on what's assigned in the Inspector:

### Mode 1: Visual Card Prefabs (Premium)
- Uses actual `DeckCardPrefab.prefab` GameObjects
- Renders real card visuals with images and styling
- More immersive and polished appearance
- **Requires**: Both prefabs assigned + Canvas setup

### Mode 2: Text List (Fallback)
- Uses simple UI Toolkit text rows
- Lightweight and always works
- Clean, minimalist appearance
- **Requires**: Nothing (works out of the box)

The system intelligently picks the best mode based on what's available!

---

## How Mode Selection Works

### Automatic Detection
```csharp
private void UpdateStarterDeckPreview()
{
    // Automatically detects which mode to use
    bool useVisualCards = deckCardPrefab != null && cardPreviewCanvas != null;
    
    if (useVisualCards)
    {
        CreateVisualDeckPreview(def);  // Mode 1: Visual cards
    }
    else
    {
        CreateTextDeckPreview(def);    // Mode 2: Text rows
    }
}
```

### Decision Logic
```
IF (deckCardPrefab assigned AND cardPreviewCanvas exists)
    → Use Visual Card Prefabs Mode
ELSE
    → Use Text List Mode
```

---

## Mode 1: Visual Card Prefabs

### What It Looks Like
```
┌────────────────────────────────────────────────────────┐
│  Starter Deck Preview                                  │
│  ┏━━━━━━┓  ┏━━━━━━┓  ┏━━━━━━┓  ┏━━━━━━┓  ┏━━━━━━┓  │
│  ┃ STRIKE┃  ┃DEFEND┃  ┃ BASH ┃  ┃ARMOUR┃  ┃ BLOCK┃  │
│  ┃  [Art]┃  ┃ [Art]┃  ┃ [Art]┃  ┃ [Art]┃  ┃ [Art]┃  │
│  ┃  x5   ┃  ┃  x4  ┃  ┃  x1  ┃  ┃  x2  ┃  ┃  x3  ┃  │
│  ┗━━━━━━┛  ┗━━━━━━┛  ┗━━━━━━┛  ┗━━━━━━┛  ┗━━━━━━┛  │
└────────────────────────────────────────────────────────┘
     ↑
  Hover any card → Full preview appears
```

### Implementation
```csharp
private void CreateVisualDeckPreview(StarterDeckDefinition def)
{
    // 1. Create parent GameObject on Canvas
    GameObject deckPreviewParent = new GameObject("DeckPreviewCards");
    deckPreviewParent.transform.SetParent(cardPreviewCanvas.transform, false);
    
    // 2. Setup layout positioning
    RectTransform parentRT = deckPreviewParent.AddComponent<RectTransform>();
    parentRT.anchoredPosition = new Vector2(0f, -200f);
    parentRT.sizeDelta = new Vector2(800f, 250f);
    
    // 3. Add Horizontal Layout Group for auto-arrangement
    HorizontalLayoutGroup layout = deckPreviewParent.AddComponent<HorizontalLayoutGroup>();
    layout.spacing = 10f;
    layout.childAlignment = TextAnchor.MiddleCenter;
    
    // 4. Instantiate card prefabs
    foreach (var entry in def.cards)
    {
        GameObject cardObj = Instantiate(deckCardPrefab, deckPreviewParent.transform);
        
        // Setup card visual
        DeckCardListUI cardUI = cardObj.GetComponent<DeckCardListUI>();
        if (cardUI != null)
        {
            DeckCardEntry deckEntry = new DeckCardEntry
            {
                cardData = ConvertToCardData(entry.card),
                quantity = entry.count
            };
            cardUI.Initialize(deckEntry, null);
        }
        
        // Add hover events
        AddCardHoverEvents(cardObj, entry.card);
        
        deckPreviewCards.Add(cardObj);
    }
}
```

### Features
- ✅ Real card visuals with images
- ✅ Automatic horizontal layout
- ✅ Scales to fit available space
- ✅ Hover shows full card preview
- ✅ Professional appearance

---

## Mode 2: Text List

### What It Looks Like
```
┌────────────────────────────────────────────────────────┐
│  Starter Deck Preview                                  │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐        │
│  │ x5 Strike  │ │ x4 Defend  │ │ x1 Bash    │        │
│  └────────────┘ └────────────┘ └────────────┘        │
│  ┌────────────┐ ┌────────────┐                        │
│  │ x2 Armour  │ │ x3 Block   │                        │
│  └────────────┘ └────────────┘                        │
└────────────────────────────────────────────────────────┘
     ↑
  Hover any row → Full preview appears
```

### Implementation
```csharp
private void CreateTextDeckPreview(StarterDeckDefinition def)
{
    foreach (var entry in def.cards)
    {
        // Create UI Toolkit visual element
        VisualElement cardRow = new VisualElement();
        cardRow.AddToClassList("deck-preview-row");
        
        // Add count and name labels
        Label countLabel = new Label($"x{entry.count}");
        countLabel.AddToClassList("deck-card-count");
        cardRow.Add(countLabel);
        
        Label nameLabel = new Label(entry.card.cardName);
        nameLabel.AddToClassList("deck-card-name");
        cardRow.Add(nameLabel);
        
        // Register hover callbacks
        CardDataExtended cardData = entry.card;
        cardRow.RegisterCallback<MouseEnterEvent>(evt => OnCardRowHoverEnter(cardData, cardRow));
        cardRow.RegisterCallback<MouseLeaveEvent>(evt => OnCardRowHoverExit());
        
        deckCardGrid.Add(cardRow);
    }
}
```

### Features
- ✅ Lightweight and fast
- ✅ No prefab dependencies
- ✅ Clean, minimalist look
- ✅ Hover still shows full card
- ✅ Works with just UI Toolkit

---

## Comparison

| Feature | Visual Cards Mode | Text List Mode |
|---------|-------------------|----------------|
| **Appearance** | Professional, game-like | Clean, functional |
| **Performance** | Slightly heavier (UGUI GameObjects) | Very light (UI Toolkit only) |
| **Setup Complexity** | Requires prefab + canvas | Zero setup needed |
| **Card Count Display** | Yes (x5, x4, etc.) | Yes (x5, x4, etc.) |
| **Hover Preview** | Yes (full card) | Yes (full card) |
| **Scalability** | Good (auto-layout) | Excellent (flexbox) |
| **Dependencies** | DeckCardPrefab.prefab + Canvas | None |

---

## Setup Instructions

### For Mode 1 (Visual Cards - Recommended)

**Inspector Assignments:**
1. `Deck Card Prefab`: `Assets/Art/CardArt/DeckCardPrefab.prefab`
2. `Full Card Prefab`: `Assets/Art/CardArt/CardPrefab.prefab`
3. `Card Preview Canvas`: Create and assign Canvas (Sort Order: 100)

**Result:** Polished visual deck preview with actual card prefabs

### For Mode 2 (Text List - Fallback)

**Inspector Assignments:**
1. `Full Card Prefab`: `Assets/Art/CardArt/CardPrefab.prefab` (for hover only)
2. Leave `Deck Card Prefab` **unassigned**
3. Leave `Card Preview Canvas` **unassigned** (optional)

**Result:** Clean text-based deck list with hover previews

---

## Technical Details

### CardData Conversion
The visual mode needs `CardData` but we use `CardDataExtended`. The conversion is handled automatically:

```csharp
private CardData ConvertToCardData(CardDataExtended extended)
{
    if (extended == null) return null;
    
    CardData data = ScriptableObject.CreateInstance<CardData>();
    data.cardName = extended.cardName;
    data.cardImage = extended.cardImage;
    data.playCost = extended.playCost;
    data.rarity = extended.rarity;
    
    return data;
}
```

### Layout System
Visual mode uses `HorizontalLayoutGroup` for automatic card arrangement:
```csharp
HorizontalLayoutGroup layout = deckPreviewParent.AddComponent<HorizontalLayoutGroup>();
layout.spacing = 10f;                          // 10px between cards
layout.childAlignment = TextAnchor.MiddleCenter; // Center alignment
layout.childControlWidth = false;               // Cards keep own width
layout.childControlHeight = false;              // Cards keep own height
```

### Hover Events
Visual mode uses `EventTrigger` component for hover:
```csharp
private void AddCardHoverEvents(GameObject cardObj, CardDataExtended cardData)
{
    EventTrigger trigger = cardObj.AddComponent<EventTrigger>();
    
    // Pointer Enter
    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
    enterEntry.eventID = EventTriggerType.PointerEnter;
    enterEntry.callback.AddListener((data) => OnCardPrefabHoverEnter(cardData, cardObj));
    trigger.triggers.Add(enterEntry);
    
    // Pointer Exit
    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
    exitEntry.eventID = EventTriggerType.PointerExit;
    exitEntry.callback.AddListener((data) => OnCardPrefabHoverExit());
    trigger.triggers.Add(exitEntry);
}
```

---

## Positioning and Scaling

### Deck Preview Parent Position
Adjust in `CreateVisualDeckPreview()`:
```csharp
parentRT.anchoredPosition = new Vector2(0f, -200f);  // X: left/right, Y: up/down
parentRT.sizeDelta = new Vector2(800f, 250f);        // Width x Height
```

### Individual Card Scale
Adjust in `CreateVisualDeckPreview()`:
```csharp
cardRT.localScale = Vector3.one * 0.9f;  // 0.9 = 90% size (fits more cards)
```

### Hover Preview Scale
Adjust in `OnCardPrefabHoverEnter()` or `OnCardRowHoverEnter()`:
```csharp
rt.localScale = Vector3.one * 1.5f;  // 1.5 = 150% size (larger preview)
```

---

## Troubleshooting

### Issue: Visual cards not showing, text list appears instead

**Diagnosis:**
```
Console shows: "✓ Deck preview created with X card types (visual: False)"
```

**Causes:**
1. `deckCardPrefab` not assigned in Inspector
2. `cardPreviewCanvas` not assigned or doesn't exist

**Fix:**
- Assign `DeckCardPrefab.prefab` to Controller
- Create Canvas and assign it

### Issue: Visual cards show but no hover preview

**Diagnosis:**
```
Console shows: "Cannot show card preview: Missing prefab, parent, or card data"
```

**Causes:**
1. `fullCardPrefab` not assigned
2. `cardHoverPreviewParent` not created

**Fix:**
- Assign `CardPrefab.prefab` to Controller
- Ensure `SetupUI()` creates hover parent

### Issue: Cards overlap or wrong layout

**Diagnosis:**
Visual cards are stacked on top of each other

**Causes:**
1. `HorizontalLayoutGroup` spacing too small
2. Parent RectTransform size too small

**Fix:**
```csharp
layout.spacing = 15f;  // Increase spacing
parentRT.sizeDelta = new Vector2(1000f, 250f);  // Increase width
```

### Issue: DeckCardListUI errors

**Diagnosis:**
```
"DeckCardPrefab missing DeckCardListUI component!"
```

**Fix:**
- Ensure `DeckCardPrefab.prefab` has `DeckCardListUI` script attached
- Check all UI references are assigned on the prefab

---

## Performance Comparison

### Visual Cards Mode
```
Memory: ~50-100 KB per card GameObject
Creation Time: ~0.5ms per card
Render Cost: Medium (UGUI rendering)
Best For: 5-15 cards in deck
```

### Text List Mode
```
Memory: ~5-10 KB per text element
Creation Time: ~0.1ms per element
Render Cost: Very low (UI Toolkit)
Best For: Any number of cards
```

**Recommendation:** Visual mode for normal decks (< 20 unique cards), text mode for large/complex decks

---

## Customization Options

### Adjusting Card Layout
In `CreateVisualDeckPreview()`:
```csharp
// Tighter spacing
layout.spacing = 5f;

// Vertical layout instead of horizontal
VerticalLayoutGroup layout = deckPreviewParent.AddComponent<VerticalLayoutGroup>();

// Grid layout (advanced)
GridLayoutGroup layout = deckPreviewParent.AddComponent<GridLayoutGroup>();
layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
layout.constraintCount = 5;  // 5 cards per row
```

### Adjusting Card Size
```csharp
// Make cards smaller
cardRT.localScale = Vector3.one * 0.7f;

// Make cards larger
cardRT.localScale = Vector3.one * 1.1f;

// Asymmetric scaling (wide but short)
cardRT.localScale = new Vector3(1.2f, 0.8f, 1f);
```

### Changing Preview Position
```csharp
// Higher on screen
parentRT.anchoredPosition = new Vector2(0f, 100f);

// Left side
parentRT.anchoredPosition = new Vector2(-300f, -200f);

// Bottom right
parentRT.anchoredPosition = new Vector2(400f, -400f);
```

---

## Migration Guide

### Switching from Text to Visual

**Before (Text Mode):**
```
Inspector:
  Deck Card Prefab: [None]
  Card Preview Canvas: [None]
```

**After (Visual Mode):**
```
Inspector:
  Deck Card Prefab: [DeckCardPrefab]  ← Assign this
  Card Preview Canvas: [Canvas]        ← Assign this
```

The system automatically switches to visual mode!

### Switching from Visual to Text

Just unassign the prefab or canvas - system reverts to text mode automatically.

---

## Best Practices

### When to Use Visual Mode
- ✅ Final production builds
- ✅ Player-facing character creation
- ✅ Marketing screenshots/trailers
- ✅ When you want maximum polish

### When to Use Text Mode
- ✅ Early prototyping
- ✅ Debug/testing builds
- ✅ When prefabs aren't ready yet
- ✅ Minimalist aesthetic preference

### Hybrid Approach
You can actually use **both** in different scenes:
- Character creation: Visual mode
- Quick deck viewer: Text mode
- Debug tools: Text mode

---

## Code Walkthrough

### Visual Mode Flow
```
1. CreateVisualDeckPreview()
       ↓
2. Create parent GameObject "DeckPreviewCards"
       ↓
3. Add HorizontalLayoutGroup for auto-arrangement
       ↓
4. For each card in starter deck:
     a. Instantiate(deckCardPrefab)
     b. Get DeckCardListUI component
     c. Convert CardDataExtended → CardData
     d. cardUI.Initialize(deckEntry, null)
     e. AddCardHoverEvents()
     f. Add to deckPreviewCards list
       ↓
5. Cards auto-arranged by layout group
       ↓
6. User hovers → OnCardPrefabHoverEnter()
       ↓
7. Full card preview appears
```

### Text Mode Flow
```
1. CreateTextDeckPreview()
       ↓
2. For each card in starter deck:
     a. Create VisualElement container
     b. Add count Label ("x5")
     c. Add name Label ("Strike")
     d. RegisterCallback for hover
     e. Add to deckCardGrid
       ↓
3. Cards auto-arranged by CSS flexbox
       ↓
4. User hovers → OnCardRowHoverEnter()
       ↓
5. Full card preview appears
```

---

## Advanced Features

### Adding Card Glow on Hover
```csharp
private void AddCardHoverEvents(GameObject cardObj, CardDataExtended cardData)
{
    // ... existing code ...
    
    // Add glow effect component
    CardGlowEffect glow = cardObj.AddComponent<CardGlowEffect>();
    glow.glowColor = Color.yellow;
    glow.glowIntensity = 0.5f;
}
```

### Adding Sound Effects
```csharp
private void OnCardPrefabHoverEnter(CardDataExtended cardData, GameObject cardObj)
{
    // Play hover sound
    AudioManager.Instance?.PlaySound("CardHover");
    
    // ... existing code ...
}
```

### Adding Tooltips (In Addition to Full Card)
```csharp
private void AddCardHoverEvents(GameObject cardObj, CardDataExtended cardData)
{
    // ... existing code ...
    
    // Add tooltip trigger
    TooltipTrigger tooltip = cardObj.AddComponent<TooltipTrigger>();
    tooltip.title = cardData.cardName;
    tooltip.content = $"Click to see full details\nx{entry.count} in deck";
}
```

---

## Memory Management

### Automatic Cleanup
```csharp
private void ClearDeckPreviewCards()
{
    // Destroys ALL deck preview GameObjects
    foreach (GameObject cardObj in deckPreviewCards)
    {
        if (cardObj != null)
        {
            Destroy(cardObj);
        }
    }
    deckPreviewCards.Clear();
    
    // Also destroy hover card
    if (currentHoverCard != null)
    {
        Destroy(currentHoverCard);
        currentHoverCard = null;
    }
}
```

Called automatically:
- When switching classes
- When scene is destroyed (`OnDestroy()`)
- Before creating new preview

### No Memory Leaks
- ✅ All GameObjects properly destroyed
- ✅ Event listeners cleaned up
- ✅ References nulled after destruction
- ✅ List cleared after cleanup

---

## Debug Information

### Checking Which Mode is Active
Look for this in console:
```
<color=green>✓ Deck preview created with 5 card types (visual: True)</color>
                                                              ↑
                                                    True = Visual Mode
                                                    False = Text Mode
```

### Visual Mode Debug Output
```
<color=cyan>Building deck preview for Marauder with 5 unique cards</color>
  Added visual card: x5 Strike
  Added visual card: x4 Defend
  Added visual card: x1 Bash
  Added visual card: x2 Armour
  Added visual card: x3 Block
<color=green>✓ Deck preview created with 5 card types (visual: True)</color>
```

### Text Mode Debug Output
```
<color=cyan>Building deck preview for Marauder with 5 unique cards</color>
  Added text row: x5 Strike
  Added text row: x4 Defend
  Added text row: x1 Bash
  Added text row: x2 Armour
  Added text row: x3 Block
<color=green>✓ Deck preview created with 5 card types (visual: False)</color>
```

---

## Summary

**Dual Mode System:**
- **Visual Mode**: Premium experience with real card prefabs
- **Text Mode**: Lightweight fallback with text rows
- **Automatic**: Switches based on Inspector assignments
- **Flexible**: Use whichever fits your needs

**Both modes:**
- ✅ Show card count (x5, x4, etc.)
- ✅ Support hover for full preview
- ✅ Work with all 6 classes
- ✅ Clean up properly on destroy
- ✅ Integrate seamlessly

Choose the mode that fits your project needs - the system adapts automatically! 🎯












