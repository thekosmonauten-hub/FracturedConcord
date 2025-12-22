# Apostle Discard System Implementation

## Overview
This document describes the implementation of the discard panel system and Apostle card effects that require discarding cards.

## Discard Panel System

### Components Created

1. **DiscardPanel.cs** (`Assets/Scripts/UI/Combat/DiscardPanel.cs`)
   - Manages the discard selection UI
   - Animates cards to center screen for selection
   - Handles card selection and return to hand
   - Integrates with `CombatDeckManager` for actual discarding

2. **DiscardCardParser.cs** (`Assets/Scripts/CombatSystem/DiscardCardParser.cs`)
   - Detects if a card requires discarding
   - Parses how many cards need to be discarded

3. **DiscardPowerCalculator.cs** (`Assets/Scripts/CombatSystem/DiscardPowerCalculator.cs`)
   - Calculates "Discard Power" from discarded card stats
   - Formula: `baseDamage + baseGuard + attribute scaling bonuses`

### Discard Panel Behavior

1. **When a discard card is played:**
   - Discard panel is enabled
   - Cards in hand fly to center screen (animated)
   - Cards are scaled up for better visibility
   - Instruction text shows "Select X card(s) to discard"

2. **During selection:**
   - Cards are clickable
   - Clicking a card selects it for discard
   - Cancel button available to abort

3. **After selection:**
   - Selected card is discarded (removed from hand, added to discard pile)
   - Remaining cards return to hand (animated)
   - Discard card's effect is processed
   - Discarded card's `ifDiscardedEffect` is processed (if any)

## Apostle Card Effects

### Cards Requiring Discard

1. **Forbidden Prayer**
   - Effect: "Discard 1 card: draw 3 cards."
   - Implementation: Shows discard panel, then draws 3 cards after discard

2. **Scripture Burn**
   - Effect: "Discard 1 card. Gain 6 Guard (+Int/2 + Discard Power/2) and 3 temporary Intelligence."
   - Implementation: Shows discard panel, calculates guard with discard power, applies temp Int

### Cards with ifDiscardedEffect

1. **Forbidden Prayer**
   - ifDiscardedEffect: "If discarded: Increase spell power by {intelligence/8}."
   - Status: ✅ **IMPLEMENTED** - Spell power system integrated
   - Implementation: Applies permanent SpellPower status effect. Each point of spell power = 1% increased spell damage. Spell power stacks additively.

2. **Scripture Burn**
   - ifDiscardedEffect: "If discarded: Gain 6 Guard (+Int/4 + Discard Power/4) and draw 1 card."
   - Implementation: ✅ Parses guard formula and draws card

3. **Sacred Strike**
   - ifDiscardedEffect: "If discarded: Deal {discardPower} chaos damage to all enemies."
   - Implementation: ✅ Deals discard power as chaos damage to all enemies

4. **Divine Wrath**
   - ifDiscardedEffect: "If discarded: Deal {discardPower} chaos damage to all enemies."
   - Implementation: ✅ Deals discard power as chaos damage to all enemies

5. **Divine Ward**
   - ifDiscardedEffect: "If discarded: Gain {discardPower} Guard."
   - Implementation: ✅ Gains discard power as guard

6. **Divine Favor**
   - ifDiscardedEffect: "If discarded: Gain {discardPower} Temporary Intelligence."
   - Implementation: ✅ Gains discard power as temporary Intelligence

### Special Effects

1. **Divine Favor** (Power Card)
   - Effect: "The next card you play applies their 'discarded' effect."
   - Implementation: ✅ Sets flag `nextCardAppliesDiscardedEffect`, consumed on next card play

## Discard Power Calculation

**Formula:**
```
Discard Power = baseDamage + baseGuard + damageScaling.CalculateScalingBonus(player) + guardScaling.CalculateScalingBonus(player)
```

**Example:**
- Card with 8 damage, 0 guard, Str/4 scaling, player has 16 Str
- Discard Power = 8 + 0 + (16/4) + 0 = 12

## Integration Points

### CombatDeckManager.cs
- `PlayCard()`: Detects discard cards and shows discard panel
- `ProcessDiscardCardEffect()`: Processes discard card after selection
- `ProcessIfDiscardedEffect()`: Processes ifDiscardedEffect when card is discarded
- `ProcessDiscardCardMainEffect()`: Handles main effects of discard cards
- `ContinueCardPlayAfterDiscard()`: Continues normal card play flow after discard

### CardEffectProcessor.cs
- `ApplyPowerCard()`: Handles Divine Favor effect

### DiscardPanel.cs
- `ShowDiscardSelection()`: Shows panel and animates cards
- `OnCardSelectedForDiscard()`: Handles card selection
- `CompleteDiscardSelection()`: Completes discard and returns cards

## Setup Instructions

### Unity Setup

1. **Create DiscardPanel GameObject:**
   - Add `DiscardPanel` component
   - Create UI panel (Canvas > Panel)
   - Add instruction text (TextMeshProUGUI)
   - Add cancel button
   - Create `cardTargetParent` (empty GameObject for animation targets)

2. **Assign References in CombatDeckManager:**
   - Drag DiscardPanel to `discardPanel` field

3. **Animation Targets:**
   - Create empty GameObjects as children of `cardTargetParent`
   - Position them in center screen (spaced horizontally)
   - Cards will fly to these positions

## Spell Power System

### Implementation Details

**Status Effect Type:** `StatusEffectType.SpellPower`

**How It Works:**
- Spell power is applied as a temporary status effect (duration 999 turns = effectively entire combat)
- Each point of spell power = 1% increased spell damage
- Spell power stacks additively (multiple sources combine)
- Applies to all spell cards (Skill, Power, or cards with "Spell" tag)
- Expires at the end of combat (or after 999 turns if combat somehow lasts that long)

**Integration Points:**
- `DamageCalculation.CalculateCardDamage()`: Applies spell power multiplier after unified stat calculation
- `CombatDeckManager.ProcessIfDiscardedSpellPower()`: Parses and applies spell power from ifDiscardedEffect
- `StatusEffectManager.GetTotalMagnitude()`: Sums all spell power effects for multiplier calculation

**Example:**
- Player has 16 Intelligence
- Forbidden Prayer discarded: 16 / 8 = 2 spell power
- Next spell deals 2% increased damage
- If discarded again: 2 + 2 = 4 spell power total (4% increased spell damage)
- Spell power persists for the entire combat, then expires when combat ends

## Remaining Work

1. **Testing**: Test all discard card effects in combat, including spell power stacking
2. **UI Polish**: Fine-tune animations and positioning
3. **Visual Feedback**: Consider adding spell power indicator in UI

## Card Asset Configuration

### Discard Cards
- Description must contain: "Discard X card" or "Discard X cards"
- Example: `"Discard 1 card: draw 3 cards."`

### ifDiscardedEffect Cards
- Set `ifDiscardedEffect` field with effect description
- Use `{discardPower}` placeholder for discard power value
- Example: `"If discarded: Deal {discardPower} chaos damage to all enemies."`

### Divine Favor
- Description: `"The next card you play applies their 'discarded' effect."`
- No special fields needed

