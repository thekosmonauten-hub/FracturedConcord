# Embossing Immediate Effects System

## Overview

Embossing effects are now calculated and displayed **immediately** when applied to cards, not just during combat. This allows players to see how embossings affect their cards in real-time.

## How It Works

### 1. CardStatCalculator

A new static class `CardStatCalculator` calculates card stats (damage, etc.) with embossings applied for preview/display purposes. It works outside of combat and provides:

- **Damage Breakdown**: Shows base damage, attribute scaling, weapon damage, and all embossing contributions
- **Multi-Element Support**: Handles conversions and "Extra" damage effects (e.g., "30% of Physical as Extra Fire")
- **Level Scaling**: Automatically scales embossing effects based on embossing level (1-20)

### 2. Dynamic Variable Resolution

When a card's description contains `{damage}`, it now resolves to the **total damage including embossings**:

**Example:**
- **Before Embossing**: Heavy Strike shows "100 damage"
- **After Embossing "Smouldering Metal" (30% Physical as Extra Fire)**: Shows "130 damage"
  - 100 Physical damage
  - +30 Fire damage (from embossing)

### 3. Tooltip Breakdown

When hovering over a card with embossings, the tooltip shows a detailed breakdown:

```
Damage Breakdown:
100 Physical (50 base + 30 attributes + 20 weapon)
+30 Fire (from embossings)
= 130 total damage
```

## Implementation Details

### CardStatCalculator.CalculateCardDamage()

This method:
1. Calculates base damage + attribute scaling + weapon damage
2. Applies embossing stat scaling (flat bonuses)
3. Applies embossing damage multipliers
4. Handles conversions and "Extra" damage effects
5. Returns a `DamageBreakdown` object with all details

### Card.GetDynamicDescription()

Updated to use `CardStatCalculator` instead of `DamageCalculator.CalculateCardDamage()`:
- Works outside of combat
- Includes embossing effects immediately
- Updates when embossings are applied/removed

### CardTooltipView

Shows damage breakdown when:
- Card has embossings applied
- Card has damage values
- Breakdown includes embossing contributions

## Supported Embossing Effects

### Damage Multipliers
- `DamageMultiplier`: Generic damage increase
- `PhysicalDamageMultiplier`: Physical damage only
- `ElementalDamageMultiplier`: Fire/Cold/Lightning
- `SpellDamageMultiplier`: Spell cards only

### Flat Bonuses
- `FlatDamageBonus`: Adds flat damage

### Conversions
- `PhysicalToFireConversion`: Converts % of Physical to Fire
- `PhysicalToColdConversion`: Converts % of Physical to Cold
- `PhysicalToLightningConversion`: Converts % of Physical to Lightning

### Extra Damage
- Effects with "Extra" in description (e.g., "30% of Physical as Extra Fire")
- Uses `elementType` to determine target damage type

### Stat Scaling
- `StrengthScaling`: Scales with Strength
- `DexterityScaling`: Scales with Dexterity
- `IntelligenceScaling`: Scales with Intelligence

## Example Usage

```csharp
// Calculate damage for preview
var breakdown = CardStatCalculator.CalculateCardDamage(card, character);

// Get total damage (includes embossings)
float totalDamage = breakdown.totalDamage; // e.g., 130

// Get breakdown text for tooltip
string breakdownText = breakdown.breakdownText;
// "100 Physical (50 base + 30 attributes + 20 weapon)\n+30 Fire (from embossings)\n= 130 total damage"
```

## Future: Modifier System Integration

When the full embossing modifier system is implemented:
- `EmbossingModifierDefinition` assets will define effects
- `EmbossingModifierRegistry` will provide active effects
- `CardStatCalculator` will use the registry for more complex effects
- Event-driven processing for combat-time effects

## Notes

- Embossing effects are calculated **immediately** when applied
- Card descriptions update **automatically** when embossings change
- Tooltips show **detailed breakdowns** on hover
- All calculations respect embossing **level scaling** (1-20)

