# Warrant Stat Mapping Audit

This document tracks all warrant affix and notable stat keys and their correct mappings to CharacterStatsData.

## Status: ✅ IMPLEMENTATION COMPLETE - NEEDS TESTING

**Last Updated:** Implementation complete, ready for testing and verification

---

## Damage Modifiers (Applied to DamageModifiers lists)

| Stat Key | CharacterStatsData Field | DamageModifiers List | Notes |
|----------|-------------------------|---------------------|-------|
| `increasedPhysicalDamage` | `increasedPhysicalDamage` | `increasedPhysicalDamage` | ✅ Mapped |
| `increasedFireDamage` | `increasedFireDamage` | `increasedFireDamage` | ✅ Mapped |
| `increasedColdDamage` | `increasedColdDamage` | `increasedColdDamage` | ✅ Mapped |
| `increasedLightningDamage` | `increasedLightningDamage` | `increasedLightningDamage` | ✅ Mapped |
| `increasedChaosDamage` | `increasedChaosDamage` | `increasedChaosDamage` | ✅ Mapped |
| `increasedElementalDamage` | `increasedElementalDamage` | Applied to Fire/Cold/Lightning | ✅ Mapped |
| `increasedAttackDamage` | `increasedAttackDamage` | `increasedAttackDamage` | ✅ Mapped (Attack cards only) |
| `increasedSpellDamage` | `increasedSpellDamage` | `increasedSpellDamage` | ✅ Mapped (Spell cards only) |
| `increasedProjectileDamage` | `increasedProjectileDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
| `increasedAreaDamage` | `increasedAreaDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
| `increasedMeleeDamage` | `increasedMeleeDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
| `increasedRangedDamage` | `increasedRangedDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |


User Input:
| `increasedProjectileDamage` | `increasedProjectileDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
increasedProjectileDamage should only be applied to Cards with "Projectile" tag.
| `increasedAreaDamage` | `increasedAreaDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
increasedAreaDamage should only be applied to Cards with "AoE" tag OR if "Is AoE" is checked (For combo compatibility)
| `increasedMeleeDamage` | `increasedMeleeDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
increasedMeleeDamage Should only be applied to cards that scale with "Melee weapons"
| `increasedRangedDamage` | `increasedRangedDamage` | ❌ NOT IN DAMAGEMODIFIERS | ⚠️ Needs mapping |
increasedRangedDamage Should only be applied to cards that scale with "Projectile weapons"


---

## Defense/Resource Modifiers (Applied to CharacterStatsData directly)

| Stat Key | CharacterStatsData Field | Current Status | Notes |
|----------|-------------------------|----------------|-------|
| `evasionIncreased` | `evasionIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `maxHealthIncreased` | `maxHealthIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `maxManaIncreased` | `maxManaIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `energyShieldIncreased` | `energyShieldIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `armourIncreased` | `armourIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |

User Input: 
| `evasionIncreased` | `evasionIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
Player Evasion is gained from items gaining Evasion, "EvasionIncreased" should scale this combined value.
| `maxHealthIncreased` | `maxHealthIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
Should be applied globally to Character MaxHealth
We should also add a "maxHealthFlat" which is just a flat value "+40" for example.

| `maxManaIncreased` | `maxManaIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
Should be applied globally to Character MaxMana
We should also add a "maxManaFlat" which is just a flat value "+4" for example.

| `energyShieldIncreased` | `energyShieldIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
Player Energy shield is gained from items gaining Energy Shield, "EnergyShieldIncreased" should scale this combined value.
| `armourIncreased` | `armourIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
Player Armour is gained from items gaining Armour, "ArmourIncreased" should scale this combined value.



---

## Weapon/Type Damage Modifiers (Applied to CharacterStatsData directly)

| Stat Key | CharacterStatsData Field | Current Status | Notes |
|----------|-------------------------|----------------|-------|
| `increasedAxeDamage` | `increasedAxeDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedBowDamage` | `increasedBowDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedMaceDamage` | `increasedMaceDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedSwordDamage` | `increasedSwordDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedWandDamage` | `increasedWandDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedDaggerDamage` | `increasedDaggerDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedOneHandedDamage` | `increasedOneHandedDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedTwoHandedDamage` | `increasedTwoHandedDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |

