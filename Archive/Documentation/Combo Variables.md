### Combo Description Variables

Use these placeholders in `CardDataExtended.comboDescription`. They are resolved at runtime by `GetDynamicComboDescription(Character)`.

- **{comboDamage}**: The total combo attack increase including combo scaling.
  - Formula: `comboAttackIncrease + scalingBonus`

- **{comboGuard}**: The total combo guard increase including combo scaling (when scaling type is Strength; others may be ignored depending on design).
  - Formula: `comboGuardIncrease + scalingBonus` (scaling applied per implementation)

- **{manaRefund}**: The mana refunded when combo triggers.
  - Formula: `comboManaRefund`

- **{bolsterStacks}**: Total Bolster stacks granted when the combo triggers.
  - Formula: `1 + floor(Attribute / comboScalingDivisor)` if `comboBuffs` contains "Bolster", clamped to 10. Attribute is chosen by `comboScaling` (STR/DEX/INT).

- **{comboBuff}**: Convenience alias that resolves to the same as `{bolsterStacks}` when Bolster is the only combo buff configured for the card. Otherwise empty.

In addition to the placeholders above, literal stat-division tokens are supported and replaced with computed values at runtime:

- **STR/x**, **DEX/x**, **INT/x**
  - Example: `STR/4` becomes `floor(strength / 4)` for the current character.

### How scaling is computed

- **comboScaling**: One of `Strength | Dexterity | Intelligence | Momentum | DiscardPower | None`
- **comboScalingDivisor**: A positive number that divides the chosen attribute.

Runtime steps:
1) Pick the attribute per `comboScaling` (e.g., current character Strength).
2) Compute `scalingBonus = attribute / comboScalingDivisor` (flooring may apply in UI substitutions for STR/x literal tokens; `{comboDamage}` and `{comboGuard}` use floating math with formatting).
3) Apply to `{comboDamage}` and `{comboGuard}` as described above.

### Examples

- Show damage and guard combo bonuses with scaling:
  - `"Gain {comboGuard} Guard. Deal +{comboDamage} damage."`

- Show mana refund and a stat-derived number:
  - `"Refund {manaRefund} Mana. Also gain STR/4 Guard."`

### Notes and tips

- `{comboGuard}` already includes the guard scaling portion, so if you want "Guard increase + Combo scaling" as a single number, use `{comboGuard}`.
- You can freely mix text and variables: e.g., `"Combo: +{comboDamage} dmg, +{comboGuard} guard (after Guard)."`
- AoE toggles and ailments are not auto-inserted via tokens; describe those in text if needed.

---

## Preparation & Consumed Card Variables

Use these placeholders in `CardDataExtended.description` for cards that interact with the Preparation system (Thief class mechanic). They are resolved at runtime by `GetDynamicDescription(Character)`.

### Preparation Variables

- **{PrepareCount}**: The current number of prepared cards in the player's preparation queue.
  - Formula: `ThiefCardEffects.GetPreparedCardCount()`
  - Always available, even if the card doesn't use other preparation variables.

- **{PrepareDamage}**: The total damage bonus from prepared cards, including base damage and attribute scaling.
  - Formula: `(preparedCardDamageBase + scalingBonus) * preparedCount`
  - Scaling uses `preparedCardDamageScaling` (same AttributeScaling structure as regular damage scaling).
  - Example: If `preparedCardDamageBase = 1`, `preparedCardDamageScaling.dexterityDivisor = 3`, and player has 3 prepared cards with 15 DEX:
    - Damage per card: `1 + (15 / 3) = 6`
    - Total: `6 * 3 = 18`
  - Returns "0" if `preparedCardDamageBase` is 0 and no scaling is configured.

- **{PreparePoison}**: The total poison stacks bonus from prepared cards.
  - Formula: `preparedCardPoisonBase * preparedCount`
  - Example: If `preparedCardPoisonBase = 1` and player has 2 prepared cards:
    - Total: `1 * 2 = 2` poison stacks
  - Returns "0" if `preparedCardPoisonBase` is 0.

- **{PrepareGuard}**: The total guard bonus from prepared cards, including base guard and attribute scaling.
  - Formula: `(preparedCardGuardBase + scalingBonus) * preparedCount`
  - Scaling uses `preparedCardGuardScaling` (same AttributeScaling structure as regular guard scaling).
  - Example: If `preparedCardGuardBase = 2`, `preparedCardGuardScaling.dexterityDivisor = 2`, and player has 2 prepared cards with 12 DEX:
    - Guard per card: `2 + (12 / 2) = 8`
    - Total: `8 * 2 = 16`
  - Returns "0" if `preparedCardGuardBase` is 0 and no scaling is configured.

