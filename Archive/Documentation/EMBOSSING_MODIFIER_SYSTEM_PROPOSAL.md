# Embossing Modifier System - Migration Proposal

## Overview

Migrate the Embossing system to use the same modifier registry pattern as Reliance Auras and Ascendancy skills. This will:
- Make embossings affect card effects immediately through event-driven system
- Provide consistent architecture across all modifier systems
- Enable more complex and flexible embossing effects
- Allow easier balancing and iteration

## Current System Issues

1. **Hardcoded Switch Statements**: `EmbossingEffectProcessor` uses switch statements for each effect type
2. **Limited Event Integration**: Effects only apply at specific points (damage calculation, status application)
3. **No Event-Driven Processing**: Can't easily hook into card play events, turn events, etc.
4. **Inconsistent with Other Systems**: Reliance Auras and Ascendancy use modifier registry pattern

## Proposed Architecture

### 1. EmbossingModifierDefinition
Similar to `RelianceAuraModifierDefinition`, but for embossings:
- Links to `EmbossingEffect` asset
- Contains `ModifierEffect` list (event-driven)
- Supports level scaling (embossing level 1-20)
- Uses same `ModifierActionType` enum

### 2. EmbossingModifierRegistry
Singleton registry that:
- Loads `EmbossingModifierDefinition` assets from Resources
- Provides methods to get modifiers by embossing ID
- Handles level scaling (similar to aura scaling)
- Returns active effects for a card's embossings

### 3. EmbossingModifierEventProcessor
Event processor that:
- Subscribes to combat events (`OnCardPlayed`, `OnDamageDealt`, etc.)
- Processes embossing modifiers for active cards
- Routes actions to `ModifierEffectResolver` or `RelianceAuraModifierEffectResolver`

### 4. Migration Strategy

**Phase 1: Foundation**
- Create `EmbossingModifierDefinition` ScriptableObject
- Create `EmbossingModifierRegistry` singleton
- Create `EmbossingModifierEventProcessor` component

**Phase 2: Modifier Generation**
- Create editor tool to generate modifier definitions from existing `EmbossingEffect` assets
- Map `EmbossingEffectType` to `ModifierActionType`
- Generate appropriate event types and parameters

**Phase 3: Integration**
- Update `CardEffectProcessor` to trigger embossing events
- Replace `EmbossingEffectProcessor` calls with event-driven processing
- Test all existing embossing effects

**Phase 4: Rebalancing**
- Review all embossings for power level
- Adjust values to fit game balance
- Remove/consolidate effects that don't fit

## Benefits

1. **Immediate Effect Application**: Embossings affect cards as soon as they're played
2. **Consistent Architecture**: Same pattern as Auras and Ascendancy
3. **Event-Driven**: Can hook into any combat event
4. **Easier Balancing**: Centralized modifier definitions
5. **More Flexible**: Can create complex effects using action combinations

## Example: Damage Multiplier Embossing

**Current:**
```csharp
// In EmbossingEffectProcessor
case EmbossingEffectType.DamageMultiplier:
    additiveMultiplier += embossingValue;
    break;
```

**New:**
```csharp
// EmbossingModifierDefinition asset
- modifierId: "of_ferocity"
- linkedEmbossingId: "of_ferocity"
- effects:
  - eventType: OnCardPlayed
    actions:
      - actionType: AddDamageMorePercent
        parameters:
          percent: 0.35
          damageType: "all"
```

## Next Steps

1. Review and approve this proposal
2. Create foundation classes (ModifierDefinition, Registry, EventProcessor)
3. Create migration tool to convert existing embossings
4. Test with a few embossings first
5. Migrate all embossings
6. Rebalance values

