# Positive/Negative Status Effect Icons

## Overview

The StatusDatabase now supports separate icons for positive and negative values of buffs and temporary stats. This is useful for effects like:
- **TempStrength**: +2 Strength vs -2 Strength
- **TempDexterity**: +3 Dexterity vs -1 Dexterity  
- **TempIntelligence**: +5 Intelligence vs -2 Intelligence
- Any other buff that can have negative values

## How It Works

### StatusEffectData Fields

Each status effect entry in the database now has:
- **iconSprite**: Default icon (used when magnitude is 0 or no positive/negative icons are set)
- **positiveIconSprite**: Icon shown when magnitude > 0
- **negativeIconSprite**: Icon shown when magnitude < 0
- **usePositiveNegativeIcons**: Checkbox to enable this feature for the effect

### Automatic Icon Selection

The `StatusEffectIcon` component automatically:
1. Checks if the effect has `usePositiveNegativeIcons` enabled
2. Checks the current magnitude value
3. Selects the appropriate icon with this priority:
   - **magnitude > 0**: 
     1. Effect-specific `positiveIconSprite` (if set)
     2. Global `globalPositiveIcon` (if set)
     3. Default `iconSprite` (fallback)
   - **magnitude < 0**: 
     1. Effect-specific `negativeIconSprite` (if set)
     2. Global `globalNegativeIcon` (if set)
     3. Default `iconSprite` (fallback)
   - **magnitude == 0**: Uses `iconSprite`

### Icon Updates

The icon automatically updates when the magnitude changes (e.g., if a -2 Strength becomes -1 Strength, or if it becomes positive).

## Setup Instructions

### 1. Global Default Icons (Recommended)

1. Open your `StatusDatabase` asset
2. At the top, assign **Global Positive Icon** and **Global Negative Icon**
3. These will be used for ALL effects that have `usePositiveNegativeIcons` enabled
4. You can still override per-effect if needed

### 2. Per-Effect Icons (Optional - Override)

1. Find the status effect you want to configure (e.g., TempStrength)
2. Check **Use Positive Negative Icons**
3. Optionally assign effect-specific sprites:
   - **Icon Sprite**: Default/neutral icon (or fallback)
   - **Positive Icon Sprite**: Override for this specific effect (optional)
   - **Negative Icon Sprite**: Override for this specific effect (optional)
4. If not assigned, the global defaults will be used

### 2. Sprite Naming (Optional - for Resources.Load fallback)

If you're using `Resources.Load` as a fallback, name your sprites:
- `TempStrength` - Default icon
- `TempStrengthPositive` - Positive icon
- `TempStrengthNegative` - Negative icon

The system will automatically try to load these if the database doesn't have them assigned.

### 3. Using the Populator Tool

The `StatusDatabasePopulator` tool automatically:
- Enables `usePositiveNegativeIcons` for TempStrength, TempDexterity, and TempIntelligence
- Attempts to load positive/negative sprites from Resources if they exist

## Example: Quick Setup (Using Global Icons)

```
StatusDatabase Asset:
  Global Positive Icon: [Positive.png] - Used for all positive temp stats
  Global Negative Icon: [Negative.png] - Used for all negative temp stats

TempStrength Entry:
  Effect Type: Strength
  Effect Name: TempStrength
  Use Positive Negative Icons: ✓ (checked)
  Icon Sprite: [TempStrength.png] - Default/neutral (for magnitude = 0)
  Positive Icon Sprite: (empty - uses global)
  Negative Icon Sprite: (empty - uses global)
```

## Example: Per-Effect Override

```
TempStrength Entry:
  Effect Type: Strength
  Effect Name: TempStrength
  Use Positive Negative Icons: ✓ (checked)
  Icon Sprite: [TempStrength.png] - Default/neutral
  Positive Icon Sprite: [TempStrengthPositive.png] - Custom for this effect
  Negative Icon Sprite: [TempStrengthNegative.png] - Custom for this effect
```

## Which Effects Should Use This?

**Recommended for:**
- ✅ **TempStrength** - Can be positive or negative
- ✅ **TempDexterity** - Can be positive or negative
- ✅ **TempIntelligence** - Can be positive or negative
- ✅ Any other stat buff that can have negative values

**Not needed for:**
- ❌ **Poison** - Always negative (debuff)
- ❌ **Ignite** - Always negative (debuff)
- ❌ **Shield** - Always positive (buff)
- ❌ Effects that don't have magnitude or can't be negative

## Code Example

```csharp
// Creating a negative strength effect
StatusEffect negativeStrength = new StatusEffect(
    StatusEffectType.Strength, 
    "TempStrength", 
    -2f,  // Negative magnitude
    3, 
    false // Not a debuff (it's a stat modifier)
);

// The icon will automatically show the negative icon
statusManager.AddStatusEffect(negativeStrength);
```

## Visual Behavior

- **Positive values** (e.g., +2 Strength): Shows positive icon
- **Negative values** (e.g., -2 Strength): Shows negative icon  
- **Zero value**: Shows default icon
- **Icon updates automatically** when magnitude changes

## Fallback Behavior

If positive/negative icons aren't assigned:
1. System uses the default `iconSprite`
2. Falls back to `Resources.Load` with naming convention
3. Creates a default colored sprite as last resort

This ensures the system always displays something, even if icons aren't fully configured.

