# Momentum Guard Setup Guide

## Overview

This guide explains how to set up cards that grant Guard based on Momentum spent, like **Guardbreaker**.

## Card Setup: Guardbreaker Example

### **Card Asset Configuration**

For `Guardbreaker_Extended.asset`:

1. **Card Type**: `Skill` (not Guard - Skill cards can grant guard)
2. **Description**: `"Spend up to 2 Momentum. Gain 4 Guard per Momentum spent."`
3. **Base Guard**: `0` (guard comes from momentum, not base value)
4. **Momentum Effect Description**: 
   ```
   If you have:
   3+ Momentum: Gain 1 temporary Strength.
   4+ Momentum: Gain 2 additional Guard.
   ```

### **How It Works**

1. **Momentum Spending**:
   - Card description: `"Spend up to 2 Momentum"`
   - System automatically spends up to 2 momentum (or all if player has less)
   - Returns the amount actually spent

2. **Guard Calculation**:
   - Pattern: `"Gain 4 Guard per Momentum spent"`
   - Formula: `4 × momentumSpent`
   - Example: With 2 momentum spent = `4 × 2 = 8 guard`

3. **Threshold Effects** (from `momentumEffectDescription`):
   - `"If you have 3+ Momentum"` → Checked before spending
   - `"If you have 4+ Momentum"` → Checked before spending
   - These are displayed in AdditionalEffectsText but need to be implemented in card logic

## Setup Steps

### **Step 1: Card Description Format**

Use this exact pattern in the card's `description` field:

```
Spend up to X Momentum. Gain Y Guard per Momentum spent.
```

**Examples**:
- `"Spend up to 2 Momentum. Gain 4 Guard per Momentum spent."`
- `"Spend all Momentum. Gain 3 Guard per Momentum spent."`
- `"Spend up to 3 Momentum. Gain 5 Guard per Momentum spent."`

### **Step 2: Card Type**

- Use `Skill` card type (not `Guard`)
- Skill cards can grant guard, damage, evasion, etc.

### **Step 3: Base Guard Value**

- Set `block` (base guard) to `0`
- Guard comes entirely from momentum spending

### **Step 4: Momentum Effect Description**

Fill in the `momentumEffectDescription` field for threshold effects:

```
If you have:
3+ Momentum: Gain 1 temporary Strength.
4+ Momentum: Gain 2 additional Guard.
```

This text will appear in the `AdditionalEffectsText` area on the card.

## How the System Works

### **Automatic Parsing**

The system automatically:
1. ✅ Detects `"Guard per Momentum spent"` pattern
2. ✅ Parses `"Spend up to X Momentum"` or `"Spend all Momentum"`
3. ✅ Spends the momentum
4. ✅ Calculates guard: `baseGuardPerMomentum × momentumSpent`
5. ✅ Applies guard to player
6. ✅ Updates UI

### **Code Flow**

```
ApplySkillCard()
  ↓
Detects "Guard per Momentum spent" pattern
  ↓
Spends momentum (up to limit or all)
  ↓
Calculates: 4 × momentumSpent
  ↓
Applies guard to player
  ↓
Updates guard display
```

## Example: Guardbreaker Card

### **Current Setup**

```yaml
cardName: Guardbreaker
cardType: Skill
playCost: 1
description: Spend up to 2 Momentum. Gain 4 Guard per Momentum spent.
block: 0
momentumEffectDescription: |
  If you have:
  3+ Momentum: Gain 1 temporary Strength.
  4+ Momentum: Gain 2 additional Guard.
```

### **How It Works in Combat**

**Scenario 1: Player has 5 Momentum**
1. Card played: "Spend up to 2 Momentum"
2. System spends: 2 momentum (player now has 3)
3. Guard calculation: `4 × 2 = 8 guard`
4. Player gains: 8 guard
5. Threshold check: Has 3+ momentum? Yes → (needs implementation for temp Strength)
6. Threshold check: Has 4+ momentum? No (only 3 left) → No bonus

**Scenario 2: Player has 1 Momentum**
1. Card played: "Spend up to 2 Momentum"
2. System spends: 1 momentum (all available, player now has 0)
3. Guard calculation: `4 × 1 = 4 guard`
4. Player gains: 4 guard

**Scenario 3: Player has 0 Momentum**
1. Card played: "Spend up to 2 Momentum"
2. System spends: 0 momentum (none available)
3. Warning logged: "Requires momentum but player has none!"
4. No guard gained

## Advanced: Threshold Effects

The `momentumEffectDescription` field displays threshold effects, but they need to be implemented in the card logic.

### **Current Status**

- ✅ **Display**: Threshold effects show in AdditionalEffectsText
- ⚠️ **Implementation**: Threshold effects (temp Strength, bonus Guard) need to be added to `ApplySkillCard()` or `ApplyGuardCard()`

### **Future Implementation**

To implement threshold effects, you would add logic like:

```csharp
// In ApplySkillCard() or ApplyGuardCard()
if (player.Momentum.HasMomentum(3))
{
    // Apply temporary Strength
    // (needs StatusEffectManager integration)
}

if (player.Momentum.HasMomentum(4))
{
    // Add bonus guard
    guardAmount += 2f;
}
```

## Testing Checklist

- [ ] Card description uses exact pattern: `"Gain X Guard per Momentum spent"`
- [ ] Card type is `Skill` (not `Guard`)
- [ ] Base guard (`block`) is `0`
- [ ] Momentum spending works correctly
- [ ] Guard calculation is correct (base × momentum spent)
- [ ] Guard is applied to player
- [ ] Guard UI updates correctly
- [ ] Works with 0 momentum (shows warning, no guard)
- [ ] Works with partial momentum (spends what's available)
- [ ] Momentum effect description displays in UI

## Troubleshooting

### **Issue: No Guard Gained**

**Check**:
1. Does description contain `"Guard per Momentum spent"`? (case-insensitive)
2. Does player have momentum to spend?
3. Is the pattern exactly: `"Gain X Guard per Momentum spent"`?

### **Issue: Wrong Guard Amount**

**Check**:
1. Is the base guard value correct? (e.g., `4` in "Gain 4 Guard")
2. Is momentum being spent correctly?
3. Check console logs for momentum spending messages

### **Issue: Momentum Not Spent**

**Check**:
1. Does description contain `"Spend up to X Momentum"` or `"Spend all Momentum"`?
2. Is the pattern exactly correct?
3. Check console for parsing errors

## Summary

**Guardbreaker Setup**:
1. ✅ Description: `"Spend up to 2 Momentum. Gain 4 Guard per Momentum spent."`
2. ✅ Card Type: `Skill`
3. ✅ Base Guard: `0`
4. ✅ Momentum Effect Description: Threshold effects (for display)
5. ✅ System automatically handles momentum spending and guard calculation

The system is now fully integrated and ready to use!

