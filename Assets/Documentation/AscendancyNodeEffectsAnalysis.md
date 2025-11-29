# Ascendancy Node Effects Analysis

## Overview

This document analyzes all 6 Ascendancy classes and their nodes to identify:
1. What events each node needs to listen to
2. What effects each node needs to execute
3. What new systems/mechanics need to be built
4. Implementation priority recommendations

---

## 📊 Summary Statistics

**Total Ascendancies Analyzed:** 6
- **Marauder:** 3 (Bastion of Tolerance, Crumbling Earth, Disciple of War)
- **Witch:** 3 (Archanum Bladeweaver, Profane Vessel, Temporal Savant)

**Total Nodes:** ~150+ nodes across all Ascendancies

**Core Mechanics Identified:**
- Tolerance (stack-based damage reduction)
- Crumble (delayed damage explosion)
- Battle Rhythm (card type tracking)
- Arcane Rebound (spell → attack synergy)
- Corruption (self-damage → power)
- Temporal Threads (delayed card effects)

---

## 🔍 Detailed Analysis by Ascendancy

### 1. **Marauder: Bastion of Tolerance**

#### Core Mechanic: Tolerance
- **Stack Type:** Already exists in `StackType.Tolerance` ✅
- **Events Needed:**
  - `OnDamageTaken` - Gain 1 stack when hit
  - `OnKill` - Gain 2 stacks on kill
  - `OnCombatStart` - Initialize stacks
- **Effects Needed:**
  - `add_stack` (Tolerance)
  - `reduce_damage_percent` (3% per stack, max 10)
  - `set_damage_reduction_multiplier` (scales with stacks)

#### Key Nodes Requiring Implementation:

**Start Node: Tolerance**
- ✅ Uses existing `StackType.Tolerance`
- Needs: `OnCombatStart`, `OnDamageTaken`, `OnKill` events
- Effect: Stack management + damage reduction

**Iron Will** (Major Node)
- **Events:** `OnTurnStart`, `OnTurnEnd`
- **Effects:**
  - `modify_guard_persistence` (50% effectiveness between turns)
  - `set_attack_cannot_miss` (boolean flag)

**Immovable Object** (Major Node)
- **Events:** `OnDamageTaken`
- **Conditions:** `damage_percent_of_max_hp > 0.10`
- **Effects:**
  - `add_guard` (5% of Attributes + Armour)
  - Needs attribute/armour calculation access

**Braced Assault** (Major Node)
- **Events:** `OnGuardUsed`, `OnAttackUsed`
- **Effects:**
  - Track guard sequence count
  - `add_damage_more_percent` (20% per guard in sequence, max 100%)
  - Complex: Needs to track "guard sequence" state

**Bulwark Pulse** (Major Node)
- **Events:** `OnGuardGained`
- **Effects:**
  - `deal_damage_to_all_enemies` (10% of guard gained as Physical)
  - Needs access to guard amount gained

**Shock Absorber** (Major Node)
- **Events:** `OnDamageTaken`
- **Effects:**
  - `convert_damage_to_bleed` (25% of damage → Bleed over 2 turns)
  - Complex: Needs custom "Pressure" status or use existing Bleed

**Wall of War** (Major Node)
- **Events:** `OnCombatStart`
- **Effects:**
  - `add_guard` (35% of max HP)
  - `modify_max_guard_multiplier` (2x max HP)

**Minor Nodes:**
- Most are stat increases: `increased_tolerance_effect`, `increased_guard_effectiveness`
- Simple: Can use existing stat modifier system

---

### 2. **Marauder: Crumbling Earth**

#### Core Mechanic: Crumble
- **Status Effect:** Already exists as `StatusEffectType.Crumble` ✅
- **Events Needed:**
  - `OnDamageDealt` - Apply Crumble stacks (10% of damage)
  - `OnEnemyDeath` - Trigger Crumble explosion
  - `OnStatusExpire` - Trigger Crumble explosion
