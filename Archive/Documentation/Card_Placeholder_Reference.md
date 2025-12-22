# Card Placeholder Reference

This reference documents every placeholder token that is resolved at runtime when cards are rendered in Dexiled.  Use it when authoring `CardDataExtended` assets, combo descriptions, or any other templated card strings.

> **Terminology**
> - **Runtime card**: the legacy `Card` instance produced from `CardData` / `CardDataExtended` when decks are loaded.
> - **Character**: the active runtime character used to compute scaling.  Equipment scenes, combat, and other post-character-creation flows always pass a character.
> - **Template**: the string stored in the asset (e.g. `description`, `comboDescription`).  At runtime the template is copied to the card and the placeholders below are replaced.

---

## 1. Core Description Placeholders (`CardDataExtended.description`)

| Placeholder | Replacement (with character) | Fallback (no character) | Notes |
|-------------|------------------------------|--------------------------|-------|
| `{damage}` | Total damage after scaling and weapon bonuses | Base damage (`damage`) | Uses `DamageCalculator.CalculateCardDamage`. Applies whenever the card has damage > 0. |
| `{baseDamage}` | Base damage value | Base damage value | Useful for “X + Y” style tooltips. |
| `{strBonus}` `{dexBonus}` `{intBonus}` | Multiplicative bonus from damage scaling | Removed if scaling absent | Requires character. |
| `{strDivisor}` `{dexDivisor}` `{intDivisor}` | Attribute ÷ divisor (damage scaling) | Removed if divisor ≤ 0 or no character | e.g. INT/2. |
| `{guard}` | Total guard after scaling | Base guard (`block`) | Evaluated for any card that grants block (`block > 0`). |
| `{baseGuard}` | Base guard value | Base guard value |  |
| `{guardStrBonus}` `{guardDexBonus}` `{guardIntBonus}` | Multiplicative bonus from guard scaling | Removed if scaling absent | Mirrors `{strBonus}` etc. for guard. |
| `{guardStrDivisor}` `{guardDexDivisor}` `{guardIntDivisor}` | Attribute ÷ divisor (guard scaling) | Removed if divisor ≤ 0 or no character | Use for text like “Gain {guard} Guard ({baseGuard} + INT/{guardIntDivisor}).” |
| `{manaCost}` / `{cost}` | `playCost` | `playCost` | Always replaced even if character is null. |
| `{aoeTargets}` | `aoeTargets` or `1` | `aoeTargets` or `1` | Guarantees a numeric value even if AoE flag is false. |
| `{str}` `{dex}` `{int}` | Character attributes | Attribute defaults (no replacement when character null) | Replaced with the current character’s primary stats. |
| `{weaponDamage}` | Damage contributed by the relevant weapon type | Removed when scaling off | Uses `Character.weapons.GetWeaponDamage` for melee/projectile/spell depending on card flags. |

**Sanitising unknown placeholders**: if a template still contains tokens after evaluation (for example `{foo}`), runtime sanitisation strips them so the player never sees raw braces.

### Example
```
Deal {damage} Lightning damage. Gain {guard} Guard.
```
With a character (INT 40, scaling INT/2):
```
Deal 24 Lightning damage. Gain 4 Guard.
```
Without a character (e.g. card preview before creation):
```
Deal 6 Lightning damage. Gain 2 Guard.
```

---

## 2. Combo Description Placeholders (`CardDataExtended.comboDescription`)

The combination system uses a dedicated resolver (`GetDynamicComboDescription`). Available tokens:

| Placeholder | Replacement | Notes |
|-------------|-------------|-------|
| `{comboDamage}` | `comboAttackIncrease + scalingBonus` | `scalingBonus` depends on `comboScaling` and `comboScalingDivisor`. |
| `{comboGuard}` | `comboGuardIncrease + guardScalingBonus` | Guard bonus mirrors damage calculation. |
| `{manaRefund}` | `comboManaRefund` | Integer value. |
| `{bolsterStacks}` | `1 + floor(attribute / divisor)` (clamped 1–10) when Bolster is present | Only evaluated when `comboBuffs` contains "Bolster". |
| `{comboBuff}` | Same as `{bolsterStacks}` (convenience alias) | Empty string when no matching buff. |

**Literal stat fragments** inside combo templates *(and normal descriptions)* are also replaced:

- `STR/x`, `DEX/x`, `INT/x` → `floor(attribute / x)` (only when a character exists).

For additional guidance, see `Assets/Documentation/Combo Variables.md` (this document complements that one).

---

## 3. Supplemental Effect Text Generation

`CardDisplay` automatically assembles extra lines if certain tags are present:

- **Dual Wield** → `"Dual Wield: Deal damage with both weapons"`
- **Discard** → `"Discard: Special effect when discarded"`
- **AoE** → `"Hits {aoeTargets} enemies"`

These strings are appended beneath the combo description in Equipment/Deck UI’s "AdditionalEffectText" block. They do not require explicit placeholders.

---

## 4. Combo & Channeling Fields (Asset Inspector Reference)

When editing `CardDataExtended` in the inspector:

- **Damage Scaling → Strength/Dexterity/Intelligence** control multiplicative factors.
- **Damage Scaling → Strength/Dexterity/Intelligence Divisor** (new) support additive divisors (e.g. `2` for `STAT/2`). Same structure applies to Guard scaling.
- **Combo Scaling Type / Divisor** follow the rules described in the combo placeholders table.

Ensure divisors are positive. Zero/negative values are ignored at runtime.

---

## 5. Adding New Placeholders

When introducing new mechanics:

1. Extend `AttributeScaling` (for general scaling) or the relevant resolver method.
2. Add documentation here to keep designers aligned.
3. Provide sanitising defaults so empty braces never reach the UI.

Remember to run the Equipment screen or combat scenes after changes to confirm new tokens render correctly in the card carousel, deck builder, and hover previews.
