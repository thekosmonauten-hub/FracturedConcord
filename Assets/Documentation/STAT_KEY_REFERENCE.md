# Stat Key Reference Guide

**Purpose**: Complete reference of all stat keys that are currently implemented and working in the game. Use this when creating affixes for Effigies, Warrants, or any other item system.

**Last Updated**: After Warrant System Phase 3 implementation

---

## 📋 How to Use This Guide

When creating an `AffixModifier`, use the `statName` field with one of the stat keys listed below. The stat keys are **case-insensitive** (e.g., `"strength"` and `"Strength"` both work).

### Modifier Types
- **Flat**: Adds a fixed value (e.g., `+10 Strength`)
- **Increased**: Adds a percentage value (e.g., `+15% increased Fire Damage`)
- **More**: Multiplicative modifier (rare, usually from special sources)

### Modifier Scope
- **Global**: Affects the entire character (default for effigies)
- **Local**: Affects only the specific item (e.g., weapon-specific damage)

---

## 🎯 Core Attributes

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `strength` | Flat (int) | Base Strength attribute | `+10` |
| `dexterity` | Flat (int) | Base Dexterity attribute | `+10` |
| `intelligence` | Flat (int) | Base Intelligence attribute | `+10` |

---

## ❤️ Combat Resources

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `maxHealth` | Flat (int) | Maximum health points | `+50` |
| `maxHealthIncreased` | Increased (%) | % increased Maximum Health | `+15%` |
| `maxMana` | Flat (int) | Maximum mana points | `+5` |
| `maxManaIncreased` | Increased (%) | % increased Maximum Mana | `+20%` |
| `maxEnergyShield` | Flat (int) | Maximum energy shield points | `+30` |
| `energyShieldIncreased` | Increased (%) | % increased Energy Shield | `+25%` |
| `maxReliance` | Flat (int) | Maximum reliance points | `+50` |
| `currentGuard` | Flat (float) | Current guard amount | `+100` |

**Note**: For flat health/mana, use `maxHealthFlat` and `maxManaFlat` in warrant modifiers (stored in `warrantFlatModifiers`).

---

## ⚔️ Damage Modifiers (Increased %)

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `increasedPhysicalDamage` | Increased (%) | % increased Physical Damage | Applied to all physical damage |
| `increasedFireDamage` | Increased (%) | % increased Fire Damage | Applied to all fire damage |
| `increasedColdDamage` | Increased (%) | % increased Cold Damage | Applied to all cold damage |
| `increasedLightningDamage` | Increased (%) | % increased Lightning Damage | Applied to all lightning damage |
| `increasedChaosDamage` | Increased (%) | % increased Chaos Damage | Applied to all chaos damage |
| `increasedElementalDamage` | Increased (%) | % increased Elemental Damage | Applied to Fire/Cold/Lightning (calculated) |
| `increasedAttackDamage` | Increased (%) | % increased Attack Damage | **Only affects cards with "Attack" tag** |
| `increasedSpellDamage` | Increased (%) | % increased Spell Damage | **Only affects cards with "Spell" tag** |
| `increasedProjectileDamage` | Increased (%) | % increased Projectile Damage | **Only affects cards with "Projectile" tag** |
| `increasedAreaDamage` | Increased (%) | % increased Area Damage | **Only affects cards with "AoE" tag** |
| `increasedMeleeDamage` | Increased (%) | % increased Melee Damage | **Only affects cards that scale with Melee weapons** |
| `increasedRangedDamage` | Increased (%) | % increased Ranged Damage | **Only affects cards that scale with Projectile weapons** |

### Weapon Type Damage Modifiers

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `increasedAxeDamage` | Increased (%) | % increased Axe Damage | Requires weapon with "axe" tag |
| `increasedBowDamage` | Increased (%) | % increased Bow Damage | Requires weapon with "bow" tag |
| `increasedMaceDamage` | Increased (%) | % increased Mace Damage | Requires weapon with "mace" tag |
| `increasedSwordDamage` | Increased (%) | % increased Sword Damage | Requires weapon with "sword" tag |
| `increasedWandDamage` | Increased (%) | % increased Wand Damage | Requires weapon with "wand" tag |
| `increasedDaggerDamage` | Increased (%) | % increased Dagger Damage | Requires weapon with "dagger" tag |
| `increasedOneHandedDamage` | Increased (%) | % increased One-Handed Damage | Requires weapon with "onehanded" tag |
| `increasedTwoHandedDamage` | Increased (%) | % increased Two-Handed Damage | Requires weapon with "twohanded" tag |

