# Status Effect System Guide

## Overview

The status effect system has been completely refactored to support source damage-based calculations, proper stacking rules, and new mechanics for each ailment type.

## Key Changes

### Source Damage Tracking
All status effects now track the source damage that created them:
- `sourcePhysicalDamage`
- `sourceFireDamage`
- `sourceColdDamage`
- `sourceLightningDamage`
- `sourceChaosDamage`

This allows effects to calculate their magnitude based on the actual damage dealt.

### New Status Effect Types
- **Shocked**: Increases damage taken (up to 50% based on lightning damage)
- **Stagger**: Prevents actions (base 1 turn)

## Status Effect Mechanics

### Bleeding
- **Source**: Physical damage from attacks only
- **Calculation**: 70% of physical damage per turn
- **Duration**: Base 5 turns
- **Stacking**: Only one instance can be active. If a higher bleed is applied, it replaces the existing one and refreshes duration.
- **Example**:
  ```csharp
  StatusEffect bleed = StatusEffectFactory.CreateBleeding(100f, 5); // 70 damage/turn for 5 turns
  ```

### Poison
- **Source**: Physical and/or Chaos damage
- **Calculation**: 30% of (physical + chaos) damage per turn
- **Duration**: Base 2 turns
- **Stacking**: Stacks independently. Each stack has its own duration and magnitude.
- **Example**:
  ```csharp
  StatusEffect poison = StatusEffectFactory.CreatePoison(50f, 50f, 2); // 30 damage/turn for 2 turns
  ```

### Ignite
- **Source**: Fire damage
- **Calculation**: 90% of fire damage per turn
- **Duration**: Base 4 turns
- **Tick Interval**: 1 turn (tickInterval = 1f)
- **Stacking**: Stacks magnitude
- **Example**:
  ```csharp
  StatusEffect ignite = StatusEffectFactory.CreateIgnite(100f, 4); // 90 damage/turn for 4 turns
  ```

### Chilled
- **Source**: Cold damage (always applied by cold damage)
- **Effect**: Reduces energy gain up to 30% based on cold damage
- **Duration**: Base 2 turns
- **Stacking**: Updates energy reduction if higher cold damage
- **Example**:
  ```csharp
  StatusEffect chilled = StatusEffectFactory.CreateChilled(50f, 2); // Reduces energy gain
  ```

### Frozen
- **Source**: Cold damage
- **Effect**: Prevents actions
- **Duration**: 1-2 turns based on cold damage as percentage of target's max HP
  - Base: 1 turn
  - 2 turns if cold damage >= 10% of target's max HP
- **Stacking**: Replaces existing freeze
- **Note**: Only the COLD damage portion is considered, not total damage
- **Example**:
  ```csharp
  StatusEffect frozen = StatusEffectFactory.CreateFrozen(60f, 500f); // 60 cold damage, 500 max HP = 12% = 2 turns
  ```

### Shocked
- **Source**: Lightning damage
- **Effect**: Increases damage taken up to 50% (1.0x to 1.5x multiplier)
- **Duration**: Base 2 turns
- **Stacking**: Updates multiplier if higher lightning damage
- **Example**:
  ```csharp
  StatusEffect shocked = StatusEffectFactory.CreateShocked(100f, 2); // Up to 1.5x damage taken
  ```

### Stagger
- **Source**: Various (stun effects)
- **Effect**: Prevents actions
- **Duration**: Base 1 turn
- **Stacking**: Replaces existing stagger
- **Example**:
  ```csharp
  StatusEffect stagger = StatusEffectFactory.CreateStagger(1);
  ```

### Vulnerability
- **Source**: Various
- **Effect**: 20% more damage for the next damage instance
- **Duration**: 1 turn (consumed after one damage instance)
- **Stacking**: Replaces existing vulnerability
- **Example**:
  ```csharp
  StatusEffect vulnerable = StatusEffectFactory.CreateVulnerability(1);
  ```

### Crumble
- **Source**: Physical damage (stored as debuff)
- **Effect**: Stores X% of physical damage taken. Deals stored damage when expires.
- **Duration**: Varies
- **Stacking**: Stacks magnitude (stored damage)
- **Example**:
  ```csharp
  StatusEffect crumble = StatusEffectFactory.CreateCrumble(50f, 3); // Stores 50 damage, deals on expire
  ```

### Chaos DoT
- **Source**: Chaos damage (from cards that specifically say "Chaos damage over time")
- **Calculation**: Uses magnitude directly (not percentage-based)
- **Duration**: Varies
- **Stacking**: Stacks magnitude
- **Example**:
  ```csharp
  StatusEffect chaosDot = StatusEffectFactory.CreateChaosDot(25f, 3); // 25 damage/turn for 3 turns
  ```

### Bolster
- **Source**: Various buffs
- **Effect**: 2% damage reduction per stack, max 20% at 10 stacks
- **Duration**: Varies
- **Stacking**: Stacks up to 10 (capped automatically)
- **Example**:
  ```csharp
  StatusEffect bolster = StatusEffectFactory.CreateBolster(5f, 3); // 10% damage reduction (5 stacks)
  ```

