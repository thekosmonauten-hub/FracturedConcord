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

- `{comboGuard}` already includes the guard scaling portion, so if you want “Guard increase + Combo scaling” as a single number, use `{comboGuard}`.
- You can freely mix text and variables: e.g., `"Combo: +{comboDamage} dmg, +{comboGuard} guard (after Guard)."`
- AoE toggles and ailments are not auto-inserted via tokens; describe those in text if needed.


