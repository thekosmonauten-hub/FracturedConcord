# Debug Log Controls - CombatDeckManager

## Overview

The `CombatDeckManager` now has granular control over debug logging to reduce console spam while keeping critical information visible.

## Log Categories

All log controls are available in the Unity Inspector under the **"Debug Logging Settings"** header.

### 1. **Log Card Draws** (Default: Enabled)
- Card draw operations
- Reshuffle operations
- Initial hand drawing

**Example logs:**
- `=== DrawCards called: Drawing 5 cards ===`
- `Draw pile empty! Reshuffling 10 cards from discard pile...`
- `Drawing card #1: Heavy Strike`

### 2. **Log Card Operations** (Default: Enabled)
- Card play/discard operations
- Action queue operations
- Card cleanup

**Example logs:**
- `[ActionQueue] Queueing card: Heavy Strike`
- `[ActionQueue] Heavy Strike resolved - effects applied...`
- `Discarded: Heavy Strike`

### 3. **Log Animations** (Default: Disabled)
- Animation completion events
- Discard animations
- Card draw animations
- Animation step progress

**Example logs:**
- `[Animation] Card play animation complete for Heavy Strike...`
- `→ [Discard] Animation step 1/3 complete...`
- `All 3 animations complete for Heavy Strike, returning to pool...`

### 4. **Log Card Effects** (Default: Disabled)
- Card effect processing
- Channeling state changes
- Combo applications
- Charge modifier effects
- Damage calculations

**Example logs:**
- `Applying card effects for Heavy Strike...`
- `[Channeling] Player began channeling...`
- `[Combo] Combo will apply to Heavy Strike...`
- `[ChargeModifier] HitsTwice: Applying card effect again!`

### 5. **Log State Transitions** (Default: Disabled)
- Card state changes (Queued → Resolving → Resolved)
- State validation

**Example logs:**
- `[State Transition] Heavy Strike: Queued → Resolving`

### 6. **Log Critical Errors** (Default: Enabled, Recommended: Always Enabled)
- Card loss detection
- Critical errors and warnings
- Safety timeout warnings
- Failed operations

**Example logs:**
- `[CARD LOSS DETECTED] Card count mismatch!`
- `[RESHUFFLE BUG] Card count mismatch!`
- `[Safety Timeout] Card animation stuck...`

### 7. **Show Debug Logs** (Default: Disabled)
- Verbose debugging information
- Detailed state information
- Technical details

This is the most verbose setting - enables all detailed logging.

### 8. **Enable Card Count Validation** (Default: Enabled)
- Periodic card count validation
- Card loss detection
- Deck state tracking

## Recommended Settings

### **Production / Normal Play**
```
✓ Log Card Draws: Enabled
✓ Log Card Operations: Enabled
✗ Log Animations: Disabled
✗ Log Card Effects: Disabled
✗ Log State Transitions: Disabled
✓ Log Critical Errors: Enabled (Always keep enabled!)
✗ Show Debug Logs: Disabled
✓ Enable Card Count Validation: Enabled
```

### **Debugging Card Issues**
```
✓ Log Card Draws: Enabled
✓ Log Card Operations: Enabled
✓ Log Animations: Enabled (if investigating animation issues)
✓ Log Card Effects: Enabled (if investigating effect issues)
✓ Log State Transitions: Enabled (if investigating state issues)
✓ Log Critical Errors: Enabled
✓ Show Debug Logs: Enabled (for maximum detail)
✓ Enable Card Count Validation: Enabled
```

### **Minimal Logging (Quiet Mode)**
```
✗ Log Card Draws: Disabled
✗ Log Card Operations: Disabled
✗ Log Animations: Disabled
✗ Log Card Effects: Disabled
✗ Log State Transitions: Disabled
✓ Log Critical Errors: Enabled (Keep enabled for safety!)
✗ Show Debug Logs: Disabled
✓ Enable Card Count Validation: Enabled (Still tracks, just logs less)
```

## Notes

- **Card Loss Detection** logs will always appear when enabled, regardless of other settings
- **Critical Errors** should remain enabled to catch important issues
- Animation logs are very verbose - only enable when debugging animation issues
- Card effect logs can be noisy - enable when debugging specific card mechanics
- The `Show Debug Logs` flag enables the most detailed logging (used with other flags)

## Finding Logs

All logs are prefixed with their category:
- `[ActionQueue]` - Action queue operations
- `[Animation]` - Animation events
- `[Channeling]` - Channeling mechanics
- `[Combo]` - Combo applications
- `[Discard]` - Discard operations
- `[CardPlay]` - Card play operations
- `[CARD LOSS DETECTED]` - Card loss warnings (Critical!)
- `[RESHUFFLE BUG]` - Reshuffle issues (Critical!)

Use Unity's Console filter to show only specific categories if needed.

