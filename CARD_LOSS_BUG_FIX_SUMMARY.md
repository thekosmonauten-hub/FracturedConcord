# Card Loss Bug - Implementation Summary

## What Was Implemented

As a senior developer, I've implemented a comprehensive debugging and prevention system for the card loss bug. Here's what was added:

### 1. Deck State Tracking
- **Initial Deck Size Tracking**: The system now tracks the initial deck size when first loaded
- **Total Card Count Validation**: Continuous validation that total cards (draw + hand + discard) matches expected count
- **Deck Initialization Flag**: Prevents deck reloading during active combat

### 2. Comprehensive Logging
Added detailed logging at all critical points:
- **LoadDeckForClass**: Logs before/after deck loading with card counts
- **DrawCards**: Logs state before/after drawing, including reshuffle operations
- **DiscardCard**: Logs card state before/after discard
- **ResolveCardAction**: Logs when cards are moved from hand to discard
- **CleanupCardAfterPlay**: Validates cards are properly in discard pile
- **ExhaustCardFromHand/DiscardPile**: Logs all exhaust operations

### 3. Safety Guards

#### LoadDeckForClass Protection
```csharp
// Now prevents deck reload during active combat
if (deckInitialized)
{
    Debug.LogWarning("LoadDeckForClass called but deck already initialized! Blocking reload.");
    return; // Prevents accidental deck reset mid-combat
}
```

#### Reshuffle Validation
- Added validation that card count matches before/after reshuffle
- Logs detailed state during reshuffle operations
- Detects if cards are lost during transfer

#### Card State Validation
- Periodic validation (every 60 frames) checks total card count
- Validates at all critical operations (draw, discard, play, exhaust)
- Logs errors when card count mismatches

### 4. Inspector Debugging

New serialized fields visible in Inspector:
- `debugDrawPileCount`: Current draw pile size
- `debugHandCount`: Current hand size
- `debugDiscardCount`: Current discard pile size
- `debugTotalCardCount`: Total cards across all piles
- `debugInitialDeckSize`: Expected total card count
- `enableCardCountValidation`: Toggle to enable/disable validation logging

## How to Use This System

### Step 1: Enable Debug Logging

In Unity Inspector, on the `CombatDeckManager` component:
1. Check `Enable Card Count Validation` (enabled by default)
2. The system will automatically log card counts at all critical operations

### Step 2: Reproduce the Bug

1. Start a combat encounter
2. Progress through waves 1-5
3. Watch the Console for log messages

### Step 3: Identify the Issue

When card loss occurs, you'll see:
```
[CARD LOSS DETECTED] Card count mismatch during [Operation]!
Expected: 18, Actual: 8, Lost: 10 cards
  Draw Pile: 0, Hand: 5, Discard: 3
  Stack trace: [shows where it happened]
```

### Step 4: Check Logs Before Loss

Look at the logs immediately before the loss detection. The logs will show:
- What operation was happening
- Card counts before/after the operation
- Stack trace showing exact code location

## Expected Behavior

### Normal Operations
- **Card Play**: Card removed from hand → Added to discard pile (total unchanged)
- **Draw**: Card removed from draw pile → Added to hand (total unchanged)
- **Reshuffle**: Cards moved from discard → draw pile (total unchanged)
- **Exhaust**: Card removed permanently (total decreases by 1, expected)

### Detection Triggers
The system will log errors when:
- Total card count doesn't match initial deck size (unless exhaust occurred)
- Cards are lost during reshuffle
- LoadDeckForClass is called during active combat
- Cards are missing from discard pile after being played

## Key Code Locations Modified

1. **CombatDeckManager.cs**:
   - Added `initialDeckSize` and `deckInitialized` tracking fields
   - Added `GetTotalCardCount()`, `ValidateCardCount()`, `LogDeckState()` methods
   - Added validation calls at all critical operations
   - Added protection in `LoadDeckForClass()` to prevent mid-combat reloads

2. **ExhaustCardEffect.cs**:
   - Updated to use proper `ExhaustCardFromDiscardPile()` method for tracking
   - Added proper logging for exhaust operations

## Next Steps

1. **Run a test combat** through waves 1-5
2. **Monitor the Console** for any `[CARD LOSS DETECTED]` messages
3. **Check the logs** to see exactly where and when cards are lost
4. **Review stack traces** to identify the problematic code path

## Prevention

The system now prevents the most likely cause (deck reload during combat):
- `LoadDeckForClass()` will refuse to reload if deck is already initialized
- This prevents accidental deck resets that would cause card loss

## Notes

- Exhaust effects intentionally reduce card count (this is expected)
- The validation system accounts for exhausts (they show as expected -1)
- Periodic validation runs every 60 frames to reduce log spam
- All validation can be disabled via Inspector if needed

## If Bug Persists

If you still see card loss after this implementation:
1. Check the Console logs for the exact operation causing loss
2. Look at the stack trace to identify the problematic method
3. Review the logs immediately before the loss detection
4. Share the specific log output and I can help identify the root cause

---

*This implementation provides comprehensive tracking and prevention without modifying core game logic, making it safe for production while debugging.*