- **Effects Needed:**
  - `apply_crumble_status` (with magnitude = 10% of damage)
  - `trigger_crumble_explosion` (deal stored damage)

#### Key Nodes Requiring Implementation:

**Start Node: Crumbling Earth**
- ✅ Uses existing `StatusEffectType.Crumble`
- Needs: `OnDamageDealt` event (NEW - not in proposed list!)
- Effect: Apply Crumble based on damage dealt

**Blood Price** (Major Node)
- **Events:** `OnAttackUsed`
- **Effects:**
  - `apply_status` ("Pressure" - 3% of Max HP)
  - `add_damage_more_percent` (30% more damage)
  - Needs custom "Pressure" status or use existing

**Spring of Rage** (Major Node)
- **Events:** `OnTurnStart` (check life %)
- **Conditions:** `life_percent < 0.80`
- **Effects:**
  - `modify_max_mana_percent` (+10%)
  - `modify_crumble_magnitude_more` (30% more)
  - Complex: Needs conditional stat modification

**Final Offering** (Major Node)
- **Events:** `OnDamageTaken` (check life %)
- **Conditions:** `life_percent <= 0.25`
- **Effects:**
  - `modify_damage_multiplier` (2x damage)
  - `trigger_all_crumble_stacks` (instant explosion)
  - Complex: Needs to find all enemies with Crumble and trigger

**Trembling Echo** (Major Node)
- **Events:** `OnAttackUsed`
- **Conditions:** `target_has_status` (Crumble)
- **Effects:**
  - `duplicate_attack_effect` (50% effectiveness)
  - Complex: Needs card duplication system

**Seismic Hunger** (Major Node)
- **Events:** `OnCrumbleExplosion` (NEW event type)
- **Effects:**
  - `heal_percent_missing_hp` (10% of damage dealt)
  - Needs custom event for Crumble explosions

**Stoneskin** (Major Node)
- **Events:** `OnStatusApplied`
- **Effects:**
  - `immune_to_status` (Bleed, Ignite)
  - Simple: Status immunity flag

**Thrill of Agony** (Major Node)
- **Events:** `OnStatusApplied`, `OnTurnStart`
- **Conditions:** `has_status` (Bleed OR Ignite)
- **Effects:**
  - `modify_crumble_magnitude_more` (30% more)
  - Conditional modifier

---

### 3. **Marauder: Disciple of War**

#### Core Mechanic: Battle Rhythm
- **Stack Type:** NEW - needs `StackType.BattleRhythm`
- **Events Needed:**
  - `OnCardPlayed` - Track card types played this turn
  - `OnTurnStart` - Reset tracking
  - `OnTurnEnd` - Check if all types played, grant stack
- **Effects Needed:**
  - `track_card_type_played` (Attack, Spell, Guard, Skill)
  - `grant_battle_rhythm_stack` (when all types played)
  - `add_damage_more_percent` (3% per stack)
  - `modify_guard_effectiveness_percent` (2% per stack)

#### Key Nodes Requiring Implementation:

**Start Node: Battle Rhythm**
- **NEW Stack Type:** `StackType.BattleRhythm`
- **Events:** `OnCardPlayed`, `OnTurnStart`, `OnTurnEnd`
- **Complex:** Needs card type tracking system
- **Effects:**
  - Track: Attack, Spell, Guard, Skill played this turn
  - Grant stack when all 4 types played
  - Apply damage/guard bonuses

**Flow of Iron** (Major Node)
- **Events:** `OnBattleRhythmGained` (NEW event)
- **Effects:**
  - `add_stack` (Flow, max 10)
  - `check_flow_threshold` (at 10: next attack +50% damage, 5 Bolster)
  - `reset_flow_on_rhythm_loss`
  - **NEW Stack Type:** `StackType.Flow`