### Consumed Card Variables

- **{ConsumedDamage}**: The total damage bonus from consuming prepared cards, including base damage and attribute scaling.
  - Formula: `(consumedCardDamageBase + scalingBonus) * preparedCount`
  - **Note**: This assumes all prepared cards will be consumed when the card is played (e.g., "Perfect Strike").
  - Scaling uses `consumedCardDamageScaling` (same AttributeScaling structure as regular damage scaling).
  - Example: If `consumedCardDamageBase = 2`, `consumedCardDamageScaling.dexterityDivisor = 3`, and player has 3 prepared cards with 18 DEX:
    - Damage per consumed card: `2 + (18 / 3) = 8`
    - Total: `8 * 3 = 24`
  - Returns "0" if `consumedCardDamageBase` is 0 and no scaling is configured.

### Examples

**Ambush** (Attack card with prepared card damage bonus):
```
Description: "Deal {damage} physical damage.\nIf you have prepared cards, deal additional {PrepareDamage} damage."
Configuration:
  - preparedCardDamageBase: 1
  - preparedCardDamageScaling.dexterityDivisor: 3
Result (with 2 prepared cards, 15 DEX): 
  "Deal 6 physical damage.\nIf you have prepared cards, deal additional 12 damage."
```

**Poisoned Blade** (Attack card with prepared card poison bonus):
```
Description: "Deal {damage} physical damage. Apply 2 Poison.\nIf you have prepared cards, apply {PreparePoison} Poison."
Configuration:
  - preparedCardPoisonBase: 1
Result (with 3 prepared cards):
  "Deal 4 physical damage. Apply 2 Poison.\nIf you have prepared cards, apply 3 Poison."
```

**Shadow Step** (Guard card with prepared card guard bonus):
```
Description: "Gain {guard} guard.\nPrepare: Gain {PrepareGuard} Guard and 1 temporary Dexterity."
Configuration:
  - preparedCardGuardBase: 2
  - preparedCardGuardScaling.dexterityDivisor: 2
Result (with 2 prepared cards, 10 DEX):
  "Gain 6 guard.\nPrepare: Gain 14 Guard and 1 temporary Dexterity."
```

**Perfect Strike** (Attack card that consumes prepared cards):
```
Description: "Consume all prepared cards and Deal {damage} physical damage.\nDeal {ConsumedDamage} damage."
Configuration:
  - consumedCardDamageBase: 2
  - consumedCardDamageScaling.dexterityDivisor: 3
Result (with 3 prepared cards, 18 DEX):
  "Consume all prepared cards and Deal 10 physical damage.\nDeal 24 damage."
```

### Use Cases

1. **Scaling Damage with Preparation**:
   - Use `{PrepareDamage}` for cards that gain damage based on prepared cards (e.g., Ambush, Twin Strike).
   - Combine with attribute scaling for Dexterity-based builds.

2. **Status Effect Synergy**:
   - Use `{PreparePoison}` for cards that apply additional poison stacks when you have prepared cards (e.g., Poisoned Blade).
   - Encourages building up a preparation queue before unleashing.

3. **Defensive Preparation**:
   - Use `{PrepareGuard}` for guard cards that scale with prepared cards (e.g., Shadow Step).
   - Provides defensive scaling for preparation-focused builds.

4. **Consumption Mechanics**:
   - Use `{ConsumedDamage}` for cards that consume all prepared cards for a powerful effect (e.g., Perfect Strike).
   - Creates a risk/reward decision: build up preparation or consume early.

5. **Combining Variables**:
   - You can use multiple preparation variables in the same description:
     ```
     "Deal {damage} damage. If you have prepared cards, deal +{PrepareDamage} damage and apply {PreparePoison} Poison."
     ```

### Configuration Notes

- **Base Values**: Set `preparedCardDamageBase`, `preparedCardPoisonBase`, `preparedCardGuardBase`, or `consumedCardDamageBase` to 0 to disable that variable.
- **Scaling**: Use `AttributeScaling` structures (same as regular damage/guard scaling) for attribute-based bonuses:
  - `strengthDivisor`, `dexterityDivisor`, `intelligenceDivisor` for additive scaling (e.g., `DEX/3`)
  - `strengthScaling`, `dexterityScaling`, `intelligenceScaling` for multiplicative scaling
- **Runtime Calculation**: All variables are calculated at runtime based on the current number of prepared cards and character attributes.
- **Fallback Behavior**: If a variable is not configured (base = 0 and no scaling), it will display as "0" in the description.


