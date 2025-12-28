# Card Loss Bug - Troubleshooting Prompt

## Problem Statement

**Severity:** High - Game-breaking bug affecting player experience

**Symptom:** During combat (specifically waves 4-5), players experience a severe reduction in available cards. The player starts with **18 cards** in their starter deck, but by the end of wave 5, they may only have **~8 cards** remaining. In worst-case scenarios, all remaining cards are non-damaging, making encounters impossible to complete.

**Impact:** Players cannot progress past wave 4-5 due to insufficient cards to deal damage to enemies, leading to guaranteed losses.

---

## Expected Behavior

1. Player starts combat with 18 cards in their starter deck
2. Cards cycle between three states:
   - **Draw Pile**: Cards available to draw
   - **Hand**: Cards currently in player's hand (max hand size)
   - **Discard Pile**: Cards that have been played/discarded
3. When draw pile is empty, discard pile should automatically reshuffle into draw pile
4. Card count should remain constant (18 cards) throughout the entire combat encounter
5. Only explicit "Exhaust" effects should permanently remove cards from combat

---

## Current System Architecture

### Key Components

**CombatDeckManager.cs** (`Assets/Scripts/CombatSystem/CombatDeckManager.cs`)
- Manages `drawPile`, `hand`, and `discardPile` (all `List<CardDataExtended>`)
- Primary methods:
  - `LoadDeckForClass()` - **Lines 477-537**: Clears all piles and loads fresh deck
  - `DrawCards()` - **Lines 843-1020**: Draws cards, handles reshuffle logic
  - `DiscardCard()` - **Lines 1470-1512**: Moves cards from hand to discard pile
  - `ExhaustCardFromHand()` - **Lines 3086-3116**: Permanently removes cards (should only be called by enemy abilities)

**CombatManager.cs** (`Assets/Scripts/UI/Combat/CombatManager.cs`)
- Handles wave transitions
- **StartNextWaveCoroutine()** - **Lines 330-385**: Draws additional cards per wave (does NOT reset deck)

**ExhaustCardEffect.cs** (`Assets/Scripts/Combat/Abilities/Effects/ExhaustCardEffect.cs`)
- Enemy ability that permanently removes cards
- Can target hand OR discard pile (line 11: `exhaustFromHand`)

---

## Critical Code Locations to Investigate

### 1. Deck Resetting During Combat (HIGH PRIORITY)

**Location:** `CombatDeckManager.LoadDeckForClass()` - **Lines 477-537**

**Issue:** This method calls `drawPile.Clear()`, `hand.Clear()`, and `discardPile.Clear()` on lines 482-484. If this is called during combat (after wave 1), it would reset the entire deck to the initial 18 cards, but only AFTER cards have already been drawn/played, potentially losing the cards that were in hand or discard.

**Questions to Answer:**
- Is `LoadDeckForClass()` being called during wave transitions?
- Is `LoadDeckForCurrentCharacter()` (line 470) being invoked mid-combat?
- Are there any event listeners or wave transition code that might trigger a deck reload?

**Debugging Strategy:**
- Add logging before each `Clear()` call: `Debug.Log($"[DECK RESET] LoadDeckForClass called - Wave: {currentWave}, Draw: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}")`
- Check call stack when deck is reset
- Verify `loadDeckOnStart` flag (line 408) isn't being toggled during combat

### 2. Reshuffle Logic Bug (MEDIUM PRIORITY)

**Location:** `CombatDeckManager.DrawCards()` - **Lines 898-912**

**Issue:** The reshuffle logic at lines 900-906 might have edge cases where cards are lost during the transfer.

```csharp
if (drawPile.Count == 0)
{
    if (discardPile.Count > 0)
    {
        Debug.Log("Draw pile empty! Reshuffling discard pile...");
        drawPile.AddRange(discardPile);  // Transfer cards
        discardPile.Clear();              // Clear discard
        ShuffleDeck();
    }
    else
    {
        Debug.LogWarning("No cards left to draw!");
        break;
    }
}
```