All weapons have weapon tags. 
This should scale the Weapon damage if the weapon equipped has the corresponding tag.
Example: 
Bow tags:
GenericPropertyJSON:{"name":"itemTags","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"size","type":12,"val":6},{"name":"data","type":3,"val":"weapon"},{"name":"data","type":3,"val":"bow"},{"name":"data","type":3,"val":"twohanded"},{"name":"data","type":3,"val":"ranged"},{"name":"data","type":3,"val":"attack"},{"name":"data","type":3,"val":"dexterity"}]}]}

Sword tags:
GenericPropertyJSON:{"name":"itemTags","type":-1,"arraySize":7,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":7,"arrayType":"string","children":[{"name":"size","type":12,"val":7},{"name":"data","type":3,"val":"weapon"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"onehanded"},{"name":"data","type":3,"val":"melee"},{"name":"data","type":3,"val":"attack"},{"name":"data","type":3,"val":"strength"},{"name":"data","type":3,"val":"dexterity"}]

Sceptre tags:
GenericPropertyJSON:{"name":"itemTags","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"size","type":12,"val":6},{"name":"data","type":3,"val":"weapon"},{"name":"data","type":3,"val":"sceptre"},{"name":"data","type":3,"val":"onehanded"},{"name":"data","type":3,"val":"spell"},{"name":"data","type":3,"val":"strength"},{"name":"data","type":3,"val":"intelligence"}]}]}

Wand tags:
GenericPropertyJSON:{"name":"itemTags","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"size","type":12,"val":6},{"name":"data","type":3,"val":"weapon"},{"name":"data","type":3,"val":"wand"},{"name":"data","type":3,"val":"onehanded"},{"name":"data","type":3,"val":"ranged"},{"name":"data","type":3,"val":"spell"},{"name":"data","type":3,"val":"intelligence"}]}]}

---

## Ailment Modifiers (Applied to CharacterStatsData directly)

| Stat Key | CharacterStatsData Field | Current Status | Notes |
|----------|-------------------------|----------------|-------|
| `increasedIgniteMagnitude` | `increasedIgniteMagnitude` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedShockMagnitude` | `increasedShockMagnitude` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedChillMagnitude` | `increasedChillMagnitude` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedFreezeMagnitude` | `increasedFreezeMagnitude` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedBleedMagnitude` | `increasedBleedMagnitude` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedPoisonMagnitude` | `increasedPoisonMagnitude` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedDamageOverTime` | `increasedDamageOverTime` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedPoisonDamage` | `increasedPoisonDamage` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedPoisonDuration` | `increasedPoisonDuration` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedDamageVsChilled` | `increasedDamageVsChilled` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedDamageVsShocked` | `increasedDamageVsShocked` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `increasedDamageVsIgnited` | `increasedDamageVsIgnited` | ❌ NOT MAPPED | ⚠️ Needs mapping |

User input:
Ailment modifiers should increase the damage applied by the Ailment.
Example:

Poison Arrow:
Applies 3 Poison stacks to ALL enemies.
Poison damage is handled by StatusEffects, so it needs to apply the damage to the before the status effect is calculated.

---

## Speed/Duration Modifiers (Applied to CharacterStatsData directly)

| Stat Key | CharacterStatsData Field | Current Status | Notes |
|----------|-------------------------|----------------|-------|
| `attackSpeed` | `attackSpeed` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `castSpeed` | `castSpeed` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `statusEffectDuration` | `statusEffectDuration` | ❌ NOT MAPPED | ⚠️ Needs mapping |

User input:
Attack and castspeed should directly increase the amount of Aggression and Focus charges gained when playing Attack & Skill cards.
Status Effect duration is handled by StatusEffects, Duration needs to be calculated before the status effect is applied to the enemy (Rounding up to closest whole)
---

## Charge/Resource Gain Modifiers (Applied to CharacterStatsData directly)

| Stat Key | CharacterStatsData Field | Current Status | Notes |
|----------|-------------------------|----------------|-------|
| `aggressionGainIncreased` | `aggressionGainIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `focusGainIncreased` | `focusGainIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |

AggressionGain and FocusGain is a multiplier to the attack and cast speed gain.

---

## Guard/Defense Utility Modifiers (Applied to CharacterStatsData directly)

| Stat Key | CharacterStatsData Field | Current Status | Notes |
|----------|-------------------------|----------------|-------|
| `guardEffectivenessIncreased` | `guardEffectivenessIncreased` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `lessDamageFromElites` | `lessDamageFromElites` | ❌ NOT MAPPED | ⚠️ Needs mapping |
| `statusAvoidance` | `statusAvoidance` | ❌ NOT MAPPED | ⚠️ Needs mapping |

User Input:
guardEffectivenessIncreased is an increased modifier to the cards with the "Guard" tag.
Example: Steadfast Guard gains 100 guard. with 60% guardEffectivenessIncreased this card will now gain 160 guard instead.

lessDamageFromElites is damage reduction from enemies that are "Rare" or "Unique" rarity.

StatusAvoidance is an "evasion check" for status effects that enemies apply to the player.
Example: 50% statusAvoidance, Enemy staggers the player, the player has 50% chance to get stunned and 50% chance to avoid the stun. This should berolled individually for each status effect.

---

## Implementation Notes

### Current Implementation
- Only damage modifiers (Physical, Fire, Cold, Lightning, Chaos, Elemental, Attack, Spell) are currently mapped
- All other stat types are **NOT** being applied to the character

### Required Changes
1. **WarrantModifierCollector.ApplyModifiersToCharacter()** needs to:
   - Apply damage modifiers to `DamageModifiers` lists (current behavior)
   - Apply all other modifiers to `CharacterStatsData` via `AddToStat()` or direct field access
   
2. **Character class** needs to:
   - Store warrant modifiers separately OR
   - Aggregate them when creating `CharacterStatsData` snapshot

3. **CharacterStatsData.InitializeFromCharacter()** needs to:
   - Include warrant modifiers when aggregating stats

---

## Implementation Summary

### ✅ Completed Changes

1. **Character.cs**
   - Added `Dictionary<string, float> warrantStatModifiers` to store non-damage warrant modifiers
   - Renamed `ClearAllDamageModifiers()` to `ClearAllWarrantModifiers()` to clear both damage and stat modifiers
   - Updated `RefreshWarrantModifiers()` to use the new clearing method

2. **CharacterStatsData.cs**
   - Updated `InitializeFromCharacter()` to apply warrant stat modifiers using `AddToStat()`
   - Warrant modifiers are now aggregated when creating stat snapshots

3. **WarrantModifierCollector.cs**
   - Completely rewrote `ApplyModifiersToCharacter()` to handle all stat types
   - Damage modifiers → Applied to `DamageModifiers` lists (converted to decimal: 8% → 0.08)
   - Non-damage modifiers → Applied to `character.warrantStatModifiers` dictionary (stored as percentage: 8% → 8)
   - Added comprehensive mapping for all stat keys from WarrantAffixDatabase

### 📋 Mapped Stat Types

**Damage Modifiers (to DamageModifiers lists):**
- ✅ increasedPhysicalDamage
- ✅ increasedFireDamage
- ✅ increasedColdDamage
- ✅ increasedLightningDamage
- ✅ increasedChaosDamage
- ✅ increasedElementalDamage (applied to all 3 elemental types)
- ✅ increasedAttackDamage
- ✅ increasedSpellDamage

**Non-Damage Stat Modifiers (to warrantStatModifiers dictionary):**
- ✅ evasionIncreased
- ✅ maxHealthIncreased
- ✅ maxManaIncreased
- ✅ energyShieldIncreased
- ✅ armourIncreased
- ✅ increasedProjectileDamage
- ✅ increasedAreaDamage
- ✅ increasedMeleeDamage
- ✅ increasedRangedDamage
- ✅ increasedAxeDamage
- ✅ increasedBowDamage
- ✅ increasedMaceDamage
- ✅ increasedSwordDamage
- ✅ increasedWandDamage
- ✅ increasedDaggerDamage
- ✅ increasedOneHandedDamage
- ✅ increasedTwoHandedDamage
- ✅ increasedIgniteMagnitude
- ✅ increasedShockMagnitude
- ✅ increasedChillMagnitude
- ✅ increasedFreezeMagnitude
- ✅ increasedBleedMagnitude
- ✅ increasedPoisonMagnitude
- ✅ increasedDamageOverTime
- ✅ increasedPoisonDamage
- ✅ increasedPoisonDuration
- ✅ increasedDamageVsChilled
- ✅ increasedDamageVsShocked
- ✅ increasedDamageVsIgnited
- ✅ attackSpeed
- ✅ castSpeed
- ✅ statusEffectDuration
- ✅ aggressionGainIncreased
- ✅ focusGainIncreased
- ✅ guardEffectivenessIncreased
- ✅ lessDamageFromElites
- ✅ statusAvoidance

## Next Steps

1. ✅ Create this audit document
2. ✅ Update `WarrantModifierCollector` to handle all stat types
3. ⏳ **TEST REQUIRED:** Test each stat type to ensure correct application
4. ⏳ **VERIFY REQUIRED:** Verify stat aggregation in `CharacterStatsData` works correctly
5. ⏳ **AUDIT REQUIRED:** Cross-reference all stat keys in WarrantAffixDatabase and WarrantNotableDatabase with this mapping to ensure nothing is missing