---

## ➕ Added Damage (Flat)

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `addedPhysicalDamage` | Flat (float) | Adds flat Physical Damage | `+5` to `+15` |
| `addedFireDamage` | Flat (float) | Adds flat Fire Damage | `+3` to `+10` |
| `addedColdDamage` | Flat (float) | Adds flat Cold Damage | `+3` to `+10` |
| `addedLightningDamage` | Flat (float) | Adds flat Lightning Damage | `+3` to `+10` |
| `addedChaosDamage` | Flat (float) | Adds flat Chaos Damage | `+2` to `+8` |
| `addedElementalDamage` | Flat (float) | Adds flat Elemental Damage (calculated) | Auto-calculated |
| `addedSpellDamage` | Flat (float) | Adds flat Spell Damage | `+4` to `+12` |
| `addedAttackDamage` | Flat (float) | Adds flat Attack Damage | `+4` to `+12` |
| `addedProjectileDamage` | Flat (float) | Adds flat Projectile Damage | `+3` to `+9` |
| `addedAreaDamage` | Flat (float) | Adds flat Area Damage | `+3` to `+9` |
| `addedMeleeDamage` | Flat (float) | Adds flat Melee Damage | `+4` to `+12` |
| `addedRangedDamage` | Flat (float) | Adds flat Ranged Damage | `+4` to `+12` |

---

## 🛡️ Defense Stats

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `armour` | Flat (int) | Base Armour value | From equipped items |
| `armourIncreased` | Increased (%) | % increased Armour | Scales base armour from items |
| `evasion` | Flat (float) | Base Evasion value | From equipped items |
| `evasionIncreased` | Increased (%) | % increased Evasion | Scales base evasion from items |
| `energyShield` | Flat (int) | Base Energy Shield value | From equipped items |
| `blockChance` | Flat (float) | Base Block Chance (%) | `0.0` to `1.0` (0% to 100%) |
| `blockChanceIncreased` | Increased (%) | % increased Block Chance | Adds to base block chance |
| `blockEffectivenessIncreased` | Increased (%) | % increased Block Effectiveness | Increases damage blocked |
| `dodgeChance` | Flat (float) | Base Dodge Chance (%) | `0.0` to `1.0` (0% to 100%) |
| `dodgeChanceIncreased` | Increased (%) | % increased Dodge Chance | Adds to base dodge chance |
| `spellDodgeChance` | Flat (float) | Spell Dodge Chance (%) | `0.0` to `1.0` |
| `spellBlockChance` | Flat (float) | Spell Block Chance (%) | `0.0` to `1.0` |
| `criticalStrikeAvoidance` | Flat (float) | % chance to avoid critical strikes | `0.0` to `1.0` |

---

## 🔥 Resistances

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `physicalResistance` | Flat (float) | Physical Resistance (%) | `+10%` |
| `fireResistance` | Flat (float) | Fire Resistance (%) | `+15%` |
| `coldResistance` | Flat (float) | Cold Resistance (%) | `+15%` |
| `lightningResistance` | Flat (float) | Lightning Resistance (%) | `+15%` |
| `chaosResistance` | Flat (float) | Chaos Resistance (%) | `+10%` |
| `elementalResistance` | Flat (float) | Elemental Resistance (calculated) | Auto-calculated average |
| `allResistance` | Flat (float) | All Resistance (calculated) | Auto-calculated average |
| `physicalResistanceIncreased` | Increased (%) | % increased Physical Resistance | `+20%` |
| `allElementalResistancesIncreased` | Increased (%) | % increased All Elemental Resistances | Applies to Fire/Cold/Lightning |
| `fireResistancePenetration` | Flat (float) | % Fire Resistance Penetration | Reduces enemy fire resistance |

---

