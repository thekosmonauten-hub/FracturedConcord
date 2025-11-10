# Character Creation Deck Preview - Quick Reference

## 🎯 Two Display Modes

### Visual Card Prefabs Mode (Recommended)
**Looks**: Real card GameObjects with art and styling  
**Setup**: Assign both prefabs + canvas  
**Best For**: Final builds, polished experience

### Text List Mode (Fallback)
**Looks**: Simple text rows (x5 Strike, x4 Defend)  
**Setup**: No prefabs needed  
**Best For**: Prototyping, minimal dependencies

---

## 📋 Quick Setup

### Option A: Visual Cards (Premium)
```
CharacterCreationController Inspector:
✅ Deck Card Prefab → DeckCardPrefab.prefab
✅ Full Card Prefab → CardPrefab.prefab
✅ Card Preview Canvas → Canvas (Sort Order: 100)

Result: Beautiful visual deck with hoverable card prefabs
```

### Option B: Text List (Simple)
```
CharacterCreationController Inspector:
✅ Full Card Prefab → CardPrefab.prefab
❌ Deck Card Prefab → [Leave empty]
❌ Card Preview Canvas → [Leave empty]

Result: Clean text list with hoverable full previews
```

---

## 🎮 How It Works

### When Class is Selected
1. User clicks class button
2. System finds StarterDeckDefinition for that class
3. Deck preview auto-updates:
   - **Visual Mode**: Spawns card prefab GameObjects
   - **Text Mode**: Creates UI Toolkit text rows
4. Cards appear in grid/layout

### When Card is Hovered
1. Mouse enters card area
2. System instantiates full CardPrefab
3. Full card appears (larger, detailed)
4. Mouse leaves → card disappears

---

## ⚙️ Key Files

```
Assets/UI/
├── CharacterCreationUI.uxml         ← Layout
└── CharacterCreationUI.uss          ← Styles (USE USS!)

Assets/Scripts/UI/
└── CharacterCreationController.cs   ← Logic

Assets/Art/CardArt/
├── DeckCardPrefab.prefab           ← Compact (visual mode)
└── CardPrefab.prefab               ← Full (hover preview)

Assets/Resources/StarterDecks/
├── MarauderStarterDeck.asset       ← Required!
├── WitchStarterDeck.asset          ← Required!
├── RangerStarterDeck.asset         ← Required!
├── ThiefStarterDeck.asset          ← Required!
├── ApostleStarterDeck.asset        ← Required!
└── BrawlerStarterDeck.asset        ← Required!
```

---

## 🐛 Common Issues

| Problem | Fix |
|---------|-----|
| No deck preview shows | Create StarterDeckDefinition for that class |
| Text mode instead of visual | Assign deckCardPrefab + canvas |
| Hover doesn't work | Assign fullCardPrefab |
| Cards overlap | Increase HorizontalLayoutGroup spacing |
| Wrong position | Adjust anchoredPosition in code |
| Canvas conflict | Set Sort Order to 100+ |

---

## 🔧 Quick Adjustments

### More/Fewer Cards Visible
```css
/* In CharacterCreationUI.uss */
.deck-preview-scroll {
    width: 750px;   /* Wider = more cards visible */
    height: 220px;  /* Taller = more rows visible */
}
```

### Card Spacing
```csharp
// In CreateVisualDeckPreview()
layout.spacing = 15f;  // Pixels between cards
```

### Hover Card Size
```csharp
// In OnCardPrefabHoverEnter() or OnCardRowHoverEnter()
rt.localScale = Vector3.one * 2.0f;  // 2x = double size
```

---

## ✨ Features

Both modes include:
- ✅ All 6 classes supported
- ✅ Card count display (x5, x4, etc.)
- ✅ Hover for full card preview
- ✅ Smooth animations
- ✅ Automatic cleanup
- ✅ Memory efficient

---

## 📊 Mode Comparison

| Aspect | Visual Cards | Text List |
|--------|-------------|-----------|
| Setup Time | 5 min | 0 min |
| Visual Appeal | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| Performance | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Dependencies | 2 prefabs + canvas | Just 1 prefab |
| Hover Preview | ✅ Yes | ✅ Yes |

---

## 🚀 You're Ready!

All code is implemented and working. Just:
1. Assign prefabs in Inspector (choose your mode)
2. Create Canvas if using visual mode
3. Ensure all classes have StarterDeckDefinition assets
4. Test in Play Mode!

**Full documentation**: See `CharacterCreation_DualMode_Implementation.md` for technical details!












