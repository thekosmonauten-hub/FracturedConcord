# Guardbreaker: Momentum Guard with Attribute Scaling

## Your Setup

**Guardbreaker Card**:
- **Description**: `"Spend up to 2 Momentum. Gain {guard} Guard per Momentum spent."`
- **Base Guard** (`block`): `4`
- **Guard Scaling**: `dexterityDivisor: 8`
- **Player Dexterity**: `16` (example)

## How It Works

### **Step 1: Guard Per Momentum Calculation**

The system uses the card's **actual guard calculation** (not just parsing a number):

```csharp
guardPerMomentum = DamageCalculator.CalculateGuardAmount(card, character)
```

This calculates:
- **Base Guard**: `4` (from `block` field)
- **Dexterity Scaling**: `16 / 8 = 2` (from `dexterityDivisor: 8`)
- **Total Per Momentum**: `4 + 2 = 6 guard`

### **Step 2: Multiply by Momentum Spent**

If you spend **2 momentum**:
- **Total Guard**: `6 × 2 = 12 guard`

### **Step 3: Apply to Player**

The player gains **12 guard** total.

## Example Scenarios

### **Scenario 1: Spend 2 Momentum**
- Guard per momentum: `4 + (16/8) = 6`
- Total guard: `6 × 2 = 12 guard` ✅

### **Scenario 2: Spend 1 Momentum**
- Guard per momentum: `4 + (16/8) = 6`
- Total guard: `6 × 1 = 6 guard` ✅

### **Scenario 3: Different Dexterity (e.g., 24)**
- Guard per momentum: `4 + (24/8) = 4 + 3 = 7`
- Total guard (2 momentum): `7 × 2 = 14 guard` ✅

## Key Points

1. ✅ **Uses Card's Guard Calculation**: The system uses `DamageCalculator.CalculateGuardAmount()` which includes:
   - Base guard (`block` field)
   - All guard scaling (strength, dexterity, intelligence)
   - All divisors (strengthDivisor, dexterityDivisor, intelligenceDivisor)

2. ✅ **{guard} Placeholder Works**: The `{guard}` placeholder in descriptions will resolve to the calculated value (base + scaling)

3. ✅ **Per Momentum**: The calculated guard is multiplied by momentum spent

4. ✅ **Automatic**: No special setup needed - just use the pattern `"Gain {guard} Guard per Momentum spent"` in the description

## Card Asset Configuration

**Required Fields**:
- `description`: `"Spend up to 2 Momentum. Gain {guard} Guard per Momentum spent."`
- `block`: `4` (base guard)
- `guardScaling.dexterityDivisor`: `8` (scaling)

**Optional**:
- `guardScaling.strengthScaling`: Additional scaling
- `guardScaling.dexterityScaling`: Multiplicative scaling
- `momentumEffectDescription`: Threshold effects for display

## Formula

```
Guard Per Momentum = baseGuard + guardScaling.CalculateScalingBonus(character)
Total Guard = Guard Per Momentum × Momentum Spent
```

**Your Example**:
```
Guard Per Momentum = 4 + (16 / 8) = 4 + 2 = 6
Total Guard (2 momentum) = 6 × 2 = 12
```

## Summary

✅ **Yes, it will use your card's base guard (4) and dexterity divisor (8) for the guard effect per momentum spent!**

The system automatically:
1. Calculates guard using `DamageCalculator.CalculateGuardAmount()` (includes base + scaling)
2. Multiplies by momentum spent
3. Applies to player

No additional configuration needed - just set up your card with the correct `block` and `guardScaling` values!

