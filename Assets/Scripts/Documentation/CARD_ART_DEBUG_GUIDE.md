# Card Art Debug Guide - Combat Scene

## Current System Architecture

Your Combat Scene uses:
- ✅ **CombatDeckManager** → Loads `List<Card>` from JSON via `DeckLoader`
- ✅ **AnimatedCombatUI** → Displays cards in hand
- ✅ **CardVisualizer** → Renders individual cards with art

**This is correct!** You're using Card (JSON) objects, not CardData.

---

## Debug Console Output

When you **Play Combat Scene**, you should see this output trace:

### 1. Deck Loading Phase:
```
✅ TEST MODE: Loading Marauder starter deck...
✅ Loaded card art: CardArt/HeavyStrike
✅ Loaded card art: CardArt/Brace
✅ ... (for each card)
✅ Loaded Marauder deck: 18 cards
```

### 2. Initial Hand Draw:
```
✅ CombatDeckManager: Drew 5 cards for initial hand
```

### 3. Card Visualization (FOR EACH CARD):
```
<color=cyan>CardVisualizer.SetCard() called for: Heavy Strike</color>
  - Card Art Sprite: ✅ LOADED or ❌ NULL
  - Card Art Name: 'CardArt/HeavyStrike'
  - cardArtImage component: ✅ Found or ❌ NULL
```

### 4. Art Display:
```
<color=green>✓ Found card art Image: CardImage</color>
<color=lime>✓ Card art displayed for Heavy Strike!</color>
```

---

## What Each Message Means

| Console Message | Meaning | If Missing |
|-----------------|---------|------------|
| `Loaded card art: CardArt/HeavyStrike` | DeckLoader successfully loaded sprite | Check Resources/CardArt/HeavyStrike.png exists |
| `Card Art Sprite: ✅ LOADED` | Card object has sprite | JSON cardArtName is working |
| `cardArtImage component: ✅ Found` | CardVisualizer found Image component | Prefab has "CardImage" GameObject |
| `✓ Card art displayed` | Sprite assigned to Image | Everything worked! |

---

## Troubleshooting by Console Output

### Case 1: "Failed to load card art"
```
❌ Failed to load card art: CardArt/HeavyStrike
```
**Problem:** Sprite file missing or wrong path  
**Fix:** 
- Check file exists: `Resources/CardArt/HeavyStrike.png`
- Check import settings: Texture Type = "Sprite (2D and UI)"

---

### Case 2: "Card Art Sprite: ❌ NULL"
```
✅ Loaded Marauder deck: 18 cards
❌ Card Art Sprite: ❌ NULL
```
**Problem:** DeckLoader didn't assign sprite to Card object  
**Fix:**
- JSON might be missing `cardArtName` field
- Or DeckLoader.LoadCardArt() failed silently
- Check earlier logs for "Loaded card art" messages

---

### Case 3: "cardArtImage component: ❌ NULL"
```
✅ Card Art Sprite: ✅ LOADED
❌ cardArtImage component: ❌ NULL
⚠️ No card art Image found
```
**Problem:** CardVisualizer can't find Image component in prefab  
**Fix:**
- Open CardPrefab_combat
- Verify "CardImage" GameObject exists
- Verify it has Image component attached
- Or manually assign in Inspector: CardVisualizer → Card Art Image

---

### Case 4: Everything loads but not visible
```
✅ Card Art Sprite: ✅ LOADED
✅ cardArtImage component: ✅ Found
✅ ✓ Card art displayed
❌ But still can't see it in game
```
**Problem:** Image component settings or Z-order  
**Fix:**
- Check Image component enabled
- Check Image color is white (not transparent)
- Check RectTransform has size > 0
- Check "CardImage" is visible in hierarchy (not behind other elements)

---

## Step-by-Step Test

### 1. Enable Test Mode
In scene, find **CombatDeckManager**:
```
Inspector → Testing (Quick Test Mode)
✅ Test Load Marauder Deck On Start
```

### 2. Clear Console
Unity → Console → Clear

### 3. Play Scene
Hit Play button

### 4. Read Console Output
Look for the messages above and identify which stage fails

### 5. Share Results
Copy the console output and we'll fix the exact issue!

---

## Quick Verification Commands

### Check if CombatDeckManager is loading cards:
```csharp
// In Console while playing:
CombatDeckManager.Instance.GetDrawPileCount() // Should be > 0
CombatDeckManager.Instance.GetHandCount() // Should be 5
```

### Check if cards have art:
```csharp
// First card in hand should have sprite
var hand = CombatDeckManager.Instance.GetHand();
if (hand.Count > 0)
{
    Debug.Log($"First card: {hand[0].cardName}");
    Debug.Log($"Has art: {hand[0].cardArt != null}");
}
```

---

## Expected Full Console Trace (Success)

```
<color=yellow>TEST MODE: Loading Marauder starter deck...</color>
<color=green>Loaded card art: CardArt/HeavyStrike</color>
<color=green>Loaded card art: CardArt/Brace</color>
<color=green>Loaded card art: CardArt/GroundSlam</color>
... (more cards)
<color=green>✓ Loaded Marauder deck: 18 cards</color>
CombatDeckManager: Drew 5 cards for initial hand

<color=cyan>CardVisualizer.SetCard() called for: Heavy Strike</color>
  - Card Art Sprite: ✅ LOADED
  - Card Art Name: 'CardArt/HeavyStrike'
  - cardArtImage component: ✅ Found
<color=green>✓ Found card art Image: CardImage</color>
<color=lime>✓ Card art displayed for Heavy Strike!</color>

... (repeat for each card in hand)
```

---

## What to Do Next

1. **Play Combat Scene**
2. **Check Console**
3. **Share the debug output** - I'll tell you exactly what's wrong!

The detailed logging will show us exactly where the card art pipeline is breaking. 🔍