## 🎯 Critical Strike Modifiers

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `increasedCriticalStrikeChance` | Increased (%) | % increased Critical Strike Chance | Generic crit chance |
| `increasedCriticalStrikeMultiplier` | Increased (%) | % increased Critical Strike Multiplier | Generic crit multiplier |
| `increasedCriticalChanceWithDaggers` | Increased (%) | % increased Crit Chance with Daggers | Weapon-specific |
| `increasedCriticalChanceWithSwords` | Increased (%) | % increased Crit Chance with Swords | Weapon-specific |
| `increasedCriticalMultiplierWithDaggers` | Increased (%) | % increased Crit Multiplier with Daggers | Weapon-specific |
| `increasedCriticalMultiplierWithSwords` | Increased (%) | % increased Crit Multiplier with Swords | Weapon-specific |
| `increasedCriticalChanceWithColdSkills` | Increased (%) | % increased Crit Chance with Cold Skills | Element-specific |
| `increasedCriticalChanceWithLightningCards` | Increased (%) | % increased Crit Chance with Lightning Cards | Element-specific |
| `increasedCriticalChanceVsFullLife` | Increased (%) | % increased Crit Chance vs Full Life Enemies | Conditional |
| `increasedCriticalChanceVsCursed` | Increased (%) | % increased Crit Chance vs Cursed Enemies | Conditional |
| `increasedCriticalChanceWithProjectileCards` | Increased (%) | % increased Crit Chance with Projectile Cards | Card-type specific |
| `increasedCriticalChanceWithPreparedCards` | Increased (%) | % increased Crit Chance with Prepared Cards | Card-type specific |
| `increasedSpellCriticalChance` | Increased (%) | % increased Spell Critical Chance | Spell-specific |
| `increasedSpellCriticalMultiplier` | Increased (%) | % increased Spell Critical Multiplier | Spell-specific |
| `increasedCriticalDamageMultiplier` | Increased (%) | % increased Critical Damage Multiplier | Generic crit damage |

---

## 🦠 Ailment Modifiers

### Ailment Chance

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `chanceToShock` | Flat (float) | Chance to Shock on Hit (%) | `0.0` to `1.0` (0% to 100%) |
| `chanceToChill` | Flat (float) | Chance to Chill on Hit (%) | `0.0` to `1.0` |
| `chanceToFreeze` | Flat (float) | Chance to Freeze on Hit (%) | `0.0` to `1.0` |
| `chanceToIgnite` | Flat (float) | Chance to Ignite on Hit (%) | `0.0` to `1.0` |
| `chanceToBleed` | Flat (float) | Chance to Bleed on Hit (%) | `0.0` to `1.0` |
| `chanceToPoison` | Flat (float) | Chance to Poison on Hit (%) | `0.0` to `1.0` |
| `randomAilmentChance` | Flat (float) | Chance to apply Random Ailment (%) | `0.0` to `1.0` |
| `ailmentApplicationChanceIncreased` | Increased (%) | % increased Ailment Application Chance | Generic ailment chance boost |

### Ailment Magnitude (Damage/Effect)

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `increasedIgniteMagnitude` | Increased (%) | % increased Ignite Magnitude | Increases ignite damage |
| `increasedShockMagnitude` | Increased (%) | % increased Shock Magnitude | Increases shock effect |
| `increasedChillMagnitude` | Increased (%) | % increased Chill Magnitude | Increases chill effect |
| `increasedFreezeMagnitude` | Increased (%) | % increased Freeze Magnitude | Increases freeze effect |
| `increasedBleedMagnitude` | Increased (%) | % increased Bleed Magnitude | Increases bleed damage |
| `increasedPoisonMagnitude` | Increased (%) | % increased Poison Magnitude | Increases poison damage |
| `increasedDamageOverTime` | Increased (%) | % increased Damage Over Time | Generic DoT boost |
| `increasedPoisonDamage` | Increased (%) | % increased Poison Damage | Poison-specific damage |
| `increasedPoisonDuration` | Increased (%) | % increased Poison Duration | Poison duration extension |

### Ailment Effectiveness

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `chillEffectivenessIncreased` | Increased (%) | % increased Chill Effectiveness | Makes chill more effective |
| `shockEffectivenessIncreased` | Increased (%) | % increased Shock Effectiveness | Makes shock more effective |
| `ailmentDurationIncreased` | Increased (%) | % increased Ailment Duration | Generic ailment duration boost |

### Situational Damage vs Ailments

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `increasedDamageVsChilled` | Increased (%) | % increased Damage vs Chilled Enemies | Conditional damage |
| `increasedDamageVsShocked` | Increased (%) | % increased Damage vs Shocked Enemies | Conditional damage |
| `increasedDamageVsIgnited` | Increased (%) | % increased Damage vs Ignited Enemies | Conditional damage |

---

