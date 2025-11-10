# 🔮 Preparation System - Complete Guide

## Overview

The **Preparation System** allows players to store cards for future turns, building up power over time before unleashing devastating effects. This adds a strategic timing layer to combat, rewarding planning and patience.

---

## 📋 Table of Contents

1. [Core Concept](#core-concept)
2. [System Architecture](#system-architecture)
3. [Creating Preparation Cards](#creating-preparation-cards)
4. [Integration Guide](#integration-guide)
5. [Stat Synergies](#stat-synergies)
6. [UI Setup](#ui-setup)
7. [Example Cards](#example-cards)
8. [Troubleshooting](#troubleshooting)

---

## Core Concept

### **How It Works**

1. **Prepare**: Play a card with the "Prepare" tag → It moves to a special "Prepared Zone"
2. **Charge**: Each turn, the card gains bonus multipliers based on stats
3. **Unleash**: Click the prepared card OR play an "Unleash" card to trigger it

### **Key Features**

- **Accumulating Power**: Cards gain +50%+ damage per turn (configurable)
- **Stat Scaling**: INT/DEX/STR all enhance prepared cards differently
- **Multiple Triggers**: Manual click, Unleash cards, or auto-unleash
- **Visual Feedback**: Glowing cards with turn counters

---

## System Architecture

### **Core Components**

```
Assets/Scripts/CombatSystem/Preparation/
├── PreparedCard.cs              // Data structure for a single prepared card
├── PreparationManager.cs        // Singleton manager (handles all logic)
└── PreparedCardsUI.cs          // Visual display component
```

### **Flow Diagram**

```
[Play Card with "Prepare" tag]
           ↓
[PreparationManager.PrepareCard()]
           ↓
[PreparedCard created & stored]
           ↓
[Each Turn: PreparedCard.OnTurnEnd()]
           ├→ Charge counter increases
           ├→ Multipliers accumulate
           └→ Check for auto-unleash
           ↓
[Unleash Trigger]
   ├→ Manual Click (costs energy)
   ├→ Unleash Card (free)
   └→ Auto-Decay (free, reduced power)
           ↓
[PreparationManager.UnleashCard()]
           ↓
[Apply stored damage + bonuses]
```

---

## Creating Preparation Cards

### **Step 1: Create CardDataExtended Asset**

1. Right-click in Project → `Create > Dexiled > Cards > Card Data Extended`
2. Name it (e.g., `InfernalCharge_Extended`)

### **Step 2: Configure Preparation Fields**

In the Inspector, scroll to **"Preparation System"** section:

| Field | Description | Example |
|-------|-------------|---------|
| `canPrepare` | Enable preparation for this card | `true` |
| `maxPrepTurns` | Max turns it can be stored (base) | `3` |
| `multiplierPerTurn` | Bonus per turn (0.5 = +50%) | `0.5` |
| `unleashCondition` | How it unleashes | `"manual"` |
| `unleashEffect` | What happens when unleashed | `"deal_stored_damage"` |
| `prepareDescription` | Flavor text | `"Charges over time..."` |

### **Step 3: Add Tags**

Add these tags to the `tags` list:
- **Required**: `"Prepare"` - Marks card as preparable
- **Optional**: `"Fire"`, `"Combo"`, etc. for synergies

### **Step 4: Configure Stats**

Set normal card stats:
- `baseDamage` - Initial damage value
- `playCost` - Energy cost to prepare
- `damageScaling` - STR/DEX/INT scaling
- `isAoE` - Whether unleash hits all enemies

---

## Unleash Conditions

Choose how the card unleashes:

| Condition | Behavior | Use Case |
|-----------|----------|----------|
| `manual` | Click to unleash (costs energy) | Default for all cards |
| `auto_on_max` | Auto-unleashes at max turns | Forced timing cards |
| `triggered` | Needs specific Unleash card | Combo-focused decks |
| `decay` | Auto after max + decay penalty | Risk/reward cards |

---

## Unleash Effects

Choose what happens when unleashed:

| Effect | Behavior | Example |
|--------|----------|---------|
| `deal_stored_damage` | Deals accumulated damage to enemies | Infernal Charge |
| `apply_buffs` | Applies stored buffs to player | Power cards |
| `hybrid` | Both damage AND buffs | Combo cards |

---

## Integration Guide

### **1. Add PreparationManager to Combat Scene**

```csharp
// In your combat scene:
GameObject prepManager = new GameObject("PreparationManager");
prepManager.AddComponent<PreparationManager>();
```

Or simply call `PreparationManager.Instance` - it auto-creates!

### **2. Add PreparedCardsUI Component**

```csharp
// In your combat UI hierarchy:
GameObject prepUIObj = new GameObject("PreparedCardsUI");
PreparedCardsUI prepUI = prepUIObj.AddComponent<PreparedCardsUI>();

// Assign references:
prepUI.preparedCardPrefab = yourCardPrefab; // Same as hand cards
prepUI.playerCharacterTransform = playerTransform;
```

### **3. Link to PreparationManager**

```csharp
PreparationManager.Instance.preparedCardsUI = prepUI;
```

### **4. Test It!**

- Create a deck with a Preparation card
- Enter combat
- Play the card → Should move to prepared zone
- End turn → Counter increases
- Click prepared card → Unleashes with bonus damage!

---

## Stat Synergies

### **Intelligence (INT)**

**Effect**: Increases maximum storage duration

```
Base Max Turns: 3
INT Bonus: +1 turn per 20 INT
Example: 40 INT → 5 max turns
```

**Code Location**: `PreparedCard.UpdateStatBonuses()`

### **Dexterity (DEX)**

**Effect**: Faster charging (multi-charges per turn)

| DEX | Charges/Turn | Effect |
|-----|--------------|--------|
| 0-29 | 1 | Normal charging |
| 30-49 | 2 | 2x faster |
| 50+ | 3 | 3x faster + Overcharge |

**Overcharge**: At 30+ DEX, cards can charge beyond max turns without decay!

**Code Location**: `PreparedCard.CalculateChargeRate()`

### **Strength (STR)**

**Effect**: Adds flat damage per turn

```
Bonus = STR × 0.5 × turns_prepared
Example: 30 STR, 3 turns = +45 damage
```

**Code Location**: `PreparedCard.UpdateStatBonuses()`

### **Combined Multiplier**

All stats contribute to the damage multiplier:

```
Final Damage = BaseDamage × (1 + multiplier) + STR_bonus
Example:
  Base: 10 damage
  3 turns × 0.5 = +150% (15 damage)
  STR bonus: +15
  Total: 40 damage
```

---

## UI Setup

### **Positioning**

Prepared cards appear **near the player character sprite** in combat.

Configure in `PreparedCardsUI`:
- `cardSpacing`: Horizontal spacing (default: 120px)
- `verticalOffset`: Distance above player (default: 200px)

### **Visual Effects**

Each prepared card shows:
1. **Glow Effect**: Pulsing cyan aura
2. **Turn Counter Badge**: "2/3" indicator
3. **Color Coding**:
   - Cyan: Normal charging
   - Gold: Overcharged (DEX bonus)
   - Red: Decaying

### **Interaction**

- **Hover**: Card scales up (1.1x)
- **Click**: Attempts manual unleash
  - Success: Card unleashes with bonus damage
  - Fail: Shows "Not enough energy" feedback

---

## Example Cards

### **1. Infernal Charge** (AoE Fire Spell)

```yaml
canPrepare: true
maxPrepTurns: 3
multiplierPerTurn: 0.5
unleashCondition: manual
unleashEffect: deal_stored_damage
tags: [Fire, Spell, Prepare]
```

**Effect**: Prepare for 3 turns, gain +50% damage/turn, unleash for AoE Fire damage

**Strategy**: High INT builds (longer storage) + Fire synergies

### **2. Ambush** (Physical Attack)

```yaml
canPrepare: true
maxPrepTurns: 3
multiplierPerTurn: 0.4
unleashCondition: manual
unleashEffect: deal_stored_damage
tags: [Attack, Physical, Prepare, Combo]
```

**Special**: If unleashed after 2+ turns, **always Crits!**

**Strategy**: DEX builds (fast charging) + Weapon scaling

### **3. Cataclysmic Focus** (Power Card)

```yaml
canPrepare: false  // This card is NOT prepared
tags: [Power, Buff, Preparation]
```

**Effect**: While you have ANY prepared cards, gain +10% Spell Power per turn

**Strategy**: Play early, then prepare multiple spells for massive payoff

---

## Creating an "Unleash" Card

Want a card that triggers all prepared cards at once?

```yaml
cardName: "Unleash the Storm"
playCost: 1
tags: [Unleash]
canPrepare: false
description: "Unleash all prepared cards instantly."
```

**Code**: `CombatManager.PlayCard()` already handles "Unleash" tag!

---

## Troubleshooting

### **Cards Not Preparing**

**Check**:
1. `canPrepare = true` in Inspector
2. `"Prepare"` tag is added
3. PreparationManager exists in scene
4. Prepared slots not full (max 3)

**Debug**: Look for `[PreparationManager] Prepared card:` log

### **No Visual Feedback**

**Check**:
1. PreparedCardsUI component added to scene
2. `preparedCardPrefab` assigned
3. `playerCharacterTransform` assigned
4. Canvas has GraphicRaycaster

### **Unleash Not Working**

**Check**:
1. Enough energy for manual unleash
2. Check `unleashCondition` matches trigger
3. Look for `[PreparationManager] Unleashed:` log

### **Stat Bonuses Not Applying**

**Check**:
1. Character stats are correctly set
2. `PreparedCard.OnTurnEnd()` is called
3. Look for `[Preparation] stat bonuses:` log

---

## Advanced: Custom Unleash Effects

Want unique unleash behaviors? Modify `PreparationManager.ApplyUnleashEffect()`:

```csharp
switch (unleashEffect?.ToLower())
{
    case "my_custom_effect":
        // Your custom logic here
        ApplyCustomEffect(prepared, player);
        break;
        
    // ... existing cases ...
}
```

---

## Performance Notes

- **Max Slots**: Limited to 3 by default (configurable)
- **Turn Processing**: O(n) where n = prepared cards
- **Memory**: Each prepared card ~1KB
- **UI Updates**: Only on turn end + unleash events

---

## Future Enhancements

Possible additions:
- [ ] Preparation-specific relics/equipment
- [ ] Overcharge penalties (risk vs reward)
- [ ] Multi-target unleash selection
- [ ] Preparation chains (unleash triggers prepare)
- [ ] Decay visualization (cracking/fading cards)

---

## Summary

The Preparation System adds **temporal strategy** to combat:

✅ **Easy to Use**: Just add tags and configure Inspector fields
✅ **Flexible**: Multiple unleash methods and effects
✅ **Scalable**: Stat synergies reward build diversity
✅ **Visual**: Clear feedback via glowing cards and counters

**Next Steps**: Create your first Preparation card and test it in combat!

---

**Questions? Issues?** Check the TODO list in PreparedCard.cs or PreparationManager.cs for integration points.











