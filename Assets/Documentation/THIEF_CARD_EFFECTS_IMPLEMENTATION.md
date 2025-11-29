# Thief Card Effects Implementation

## Overview
This document describes the implementation of Thief-specific card effects, including dual wield interactions and preparation bonuses.

## Implemented Features

### 1. Dual Wield Detection
- **System**: `ThiefCardEffects.IsDualWielding(Character player)`
- **Logic**: Checks if player has 2+ weapons equipped
- **Usage**: Automatically detects dual wielding for all Thief cards

### 2. Dual Wield Effects
All Thief cards have `dualWieldEffect` fields that trigger when dual wielding:

- **Twin Strike**: "Deal 1(+Dex/4) off-hand damage to all enemies."
- **Shadow Step**: "Gain 6 Guard (+Dex/6) and 250(+Dex*2) evasion"
- **Poisoned Blade**: "Apply 3 Poison and gain 1 temporary Dexterity."
- **Feint**: "Advance prepared cards charge by 2 instead."
- **Perfect Strike**: "Deal +4 (+dex/3) damage per consumed card instead."
- **Ambush**: "Deal +2 (+Dex/3) damage per prepared card instead."

### 3. Prepared Card Interactions

#### **Ambush**
- **Effect**: "+1 (+Dex/3) damage per prepared card"
- **Dual Wield**: "+2 (+Dex/3) damage per prepared card"
- **Implementation**: Checks prepared card count and adds bonus damage

#### **Poisoned Blade**
- **Effect**: "Apply +1 Poison per prepared card"
- **Implementation**: Applies additional Poison stacks based on prepared card count

#### **Perfect Strike**
- **Effect**: "Consume all prepared cards. Deal +2 (+dex/3) damage per consumed card"
- **Dual Wield**: "+4 (+dex/3) damage per consumed card"
- **Implementation**: Consumes all prepared cards and adds bonus damage

#### **Feint**
- **Effect**: "Advance prepared cards charge by 1"
- **Dual Wield**: "Advance prepared cards charge by 2"
- **Implementation**: Advances all prepared cards by the specified amount

### 4. Preparation Bonuses

#### **Twin Strike**
- **Base**: "Deal 5 physical damage (+Dex/4)"
- **Prepare**: "When this card is unleashed, deal 7 damage (+Dex/2)"
- **Status**: ✅ **IMPLEMENTED** - Uses preparation bonus damage when unleashed
- **Implementation**: `PreparedCard.CalculateUnleashDamage()` parses "Prepare:" text and uses 7 + Dex/2 instead of base damage

#### **Shadow Step**
- **Base**: "Gain 6 Guard (+Dex/6)"
- **Prepare**: "Gain +2 (+Dex/2) Guard and 1 temporary Dexterity"
- **Status**: ✅ **IMPLEMENTED** - Applies preparation bonus guard and temp dex when unleashed
- **Implementation**: `PreparationManager.ApplyUnleashBuffs()` parses "Prepare:" text and applies bonus guard + temp dex

## Integration Points

### CardEffectProcessor.cs
- `ApplyAttackCard()`: Handles Ambush, Perfect Strike dual wield effects
- `ApplyGuardCard()`: Handles Shadow Step dual wield effects
- `ApplySkillCard()`: Handles Feint, Poisoned Blade effects

### ThiefCardEffects.cs
- `IsDualWielding()`: Detects dual wielding
- `GetPreparedCardCount()`: Gets count of prepared cards
- `ConsumeAllPreparedCards()`: Consumes all prepared cards
- `AdvancePreparedCardCharges()`: Advances prepared card charges
- `ProcessDualWieldEffect()`: Parses and applies dual wield effects

## Implementation Details

### Preparation Bonus Parser
- **File**: `Assets/Scripts/CombatSystem/PreparationBonusParser.cs`
- **Methods**:
  - `ParsePreparationDamage()`: Extracts damage bonus from "Prepare:" text
  - `ParsePreparationGuard()`: Extracts guard bonus from "Prepare:" text
  - `ParsePreparationTempDexterity()`: Extracts temporary Dexterity bonus
  - `HasPreparationBonus()`: Checks if card has preparation bonus

### Integration Points
- **PreparedCard.CalculateUnleashDamage()**: Checks for preparation bonus and uses it instead of base damage
- **PreparationManager.ApplyUnleashBuffs()**: Applies preparation bonus guard and temporary stats

## Remaining Work

1. **Dynamic Descriptions**: Update card descriptions with dynamic placeholders for prepared card counts
2. **Testing**: Test all Thief card effects in combat

## Card Asset Configuration

### Required Fields
- `dualWieldEffect`: Text description of dual wield effect
- `description`: Main card description (may include "Prepare:" text)

### Example: Twin Strike
```yaml
description: 'Deal 5 physical damage (+Dex/4).\nPrepare: When this card is unleashed, deal 7 damage (+Dex/2).'
dualWieldEffect: 'Dual: Deal 1(+Dex/4) off-hand damage to all enemies.'
```

### Example: Shadow Step
```yaml
description: 'Gain 6 Guard (+Dex/6).\nPrepare: Gain +2 (+Dex/2) Guard and 1 temporary Dexterity.'
dualWieldEffect: Gain 6 Guard (+Dex/6) and 250(+Dex*2) evasion
```

