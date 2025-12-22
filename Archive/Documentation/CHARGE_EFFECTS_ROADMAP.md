# Charge Effects Roadmap

## ✅ Simple Effects (Implemented)

### Aggression
- ✅ Next Card hits twice
- ✅ Next Card hits all enemies (AoE)
- ✅ Next Card always Critically Strikes
- ✅ Next Card ignores Guard or Armor

### Focus
- ✅ Next Card deals +100% increased Effect or Damage
- ✅ Next Card costs 50% less Mana

---

## 🔜 Complex Effects (Future Implementation)

### Aggression
- **Next Card gains +5% More Damage per Aggression Charge stored**
  - Requires: Charge count tracking, damage multiplier system
  - Implementation: Query `aggressionCharges` from `CombatDeckManager`, apply `(1 + charges * 0.05)` multiplier

- **Next Card triggers "Combo" regardless of combo state**
  - Requires: Combo system integration, card combo state tracking
  - Implementation: Force combo flag on next card play, bypass combo requirements

### Focus
- **Next Card applies its associated Ailment at maximum intensity**
  - Requires: Ailment system, card-to-ailment mapping, intensity scaling
  - Implementation: Query card's ailment type, apply at max stacks/intensity

- **Next Card repeats itself after 1 turn**
  - Requires: Card scheduling system, delayed execution queue
  - Implementation: Queue card effect to re-execute after enemy turn completes

- **Next Card targets an additional enemy**
  - Requires: Multi-targeting logic, target selection UI
  - Implementation: Allow player to select second target, apply effect to both

- **Next Card scales off your highest attribute instead of its default**
  - Requires: Attribute system integration, card attribute mapping
  - Implementation: Detect highest of STR/DEX/INT, override card's base attribute scaling

---

## Notes
- All simple effects are consumed on the next card play
- Complex effects may require additional systems or UI elements
- Consider player preferences/settings for default charge effects

