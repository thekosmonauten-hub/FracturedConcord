# Character Creation - Visual Layout Guide

## Screen Layout

```
┌─────────────────────────────────────────────────────────────────┐
│                    CHARACTER CREATION                            │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Character Name                                           │  │
│  │  [ Enter Character Name                                ] │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Choose Your Class                                        │  │
│  │  ┌────────┐  ┌────────┐  ┌────────┐                     │  │
│  │  │ WITCH  │  │MARAUDER│  │ RANGER │                     │  │
│  │  │ Int... │  │ Str... │  │ Dex... │                     │  │
│  │  └────────┘  └────────┘  └────────┘                     │  │
│  │  ┌────────┐  ┌────────┐  ┌────────┐                     │  │
│  │  │ THIEF  │  │ APOSTLE│  │BRAWLER │                     │  │
│  │  │ Dex/Int│  │ Str/Int│  │ Str/Dex│                     │  │
│  │  └────────┘  └────────┘  └────────┘                     │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Starter Deck Preview                                     │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌─────────┐│ │  │
│  │  │ │ x5 Strike│ │ x4 Defend│ │ x1  Bash │ │x2 Armour││ │  │
│  │  │ └──────────┘ └──────────┘ └──────────┘ └─────────┘│ │  │
│  │  │ ┌──────────┐                             ↑          │ │  │
│  │  │ │ x3 Block │                             │          │ │  │
│  │  │ └──────────┘                     Hover → Shows Full│ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌────────────────────┐           ┌──────────────────────────┐ │
│  │ Back to Main Menu  │           │  Create Character        │ │
│  └────────────────────┘           └──────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Hover Card Preview

When you hover over a deck card row:

```
     Deck Preview                    →           Hover Preview
┌─────────────────────┐                    ┌──────────────────┐
│ x5 Strike      ←────┼─── Hover Here      │                  │
└─────────────────────┘                    │   ╔════════════╗ │
                                           │   ║   STRIKE   ║ │
                                           │   ║            ║ │
                                           │   ║  [Card     ║ │
                                           │   ║   Image]   ║ │
                                           │   ║            ║ │
                                           │   ║  Deal 6    ║ │
                                           │   ║  damage    ║ │
                                           │   ║            ║ │
                                           │   ║  Cost: 1   ║ │
                                           │   ╚════════════╝ │
                                           └──────────────────┘
                                             Full Card Preview
                                             (CardPrefab.prefab)
```

## Component Hierarchy

### UI Toolkit Side (UI Document)
```
UIDocument (CharacterCreationUI)
└── main-container
    └── Background
        ├── name-section
        │   ├── "Character Name" (Label)
        │   └── CharacterNameInput (TextField)
        ├── class-section
        │   ├── "Choose Your Class" (Label)
        │   └── class-grid
        │       ├── WitchButton
        │       ├── MarauderButton
        │       ├── RangerButton
        │       ├── ThiefButton
        │       ├── ApostleButton
        │       └── BrawlerButton
        ├── preview-section
        │   ├── "Starter Deck Preview" (Label)
        │   └── StarterDeckPreview (ScrollView)
        │       └── DeckCardGrid (VisualElement) ← POPULATED DYNAMICALLY
        │           ├── deck-preview-row (x5 Strike)
        │           ├── deck-preview-row (x4 Defend)
        │           └── ... (more cards)
        └── action-buttons
            ├── BackButton
            └── CreateCharacterButton
```

### UGUI Side (Canvas)
```
CardPreviewCanvas (Canvas - Sort Order: 100)
└── CardHoverPreviewParent (RectTransform)
    └── HoverPreview_[CardName] ← CREATED ON HOVER
        └── CardPrefab instance
            ├── CardBackground
            ├── CardImage
            ├── CardName (Text)
            ├── Description (Text)
            ├── CostBubble
            └── ... (all card UI elements)
```

## Data Flow

### When Class is Selected
```
User clicks class button
       ↓
OnClassSelected("Marauder")
       ↓
UpdateStarterDeckPreview()
       ↓
StarterDeckManager.GetDefinitionForClass("Marauder")
       ↓
MarauderStarterDeck.asset loaded
       ↓
For each CardEntry in deck:
  Create VisualElement row
  Register hover callbacks
  Add to DeckCardGrid
       ↓
Deck preview displayed!
```

### When Card is Hovered
```
Mouse enters card row
       ↓
OnCardRowHoverEnter(cardData, cardRow)
       ↓
Instantiate(fullCardPrefab, cardHoverPreviewParent)
       ↓
