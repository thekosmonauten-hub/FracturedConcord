# CharacterDisplayUI - Quick Reference Guide

Step-by-step setup for card preview with hover functionality.

---

## 🚀 Quick Setup (5 Steps)

### **Step 1: Assign Card Prefab**

1. Select `CharacterDisplayController` GameObject
2. Inspector → **Card Preview**:
   ```
   Card Prefab: CharacterScreenDeckCard
   Card Grid Container: StartingDeckContainer
   Cards Per Row: 6
   Card Spacing: 10
   ```

### **Step 2: Assign Hover Preview**

Still on `CharacterDisplayController`:

Inspector → **Full Card Preview (Hover)**:
```
Full Card Preview Prefab: CardPrefab
Full Card Preview Container: FullCardPreview
Preview Scale: 1.0
Show Hover Debug Logs: ☐
```

**How to find FullCardPreview:**
- Hierarchy: `CharacterDisplayUI → Background → LeftPage → FullCardPreview`
- Drag it into the field

### **Step 3: Enable Test Mode**

Still on `CharacterDisplayController`:

Inspector → **Test Mode (Editor Only)**:
```
Test Mode: ✅
Test Class: "Marauder"
```

### **Step 4: Verify StarterDeckManager**

1. Check if `StarterDeckManager` GameObject exists in scene
2. If it exists, verify in Inspector:
   ```
   Load Definitions From Resources: ✅
   Definitions Resources Path: "StarterDecks"
   ```
3. Verify `MarauderStarterDeck.asset` exists at:
   - `Assets/Resources/Cards/Marauder/MarauderStarterDeck.asset`

### **Step 5: Test**

1. **Press Play**
2. **Verify cards load** (should see 6 cards)
3. **Hover over a card** → Full preview should appear
4. **Move mouse away** → Preview should disappear

---

## 🎯 Expected Results

### **Card Grid (Simplified):**
- 6 cards displayed in grid
- Each shows: name, card art (as background), rarity indicator
- No cost, no description (simplified view)

### **Hover Preview (Full Details):**
- Appears when hovering over any card
- Shows: full card art, name, cost, description, effects
- Disappears when mouse leaves
- Doesn't block clicks (raycast disabled)

---

## 📊 Scene Structure

```
CharacterDisplayUI
├── CharacterDisplayController
│   ├── Card Prefab: CharacterScreenDeckCard
│   ├── Card Grid Container: StartingDeckContainer
│   ├── Full Card Preview Prefab: CardPrefab
│   └── Full Card Preview Container: FullCardPreview
└── Canvas
    └── LeftPage (angled)
        ├── StartingDeckContainer
        │   └── [Cards spawn here at runtime]
        └── FullCardPreview
            └── [Preview spawns here on hover]
```

---

## 🐛 Common Issues

| Issue | Fix |
|-------|-----|
| No cards showing | Check Test Mode enabled, verify StarterDeckManager |
| Cards show but no data | Check card prefab has DeckCardListUI |
| Hover preview not appearing | Assign Full Card Preview Prefab + Container |
| Preview blocks clicks | Already fixed - raycasting auto-disabled |
| Cards positioned wrong | Adjust `cardsPerRow` and `cardSpacing` |

---

## 🎨 Card Sprites

**For CharacterScreenDeckCard (simplified):**
- Uses `cardThumbnail` sprite
- Falls back to `cardImage` if not assigned
- Optimized for small row display

**For CardPrefab (full preview):**
- Uses `cardImage` sprite
- Full high-res artwork
- Shown on hover

**To assign sprites:**
1. Select CardDataExtended asset (e.g., `Strike.asset`)
2. Inspector → Visual Assets:
   ```
   Card Image: Strike_Full.png
   Card Thumbnail: Strike_Thumb.png (optional)
   ```

---

## 📝 File References

**Scripts:**
- `CharacterDisplayController.cs` - Main controller
- `CharacterScreenCardHover.cs` - Hover detection
- `DeckCardListUI.cs` - Simplified card display
- `DeckBuilderCardUI.cs` - Full card display

**Prefabs:**
- `CharacterScreenDeckCard.prefab` - Simplified card row
- `CardPrefab.prefab` - Full card preview

**Assets:**
- `MarauderStarterDeck.asset` - Starter deck definition
- Card sprites in `Assets/Art/CardArt/CardSprites/`

**Documentation:**
- `CHARACTER_DISPLAY_CARD_PREVIEW_SETUP.md` - Detailed card preview setup
- `CHARACTER_SCREEN_HOVER_PREVIEW_SETUP.md` - Hover system setup
- `CARD_MULTI_SPRITE_SYSTEM.md` - Multi-sprite system guide

---

**Last Updated:** 2024-12-19
**Status:** ✅ Ready to Test


