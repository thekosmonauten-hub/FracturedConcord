# Warrant Stat Implementation Plan

This document outlines the step-by-step implementation plan for all warrant stat requirements.

**Status:** 🚧 IN PROGRESS

---

## Phase 1: Fully Feasible Items (Start Here) ✅

### 1.1 Card Tag-Based Damage Modifiers
**Priority:** HIGH  
**Estimated Time:** 1-2 hours  
**Files to Modify:**
- `Assets/Scripts/Combat/Damage/StatsDamageCalculator.cs` (or wherever `ComputeFinalDamage` is)
- `Assets/Scripts/Warrants/WarrantModifierCollector.cs` (already mapped, but verify)

**Implementation Steps:**
1. Verify `DamageContext` flags are set correctly (`isProjectile`, `isArea`, `isMelee`, `isRanged`)
2. Add checks in damage calculation for:
   - `increasedProjectileDamage` → only if `ctx.isProjectile`
   - `increasedAreaDamage` → only if `ctx.isArea` OR card has "AoE" tag
   - `increasedMeleeDamage` → only if `ctx.isMelee` (which is `card.scalesWithMeleeWeapon`)
   - `increasedRangedDamage` → only if `ctx.isRanged` (which is `card.scalesWithProjectileWeapon`)
3. Test with cards that have these properties

---

### 1.2 Guard Effectiveness
**Priority:** HIGH  
**Estimated Time:** 30 minutes  
**Files to Modify:**
- `Assets/Scripts/CombatSystem/CardEffectProcessor.cs` (`CalculateGuard` method)
- `Assets/Scripts/Combat/DamageCalculation.cs` (`CalculateGuardAmount` method)

**Implementation Steps:**
1. In `CalculateGuard`/`CalculateGuardAmount`, get `guardEffectivenessIncreased` from `CharacterStatsData`
2. Apply as: `finalGuard = baseGuard * (1 + guardEffectivenessIncreased / 100)`
3. Test with a Guard card and verify the multiplier works

---

### 1.3 Flat vs Increased Modifiers (maxHealthFlat, maxManaFlat)
**Priority:** MEDIUM  
**Estimated Time:** 1 hour  
**Files to Modify:**
- `Assets/Scripts/Data/Character.cs` (add flat modifier fields)
- `Assets/Scripts/Data/CharacterStatsData.cs` (add flat modifier fields and apply them)
- `Assets/Scripts/Warrants/WarrantModifierCollector.cs` (map flat modifiers)
- `Assets/Scripts/Warrants/WarrantAffixDatabase.cs` (add flat modifier support)

**Implementation Steps:**
1. Add `maxHealthFlat` and `maxManaFlat` to `Character.warrantStatModifiers` (or separate dictionary)
2. Add fields to `CharacterStatsData`
3. Apply flat modifiers BEFORE percentage modifiers:
   - `finalMaxHealth = (baseMaxHealth + maxHealthFlat) * (1 + maxHealthIncreased / 100)`
   - `finalMaxMana = (baseMaxMana + maxManaFlat) * (1 + maxManaIncreased / 100)`
4. Update `WarrantModifierCollector` to handle flat modifiers
5. Test with both flat and increased modifiers

---

### 1.4 Status Effect Duration
**Priority:** MEDIUM  
**Estimated Time:** 1-2 hours  
**Files to Modify:**
- Status effect application system (need to find where status effects are created)
- `Assets/Scripts/Combat/DamageCalculation.cs` or status effect processor

**Implementation Steps:**
1. Find where status effects are applied to enemies
2. Get `statusEffectDuration` from `CharacterStatsData` before creating status effect
3. Apply modifier: `finalDuration = baseDuration * (1 + statusEffectDuration / 100)`
4. Round up to nearest whole number: `finalDuration = Mathf.Ceil(finalDuration)`
5. Test with a status effect card

---

## Phase 2: Feasible with Modifications ⚙️

### 2.1 Defense Modifiers Scaling Combined Values
**Priority:** HIGH  
**Estimated Time:** 2-3 hours  
**Files to Modify:**
- `Assets/Scripts/Data/EquipmentManager.cs` (track base values)
- `Assets/Scripts/Data/Character.cs` (store base defense values)
- `Assets/Scripts/Data/CharacterStatsData.cs` (apply increased modifiers to base values)

**Implementation Steps:**
1. In `EquipmentManager`, track base evasion/armour/energy shield separately from modifiers
2. Store base values in `Character` (e.g., `baseEvasionFromItems`, `baseArmourFromItems`, `baseEnergyShieldFromItems`)
3. In `CharacterStatsData.InitializeFromCharacter()`, calculate:
   - `finalEvasion = (baseEvasionFromItems) * (1 + evasionIncreased / 100)`
   - `finalArmour = (baseArmourFromItems) * (1 + armourIncreased / 100)`
   - `finalEnergyShield = (baseEnergyShieldFromItems) * (1 + energyShieldIncreased / 100)`
4. Test with items that provide these stats

---

### 2.2 Weapon Type Modifiers
**Priority:** MEDIUM  
**Estimated Time:** 2-3 hours  
**Files to Modify:**
- `Assets/Scripts/Data/CharacterWeapons.cs` (access weapon tags)
- `Assets/Scripts/Combat/DamageCalculation.cs` (apply weapon type modifiers to weapon damage)

**Implementation Steps:**
1. Modify `CharacterWeapons.GetWeaponDamage()` to accept weapon item and check tags
2. Or create new method `GetWeaponDamageWithModifiers(WeaponType, Character)` that:
   - Gets base weapon damage
   - Checks equipped weapon's `itemTags`
   - Applies appropriate `increasedXDamage` modifier from `CharacterStatsData`
