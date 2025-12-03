# Phase 4: Simple Abilities (Tier 1) - COMPLETE ✅

**Date:** December 3, 2025  
**Status:** ✅ **ALL SIMPLE BOSSES IMPLEMENTED**

---

## Overview

Implemented 4 bosses with simple abilities using existing AbilityEffect types, plus created 2 new reusable effect classes.

---

## Bosses Implemented

### **1. Shadow Shepherd (Encounter 14)** ✅
**Location:** `Assets/Resources/Enemies/Act1/PathOfFailingLight/BOSS_ShadowShepherd.asset`

**Stats:**
- Health: 280-320
- Damage: 18
- Resistances: +10% Physical, +15% Cold, +30% Chaos, -10% Lightning

**Abilities:**
1. **Mournful Toll** (Tier 1) - Summons 1-2 minions
   - Trigger: PhaseGate at 60% HP
   - Cooldown: 4 turns
   - Uses existing `SummonEffect`
   
2. **Cloak of Dusk** (Tier 3) - Passive already implemented
   - BossAbilityType.ConditionalStealth (7)
   - Becomes untargetable after being hit by 3 cards

---

### **2. Charred Homesteader (Encounter 12)** ✅
**Location:** `Assets/Resources/Enemies/Act1/AsheslopeRidge/BOSS_CharredHomesteader.asset`

**Stats:**
- Health: 300-350
- Damage: 20
- Resistances: +50% Fire, -30% Cold, +5% Physical

**Abilities:**
1. **Coalburst** (Tier 1) - Fire damage + Burn stacks
   - 25 fire damage + Burn (17.5 magnitude, 4 turns)
   - Cooldown: 2 turns
   - Uses `DamageEffect` + `StatusEffectEffect`
   
2. **Cooling Regret** (Tier 3) - Passive already implemented
   - BossAbilityType.ConditionalHeal (6)
   - Heals 15% max HP if not hit this turn

---

### **3. Fieldborn Aberrant (Encounter 9)** ✅
**Location:** `Assets/Resources/Enemies/Act1/BlightTilledMeadow/BOSS_FieldbornAberrant.asset`

**Stats:**
- Health: 270-310
- Damage: 17
- Resistances: +35% Chaos, +20% Cold, -20% Fire, -10% Physical

**Abilities:**
1. **Verdant Collapse** (Tier 1) - Chaos AoE damage
   - 22 chaos damage to player
   - Cooldown: 2 turns
   - Uses `DamageEffect`
   
2. **Bloom of Ruin** (Tier 3) - Passive already implemented
   - BossAbilityType.BloomOfRuin (10)
   - Places reactive zone that triggers on guard gain

---

### **4. Bandit Reclaimer (Encounter 10)** ✅
**Location:** `Assets/Resources/Enemies/Act1/ThornedPalisade/BOSS_BanditReclaimer.asset`

**Stats:**
- Health: 290-330
- Damage: 19
- Resistances: +15% Physical, +10% Chaos
- Initial: 1 Agitate stack

**Abilities:**
1. **Bestial Graft** (Tier 1) - Random buff
   - Randomly grants one of:
     - Strength (+3, 2 turns)
     - Bolster (2 stacks + 2 Tolerance, 3 turns)
     - Shield (20 magnitude, 2 turns)
   - Trigger: OnTurnStart
   - Cooldown: 2 turns
   - Uses new `RandomBuffEffect`
   
2. **Predation Lineage** (Tier 3) - Learn player card
   - BossAbilityType.LearnPlayerCard (11)
   - Learns a card from discard, can cast it

---

### **5. Orchard-Bound Widow (Encounter 3)** ✅
**Location:** `Assets/Resources/Enemies/Act1/WhisperingOrchard/BOSS_OrchardBoundWidow.asset`

**Stats:**
- Health: 240-280
- Damage: 16
- Resistances: +25% Chaos, +10% Lightning, -15% Fire

**Abilities:**
1. **Root Lash** (Tier 2) - Attack + Bind
   - 14 physical damage
   - Applies Bind (1 turn) - Can't play Guard cards
   - Cooldown: 2 turns
   - Uses `DamageEffect` + `StatusEffectEffect` (Bind)
   
2. **Memory Sap** (Tier 1) - Exhaust random card
   - Removes 1 random card from hand
   - Trigger: OnTurnStart
   - Cooldown: 4 turns
   - Uses new `ExhaustCardEffect`

---

## New AbilityEffect Classes Created

### **RandomBuffEffect.cs** ✅
**Location:** `Assets/Scripts/Combat/Abilities/Effects/RandomBuffEffect.cs`

**Purpose:** Randomly select and apply one buff from a list

**Features:**
- List of possible `StatusEffectEffect` buffs
- Can apply all or pick one random
- Perfect for "gains random buff" mechanics

**Usage:**
```csharp
RandomBuffEffect:
  possibleBuffs:
    - Strength (+3, 2 turns)
    - Bolster (2 stacks, 3 turns)
    - Shield (20, 2 turns)
  applyAllBuffs: false  // Pick one random
```