**Guarded Motion** (Major Node)
- **Events:** `OnGuardUsed`, `OnAttackUsed`
- **Effects:**
  - `add_extra_hit` (next attack gains +1 hit, 40% damage)
  - Complex: Multi-strike system

**Tempered Resolve** (Major Node)
- **Events:** `OnBattleRhythmLost` (NEW event)
- **Effects:**
  - `add_damage_reduction_percent` (20%, fades 5% per turn)
  - Complex: Temporary stat that decays

**Conductor's Strike** (Major Node)
- **Events:** `OnAttackUsed`
- **Conditions:** Check card types played this turn
- **Effects:**
  - `add_damage_more_percent` (5% per different card type)
  - `modify_crit_damage_more` (at 3 types: +100% crit damage)
  - Complex: Needs turn history tracking

**Tactical Transference** (Major Node)
- **Events:** `OnTurnEnd`
- **Conditions:** `has_battle_rhythm` (check if ended turn with rhythm)
- **Effects:**
  - `add_stack` (Momentum, 2 stacks)
  - Uses existing `StackType.Momentum` ✅

**Pulse of War** (Major Node)
- **Events:** `OnBattleRhythmGained`
- **Effects:**
  - `deal_damage_to_all_enemies` (10% of Strength as shockwave)
  - Track pulse count, every 5: `add_stack` (Flow, 1) + `restore_mana_percent` (10%)
  - Complex: Pulse counter system

**Flowbreaker** (Major Node)
- **Events:** `OnBattleRhythmLost` (deliberate - duplicate card type)
- **Effects:**
  - `deal_damage_to_all_enemies` (30% of Guard as AoE)
  - Complex: Detect "deliberate" loss vs natural loss

**Martial Refrain** (Major Node)
- **Events:** `OnBattleRhythmLost`, `OnTurnStart`
- **Effects:**
  - `duplicate_first_card` (50% effectiveness, +30% crit if Attack)
  - Complex: Card duplication system

**Second Beat** (Major Node)
- **Events:** `OnCardPlayed`
- **Effects:**
  - Track cards played this turn
  - Every 3rd card: `echo_card_effect` (30% effectiveness)
  - If Attack: repeat on same target
  - If Guard: gain 30% of guard value again
  - Complex: Card echo system

---

### 4. **Witch: Archanum Bladeweaver**

#### Core Mechanic: Arcane Rebound
- **Events Needed:**
  - `OnSpellCast` - Mark next attack for bonus
  - `OnAttackUsed` - Check if marked, apply bonus
- **Effects Needed:**
  - `mark_next_attack` (store spell element/type)
  - `apply_arcane_rebound_bonus` (20% more damage OR +1 hit OR +50 elemental damage)

#### Key Nodes Requiring Implementation:

**Start Node: Arcane Rebound**
- **Events:** `OnSpellCast`, `OnAttackUsed`
- **Effects:**
  - Store "next attack bonus" state
  - Apply one of: 20% more damage, +1 hit, +50 elemental damage
  - Complex: State tracking between events

**Elemental Branding** (Choice Node)
- **Events:** `OnAttackUsed`
- **Effects:**
  - **Frost Brand:** Every 3rd attack → `apply_status_to_all_enemies` (Weak)
  - **Storm Brand:** Every 3rd attack → `apply_status_to_all_enemies` (Shocked)
  - **Flame Brand:** On attack → `apply_status` (Burning Sigil, 3s, detonates on expire/death)
  - Complex: Attack counter + brand-specific effects

**Mirrorsteel Guard** (Major Node)
- **Events:** `OnCombatStart`, `OnDamageTaken`, `OnGuardUsed`
- **Effects:**
  - `set_stack` (Mirrorsteel, 3 at combat start)
  - `reduce_damage_percent` (40% per stack)
  - `consume_stack` (Mirrorsteel, 1 on hit)
  - `add_stack` (Mirrorsteel, 1 if Guard after Spell)
  - **NEW Stack Type:** `StackType.Mirrorsteel`