3. Update `CalculateCardDamage` to use the new method
4. Test with different weapon types

---

### 2.3 Ailment Damage Modifiers
**Priority:** MEDIUM  
**Estimated Time:** 2-3 hours  
**Files to Modify:**
- Status effect damage calculation system (need to find where status effects deal damage)
- `Assets/Scripts/Combat/DamageCalculation.cs` or status effect processor

**Implementation Steps:**
1. Find where status effects calculate damage (e.g., Poison damage per turn)
2. Get ailment modifiers from `CharacterStatsData` before calculating damage
3. Apply modifiers:
   - `poisonDamage = basePoisonDamage * (1 + increasedPoisonDamage / 100) * (1 + increasedPoisonMagnitude / 100)`
   - Similar for other ailments
4. Test with Poison Arrow or similar ailment card

---

### 2.4 Conditional Damage (vs Chilled, Shocked, Ignited)
**Priority:** LOW  
**Estimated Time:** 1 hour  
**Files to Modify:**
- `Assets/Scripts/Combat/Damage/StatsDamageCalculator.cs`

**Implementation Steps:**
1. In `StatsDamageCalculator.ComputeFinalDamage()`, check `ctx.targetChilled`, `ctx.targetShocked`, `ctx.targetIgnited`
2. Add appropriate `increasedDamageVsX` modifiers to increased damage calculation
3. Ensure `DamageContext` is populated correctly with target status
4. Test with cards that deal damage to statused enemies

---

## Phase 3: Needs Investigation 🔍

### 3.1 Attack/Cast Speed Affecting Aggression/Focus Charges
**Priority:** MEDIUM  
**Estimated Time:** 3-4 hours (including investigation)  
**Files to Investigate:**
- Search for "Aggression" and "Focus" in codebase
- Card effect processing system
- Charge gain system

**Implementation Steps:**
1. **INVESTIGATE:** Find where Aggression and Focus charges are gained
2. Identify the base charge gain amount for Attack/Skill cards
3. Apply `attackSpeed` multiplier for Attack cards: `finalGain = baseGain * (1 + attackSpeed / 100)`
4. Apply `castSpeed` multiplier for Skill cards: `finalGain = baseGain * (1 + castSpeed / 100)`
5. Test with Attack and Skill cards

---

### 3.2 AggressionGain/FocusGain as Multipliers
**Priority:** MEDIUM  
**Estimated Time:** 1 hour (after 3.1 is complete)  
**Files to Modify:**
- Same as 3.1

**Implementation Steps:**
1. After implementing 3.1, apply `aggressionGainIncreased` and `focusGainIncreased` as additional multipliers
2. Formula: `finalGain = baseGain * (1 + speedModifier / 100) * (1 + gainModifier / 100)`
3. Test with both speed and gain modifiers

---

### 3.3 Less Damage from Elites
**Priority:** LOW  
**Estimated Time:** 1-2 hours  
**Files to Modify:**
- Enemy damage application system
- `Assets/Scripts/Combat/DamageCalculation.cs`

**Implementation Steps:**
1. Find where enemy damage is applied to player
2. Check enemy rarity (Rare/Unique)
3. If elite, apply `lessDamageFromElites` as damage reduction: `finalDamage = damage * (1 - lessDamageFromElites / 100)`
4. Test with Rare/Unique enemies

---

### 3.4 Status Avoidance
**Priority:** LOW  
**Estimated Time:** 2-3 hours  
**Files to Modify:**
- Status effect application system (where enemies apply status to player)

**Implementation Steps:**
1. Find where enemies apply status effects to player
2. Before applying status, check `statusAvoidance` from `CharacterStatsData`
3. Roll: `Random.Range(0f, 100f) < statusAvoidance` → avoid status
4. Apply check individually for each status effect
5. Test with enemy status effects

---

## Implementation Order

1. ✅ **Phase 1.1** - Card Tag-Based Damage Modifiers (START HERE)
2. ✅ **Phase 1.2** - Guard Effectiveness
3. ✅ **Phase 1.3** - Flat vs Increased Modifiers
4. ✅ **Phase 1.4** - Status Effect Duration
5. ⏳ **Phase 2.1** - Defense Modifiers Scaling
6. ⏳ **Phase 2.2** - Weapon Type Modifiers
7. ⏳ **Phase 2.3** - Ailment Damage Modifiers
8. ⏳ **Phase 2.4** - Conditional Damage
9. ⏳ **Phase 3.1** - Attack/Cast Speed → Charges (INVESTIGATE FIRST)
10. ⏳ **Phase 3.2** - AggressionGain/FocusGain Multipliers
11. ⏳ **Phase 3.3** - Less Damage from Elites
12. ⏳ **Phase 3.4** - Status Avoidance

---

## Testing Checklist

For each implemented feature:
- [ ] Test with warrant that provides the modifier
- [ ] Verify modifier is applied correctly
- [ ] Test edge cases (0%, 100%, negative values if applicable)
- [ ] Verify modifier clears when warrant is removed
- [ ] Check debug logs for correct values
- [ ] Verify UI displays correct values (if applicable)

---

## Notes

- All percentage values from warrants are stored as percentages (8 = 8%), not decimals
- Damage modifiers are converted to decimals (8% → 0.08) for `DamageModifiers` lists
- Stat modifiers are stored as percentages (8 = 8%) in `warrantStatModifiers` dictionary
- Always test with both single and multiple warrants providing the same modifier