Get DeckBuilderCardUI component
       ↓
cardUI.Initialize(cardData, null, tempCharacter)
       ↓
Card renders with full details
       ↓
Position and scale card
       ↓
SetActive(true) + SetAsLastSibling()
       ↓
Full card preview visible!
```

### When Hover Ends
```
Mouse leaves card row
       ↓
OnCardRowHoverExit()
       ↓
Destroy(currentHoverCard)
       ↓
currentHoverCard = null
       ↓
Card preview hidden!
```

## File Locations

### UI Toolkit Files
```
Assets/UI/
├── CharacterCreationUI.uxml    ← Layout structure
└── CharacterCreationUI.uss     ← Styling (USE USS, NOT CSS!)
```

### Scripts
```
Assets/Scripts/UI/
└── CharacterCreationController.cs    ← Main controller logic
```

### Prefabs
```
Assets/Art/CardArt/
├── CardPrefab.prefab          ← Full card (REQUIRED for hover)
└── DeckCardPrefab.prefab      ← Compact card (optional)
```

### Starter Deck Definitions
```
Assets/Resources/StarterDecks/
├── MarauderStarterDeck.asset   ← Create if missing
├── WitchStarterDeck.asset      ← Create if missing
├── RangerStarterDeck.asset     ← Create if missing
├── ThiefStarterDeck.asset      ← Create if missing
├── ApostleStarterDeck.asset    ← Create if missing
└── BrawlerStarterDeck.asset    ← Create if missing
```

## Inspector Checklist

### CharacterCreationController Component
```
┌─────────────────────────────────────────────┐
│ Character Creation Controller (Script)      │
├─────────────────────────────────────────────┤
│ UI References                               │
│   UI Document: [CharacterCreationUI]        │
│                                              │
│ Card Prefabs                                │
│   Deck Card Prefab: [DeckCardPrefab]       │
│   Full Card Prefab: [CardPrefab] ← ASSIGN! │
│                                              │
│ UGUI References                             │
│   Card Preview Canvas: [Canvas] ← ASSIGN!   │
│   Card Hover Preview Parent: [Auto-created] │
│                                              │
│ Deck Management                             │
│   Starter Deck Manager: [Auto-found]        │
└─────────────────────────────────────────────┘
```

## Quick Test Steps

1. ✅ Open Character Creation scene
2. ✅ Enter Play Mode
3. ✅ Click "MARAUDER" button
4. ✅ See deck preview appear with cards like "x5 Strike", "x4 Defend"
5. ✅ Hover over "x5 Strike" row
6. ✅ See full Strike card appear on the right side
7. ✅ Move mouse away from row
8. ✅ See full card disappear
9. ✅ Repeat for other cards
10. ✅ Switch to different class (e.g., "WITCH")
11. ✅ See deck preview update with different cards
12. ✅ Test hover on new cards

## Common Issues & Fixes

### Issue: "DeckCardGrid not found!"
```
Fix: UXML file not properly updated. Verify CharacterCreationUI.uxml has:
<ui:VisualElement name="DeckCardGrid" class="deck-card-grid" />
```

### Issue: Deck preview has no cards
```
Fix: Check console for "No starter deck definition found"
Create StarterDeckDefinition asset for that class
```

### Issue: Hover card appears at wrong location
```
Fix: Adjust cardHoverPreviewParent position in SetupUI():
rt.anchoredPosition = new Vector2(300f, 0f);  // Try different values
```

### Issue: Hover doesn't work
```
Fix 1: Ensure cardPreviewCanvas has Sort Order 100+
Fix 2: Verify fullCardPrefab is assigned
Fix 3: Check that CardPrefab.prefab has DeckBuilderCardUI component
```

## Performance Notes

- **Memory Efficient**: Cards only instantiated on hover, destroyed on exit
- **No Pooling Needed**: Hover events are infrequent
- **Automatic Cleanup**: `OnDestroy()` cleans up all preview cards
- **Lazy Loading**: Deck definitions loaded from Resources only when needed

## Next Steps

Once the basic system is working, consider:
1. **Add Animations**: Fade in/out on hover
2. **Sound Effects**: Subtle sound on hover enter
3. **Deck Statistics**: Show total cards, avg cost, etc.
4. **Advanced Tooltips**: Show card synergies or combos
5. **Card Grouping**: Group by type (Attack, Guard, Skill)

---

**You're all set!** The code is ready. Just complete the Unity Editor setup steps above and you'll have a fully interactive deck preview system! 🎮












