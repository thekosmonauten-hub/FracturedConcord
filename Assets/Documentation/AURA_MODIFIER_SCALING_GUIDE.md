# Reliance Aura Modifier Scaling Guide

## Overview

Reliance Aura modifiers automatically scale their numeric parameters based on the aura's level (1-20). This guide explains how to set up modifier definitions to support level scaling.

---

## Parameter Naming Convention

To enable automatic scaling, modifier definitions must store both Level 1 and Level 20 values using this naming convention:

### Format
- **Level 1 value:** `parameterName` (e.g., `fireDamage`, `lightningDamage`, `spreadCount`)
- **Level 20 value:** `parameterName_level20` (e.g., `fireDamage_level20`, `lightningDamage_level20`, `spreadCount_level20`)

### Example: Pyreheart Mantle

**Modifier Definition Parameters:**
```
fireDamage = 90 (Level 1: 90 flat Fire damage)
fireDamage_level20 = 160 (Level 20: 160 flat Fire damage)
```

**Scaling Calculation:**
- Level 1: 90 damage
- Level 10: ~123 damage (interpolated)
- Level 20: 160 damage

---

## Setting Up Modifier Definitions

### Step 1: Create the Modifier Definition Asset

1. In Unity, right-click in Project window
2. Select: `Create > Dexiled > Reliance Aura > Modifier Definition`
3. Name it: `[AuraName]_[EffectName]` (e.g., `PyreheartMantle_FireDamage`)

### Step 2: Configure Basic Info

- **Modifier ID:** `PyreheartMantle_FireDamage`
- **Linked Aura Name:** `Pyreheart Mantle`
- **Description:** `Adds 90-160 flat Fire damage to attacks`

### Step 3: Add Effects

1. Click `+` to add a new `ModifierEffect`
2. Set **Event Type:** `OnCombatStart` (or appropriate event)
3. Click `+` to add a new `ModifierAction`
4. Set **Action Type:** `AddFlatDamage` (or appropriate action)

### Step 4: Add Level 1 Parameters

In the `ModifierAction`'s `Parameter Dict`, add:

| Key | Type | Value |
|-----|------|-------|
| `damageType` | String | `Fire` |
| `damage` | Float | `90` |

### Step 5: Add Level 20 Parameters

Add the Level 20 version with `_level20` suffix:

| Key | Type | Value |
|-----|------|-------|
| `damage_level20` | Float | `160` |

**Important:** The parameter name must be exactly `[baseName]_level20` (e.g., `damage_level20`).

---

## Supported Parameter Types

### Numeric Types (Auto-Scaled)

- **Float:** Interpolated linearly between Level 1 and Level 20
- **Int:** Interpolated and rounded to nearest integer

### Non-Numeric Types (Not Scaled)

- **String:** Copied as-is (e.g., `damageType`, `statusType`)
- **Bool:** Copied as-is (e.g., `applyShatterWeakness`)

---

## Examples

### Example 1: Flat Damage (Pyreheart Mantle)

**Action Type:** `AddFlatDamage`

**Parameters:**
```
damageType = "Fire"
damage = 90
damage_level20 = 160
```

**Result:**
- Level 1: +90 Fire damage
- Level 10: +123 Fire damage
- Level 20: +160 Fire damage

---

### Example 2: Status Spread (Emberwake Echo)

**Action Type:** `SpreadStatusOnKill`

**Parameters:**
```
statusType = "Ignite"
spreadCount = 1
spreadCount_level20 = 2
magnitude = 0
```

**Result:**
- Level 1: Spread Ignite to 1 random enemy
- Level 10: Spread Ignite to ~1.5 random enemies (rounded to 1 or 2)
- Level 20: Spread Ignite to 2 random enemies

---

### Example 3: Percentage Damage (Law of Permafrost)

**Action Type:** `ShatterOnKill`

**Parameters:**
```
damagePercentOfMaxLife = 15.0
damagePercentOfMaxLife_level20 = 25.0
requiredStatus = "Chill"
```

**Result:**
- Level 1: Deal 15% of max life as damage
- Level 10: Deal ~20% of max life as damage
- Level 20: Deal 25% of max life as damage

---

## Backwards Compatibility

If a parameter **does not** have a `_level20` version:
- The base value is used as-is (assumed to be Level 1)
- No scaling is applied
- This maintains compatibility with existing modifiers

**Example:**
```
damageType = "Fire"  (No _level20 version - copied as-is)
damage = 90          (No _level20 version - used as-is, no scaling)
```

---

## Testing Scaling

### In Unity Editor

1. Set character's aura level:
```csharp
AuraExperienceManager.Instance.AddAuraExperience("Pyreheart Mantle", 10000);
// This will level the aura up to level 20
```

2. Check scaled values:
```csharp
Character character = CharacterManager.Instance.GetCurrentCharacter();
List<ModifierEffect> effects = 
    RelianceAuraModifierRegistry.Instance.GetActiveEffects(character);
// Inspect effects[0].actions[0].parameterDict to see scaled values
```

### Debug Logging

The scaling system logs when effects are scaled:
```
[AuraExperience] Applied 50 XP to 2 active auras
[Aura Level Up] Pyreheart Mantle reached level 5!
```

---

## Common Issues

### Issue: Parameters Not Scaling

**Cause:** Missing `_level20` parameter

**Solution:** Add the `_level20` version of the parameter

### Issue: Wrong Scaling Values

**Cause:** Parameter name mismatch (e.g., `damageLevel20` instead of `damage_level20`)

**Solution:** Ensure exact naming: `[baseName]_level20`

### Issue: String Parameters Being Scaled

**Cause:** String parameters are copied as-is (by design)

**Solution:** This is correct behavior - only numeric values are scaled

---

## Best Practices

1. **Always provide Level 20 values** for numeric parameters that should scale
2. **Use descriptive parameter names** (e.g., `fireDamage` not `dmg`)
3. **Test at multiple levels** (1, 10, 20) to verify scaling
4. **Document scaling ranges** in the modifier description
5. **Keep Level 1 and Level 20 values proportional** for smooth progression

---

## Integration with Modifier Generator

The `RelianceAuraModifierGenerator` editor tool can be updated to automatically add `_level20` parameters when parsing TSV data. This would require:

1. Parsing Level 1 and Level 20 effect strings
2. Extracting numeric values
3. Creating both base and `_level20` parameters

**Future Enhancement:** Auto-generate `_level20` parameters from TSV import.