**Questions to Answer:**
- What happens if `discardPile` is cleared before `AddRange()` completes?
- Are there race conditions if multiple draw calls happen simultaneously?
- Is `hand` properly accounted for? (Cards in hand should not be counted as "lost" - they're still in play)

**Debugging Strategy:**
- Log total card count: `drawPile.Count + hand.Count + discardPile.Count` before and after reshuffle
- Add validation: `if (drawPile.Count + hand.Count + discardPile.Count != expectedTotal) { Debug.LogError("CARD COUNT MISMATCH!") }`
- Check for concurrent draw operations

### 3. Exhaust Effects Removing Too Many Cards (MEDIUM PRIORITY)

**Location:** `ExhaustCardEffect.Execute()` - **Lines 13-71**

**Issue:** Enemy abilities can exhaust cards from either hand OR discard pile. If exhaust effects are being applied too frequently or incorrectly, cards could be permanently removed.

**Questions to Answer:**
- How many exhaust effects are in enemy ability pools for waves 4-5?
- Are exhaust effects properly configured (`exhaustFromHand` flag)?
- Is there validation to prevent exhausting when piles are already low?

**Debugging Strategy:**
- Log every exhaust operation with context: `Debug.Log($"[EXHAUST] Removed {cardName} from {source}. Remaining: Draw={drawCount}, Hand={handCount}, Discard={discardCount}")`
- Track total cards exhausted per combat
- Verify exhaust effects aren't accidentally targeting draw pile

### 4. Card State Mismatch / Visual Desync (LOW PRIORITY)

**Location:** `CombatDeckManager` state management - **Lines 68-262**

**Issue:** Cards have visual GameObjects (`handVisuals`) that are tracked separately from the card data (`hand`). If these get out of sync, cards might appear "lost" but are actually still in the system.

**Questions to Answer:**
- Are `handVisuals` and `hand` lists always in sync?
- What happens if a card GameObject is destroyed but the card data remains?
- Are there any cleanup operations that remove visuals without updating data?

**Debugging Strategy:**
- Validate list lengths match: `if (hand.Count != handVisuals.Count) { Debug.LogError("HAND/VISUAL MISMATCH!") }`
- Check `CardRuntimeManager.ClearAllCards()` calls (line 489)
- Verify card destruction logic

---

## Recommended Debugging Approach

### Phase 1: Add Comprehensive Logging

1. **Create a debug method** in `CombatDeckManager`:
```csharp
private void LogDeckState(string context)
{
    int totalCards = drawPile.Count + hand.Count + discardPile.Count;
    Debug.Log($"[DECK STATE] {context} | Draw: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}, Total: {totalCards}");
    
    if (totalCards != 18) // Replace 18 with actual starting deck size
    {
        Debug.LogError($"[CARD LOSS DETECTED] Total cards changed from 18 to {totalCards} during {context}");
    }
}
```

2. **Call this method at key points:**
   - Start of `LoadDeckForClass()` (before clears)
   - End of `LoadDeckForClass()` (after loading)
   - Start of `DrawCards()` (before draw)
   - After reshuffle (line 906)
   - After each `DiscardCard()` call
   - After each `ExhaustCardFromHand()` call
   - Start/end of each wave transition

### Phase 2: Reproduce the Bug

1. Start a combat encounter
2. Progress through waves 1-5
3. Log deck state at the start of each wave
4. Log deck state whenever a card is played/discarded/exhausted
5. When card count drops, check the logs immediately before/after

### Phase 3: Identify Root Cause

Based on logs, identify:
- **When** cards are lost (which wave, which action)
- **Where** cards are lost (which method, which pile)
- **How many** cards are lost (all at once or gradually)

### Phase 4: Verify Fix

1. Implement fix
2. Add unit test that simulates 5 waves of combat
3. Verify card count remains constant throughout
4. Test edge cases (empty draw pile, full hand, exhaust effects)

---

## Additional Context

### Known Card Flow Patterns

1. **Normal Play:**
   - Card in hand → Played → Moved to discard pile
   - At end of turn: Hand → Discard pile, then draw new hand

2. **Deck Reshuffle:**
   - Draw pile empty → Discard pile → Draw pile (shuffled)

3. **Exhaust:**
   - Card removed permanently (should only happen via enemy abilities)

### Potential Integration Points

- Check if any combat event listeners trigger deck reloads
- Verify wave transition code doesn't call initialization methods
- Check for scene reloading or CombatDeckManager instance recreation mid-combat
- Verify `CombatDeckManager.Instance` singleton pattern isn't creating new instances

---

## Success Criteria

The bug is fixed when:
1. Card count remains at 18 (or starting deck size) throughout entire combat
2. All cards are accounted for in one of: draw pile, hand, or discard pile
3. Only explicit exhaust effects remove cards permanently
4. Deck reshuffle works correctly when draw pile is empty
5. No deck resets occur during wave transitions

---

## Questions for Investigation

1. **When exactly does card loss occur?** (Wave transition? Card play? Discard? Enemy turn?)
2. **Is the loss gradual or sudden?** (10 cards → 9 cards → 8 cards, or 18 cards → 8 cards instantly?)
3. **Are cards missing from all piles, or just one?** (e.g., draw pile has cards but discard is empty)
4. **Does this happen with specific character classes or all classes?**
5. **Are there any specific enemy types or abilities present when this occurs?**
6. **Does this happen consistently or only sometimes?**

---

*This prompt was enhanced from an initial description to provide comprehensive troubleshooting guidance for a senior C#/Unity developer.*