## ⚡ Speed & Duration Modifiers

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `attackSpeed` | Increased (%) | % increased Attack Speed | **Affects Aggression charge gain** |
| `castSpeed` | Increased (%) | % increased Cast Speed | **Affects Focus charge gain** |
| `movementSpeed` | Flat (float) | Movement Speed multiplier | `1.0` = base speed |
| `movementSpeedIncreased` | Increased (%) | % increased Movement Speed | |
| `statusEffectDuration` | Increased (%) | % increased Status Effect Duration | **Rounds up to nearest whole number** |
| `skillEffectDuration` | Flat (float) | Skill Effect Duration multiplier | |
| `attackRange` | Flat (float) | Attack Range | |
| `projectileSpeed` | Flat (float) | Projectile Speed | |
| `areaOfEffect` | Flat (float) | Area of Effect multiplier | |

---

## 🔋 Charge/Resource Gain Modifiers

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `aggressionGainIncreased` | Increased (%) | % increased Aggression Gain | **Multiplier on top of attackSpeed** |
| `focusGainIncreased` | Increased (%) | % increased Focus Gain | **Multiplier on top of castSpeed** |

**Formula**: `finalGain = baseGain * (1 + speed / 100) * (1 + gainIncreased / 100)`

---

## 🛡️ Guard & Defense Utilities

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `guardEffectiveness` | Flat (float) | Base Guard Effectiveness | Additive to guard from cards |
| `guardEffectivenessIncreased` | Increased (%) | % increased Guard Effectiveness | **Applied to cards with "Guard" tag** |
| `guardRetentionIncreased` | Increased (%) | % increased Guard Retention | % more guard retained next turn |
| `damageReductionWhileGuarding` | Flat (float) | % Damage Reduction while Guarding | `0.0` to `1.0` |
| `guardBreakChance` | Flat (float) | Chance to Break Enemy Guard (%) | `0.0` to `1.0` |
| `lessDamageFromElites` | Flat (float) | % Less Damage from Elites | **Applied vs Rare/Unique enemies** |
| `statusAvoidance` | Flat (float) | % Status Effect Avoidance | **Evasion check for enemy-applied debuffs** |

---

## 💊 Recovery & Leech

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `lifeRegeneration` | Flat (float) | Life Regeneration per turn | `+5` |
| `lifeRegenerationIncreased` | Increased (%) | % increased Life Regeneration | `+20%` |
| `lifeRecoveryRateIncreased` | Increased (%) | % increased Life Recovery Rate | `+15%` |
| `energyShieldRegeneration` | Flat (float) | Energy Shield Regeneration per turn | `+3` |
| `manaRegeneration` | Flat (float) | Mana Regeneration per turn | `+2` |
| `manaRegenerationIncreased` | Increased (%) | % increased Mana Regeneration | `+25%` |
| `relianceRegeneration` | Flat (float) | Reliance Regeneration per turn | `+10` |
| `lifeSteal` | Flat (float) | Life Steal on Hit (%) | `0.05` (5% of damage) |
| `lifeStealOnHitIncreased` | Increased (%) | % increased Life Steal | `+30%` |
| `manaSteal` | Flat (float) | Mana Steal on Hit (%) | `0.02` (2% of damage) |
| `manaLeechChance` | Flat (float) | Chance to Leech Mana on Hit (%) | `0.1` (10%) |
| `energyShieldLeech` | Flat (float) | Energy Shield Leech on Hit (%) | `0.03` (3% of damage) |
| `lifeOnKill` | Flat (float) | Life Gained on Kill | `+10` or `+5%` |

**Legacy Support**: `lifeLeech` and `manaLeech` map to `lifeSteal` and `manaSteal`.

---

## 🎴 Card System Stats

### Base Stats

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `cardsDrawnPerTurn` | Flat (int) | Cards Drawn per Turn | `+1` |
| `maxHandSize` | Flat (int) | Maximum Hand Size | `+2` |
| `discardPileSize` | Flat (int) | Discard Pile Size | `+5` |
| `exhaustPileSize` | Flat (int) | Exhaust Pile Size | `+3` |
| `cardDrawChance` | Flat (float) | Card Draw Chance (%) | `0.1` (10%) |
| `cardRetentionChance` | Flat (float) | Card Retention Chance (%) | `0.15` (15%) |
| `cardUpgradeChance` | Flat (float) | Card Upgrade Chance (%) | `0.05` (5%) |
| `discardPower` | Flat (float) | Discard Power | `+10` |
| `manaPerTurn` | Flat (float) | Mana per Turn | `+1` |

