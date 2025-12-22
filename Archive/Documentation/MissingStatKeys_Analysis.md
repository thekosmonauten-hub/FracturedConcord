# Missing Stat Keys Analysis

## Summary
This document lists all stat strings that failed to map during Notable import and proposes implementation strategies.

## Categories of Missing Stats

### 1. Critical Strike Stats (Generic & Conditional)
**Missing Keys:**
- `increasedCriticalStrikeChance` - Generic crit chance
- `increasedCriticalStrikeMultiplier` - Generic crit multiplier
- `increasedCriticalChanceWithDaggers` - Weapon-specific crit chance
- `increasedCriticalChanceWithSwords` - Weapon-specific crit chance
- `increasedCriticalMultiplierWithDaggers` - Weapon-specific crit multiplier
- `increasedCriticalMultiplierWithSwords` - Weapon-specific crit multiplier
- `increasedCriticalChanceWithColdSkills` - Element-specific crit chance
- `increasedCriticalChanceWithLightningCards` - Card type crit chance
- `increasedCriticalChanceVsFullLife` - Conditional crit chance
- `increasedCriticalChanceVsCursed` - Conditional crit chance
- `increasedCriticalChanceWithProjectileCards` - Card type crit chance
- `increasedCriticalChanceWithPreparedCards` - Card state crit chance
- `increasedSpellCriticalChance` - Spell-specific crit chance
- `increasedSpellCriticalMultiplier` - Spell-specific crit multiplier
- `increasedCriticalDamageMultiplier` - Generic crit damage multiplier

**Implementation Strategy:**
- Add generic `increasedCriticalStrikeChance` and `increasedCriticalStrikeMultiplier` fields
- For weapon/element-specific crit, use existing `criticalChance` as base and add conditional modifiers
- Consider a `CriticalStrikeContext` struct for conditional crit calculations (similar to `DamageContext`)

### 2. Life/Mana Regeneration & Recovery
**Missing Keys:**
- `lifeRegenerationIncreased` - % increased life regen per turn
- `lifeRecoveryRateIncreased` - % increased life recovery rate
- `manaRegenerationIncreased` - % increased mana regen per turn
- `FlatManaRegeneration` - Flat Mana regeneration per turn (Flat stat)
- `lifeRecoveryPerTurn` - Flat life recovery per turn (flat stat)

**Implementation Strategy:**
- Add `lifeRegenerationIncreased` and `manaRegenerationIncreased` as % multipliers
- These should multiply the base `lifeRegeneration` and `manaRegeneration` values
- Flat recovery stats can use existing `lifeRegeneration` field directly

### 3. Guard System Stats
**Missing Keys:**
- `guardRetentionIncreased` - % more guard retained next turn
- `guardEffectiveness` - Additive to Guard provided by cards (base stat)
- `guardEffectivenessIncreased` - % increased to guardEffectiveness stat (additive multiplier)
- `damageReductionWhileGuarding` - % damage reduction when guarding
- `guardBreakChance` - Chance to break enemy guard

**Implementation Strategy:**
- Add to existing guard system in `GuardSystem.cs`
- `guardRetentionIncreased` affects how much guard carries over between turns
- `guardEffectiveness` is base stat that adds to guard from cards (additive)
- `guardEffectivenessIncreased` is % increased multiplier to `guardEffectiveness` (additive)
- `damageReductionWhileGuarding` is a conditional multiplier when `currentGuard > 0`

### 4. Block System Stats
**Missing Keys:**
- `blockChanceIncreased` - % increased block chance
- `blockEffectivenessIncreased` - % increased block effect/damage reduction

**Implementation Strategy:**
- `blockChanceIncreased` should additively increase `blockChance`
- `blockEffectivenessIncreased` affects damage reduction when block triggers

### 5. Resistance & Penetration
**Missing Keys:**
- `physicalResistanceIncreased` - % increased physical resistance
- `fireResistancePenetration` - % penetration of fire resistance
- `allElementalResistancesIncreased` - % increased to all elemental resists

