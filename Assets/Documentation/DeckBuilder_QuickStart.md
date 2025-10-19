# Deck Builder System - Quick Start Guide

## 🚀 5-Minute Setup

### Step 1: Install LeanTween (2 minutes)

1. Open Unity Asset Store
2. Search "LeanTween"
3. Import (free asset)

*Or download from: https://github.com/dentedpixel/LeanTween*

---

### Step 2: Create DeckManager (1 minute)

1. Open your **MainGameUI** scene
2. **GameObject → Create Empty** → Name it `DeckManager`
3. Add Component → `DeckManager`
4. Assign **Card Database** field → Drag `CardDatabase` from Resources folder
5. Done! It will persist automatically.

---

### Step 3: Create CardPrefab (2 minutes)

1. **GameObject → UI → Image** → Name it `CardPrefab`
2. Add Component → `Button`
3. Add Component → `DeckBuilderCardUI`
4. Create children:
   - `CardBackground` (Image)
   - `CardImage` (Image)
   - `CardName` (TextMeshProUGUI)
   - `CostText` (TextMeshProUGUI)
   - `DescriptionText` (TextMeshProUGUI)
   - `QuantityText` (TextMeshProUGUI)
   - `DisabledOverlay` (Image, deactivate)

5. **Assign all children** to `DeckBuilderCardUI` component fields
6. Drag to **Prefabs/UI/DeckBuilder/** folder

---

### Step 4: Create DeckCardPrefab (2 minutes)

Same as Step 3, but simpler:
- `CardBackground` (Image)
- `CardNameText` (TextMeshProUGUI)
- `CostText` (TextMeshProUGUI)
- `QuantityText` (TextMeshProUGUI)

Add `DeckCardListUI` component, assign fields, save as prefab.

---

### Step 5: Create DeckBuilder Scene (Detailed in full guide)

See **DeckBuilder_System_Guide.md** for complete scene setup.

**Minimum viable setup:**
```
Canvas
├── DeckBuilderUI (Empty GameObject + DeckBuilderUI component)
├── CardScrollView (Scroll Rect)
│   └── CardGrid (Grid Layout Group)
├── DeckListScrollView (Scroll Rect)
│   └── DeckListContainer (Vertical Layout Group)
└── Buttons (Done, Back)
```

Assign all references in `DeckBuilderUI` component.

---

## 🎮 Usage

### Opening Deck Builder from Main Menu

```csharp
// In MainMenuController
[SerializeField] private Button deckBuilderButton;

private void Start()
{
    deckBuilderButton.onClick.AddListener(() => 
    {
        SceneManager.LoadScene("DeckBuilder");
    });
}
```

### Loading Active Deck in Combat

```csharp
// In CombatManager.Start() or InitializeCombat()
if (DeckManager.Instance.HasActiveDeck())
{
    List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
    drawPile = new List<Card>(deckCards);
    ShuffleDeck();
}
```

---

## 📝 Deck Rules

- **Min Deck Size**: 20 cards
- **Max Deck Size**: 40 cards
- **Max Copies**: 6 per card (standard)
- **Unique Cards**: 1 copy max

Edit these in `DeckBuilderConstants` class:
```csharp
public static class DeckBuilderConstants
{
    public const int MIN_DECK_SIZE = 20;
    public const int MAX_DECK_SIZE = 40;
    public const int MAX_STANDARD_COPIES = 6;
    public const int MAX_UNIQUE_COPIES = 1;
}
```

---

## 🎨 Customization

### Change Colors

**In DeckBuilderUI:**
- `validDeckColor` → Green indicator
- `invalidDeckColor` → Red indicator

**In DeckBuilderCardUI:**
- `normalColor` → Default card background
- `hoverColor` → Hover highlight
- `disabledColor` → Maxed out cards

**In DeckCardListUI:**
- `rarityColors` array → Name colors by rarity

### Change Animations

**In DeckBuilderCardUI:**
- `hoverScale` → 1.15 (15% larger on hover)
- `animationDuration` → 0.2 seconds

---

## 🛠️ Troubleshooting

### "LeanTween not found"
→ Install LeanTween or remove animation calls

### "CardDatabase is null"
→ Assign CardDatabase in DeckBuilderUI Inspector

### "No cards showing"
→ Check CardDatabase.allCards is populated

### "Deck not persisting"
→ Ensure DeckManager has DontDestroyOnLoad

---

## 📚 Full Documentation

For complete guide, see: **DeckBuilder_System_Guide.md**

Includes:
- Detailed scene setup
- Prefab hierarchy
- Script reference
- Advanced features
- Best practices
- Integration examples

---

## ✅ Testing Checklist

Quick test before going live:

1. [ ] Open Deck Builder scene
2. [ ] Click a card → Adds to deck
3. [ ] Click deck card → Removes from deck
4. [ ] Try adding 7 copies → Blocked
5. [ ] Click "Save" → No errors
6. [ ] Click "Done" → Returns to menu
7. [ ] Start combat → Deck loaded
8. [ ] No console errors

---

## 🎯 Next Steps

1. **Create the UI** (prefabs & scene)
2. **Test with 5-10 cards** first
3. **Polish visuals** to match your game
4. **Add tooltips** for better UX
5. **Implement deck templates** (optional)

**You're ready to build decks!** 🃏✨