---

### **ExhaustCardEffect.cs** ✅
**Location:** `Assets/Scripts/Combat/Abilities/Effects/ExhaustCardEffect.cs`

**Purpose:** Remove cards from player's hand or discard pile

**Features:**
- Exhaust from hand or discard
- Configurable count
- Shows combat message
- Removes card visual

**Usage:**
```csharp
ExhaustCardEffect:
  cardsToExhaust: 1
  exhaustFromHand: true
```

**CombatDeckManager Integration:**
- Added `ExhaustCardFromHand(int index)` method
- Removes card from hand list
- Returns visual to pool
- Repositions remaining cards

---

## Boss Ability Summary

| Boss | Tier 1 (Simple) | Tier 3 (Complex) | Total |
|------|----------------|------------------|-------|
| Shadow Shepherd | Mournful Toll (summon) | Cloak of Dusk | 2 |
| Charred Homesteader | Coalburst (burn) | Cooling Regret | 2 |
| Fieldborn Aberrant | Verdant Collapse (AoE) | Bloom of Ruin | 2 |
| Bandit Reclaimer | Bestial Graft (random buff) | Predation Lineage | 2 |
| Orchard-Bound Widow | Memory Sap (exhaust), Root Lash (bind) | - | 2 |

**Total:** 5 bosses, 10 abilities (6 simple + 4 complex)

---

## Unity Editor Configuration Required

For each boss, you need to:

1. **Open the boss asset** (e.g., `BOSS_ShadowShepherd.asset`)
2. **Link abilities:**
   - Find "Abilities (Scriptable)" section
   - Add ability assets to the list
3. **Verify boss abilities:**
   - Check "Boss Abilities (Complex)" has correct enum values

### **Quick Reference:**

**Shadow Shepherd:**
- Abilities: MournfulToll
- Boss Abilities: ConditionalStealth (7)

**Charred Homesteader:**
- Abilities: Coalburst
- Boss Abilities: ConditionalHeal (6)

**Fieldborn Aberrant:**
- Abilities: VerdantCollapse
- Boss Abilities: BloomOfRuin (10)

**Bandit Reclaimer:**
- Abilities: BestialGraft
- Boss Abilities: LearnPlayerCard (11)

**Orchard-Bound Widow:**
- Abilities: RootLash, MemorySap
- Boss Abilities: (none - uses status effects only)

---

## Files Created (13 total)

### Boss Assets (5):
1. BOSS_ShadowShepherd.asset
2. BOSS_CharredHomesteader.asset
3. BOSS_FieldbornAberrant.asset
4. BOSS_BanditReclaimer.asset
5. BOSS_OrchardBoundWidow.asset

### Ability Assets (6):
6. ShadowShepherd_MournfulToll.asset
7. CharredHomesteader_Coalburst.asset
8. FieldbornAberrant_VerdantCollapse.asset
9. BanditReclaimer_BestialGraft.asset
10. OrchardWidow_RootLash.asset
11. OrchardWidow_MemorySap.asset

### Code Files (2):
12. RandomBuffEffect.cs
13. ExhaustCardEffect.cs

---

## Compilation Status

✅ All files created  
✅ No linter errors  
✅ New effect classes compile  
✅ Integration methods added  

---

## Testing Checklist

### Shadow Shepherd:
- [ ] Spawns with correct stats
- [ ] Uses Mournful Toll at 60% HP
- [ ] Summons 1-2 minions
- [ ] Gains stealth after 3 card hits

### Charred Homesteader:
- [ ] Uses Coalburst attack
- [ ] Applies Burn (4 turns)
- [ ] Heals if not hit each turn

### Fieldborn Aberrant:
- [ ] Uses Verdant Collapse
- [ ] Deals chaos damage
- [ ] Bloom of Ruin triggers on guard gain

### Bandit Reclaimer:
- [ ] Gains random buff at turn start
- [ ] Can learn player card (not yet fully implemented)

### Orchard-Bound Widow:
- [ ] Root Lash applies Bind
- [ ] Can't play Guard cards while Bound
- [ ] Memory Sap exhausts card from hand

---

## Known Limitations

### **Bestial Graft (Random Buff):**
- Requires `RandomBuffEffect` with proper GUID
- Unity may regenerate GUIDs - check meta files

### **Memory Sap (Exhaust):**
- Requires `ExhaustCardEffect` with proper GUID
- Unity may regenerate GUIDs - check meta files

### **Predation Lineage (Learn Card):**
- Complex ability - handler exists but needs:
  - Card learning trigger (phase gate at 70% HP)
  - Card casting logic (execute player card as enemy)
  - This is a Tier 3 ability for Phase 7

---

## Next Steps

**Immediate:**
1. Open Unity Editor
2. Link abilities to bosses
3. Test each boss in combat

**Phase 5 (Next):**
- Create moderate complexity abilities
- ScalingDamageEffect, BreakGuardEffect
- Implement Tier 2 bosses

---

**Phase 4 Complete!** 5 more bosses ready to test! 🎮