**Implementation Strategy:**
- Add `physicalResistanceIncreased` as % multiplier to base `physicalResistance`
- `fireResistancePenetration` is a damage calculation modifier (reduces enemy resistance)
- `allElementalResistancesIncreased` applies to fire/cold/lightning simultaneously

### 6. Card System Stats
**Missing Keys:**
- `cardDrawChanceIncreased` - % increased chance to draw cards
- `handSizeIncreased` - Flat increase to hand size
- `manaCostEfficiencyIncreased` - % reduced mana costs
- `manaRefundChance` - Chance to refund mana on card play
- `cardCycleSpeedIncreased` - % increased card cycle/draw speed
- `preparedCardEffectivenessIncreased` - % increased effect of prepared cards
- `discardCostReduction` - Flat reduction to discard costs
- `discardPowerIncreased` - % increased discard effect
- `delayedCardEffectivenessIncreased` - % increased delayed card effect
- `skillCardEffectivenessIncreased` - % increased skill card effect
- `spellPowerIncreased` - % increased spell power/effectiveness
- `echoCardEffectivenessIncreased` - % increased echo card effectiveness

**Implementation Strategy:**
- Most of these are card-specific mechanics that may need a separate `CardStatsData` system
- For now, add to `CharacterStatsData` as they affect card gameplay
- `handSizeIncreased` is likely an integer (flat), not a percentage

### 7. Movement & Mobility
**Missing Keys:**
- `movementSpeedIncreased` - % increased movement speed

**Implementation Strategy:**
- Add `movementSpeedIncreased` field
- May need to integrate with movement system if one exists

### 8. Dodge & Avoidance
**Missing Keys:**
- `dodgeChanceIncreased` - % increased dodge chance
- `criticalStrikeAvoidance` - % chance to avoid critical strikes
- `debuffExpirationRateIncreased` - % faster debuff expiration

**Implementation Strategy:**
- `dodgeChanceIncreased` additively increases `dodgeChance`
- `criticalStrikeAvoidance` reduces incoming crit chance
- `debuffExpirationRateIncreased` affects status effect duration reduction

### 9. Conditional Damage Modifiers
**Missing Keys:**
- `increasedDamageVsCursed` - % increased damage vs cursed enemies
- `increasedDamageVsBlocked` - % increased damage vs blocked enemies
- `increasedDamageVsSlowed` - % increased damage vs slowed enemies
- `increasedDamageWhileInShadow` - % increased damage while in shadow state
- `increasedDamageOnCriticalStrikes` - % increased damage when you crit
- `increasedDamageAfterDiscard` - % increased damage after discarding (temporary)
- `increasedDamageWhenBelowLifeThreshold` - Conditional damage (multiple thresholds)
- `increasedDamageVsRareAndUnique` - % increased damage vs elite enemies
- `increasedDamageWithConsecutiveAttacks` - Scaling damage with combo
- `increasedDamageOnEveryNthAttack` - Conditional damage (e.g., every 3rd attack)

**Implementation Strategy:**
- These are situational modifiers that need context during damage calculation
- Extend `DamageContext` struct to include target state (cursed, blocked, slowed, etc.)
- Use conditional checks in `DamageCalculator` to apply these modifiers

### 10. Life Steal & Recovery
**Missing Keys:**
- `lifeStealOnHit` - % of damage steal as life
- `lifeStealOnHitIncreased` - % increased life steal
- `lifeOnKill` - Flat or % life gained on kill
- `manaStealChance` - Chance to steal mana on hit

**Implementation Strategy:**
- Add LifeSteal stats to `CharacterStatsData`
- Implement Lifesteal calculation in damage resolution (after damage is dealt)
- `lifeOnKill` triggers on enemy death

### 11. Ailment & Status Effect Stats
**Missing Keys:**
- `ailmentDurationIncreased` - % increased ailment duration (generic)
- `ailmentApplicationChanceIncreased` - % increased chance to apply ailments
- `slowChance` - Chance to slow enemies
- `freezeChance` - Chance to freeze enemies
- `curseApplicationChance` - Chance to apply curses
- `curseDurationIncreased` - % increased curse duration
- `curseEffectivenessIncreased` - % increased curse effect
- `randomAilmentChance` - Chance to apply random ailment
- `chillEffectivenessIncreased` - % increased chill effectiveness
- `shockEffectivenessIncreased` - % increased shock effectiveness