**Prismatic Arsenal** (Major Node)
- **Events:** `OnAttackUsed`
- **Conditions:** `previous_card_was_spell`
- **Effects:**
  - `convert_damage_type` (50% of weapon damage → spell's element)
  - Complex: Damage type conversion system

**Twin Arts Invocation** (Major Node)
- **Events:** `OnAttackUsed`, `OnSpellCast`
- **Effects:**
  - Track attack/spell count
  - Every 3rd Attack: `cast_last_spell`
  - Every 3rd Spell: `perform_last_attack`
  - Complex: Card history + duplication system

**Flux Weaving** (Major Node)
- **Events:** `OnCardPlayed`
- **Effects:**
  - Track last card type
  - If alternates (Attack→Spell or Spell→Attack):
    - `add_damage_more_percent` (15% for next action)
    - `add_guard_more_percent` (15% for next action)
  - Complex: Alternation detection

**Blades of Conduction** (Major Node)
- **Events:** `OnDamageCalculation` (NEW event - during damage calc)
- **Effects:**
  - `add_weapon_damage_from_spell_power` (20% of spell power)
  - `add_spell_damage_from_weapon` (20% of weapon damage)
  - Complex: Cross-scaling system

**Blades Ascend, Spells Break** (Major Node)
- **Events:** `OnCardPlayed`
- **Effects:**
  - Track cards played this turn
  - Every 4th card (Attack or Spell):
    - `mark_next_card_as_hybrid` (scales with both weapon + spell damage)
  - Complex: Hybrid scaling system

---

### 5. **Witch: Profane Vessel**

#### Core Mechanic: Corruption
- **Stack Type:** NEW - needs `StackType.Corruption`
- **Events Needed:**
  - `OnCardPlayed` - Gain 1 Corruption, lose 1% current HP
  - `OnCorruptionThreshold` (10, 20, 30, etc.) - Trigger Chaos Surge
- **Effects Needed:**
  - `add_stack` (Corruption)
  - `lose_life_percent_current` (1% current HP)
  - `add_damage_percent` (2% per stack)
  - `trigger_chaos_surge` (at thresholds)
  - `add_corruption_flow` (stacking, +1 corruption per card per stack)

#### Key Nodes Requiring Implementation:

**Start Node: Unstable Corruption**
- **NEW Stack Type:** `StackType.Corruption`
- **Events:** `OnCardPlayed`, `OnCorruptionThreshold` (custom event)
- **Effects:**
  - `add_stack` (Corruption, 1 per card)
  - `lose_life_percent_current` (1% current HP)
  - `add_damage_percent` (2% per stack)
  - `add_corruption_flow` (at thresholds: +1 per stack)
  - `trigger_chaos_surge` (at 10, 20, 30, 40...)
  - `lose_life_percent_max` (5% max HP at thresholds)
  - Complex: Threshold detection system

**Feast of Pain** (Major Node)
- **Events:** `OnKill`
- **Conditions:** `corruption_stacks >= 10`
- **Effects:**
  - `heal_percent_missing_hp` (10%)
  - Simple conditional

**Searing Insight** (Major Node)
- **Events:** `OnCardPlayed`
- **Conditions:** `card_is_fire_or_chaos`
- **Effects:**
  - `lose_life_percent_max` (2% max HP)
  - `add_damage_more_percent` (8% for 1 turn, stacks to 5)
  - Complex: Temporary stacking buff

**Pact of the Harvester** (Major Node)
- **Events:** `OnDamageTaken` (self-inflicted), `OnCorruptionThreshold` (50)
- **Effects:**
  - `modify_self_damage_taken_percent` (+25% increased)
  - `add_damage_more_percent` (40% per 20 Corruption)
  - `trigger_profane_harvest` (at 50 Corruption):
    - Release all Corruption
    - `heal_percent_missing_hp` (1% per Corruption)
    - `modify_max_mana_percent` (+20% for 1 turn)
    - `mark_next_3_cards` (extra Chaos damage, +30% more base damage)
  - Complex: Multi-effect trigger system

**Abyssal Bargain** (Major Node)
- **Events:** `OnCorruptionThreshold` (20, 40, 60...)
- **Effects:**
  - `restore_mana_percent` (25%)
  - `draw_cards` (1)
  - Simple threshold trigger

**Tormented Knowledge** (Major Node)
- **Events:** `OnSelfDamageTaken` (NEW event)
- **Effects:**
  - `mark_next_card` (+30% more effect)
  - Complex: Self-damage detection

**Chaotic Pendulum** (Major Node)
- **Events:** `OnWaveStart` (NEW event), `OnCardPlayed`
- **Effects:**
  - `set_elemental_buff` (alternates Fire/Chaos, +25% more damage)
  - `add_corruption` (1 if playing pendulum element)
  - Complex: Wave-based buff system

**Descent of Thought** (Major Node)
- **Events:** `OnSelfDamageTaken`
- **Effects:**
  - `add_attribute_temporary` (33% chance: +1 STR/DEX/INT for combat)
  - Complex: Random attribute gain

**Blood Pact** (Major Node)
- **Events:** `OnCombatStart`
- **Effects:**
  - `lose_life_percent_max` (10% max HP)
  - `add_damage_more_percent` (+25% for 3 turns)
  - `add_cards_drawn` (+1 for 3 turns)
  - Complex: Temporary multi-stat buff

**Echo of Agony** (Major Node)
- **Events:** `OnChaosSurgeTriggered` (NEW event)
- **Effects:**
  - `duplicate_next_card` (deals damage as extra Chaos)
  - Complex: Event chain system

---

### 6. **Witch: Temporal Savant**

#### Core Mechanic: Temporal Threads
- **Events Needed:**
  - `OnCardDraw` - 15% chance to mark as "Temporal"
  - `OnTurnStart` - Trigger Temporal card echoes
- **Effects Needed:**
  - `mark_card_as_temporal` (on draw)
  - `echo_temporal_card` (at start of next turn, 75% power)
  - `modify_temporal_card_effect` (various bonuses)

#### Key Nodes Requiring Implementation:

**Start Node: Temporal Threads**
- **Events:** `OnCardDraw`, `OnTurnStart`
- **Effects:**
  - `mark_card_as_temporal` (15% chance on draw)
  - `echo_temporal_card` (at start of next turn)
  - Complex: Card marking + delayed execution system

**Future Echo** (Major Node)
- **Events:** `OnCardPlayed`, `OnTurnStart`
- **Effects:**
  - `store_first_card` (each turn)
  - `echo_stored_card` (after 2 turns, 75% power)
  - Complex: Multi-turn delay system

**Hourglass Paradox** (Major Node)
- **Events:** `OnTurnEnd`, `OnTurnStart`
- **Conditions:** `mana_spent_this_turn == 0`
- **Effects:**
  - `add_cards_drawn` (+2)
  - `modify_max_mana_percent` (+10% next turn)
  - `set_next_card_cost` (0 mana)
  - Complex: Turn state tracking

**Borrowed Power** (Major Node)
- **Events:** `OnCardPlayed`
- **Conditions:** `card_is_temporal`
- **Effects:**
  - `modify_card_effect_percent` (+25% effect)
  - `modify_card_cost` (+1 mana)
  - Simple: Conditional card modification

**Suspended Moment** (Major Node)
- **Events:** `OnDelayedCardUsed` (NEW event)
- **Effects:**
  - `modify_dot_tick_rate` (2x speed for 2 turns)
  - Complex: Status effect tick rate modification

**Echoing Will** (Major Node)
- **Events:** `OnTemporalCardEchoed` (NEW event)
- **Effects:**
  - Track echo count
  - `modify_temporal_card_base_power_percent` (+10% per echo, max 80%)
  - Complex: Combat-long stacking modifier

**Chrono Collapse** (Major Node)
- **Events:** `OnEnemyDeath`
- **Effects:**
  - `spread_debuffs_to_random_enemy` (all debuffs from dead enemy)
  - Complex: Status effect transfer system

---

## 🎯 Required Events (Complete List)

### Core Events (Already Proposed)
- ✅ `OnCardPlayed` (exists)
- ✅ `OnTurnStart`
- ✅ `OnTurnEnd`
- ✅ `OnDamageTaken`
- ✅ `OnKill` / `OnEnemyDeath`
- ✅ `OnDiscard`
- ✅ `OnManaSpent`
- ✅ `OnStatusApplied`

### New Events Needed
- ❌ `OnAttackUsed` / `OnAttackCardPlayed`
- ❌ `OnSpellCast` / `OnSpellCardPlayed`
- ❌ `OnGuardUsed` / `OnGuardCardPlayed`
- ❌ `OnSkillUsed` / `OnSkillCardPlayed`
- ❌ `OnDamageDealt` (to enemies)
- ❌ `OnGuardGained`
- ❌ `OnCombatStart`
- ❌ `OnWaveStart`
- ❌ `OnCardDraw`
- ❌ `OnSelfDamageTaken`
- ❌ `OnBattleRhythmGained` / `OnBattleRhythmLost`
- ❌ `OnCorruptionThreshold`
- ❌ `OnChaosSurgeTriggered`
- ❌ `OnCrumbleExplosion`
- ❌ `OnTemporalCardEchoed`
- ❌ `OnDelayedCardUsed`

### Event Context Objects Needed
- `AttackData` - Card, damage, target, element
- `SpellData` - Card, damage, target, element
- `GuardData` - Card, guard amount gained
- `DamageData` - Amount, type, source, target
- `CardPlayedData` - Card, type, target, mana cost
- `StatusAppliedData` - Status type, magnitude, duration

---

## 🔧 Required Effect Resolvers

### Stack Management
- ✅ `add_stack` (uses StackSystem)
- ✅ `remove_stack` / `consume_stack`
- ✅ `set_stack` (set to specific value)
- ✅ `clear_stack`
- ❌ `add_stack_with_max` (respect max)
- ❌ `modify_max_stacks` (bonus max stacks)

### Damage Modification
- ✅ `add_flat_damage`
- ✅ `add_percent_damage` / `increased_damage`
- ❌ `add_damage_more_percent` (multiplicative)
- ❌ `modify_damage_multiplier` (2x, etc.)
- ❌ `convert_damage_type` (Physical → Fire, etc.)
- ❌ `add_elemental_damage` (Fire, Cold, Lightning)

### Damage Reduction
- ❌ `reduce_damage_percent` (flat reduction)
- ❌ `reduce_damage_more_percent` (multiplicative reduction)
- ❌ `modify_damage_taken_multiplier`

### Guard System
- ❌ `add_guard` (flat amount)
- ❌ `add_guard_percent_max_hp`
- ❌ `modify_guard_effectiveness_percent`
- ❌ `modify_guard_persistence_percent`
- ❌ `modify_max_guard_multiplier`

### Status Effects
- ✅ `apply_status` (uses StatusEffectManager)
- ✅ `remove_status`
- ❌ `immune_to_status` (status immunity flag)
- ❌ `spread_status_to_enemy` (transfer on death)
- ❌ `modify_dot_tick_rate` (2x speed, etc.)

### Card Effects
- ❌ `duplicate_card` / `echo_card_effect`
- ❌ `mark_card_as_temporal` / `mark_card_as_hybrid`
- ❌ `modify_card_cost` (set to 0, add cost)
- ❌ `modify_card_effect_percent`
- ❌ `draw_cards`
- ❌ `discard_cards`

### Healing & Life
- ❌ `heal_percent_missing_hp`
- ❌ `lose_life_percent_current`
- ❌ `lose_life_percent_max`
- ❌ `restore_mana` / `restore_mana_percent`
- ❌ `modify_max_mana_percent`

### Stat Modifications
- ✅ `modify_stat_percent` (increased)
- ❌ `modify_stat_more_percent` (multiplicative)
- ❌ `add_attribute_temporary` (STR/DEX/INT for combat)
- ❌ `modify_crit_chance_percent`
- ❌ `modify_crit_damage_more_percent`

### Special Mechanics
- ❌ `deal_damage_to_all_enemies` (AoE)
- ❌ `add_extra_hit` (multi-strike)
- ❌ `set_attack_cannot_miss`
- ❌ `track_card_types_played` (for Battle Rhythm)
- ❌ `check_threshold` (Corruption, Flow, etc.)

---

## 📋 New Stack Types Needed

Extend `StackType` enum:
```csharp
public enum StackType
{
    Agitate,           // ✅ Exists
    Tolerance,         // ✅ Exists
    Potential,         // ✅ Exists
    Momentum,          // ✅ Exists
    BattleRhythm,      // ❌ NEW
    Flow,              // ❌ NEW
    Mirrorsteel,       // ❌ NEW
    Corruption,        // ❌ NEW
    CorruptionFlow     // ❌ NEW (stacking corruption gain rate)
}
```

---

## 🚨 Complex Systems Required

### 1. **Card Type Tracking System**
- Track which card types (Attack, Spell, Guard, Skill) played each turn
- Used by: Battle Rhythm, Flux Weaving, Conductor's Strike
- **Complexity:** Medium

### 2. **Card History System**
- Store last played card(s) for duplication/echo effects
- Used by: Twin Arts Invocation, Martial Refrain, Trembling Echo
- **Complexity:** Medium

### 3. **Card Marking System**
- Mark cards as "Temporal", "Hybrid", "Next Attack Bonus", etc.
- Used by: Temporal Savant, Blades Ascend, Arcane Rebound
- **Complexity:** High

### 4. **Threshold Detection System**
- Detect when stacks reach specific values (10, 20, 30, etc.)
- Used by: Corruption, Flow (at 10)
- **Complexity:** Medium

### 5. **Delayed Effect System**
- Execute effects after N turns
- Used by: Temporal Threads, Future Echo, Shock Absorber (Bleed)
- **Complexity:** High

### 6. **Multi-Strike System**
- Add extra hits to attacks
- Used by: Guarded Motion, Arcane Rebound (+1 hit option)
- **Complexity:** Medium

### 7. **Damage Type Conversion**
- Convert damage types (Physical → Fire, etc.)
- Used by: Prismatic Arsenal
- **Complexity:** Medium

### 8. **Hybrid Scaling System**
- Cards that scale with both weapon AND spell damage
- Used by: Blades Ascend, Spells Break
- **Complexity:** High

### 9. **Status Transfer System**
- Transfer debuffs from dead enemy to random enemy
- Used by: Chrono Collapse
- **Complexity:** Medium

### 10. **Self-Damage Detection**
- Distinguish self-inflicted damage from enemy damage
- Used by: Profane Vessel nodes
- **Complexity:** Low (add source flag to damage)

---

## 💡 Implementation Recommendations

### **Should you finish all node descriptions first?**

**YES, but with caveats:**

#### ✅ **Finish Descriptions First If:**
1. You want to ensure all mechanics are fully designed before coding
2. You want to avoid rework when mechanics change
3. You're still iterating on balance/design
4. You want a complete picture of requirements

#### ⚠️ **Start Implementation Early If:**
1. You want to validate the modifier system architecture
2. You want to test with a few nodes before scaling up
3. You want to identify technical challenges early
4. You want to build systems incrementally

### **Recommended Approach: Hybrid**

1. **Phase 1: Core System (2-3 weeks)**
   - Build event system (all events)
   - Build modifier handler
   - Build basic effect resolvers (stacks, damage, status)
   - **Test with 2-3 simple nodes** from different Ascendancies

2. **Phase 2: Complete 1 Ascendancy (1-2 weeks)**
   - Finish ALL descriptions for **one Ascendancy** (recommend: Bastion of Tolerance - simplest)
   - Implement all nodes for that Ascendancy
   - Validate architecture works end-to-end

3. **Phase 3: Complex Systems (2-3 weeks)**
   - Build complex systems (card tracking, duplication, etc.)
   - Test with nodes that require them

4. **Phase 4: Remaining Ascendancies (4-6 weeks)**
   - Finish descriptions for remaining Ascendancies
   - Implement nodes in batches (by complexity)

### **Priority Order for Node Implementation:**

**Tier 1: Simple Stat Modifiers** (Easiest)
- Minor nodes with "increased X" effects
- Can use existing stat system
- **Examples:** All "Tolerance and Guard effectiveness" nodes

**Tier 2: Stack-Based Effects** (Easy)
- Nodes that just add/consume stacks
- Uses existing StackSystem
- **Examples:** Tolerance start node, Mirrorsteel Guard

**Tier 3: Event-Triggered Effects** (Medium)
- Nodes that respond to single events
- **Examples:** Bulwark Pulse, Feast of Pain

**Tier 4: Conditional Effects** (Medium-Hard)
- Nodes with conditions (life %, has status, etc.)
- **Examples:** Spring of Rage, Final Offering

**Tier 5: Complex State Tracking** (Hard)
- Nodes that track state across multiple events
- **Examples:** Battle Rhythm, Arcane Rebound, Flux Weaving

**Tier 6: Card Manipulation** (Very Hard)
- Nodes that duplicate, echo, or modify cards
- **Examples:** Twin Arts Invocation, Second Beat, Temporal Threads

### **Suggested First Implementation:**

**Start with: Bastion of Tolerance - Start Node**
- Uses existing `StackType.Tolerance`
- Simple events: `OnCombatStart`, `OnDamageTaken`, `OnKill`
- Simple effects: `add_stack`, `reduce_damage_percent`
- Validates core system works

**Then: Bastion of Tolerance - Minor Nodes**
- Simple stat increases
- Validates stat modifier system

**Then: One Complex Node**
- Pick one Tier 5/6 node to validate complex systems
- **Recommend:** Battle Rhythm (Disciple of War) - tests card tracking

---

## 📊 Complexity Breakdown

**Total Nodes:** ~150+
- **Tier 1 (Simple):** ~40 nodes (27%)
- **Tier 2 (Stack-Based):** ~20 nodes (13%)
- **Tier 3 (Event-Triggered):** ~30 nodes (20%)
- **Tier 4 (Conditional):** ~25 nodes (17%)
- **Tier 5 (State Tracking):** ~20 nodes (13%)
- **Tier 6 (Card Manipulation):** ~15 nodes (10%)

**Estimated Implementation Time:**
- **Core System:** 2-3 weeks
- **Tier 1-2 Nodes:** 1 week
- **Tier 3-4 Nodes:** 2-3 weeks
- **Tier 5-6 Nodes:** 4-6 weeks
- **Testing & Polish:** 2-3 weeks
- **Total:** ~12-18 weeks (3-4.5 months)

---

## ✅ Conclusion

**Recommendation:** 
1. **Finish descriptions for 1-2 Ascendancies** (Bastion of Tolerance + one other)
2. **Build core modifier system** with basic events/effects
3. **Implement those 2 Ascendancies completely** to validate architecture
4. **Finish remaining descriptions** while iterating on complex systems
5. **Implement remaining Ascendancies** in batches

This gives you:
- ✅ Early validation of architecture
- ✅ Working examples to test
- ✅ Clear requirements for remaining nodes
- ✅ Incremental progress

**Don't wait to finish ALL descriptions** - you'll learn a lot from implementing the first few nodes that will inform the rest of the design!