### Card System Modifiers (Increased %)

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `cardDrawChanceIncreased` | Increased (%) | % increased Card Draw Chance | |
| `handSizeIncreased` | Flat (int) | Flat increase to Hand Size | **Flat, not percentage** |
| `manaCostEfficiencyIncreased` | Increased (%) | % increased Mana Cost Efficiency | Reduces mana costs |
| `manaRefundChance` | Flat (float) | Chance to Refund Mana on Card Play (%) | `0.0` to `1.0` |
| `cardCycleSpeedIncreased` | Increased (%) | % increased Card Cycle Speed | |
| `preparedCardEffectivenessIncreased` | Increased (%) | % increased Prepared Card Effectiveness | |
| `discardCostReduction` | Flat (int) | Flat reduction to Discard Costs | **Flat, not percentage** |
| `discardPowerIncreased` | Increased (%) | % increased Discard Power | |
| `delayedCardEffectivenessIncreased` | Increased (%) | % increased Delayed Card Effectiveness | |
| `skillCardEffectivenessIncreased` | Increased (%) | % increased Skill Card Effectiveness | |
| `spellPowerIncreased` | Increased (%) | % increased Spell Power | |
| `echoCardEffectivenessIncreased` | Increased (%) | % increased Echo Card Effectiveness | |
| `chainChance` | Flat (float) | Chance for Spells to Chain (%) | `0.0` to `1.0` |
| `spellEffectVsAilmentIncreased` | Increased (%) | % increased Spell Effect vs Ailments | |

---

## 🎯 Situational/Conditional Damage Modifiers

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `increasedDamageVsCursed` | Increased (%) | % increased Damage vs Cursed Enemies | |
| `increasedDamageVsBlocked` | Increased (%) | % increased Damage vs Blocked Enemies | |
| `increasedDamageVsSlowed` | Increased (%) | % increased Damage vs Slowed Enemies | |
| `increasedDamageWhileInShadow` | Increased (%) | % increased Damage while in Shadow | |
| `increasedDamageOnCriticalStrikes` | Increased (%) | % increased Damage on Critical Strikes | |
| `increasedDamageAfterDiscard` | Increased (%) | % increased Damage after Discarding | Temporary boost |
| `increasedDamageWhenBelowLifeThreshold` | Increased (%) | % increased Damage when Below Life Threshold | Conditional |
| `increasedDamageVsRareAndUnique` | Increased (%) | % increased Damage vs Rare/Unique Enemies | |
| `increasedDamageWithConsecutiveAttacks` | Increased (%) | % increased Damage with Consecutive Attacks | Combo scaling |
| `increasedDamageOnEveryNthAttack` | Increased (%) | % increased Damage on Every Nth Attack | Conditional |
| `increasedDamageVsMarked` | Increased (%) | % increased Damage vs Marked Enemies | |

---

## 🎭 Status Effect & Curse Modifiers

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `slowChance` | Flat (float) | Chance to Slow Enemies (%) | `0.0` to `1.0` |
| `freezeChance` | Flat (float) | Chance to Freeze Enemies (%) | `0.0` to `1.0` |
| `curseApplicationChance` | Flat (float) | Chance to Apply Curses (%) | `0.0` to `1.0` |
| `curseDurationIncreased` | Increased (%) | % increased Curse Duration | |
| `curseEffectivenessIncreased` | Increased (%) | % increased Curse Effectiveness | |
| `debuffExpirationRateIncreased` | Increased (%) | % faster Debuff Expiration | |

---

## 🥊 Stun & Crowd Control

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `stunDurationIncreased` | Increased (%) | % increased Stun Duration on Enemies | |
| `staggerEffectivenessIncreased` | Increased (%) | % increased Stagger Effectiveness | |
| `increasedDamageToStaggered` | Increased (%) | % increased Damage to Staggered Enemies | |
| `reducedEnemyStaggerThreshold` | Increased (%) | % reduced Enemy Stagger Threshold | Effectively increases stagger damage |
| `increasedStaggerDuration` | Flat (float) | Flat addition to Stagger Stun Duration (turns) | Base is 1 turn |

---

