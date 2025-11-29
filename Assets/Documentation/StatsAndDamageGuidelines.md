## Stats and Damage Guidelines

- Always compute runtime totals via `StatAggregator.BuildTotals(CharacterStatsData)` when stats change (level up, gear swap, warrants, temporary buffs).
- Use the returned `ComputedStats` for:
  - Resource/defense totals: `maxHealth`, `maxMana`, `energyShield`, `armour`, `evasion`.
  - Guard/defense: `guardEffectivenessMult`, `eliteLessMult`, `statusAvoidChance`.
  - Resource gains: `aggressionGainMult`, `focusGainMult`.

- Card/Combat damage:
  - Build a `DamageContext` from card tags and target state (attack/spell, projectile/area, melee/ranged, damage type, weapon type, target ailments, elite).
  - Compute damage using `DamageCalculator.ComputeFinalDamage(baseDamage, characterStatsData, computedTotals, context)`.
  - “Increased%” are additive; “more” multipliers are multiplicative.

- Ailments:
  - Use `AilmentSystem.ComputeAilmentDuration` and `ComputeAilmentMagnitude` for duration and magnitude scaling (includes global and per-ailment increases).

- Guard:
  - Use `GuardSystem.ComputeGuardGain` for guard creation.
  - Apply elite damage reduction with `GuardSystem.ApplyIncomingDamageModifiers`.

- Notes:
  - Keep stat aggregation centralized; do not re-implement additive/multiplicative logic in feature code.
  - Extend `DamageContext` and the calculators as new tags/mechanics are introduced.