### TempStrength / TempDexterity / TempIntelligence
- **Source**: Various buffs
- **Effect**: Modifies stat for duration or rest of combat
- **Duration**: Varies (-1 for permanent)
- **Stacking**: Stacks magnitude
- **Example**:
  ```csharp
  StatusEffect strength = new StatusEffect(StatusEffectType.Strength, "TempStrength", 3f, 3, false);
  ```

## Using StatusEffectFactory

The `StatusEffectFactory` class provides helper methods to create status effects with proper source damage calculations:

```csharp
// Bleeding from 100 physical damage
StatusEffect bleed = StatusEffectFactory.CreateBleeding(100f, 5);

// Poison from 50 physical + 50 chaos damage
StatusEffect poison = StatusEffectFactory.CreatePoison(50f, 50f, 2);

// Ignite from 100 fire damage
StatusEffect ignite = StatusEffectFactory.CreateIgnite(100f, 4);

// Chilled from 50 cold damage
StatusEffect chilled = StatusEffectFactory.CreateChilled(50f, 2);

// Frozen from 60 cold damage (2 turns)
StatusEffect frozen = StatusEffectFactory.CreateFrozen(60f);

// Shocked from 100 lightning damage
StatusEffect shocked = StatusEffectFactory.CreateShocked(100f, 2);

// Stagger
StatusEffect stagger = StatusEffectFactory.CreateStagger(1);

// Vulnerability
StatusEffect vulnerable = StatusEffectFactory.CreateVulnerability(1);
```

## StatusEffectManager Helper Methods

### Checking Entity State
```csharp
// Check if entity can act (not Frozen or Staggered)
bool canAct = statusManager.CanAct();

// Get damage multipliers
float shockedMultiplier = statusManager.GetShockedDamageMultiplier(); // 1.0x to 1.5x
float vulnerableMultiplier = statusManager.GetVulnerabilityDamageMultiplier(); // 1.0x or 1.2x

// Get damage reduction
float bolsterReduction = statusManager.GetBolsterDamageReduction(); // 0% to 20%

// Get energy gain reduction
float chilledReduction = statusManager.GetChilledEnergyReduction(); // 0% to 30%

// Consume Vulnerability after damage
statusManager.ConsumeVulnerability();
```

## Integration with Damage System

When applying status effects from card damage, you need to pass the source damage:

```csharp
// Example: Apply Bleeding from a physical attack
float physicalDamage = 100f;
StatusEffect bleed = StatusEffectFactory.CreateBleeding(physicalDamage, 5);
statusManager.AddStatusEffect(bleed);

// Example: Apply Poison from mixed damage
float physicalDmg = 50f;
float chaosDmg = 50f;
StatusEffect poison = StatusEffectFactory.CreatePoison(physicalDmg, chaosDmg, 2);
statusManager.AddStatusEffect(poison);
```

## Damage Calculation Integration

Status effects that modify damage should be checked during damage calculation:

```csharp
// In damage calculation code:
float baseDamage = 100f;

// Apply Shocked multiplier
StatusEffectManager statusMgr = target.GetStatusEffectManager();
float shockedMultiplier = statusMgr.GetShockedDamageMultiplier();
baseDamage *= shockedMultiplier;

// Apply Vulnerability multiplier
float vulnerableMultiplier = statusMgr.GetVulnerabilityDamageMultiplier();
baseDamage *= vulnerableMultiplier;

// Consume Vulnerability after damage
if (vulnerableMultiplier > 1f)
{
    statusMgr.ConsumeVulnerability();
}

// Apply Bolster damage reduction
float bolsterReduction = statusMgr.GetBolsterDamageReduction();
baseDamage *= (1f - bolsterReduction);
```

## Action Prevention

Frozen and Stagger prevent actions. Check before allowing actions:

```csharp
StatusEffectManager statusMgr = entity.GetStatusEffectManager();
if (!statusMgr.CanAct())
{
    // Entity cannot act this turn
    return;
}
```

## Notes

1. **Bleeding** can only be applied by physical damage from attacks (not spells)
2. **Poison** stacks independently - each application creates a new instance
3. **Ignite** ticks per second (tickInterval = 1f), not per turn
4. **Chilled** always applies when cold damage is dealt
5. **Vulnerability** is consumed after one damage instance
6. **Bolster** is automatically capped at 10 stacks (20% max)
7. **Crumble** deals stored damage when it expires

## Migration Notes

Old code that creates status effects directly:
```csharp
// OLD (still works but not recommended)
StatusEffect poison = new StatusEffect(StatusEffectType.Poison, "Poison", 10f, 3, true);
```

New recommended approach:
```csharp
// NEW (uses source damage calculation)
StatusEffect poison = StatusEffectFactory.CreatePoison(50f, 50f, 2);
```

The old approach still works for effects that don't need source damage (like stat buffs), but for ailments, use `StatusEffectFactory` for proper calculations.