**Implementation Strategy:**
- `ailmentDurationIncreased` can be a generic multiplier for all ailment durations
- Individual ailment chances already exist (`chanceToIgnite`, etc.), but may need generic version
- `curseApplicationChance` and curse-related stats may need a separate curse system

### 12. Special Mechanics (May Not Map to Stats)
**Stat Strings That Need Special Handling:**
- "Gain double Focus charges when you Shock an enemy (once per turn)" - Special trigger effect
- "Draw 1 card when Marked enemy dies" - Trigger effect
- "Gain 5% of damage done as Guard" - Conversion effect
- "Gain +1% Life per turn for 2 turns" - Temporary buff (not a stat)
- "Reduce discard cost by 1 for one card per turn" - Conditional cost reduction
- "When you use a Skill card, reduce next card cost by 50% (once)" - Conditional cost reduction
- "2% Chance to play an extra random card" - Special trigger
- "10% chance to immaterialize once per combat (avoid one hit)" - Special defensive trigger
- "Your Damage randomly converts to another element for +10% extra damage" - Conversion effect

**Implementation Strategy:**
- These are **trigger effects** or **special mechanics**, not stat modifiers
- Consider creating a `WarrantSpecialEffect` system separate from stat modifiers
- Or mark these as "special" in the Notable system and handle them in combat logic
- For now, these can remain unmapped or use placeholder stat keys like `special_effect_*`

### 13. Damage Reduction & Mitigation
**Missing Keys:**
- `damageReductionIncreased` - Generic % damage reduction
- `damageReductionFromSpells` - % reduced damage from spells
- `damageReductionWhenStunned` - Conditional damage reduction
- `physicalDamageReductionIncreased` - % increased physical damage reduction

**Implementation Strategy:**
- Add generic `damageReductionIncreased` field
- Conditional reductions can use `DamageContext` to check attacker state
- These are "less damage taken" multipliers (multiplicative, not additive)

### 14. Stun & Crowd Control
**Missing Keys:**
- `stunDurationIncreased` - % increased stun duration on enemies
- `staggerEffectivenessIncreased` - % increased stagger effectiveness

**Implementation Strategy:**
- Add `stunDurationIncreased` field
- `staggerEffectivenessIncreased` may be related to stun system

### 15. Weapon-Specific Damage (Beyond Existing)
**Missing Keys:**
- `increasedDaggerDamage` - % increased dagger damage

**Implementation Strategy:**
- Add `increasedDaggerDamage` similar to existing weapon types
- Follows same pattern as `increasedAxeDamage`, `increasedSwordDamage`, etc.

## Implementation Priority

### High Priority (Core Combat Stats)
1. Critical strike stats (generic + conditional)
2. Life/Mana regeneration increased
3. Guard system stats
4. Block system stats
5. Resistance & penetration
6. Conditional damage modifiers
7. Life/Mana steal

### Medium Priority (Card System)
8. Card system stats (if card system is implemented)
9. Hand size (if applicable)

### Low Priority (Special Mechanics)
10. Movement speed (if movement system exists)
11. Special trigger effects (may need separate system)

## Recommended Approach

1. **Add High Priority Stats First**: Implement critical strike, regeneration, guard, block, and resistance stats
2. **Extend DamageContext**: Add fields for conditional damage (target cursed, blocked, slowed, etc.)
3. **Create Special Effects System**: For trigger-based effects that don't map to stats
4. **Update WarrantNotableImporter**: Add mappings for all new stat keys
5. **Update StatAggregator**: Include new stats in aggregation
6. **Update DamageCalculator**: Apply conditional modifiers based on context

## Notes

- Some stats may need to be **flat** (integer) rather than percentage (e.g., `handSizeIncreased`, `lifeOnKill`)
- Conditional stats require context during calculation, not just base stat values
- Special trigger effects may be better handled as `WarrantNotableDefinition` special effects rather than stat modifiers