## 🛡️ Damage Reduction & Mitigation

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `damageReductionIncreased` | Increased (%) | % increased Damage Reduction | Generic damage reduction |
| `damageReductionFromSpells` | Flat (float) | % reduced Damage from Spells | `0.0` to `1.0` |
| `damageReductionWhenStunned` | Flat (float) | % reduced Damage when Stunned | Conditional |
| `physicalDamageReductionIncreased` | Increased (%) | % increased Physical Damage Reduction | |
| `physicalReductionIncreased` | Increased (%) | % increased Physical Reduction | Shorthand |

---

## 📊 More Damage Multipliers

These are **multiplicative** modifiers (rare, usually from special sources):

| Stat Key | Type | Description | Notes |
|----------|------|-------------|-------|
| `morePhysicalDamage` | More (mult) | More Physical Damage multiplier | `1.0` = no change, `1.2` = 20% more |
| `moreFireDamage` | More (mult) | More Fire Damage multiplier | |
| `moreColdDamage` | More (mult) | More Cold Damage multiplier | |
| `moreLightningDamage` | More (mult) | More Lightning Damage multiplier | |
| `moreChaosDamage` | More (mult) | More Chaos Damage multiplier | |
| `moreElementalDamage` | More (mult) | More Elemental Damage multiplier | |
| `moreSpellDamage` | More (mult) | More Spell Damage multiplier | |
| `moreAttackDamage` | More (mult) | More Attack Damage multiplier | |
| `moreProjectileDamage` | More (mult) | More Projectile Damage multiplier | |
| `moreAreaDamage` | More (mult) | More Area Damage multiplier | |
| `moreMeleeDamage` | More (mult) | More Melee Damage multiplier | |
| `moreRangedDamage` | More (mult) | More Ranged Damage multiplier | |

**Note**: "More" multipliers are multiplicative and stack multiplicatively. They are typically rare and come from special sources.

---

## 🎯 Combat Stats

| Stat Key | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `attackPower` | Flat (int) | Attack Power | `+10` |
| `defense` | Flat (int) | Defense | `+15` |
| `criticalChance` | Flat (float) | Critical Chance (%) | `0.05` (5%) |
| `criticalMultiplier` | Flat (float) | Critical Multiplier | `1.5` (150%) |
| `accuracy` | Flat (float) | Accuracy | `+10` |

---

## 📝 Important Notes

### Stat Key Format
- All stat keys are **case-insensitive**
- Use lowercase in your code for consistency: `"strength"`, `"increasedfiredamage"`, etc.
- No spaces or special characters (except camelCase)

### Value Types
- **Flat (int)**: Integer values (e.g., `+10 Strength`)
- **Flat (float)**: Decimal values (e.g., `+5.5% Dodge Chance`)
- **Increased (%)**: Percentage values stored as-is (e.g., `15` = 15%)
- **More (mult)**: Multiplicative multiplier (e.g., `1.2` = 20% more)

### Conditional Modifiers
Many modifiers only apply under specific conditions:
- **Card tags**: `increasedAttackDamage` only affects cards with "Attack" tag
- **Weapon tags**: `increasedBowDamage` only affects weapons with "bow" tag
- **Enemy status**: `increasedDamageVsChilled` only affects chilled enemies
- **Enemy rarity**: `lessDamageFromElites` only affects Rare/Unique enemies

### Special Behaviors
- **`statusEffectDuration`**: Rounds up to nearest whole number when applied
- **`guardEffectivenessIncreased`**: Applied to cards with "Guard" tag
- **`statusAvoidance`**: Rolled individually for each status effect applied by enemies
- **`attackSpeed`/`castSpeed`**: Directly affect Aggression/Focus charge gain
- **`aggressionGainIncreased`/`focusGainIncreased`**: Multipliers on top of speed gains

---

## 🔗 Related Documentation

- `WARRANT_STAT_MAPPING_AUDIT.md` - Warrant stat implementation details
- `EFFIGY_AFFIX_SYSTEM_ANALYSIS.md` - Effigy affix system overview
- `CharacterStatsData.cs` - Source code for all stat definitions

---

## ✅ Verification

All stat keys listed here are:
- ✅ Defined in `CharacterStatsData.cs`
- ✅ Supported in `GetStatValue()` and `SetStatValue()`
- ✅ Can be used in `AddToStat()` for warrant modifiers
- ✅ Can be used in `AffixModifier.statName` for affixes

**Last Verified**: After Warrant System Phase 3 implementation

