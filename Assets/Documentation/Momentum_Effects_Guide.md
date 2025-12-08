# Momentum Effects System Guide

## Overview

The Momentum Effects system allows cards to have conditional effects that trigger based on the player's current Momentum stacks. These effects can modify card behavior, targeting, damage, and costs dynamically during combat.

## Table of Contents

1. [How Momentum Effects Work](#how-momentum-effects-work)
2. [Setting Up Momentum Effects on Cards](#setting-up-momentum-effects-on-cards)
3. [Momentum Effect Types](#momentum-effect-types)
4. [Card Play Pipeline Integration](#card-play-pipeline-integration)
5. [Prepared Cards and Momentum](#prepared-cards-and-momentum)
6. [Cost Display Updates](#cost-display-updates)
7. [Examples](#examples)
8. [Troubleshooting](#troubleshooting)

---

## How Momentum Effects Work

### Momentum Threshold Format

Momentum effects are defined in the card's `momentumEffectDescription` field using a specific format:

```
X+ Momentum: Effect description
```

or

```
X Momentum: Effect description (exact threshold)
```

**Examples:**
- `6+ Momentum: This card costs 1 less.` - Triggers when you have 6 or more momentum
- `10 Momentum: Gain 1 Energy.` - Triggers only when you have exactly 10 momentum

### Processing Flow

1. **Card Play Initiated**: When a card is played, `CardEffectProcessor.ApplyCardToEnemy()` is called
2. **Momentum Check**: `ProcessMomentumThresholdEffects()` evaluates the player's current momentum
3. **Effect Application**: `ApplyMomentumThresholdResult()` applies the parsed effects
4. **Card Execution**: The card is processed with momentum modifications applied

---

## Setting Up Momentum Effects on Cards

### Step 1: Add Momentum Effect Description

In your `CardDataExtended` asset, set the `momentumEffectDescription` field:

```
6+ Momentum: This card costs 1 less.
8+ Momentum: Deal damage to all enemies.
10 Momentum: Gain 1 Energy.
```

### Step 2: Ensure Card Type is Correct

Momentum effects work with all card types:
- **Attack Cards**: Can convert to AoE, multi-hit, random targets
- **Guard Cards**: Can gain additional guard, apply buffs
- **Skill Cards**: Can draw cards, gain energy, apply ailments
- **Power Cards**: Can grant temporary stat boosts, trigger special effects

### Step 3: Test in Combat

1. Gain momentum using cards with `GainMomentum` effects
2. Play the card with momentum effects
3. Verify the effects trigger at the correct thresholds

---

## Momentum Effect Types

### 1. Cost Reduction

**Format:** `X+ Momentum: This card costs Y less.`

**Example:**
```
6+ Momentum: This card costs 1 less.
```

**How it works:**
- Reduces the card's mana cost by the specified amount
- Applied during cost calculation in `CombatDeckManager.GetDisplayCost()`
- Card cost display updates automatically when momentum changes

**Implementation:**
- Parsed by `MomentumThresholdEffectParser.ParseEffectType()` → `CostReduction`
- Applied in `CombatDeckManager.ApplyManaCostModifierInternal()`

### 2. Convert to AoE

**Format:** `X+ Momentum: Deal damage to all enemies.`

**Example:**
```
8+ Momentum: Deal damage to all enemies.
```

**How it works:**
- Converts single-target attack to AoE
- Hits all active enemies
- Preserves multi-hit if the card is multi-hit

**Implementation:**
- Parsed as `ConvertToAoE` effect type
- Applied in `CardEffectProcessor.ApplyAttackCard()` before damage calculation
- Routes to `ApplyAoECard()` or `ApplyMultiHitAoE()` coroutine

### 3. Random Targets

**Format:** `X+ Momentum: Deal damage to Y random enemies.`

**Example:**
```
5+ Momentum: Deal damage to 2 random enemies.
```

**How it works:**
- Changes targeting from single target to random enemies
- Selects the specified number of random targets
- Preserves multi-hit if the card is multi-hit

**Implementation:**
- Parsed as `RandomTargets` effect type
- Applied in `CardEffectProcessor.ApplyAttackCard()`
- Routes to `ApplyMultiHitRandomTargets()` coroutine if multi-hit

### 4. Additional Momentum Gain

**Format:** `X+ Momentum: Gain Y Momentum instead.` or `X+ Momentum: Gain Y additional Momentum.`

**Example:**
```
3+ Momentum: Gain 2 Momentum instead.
```

**How it works:**
- Replaces or adds to the base momentum gain from card effects
- "instead" replaces the base gain, "additional" adds to it

**Implementation:**
- Parsed as `AdditionalMomentum` effect type
- Applied in `CardEffectProcessor.ApplyOnPlayEffects()`
- Handles `replaceMomentumGain` flag

### 5. Draw Cards

**Format:** `X+ Momentum: Draw Y cards.`

**Example:**
```
7+ Momentum: Draw 1 card.
```

**How it works:**
- Draws the specified number of cards from the deck
- Applied after the card is played

**Implementation:**
- Parsed as `DrawCards` effect type
- Applied in `CardEffectProcessor.ApplyMomentumThresholdResult()`
- Uses `CombatDeckManager.DrawCards()`

### 6. Temporary Stat Boost

**Format:** `X+ Momentum: Gain Y temporary Strength/Dexterity/Intelligence.`

**Example:**
```
5+ Momentum: Gain 2 temporary Strength.
```

**How it works:**
- Grants temporary stat increases for the rest of combat
- Applied via `StatusEffectManager`

**Implementation:**
- Parsed as `TemporaryStatBoost` effect type
- Applied in `CardEffectProcessor.ApplyTemporaryStatBuff()`

### 7. Energy Gain

**Format:** `X+ Momentum: Gain Y Energy.`

**Example:**
```
10 Momentum: Gain 1 Energy.
```

**How it works:**
- Restores the player's energy/mana
- Applied immediately after card play

**Implementation:**
- Parsed as `EnergyGain` effect type
- Applied in `CardEffectProcessor.ApplyMomentumThresholdResult()`
- Uses `Character.RestoreMana()`

### 8. Apply Ailment

**Format:** `X+ Momentum: Apply Y Bleed/Chill/etc. to all enemies.`

**Example:**
```
5+ Momentum: Apply 1 Bleed to all enemies.
```

**How it works:**
- Applies status effects to enemies
- Can target all enemies or specific targets

**Implementation:**
- Parsed as `ApplyAilment` effect type
- Applied in `CardEffectProcessor.ApplyMomentumThresholdResult()`
- Uses `StatusEffectManager.ApplyStatusEffect()`

### 9. Special Effects

**Format:** `X+ Momentum: Trigger Adrenaline Burst.`

**Example:**
```
10 Momentum: Trigger Adrenaline Burst.
```

**How it works:**
- Triggers special combat effects
- Custom logic per effect type

**Implementation:**
- Parsed as `SpecialEffect` effect type
- Custom handling in `CardEffectProcessor.ApplyMomentumThresholdResult()`

---

## Card Play Pipeline Integration

### Attack Cards

**Flow:**
1. `ApplyAttackCard()` is called
2. `ProcessMomentumThresholdEffects()` evaluates momentum
3. If AoE conversion: Routes to `ApplyAoECard()` or `ApplyMultiHitAoE()`
4. If random targets: Routes to `ApplyMultiHitRandomTargets()`
5. Otherwise: Continues with normal single-target processing
6. `ApplyOnPlayEffects()` processes `GainMomentum` effects
7. `ApplyMomentumThresholdResult()` applies other effects (draw, energy, etc.)

**Key Points:**
- Momentum effects are processed **before** damage calculation
- Multi-hit detection happens **before** momentum effects that might return early
- AoE and random target conversions preserve multi-hit behavior

### Guard Cards

**Flow:**
1. `ApplyGuardCard()` is called
2. `ProcessMomentumThresholdEffects()` evaluates momentum
3. Guard is calculated with momentum bonuses
4. `ApplyMomentumThresholdResult()` applies effects (draw, energy, etc.)

### Skill Cards

**Flow:**
1. `ApplySkillCard()` is called
2. `ProcessMomentumThresholdEffects()` evaluates momentum
3. `ApplyOnPlayEffects()` processes `GainMomentum` effects
4. `ApplyMomentumThresholdResult()` applies effects

### Power Cards

**Flow:**
1. `ApplyPowerCard()` is called
2. `ProcessMomentumThresholdEffects()` evaluates momentum
3. `ApplyOnPlayEffects()` processes `GainMomentum` effects
4. `ApplyMomentumThresholdResult()` applies effects

---

## Prepared Cards and Momentum

### How It Works

When a prepared card is unleashed, momentum effects are **preserved**:

1. `PreparationManager.DealUnleashDamage()` converts `PreparedCard.sourceCard` to `Card`
2. Sets `card.baseDamage` to the calculated unleash damage (preserves preparation multiplier)
3. Calls `CardEffectProcessor.ApplyCardToEnemy()` which processes momentum effects
4. Momentum threshold effects (AoE, multi-hit, random targets) are applied normally

### Example

**Card:** "Infernal Charge" (Attack card)
- Base damage: 5
- Prepared for 2 turns: 5 × (1 + 1.0) = 10 damage
- Momentum effect: `8+ Momentum: Deal damage to all enemies.`
- Player has 8 momentum

**Result:** Unleashes for 10 damage to **all enemies** (AoE conversion applied)

---

## Cost Display Updates

### Automatic Updates

Card cost displays update automatically when momentum changes:

1. `DeckBuilderCardUI` subscribes to `StackSystem.OnStacksChanged` for `StackType.Momentum`
2. When momentum changes, `UpdateDisplay()` is called
3. `CombatDeckManager.GetDisplayCost()` calculates the modified cost
4. Cost text is updated to show the new value

### Implementation

**In `DeckBuilderCardUI.cs`:**
```csharp
private void OnEnable()
{
    if (StackSystem.Instance != null)
    {
        StackSystem.Instance.OnStacksChanged += OnMomentumChanged;
    }
}

private void OnMomentumChanged(StackType stackType, int value)
{
    if (stackType == StackType.Momentum && cardData != null)
    {
        UpdateDisplay(); // Refreshes cost display
    }
}
```

**Cost Calculation:**
```csharp
int displayCost = CombatDeckManager.GetDisplayCost(extendedCard, cardData.playCost, ownerCharacter);
```

---

## Examples

### Example 1: Devastating Blow

**Card Data:**
- Type: Attack
- Base Cost: 2
- Base Damage: 8
- Momentum Effects:
  ```
  6+ Momentum: This card costs 1 less.
  8+ Momentum: Deal damage to all enemies.
  10 Momentum: Gain 1 Energy.
  ```

**Behavior:**
- **0-5 Momentum**: Costs 2, deals 8 damage to single target
- **6-7 Momentum**: Costs 1, deals 8 damage to single target
- **8-9 Momentum**: Costs 1, deals 8 damage to **all enemies** (AoE)
- **10 Momentum**: Costs 1, deals 8 damage to all enemies, **gains 1 Energy**

### Example 2: One-Two Punch (Multi-Hit)

**Card Data:**
- Type: Attack
- Base Cost: 1
- Base Damage: 4
- Is Multi-Hit: Yes
- Hit Count: 2
- Momentum Effects:
  ```
  5+ Momentum: Deal damage to 2 random enemies.
  ```

**Behavior:**
- **0-4 Momentum**: Costs 1, deals 4 damage twice to single target (2 hits)
- **5+ Momentum**: Costs 1, deals 4 damage twice to **2 random enemies** (each random target gets 2 hits)

### Example 3: Momentum Spike (Per Momentum Spent)

**Card Data:**
- Type: Attack
- Base Cost: 2
- Base Damage: 3
- Is AoE: Yes (from description "to all enemies")
- Description: "Spend all Momentum. Deal {damage} damage per Momentum spent to all enemies."
- Momentum Effects:
  ```
  5+ Momentum: Apply 1 Bleed to all enemies.
  7+ Momentum: Gain +1 Energy next turn.
  ```

**Behavior:**
- Spends all momentum
- Deals (3 + dex/6) × momentum spent damage to all enemies
- If 5+ momentum was spent: Applies 1 Bleed to all enemies
- If 7+ momentum was spent: Gains 1 Energy

---

## Troubleshooting

### Momentum Effects Not Triggering

**Check:**
1. Is `momentumEffectDescription` set correctly in the card asset?
2. Does the description match the expected format? (`X+ Momentum: Effect`)
3. Is the player's momentum high enough for the threshold?
4. Check console logs for momentum effect parsing/application

**Debug Logs:**
- `[Momentum] Processing momentum threshold effects for [CardName]`
- `[Momentum Threshold] [CardName]: Cost reduced by X`
- `[Momentum Effect] Card converted to AoE`

### Cost Not Updating

**Check:**
1. Is `DeckBuilderCardUI` subscribed to momentum changes? (Check `OnEnable()`)
2. Is `StackSystem.Instance` available?
3. Is `ownerCharacter` set correctly? (Required for cost calculation)

**Fix:**
- Ensure `Initialize(CardData, DeckBuilderUI, Character)` is called with character parameter
- Check that `StackSystem.OnStacksChanged` event is firing

### Prepared Cards Not Applying Momentum Effects

**Check:**
1. Is `PreparationManager.DealUnleashDamage()` using `CardEffectProcessor`?
2. Is the card converted properly using `ToCard()`?
3. Is `card.baseDamage` set to the unleash damage?

**Fix:**
- Ensure `DealUnleashDamage()` calls `CardEffectProcessor.ApplyCardToEnemy()`
- Verify the card's momentum effects are in `momentumEffectDescription`

### Multi-Hit Not Working with Momentum Effects

**Check:**
1. Is `isMultiHit` checked in the card asset?
2. Is `hitCount` set correctly?
3. Are momentum effects processed before multi-hit logic?

**Fix:**
- Multi-hit detection happens early in `ApplyAttackCard()` before momentum effects
- AoE/random target momentum effects preserve multi-hit via special coroutines

---

## File References

### Key Files

- **`MomentumThresholdEffectParser.cs`**: Parses momentum effect descriptions
- **`CardEffectProcessor.cs`**: Processes momentum effects during card play
- **`CombatDeckManager.cs`**: Handles cost reduction and display
- **`PreparationManager.cs`**: Ensures momentum effects work with prepared cards
- **`DeckBuilderCardUI.cs`**: Updates cost display when momentum changes

### Key Methods

- `MomentumThresholdEffectParser.ParseThresholdEffects()`: Parses effect descriptions
- `CardEffectProcessor.ProcessMomentumThresholdEffects()`: Evaluates applicable effects
- `CardEffectProcessor.ApplyMomentumThresholdResult()`: Applies the effects
- `CombatDeckManager.GetDisplayCost()`: Calculates display cost with momentum reductions
- `PreparationManager.DealUnleashDamage()`: Applies momentum effects to unleashed cards

---

## Best Practices

1. **Test Thresholds**: Always test cards at different momentum levels to ensure effects trigger correctly
2. **Clear Descriptions**: Use clear, consistent wording in momentum effect descriptions
3. **Multiple Effects**: You can have multiple momentum thresholds on one card (they all apply if conditions are met)
4. **Cost Reduction First**: Cost reduction is applied during cost calculation, before the card is played
5. **AoE/Random Before Multi-Hit**: AoE and random target conversions happen before multi-hit processing
6. **Prepared Cards**: Remember that prepared cards preserve momentum effects when unleashed

---

## Future Enhancements

Potential areas for expansion:
- Momentum decay effects
- Momentum-based card draw
- Momentum threshold visual indicators on cards
- Momentum spending effects (beyond "per Momentum spent" damage)
- Momentum-based status effect applications

---

*Last Updated: [Current Date]*
*Version: 1.0*













