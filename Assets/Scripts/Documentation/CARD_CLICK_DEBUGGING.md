# Card Click Debugging Guide

## 🔍 How to Debug Card Click Issues

When you click a card with your mouse, you should now see **detailed logs** showing every step of the process.

---

## ✅ Expected Console Output (Working Correctly):

```
═══ CARD CLICK DEBUG ═══
Clicked card GameObject: PooledCard_2
Hand visuals count: 5
Hand data count: 5
Card index in handVisuals: 2
Card clicked: Heavy Strike (Index: 2), Target pos: (1200, 600)
About to call PlayCard(2, (1200, 600))
═══ END CLICK DEBUG ═══

Playing card: Heavy Strike
═══ Applying Heavy Strike to Goblin Scout ═══
  ⚔️ Dealt 12 damage to Goblin Scout
  💔 Goblin Scout HP: 38/50
  → Card effect triggered: Heavy Strike
  → Starting discard animation for Heavy Strike...
  → Animating Heavy Strike from (1200, 600) to discard pile at (1720, 100)...
  → Animation step 1/3 complete for Heavy Strike
  → Animation step 2/3 complete for Heavy Strike
  → Animation step 3/3 complete for Heavy Strike
  → All 3 animations complete for Heavy Strike, returning to pool...
  → Card position before pool return: (1720, 100, 0)
  → Card scale before pool return: (0.2, 0.2, 0.2)
  → Card active: True
  → ✓✓✓ Heavy Strike RETURNED TO POOL! ✓✓✓
```

---

## 🐛 Possible Issues & What to Look For:

### Issue 1: Card Not Found
```
Card index in handVisuals: -1
Clicked card not found in hand visuals!
```

**Cause:** Card was already removed or not in list  
**Fix:** Check if you're clicking too fast

### Issue 2: Animations Not Completing
```
→ Starting discard animation...
→ Animation step 1/3 complete
→ Animation step 2/3 complete
(Never reaches step 3/3)
```

**Cause:** One of the 3 LeanTween animations is being cancelled  
**Fix:** Check if something else is cancelling tweens

### Issue 3: Card Not Returned to Pool
```
→ All 3 animations complete
(No "RETURNED TO POOL!" message)
```

**Cause:** cardRuntimeManager is null or ReturnCardToPool failed  
**Fix:** Check CardRuntimeManager exists and is assigned

### Issue 4: DiscardPileTransform Not Set
```
DiscardPileTransform not set! Card will disappear without animation.
```

**Cause:** No discard pile position assigned  
**Fix:** Create DiscardPilePosition GameObject and assign it

---

## 🎮 How to Test:

1. **Clear Console** (Ctrl+Shift+C)
2. **Press Play** ▶
3. **Wait for 5 cards to draw**
4. **Click ONE card with mouse**
5. **READ THE CONSOLE OUTPUT**
6. **Copy the full output and send it to me!**

The logs will tell us exactly where the process is breaking!

---

## 🔧 Quick Fixes:

### If "DiscardPileTransform not set":
```
1. Create empty GameObject: "DiscardPilePosition"
2. Position: (1720, 100, 0) or similar (bottom-right)
3. Assign to CombatDeckManager → Discard Pile Transform
```

### If animations stop at step 1 or 2:
```
Something is cancelling LeanTween animations
Check for:
  - LeanTween.cancelAll() being called
  - GameObject being destroyed
  - Card being repositioned by another system
```

### If card stays visible but small/rotated:
```
Animations completed but ReturnCardToPool not called
Check:
  - CardRuntimeManager exists in scene
  - ReturnCardToPool is public
  - Card is in the pool
```

---

## 📊 What Each Log Means:

| Log Message | Meaning |
|------------|---------|
| `═══ CARD CLICK DEBUG ═══` | Click detected |
| `Card index in handVisuals: 2` | Found card at index 2 |
| `About to call PlayCard(2, ...)` | Starting play sequence |
| `Playing card: Heavy Strike` | PlayCard method called |
| `Starting discard animation...` | Discard sequence started |
| `Animation step X/3 complete` | One of 3 animations done |
| `All 3 animations complete` | Ready to return to pool |
| `RETURNED TO POOL!` | Success! ✓ |

---

## ⚠️ Common Problems:

### Cards disappear during repositioning:
- Played card might still be in activeCards list
- Try using handVisuals instead (already fixed)

### Cards jump to wrong position:
- Position calculation mismatch
- Already fixed to match CardRuntimeManager exactly

### Cards don't squeeze together:
- Repositioning not animated
- Already fixed with animated: true

---

**Run a test and send me the console output!** 🔍

