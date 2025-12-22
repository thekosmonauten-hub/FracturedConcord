# Ranger Cards Implementation Summary

## Overview
Implemented dynamic descriptions and Dexterity-scaled evasion for all Ranger starter deck cards.

## System Changes

### 1. CardDataExtended Extensions
- **Added `evasionScaling`**: `AttributeScaling` field for Dexterity-based evasion scaling (similar to `guardScaling`)
- **Added `baseEvasion`**: Base evasion amount for Skill cards that grant evasion
- **Added `evasionDuration`**: Duration in turns for evasion buff (-1 = rest of combat)

### 2. Dynamic Description Support
- **Added `{evasion}` placeholder**: Shows total calculated evasion (flat or percentage)
- **Added `{evasionDexDivisor}` placeholder**: Shows Dexterity divisor bonus (e.g., Dex/2)
- **Added `{evasionDexBonus}` placeholder**: Shows Dexterity scaling bonus
- **Supports percentage-based evasion**: Automatically detects "% increased evasion" in descriptions

### 3. CardEffectProcessor Updates
- **Added `ApplyEvasionBuff()` method**: Applies `TempEvasion` status effect to player
- **Supports flat evasion**: Base + Dexterity scaling (e.g., Quickstep: 20 + Dex/2)
- **Supports percentage evasion**: Extracts percentage from description (e.g., Focus: 20% increased)
- **Delayed card bonus**: +30% evasion for delayed Skill cards
- **Auto-detection**: Finds `CardDataExtended` from `Card.sourceCardData` or by name lookup

## Card Updates

### Quickstep
- **Description**: `Gain {evasion} Evasion for the rest of combat.`
- **Base Evasion**: 20
- **Dexterity Scaling**: `dexterityDivisor: 2` (Dex/2)
- **Duration**: -1 (rest of combat)
- **Effect**: Applies flat evasion rating that scales with Dexterity

### Dodge
- **Description**: `Gain {guard} Guard for the rest of combat.`
- **Base Guard**: 15
- **Dexterity Scaling**: `guardScaling.dexterityDivisor: 5` (Dex/5) - already configured
- **Effect**: Guard card with Dexterity scaling (no evasion)

### Multi-Shot
- **Description**: `Deal {damage} physical damage to all enemies.`
- **Base Damage**: 10
- **Dexterity Scaling**: `damageScaling.dexterityDivisor: 2` (Dex/2) - **NEW**
- **Effect**: AoE attack with Dexterity scaling

### PackHunter
- **Description**: `Deal {damage} physical damage to a single enemy.`
- **Base Damage**: 5
- **Dexterity Scaling**: `damageScaling.dexterityDivisor: 2` (Dex/2) - already configured
- **Effect**: Single-target attack with Dexterity scaling

### Focus
- **Description**: `Recover 1 mana, or gain +1 temporary max mana for 1 turn if already full. Gain 20% increased evasion for 2 turns.`
- **Evasion Type**: Percentage-based (20% increased)
- **Duration**: 2 turns
- **Effect**: Applies 20% increased evasion (extracted from description)
- **Note**: Mana recovery effects need separate implementation

### PoisonArrow
- **Description**: `Deal 5 physical damage and apply 2 Poison stacks to all enemies.`
- **No changes needed**: Already properly configured

## How It Works

### Flat Evasion (Quickstep)
1. Base evasion (20) + Dexterity scaling (Dex/2) = total flat evasion
2. Applied as `TempEvasion` status effect
3. `TempEvasion` applies magnitude/100 as `increasedEvasion` percentage
4. For flat evasion, system converts to percentage based on base evasion rating

### Percentage Evasion (Focus)
1. System detects "% increased evasion" in description
2. Extracts percentage value (e.g., "20% increased" → 20)
3. Applies as `TempEvasion` with magnitude = percentage
4. `TempEvasion` applies 20/100 = 0.20 = 20% increased evasion

### Dynamic Descriptions
- `{evasion}`: Shows total calculated evasion
- `{damage}`: Shows total calculated damage (base + scaling + weapon)
- `{guard}`: Shows total calculated guard (base + scaling)
- `{dex}`: Shows current Dexterity value
- `{dexDivisor}`: Shows Dex/divisor bonus (for damage/guard)
- `{evasionDexDivisor}`: Shows Dex/divisor bonus (for evasion)

## Testing Checklist
- [ ] Quickstep applies evasion that scales with Dexterity
- [ ] Quickstep description shows correct evasion value
- [ ] Focus applies 20% increased evasion for 2 turns
- [ ] Multi-Shot damage scales with Dexterity
- [ ] PackHunter damage scales with Dexterity
- [ ] Dodge guard scales with Dexterity
- [ ] All dynamic descriptions update correctly
- [ ] Delayed cards get +30% evasion bonus

## Notes
- Focus card also has mana recovery effects that may need separate implementation
- Flat evasion conversion to percentage may need tuning based on player base evasion values
- All cards now use dynamic placeholders for better clarity and scaling visibility

