# Card Art Setup for Combat Scene - Quick Guide

## ✅ Fixed: CardVisualizer Now Supports Card Art!

---

## What Was Done

1. ✅ **Deleted `SimpleCombatUI.cs`** - Confirmed unused, safely removed
2. ✅ **Updated `CardVisualizer.cs`** - Added card art display support
3. ✅ **Auto-detection** - CardVisualizer finds card art Image automatically

---

## How to Enable Card Art in Combat Scene

### Step 1: Use Existing "Card Image" Component ✅

**Good news!** Your card prefab already has a "Card Image" component (used by DeckBuilderCardUI).

**CardVisualizer will automatically find and use it!** No need to add anything new.

**Your Prefab Structure:**
```
CombatCardPrefab
├── Background (Image)
├── Card Image (Image) ← ALREADY EXISTS! CardVisualizer uses this
├── CardName (Text)
├── Cost (Text)
├── Description (Text)
└── ...
```

**Auto-Detection:**
CardVisualizer looks for these names (in order):
1. "Card Image" ← **Your prefab has this!** ✅
2. "CardImage"
3. "CardArt"
4. "Art"
5. "Image"
6. "Artwork"

**Nothing to do!** Just make sure:
- ✅ Prefab has "Card Image" GameObject with Image component
- ✅ CardVisualizer is attached to card prefab root
- ✅ That's it!

---

### Step 2: Ensure Cards Load from JSON

Your combat system needs to use **DeckLoader** to load JSON cards with art.

Check `AnimatedCombatUI` or whichever script manages your hand:
- It should call `DeckLoader.LoadStarterDeck("ClassName")`
- Cards will have `cardArt` sprite loaded automatically

---

### Step 3: Test in Unity

1. **Play Combat Scene**
2. **Check Console** for:
```
<color=green>Loaded card art: CardArt/HeavyStrike</color>
<color=lime>✓ Card art displayed for Heavy Strike!</color>
```

3. **Verify cards display artwork**

---

## Troubleshooting

### Problem: "No card art Image found"

**Console Message:**
```
No card art Image found. Add an Image component named 'CardArt' to your card prefab.
```

**Solution:**
1. Open your combat card prefab
2. Add a child GameObject named "CardArt"
3. Add Image component to it
4. Adjust size/position to fit card layout

---

### Problem: Art loads but doesn't display

**Console shows:**
```
✅ Loaded card art: CardArt/HeavyStrike
❌ Card HeavyStrike has cardArtName but sprite didn't load
```

**Solution:**
1. Check sprite exists: `Resources/CardArt/HeavyStrike.png`
2. Check import settings: Texture Type = "Sprite (2D and UI)"
3. Check JSON path matches: `"cardArtName": "CardArt/HeavyStrike"`

---

### Problem: Still no art visible

**Console shows:**
```
✅ Loaded card art: CardArt/HeavyStrike
✅ Card art displayed for Heavy Strike!
❌ But still not visible in game
```

**Solutions:**
1. **Check Z-Order**: CardArt Image might be behind background
   - Reorder in hierarchy: CardArt should be ABOVE Background
2. **Check Image Settings**:
   - Image component enabled? ✅
   - Sprite assigned? ✅
   - Color tint white (255,255,255,255)? ✅
   - RectTransform visible area? ✅
3. **Check Canvas**:
   - Card prefab on Canvas
   - Canvas rendered correctly

---

## Deck Builder Scene

If you have a separate Deck Builder scene, check which script it uses:

### If using CardVisualManager:
✅ Already has Card art support (we added it earlier)

### If using CardVisualizer:
✅ Now has Card art support (just added)

### If using CustomCard (UI Toolkit):
❌ Needs separate implementation - uses VisualElements not sprites

---

## Card Prefab Setup (Quick Reference)

### Minimum Required Structure:

```
CombatCardPrefab (GameObject)
├── CardVisualizer (Component) ← Auto-finds elements
├── Background (Image)
├── CardArt (Image) ← ADD THIS for artwork
├── CardName (Text)
├── Cost (Text)
└── Description (Text)
```

### CardVisualizer Inspector:

```
┌─────────────────────────────────┐
│ Card Visualizer (Script)       │
├─────────────────────────────────┤
│ Card Name Text: [Auto-found]   │
│ Card Cost Text: [Auto-found]   │
│ Card Description: [Auto-found] │
│ Card Damage Text: [Auto-found] │
│ Card Type Text: [Auto-found]   │
│ Card Background: [Auto-found]  │
│ Card Border: [Auto-found]      │
│ Element Icon: [Optional]       │
│ Card Art Image: [Auto-found]   │ ← NEW!
└─────────────────────────────────┘
```

---

## JSON Card Art Reference

**Your JSON is already correct!**
```json
{
  "cardName": "Heavy Strike",
  "data": {
    "cardArtName": "CardArt/HeavyStrike", ✅ Perfect!
    ...
  }
}
```

---

## Complete Setup Checklist

- [x] JSON has `cardArtName` field
- [x] Sprite in `Resources/CardArt/HeavyStrike.png`
- [x] Sprite import = "Sprite (2D and UI)"
- [x] CardVisualizer updated with art support
- [x] Card prefab has "Card Image" component ← **Already exists!**
- [ ] Test in Combat Scene ← **Just test it now!**

---

## Next Steps

1. ~~**Open your combat card prefab**~~ ✅ Already has "Card Image"
2. ~~**Add "CardArt" GameObject**~~ ✅ Already exists!
3. **Play Combat Scene** ← **Just do this!**
4. **Enjoy your beautiful card art!** 🎨

---

## Summary

✅ **SimpleCombatUI removed** (was unused)
✅ **CardVisualizer updated** with card art support  
✅ **Auto-detection added** - finds CardArt Image automatically
✅ **JSON already correct** - no changes needed
🎯 **Next: Add CardArt Image to your card prefab!**

The code is ready - just add the Image component to your prefab and you're done!

