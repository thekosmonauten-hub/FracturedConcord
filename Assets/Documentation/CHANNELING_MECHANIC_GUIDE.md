# Channeling Mechanic Guide

> **Updated:** 2025-11-07

Channeling tracks when the player casts the **same card group** in succession. Once the player plays the same `GroupKey` two or more times in a row, they are considered to be channeling. Playing any card with a different `GroupKey` (or clearing the state) breaks the channel.

---

## 1. Runtime Data Model

- Each `Character` now exposes a non-persisted `ChannelingTracker` via `character.Channeling`.
- `ChannelingTracker` exposes:
  - `IsChanneling`
  - `ActiveGroupKey`
  - `ConsecutiveCastCount`
  - `RegisterCast(string groupKey)` → updates state and returns a `ChannelingState` snapshot
  - `BreakChannel()` / `Reset()` → clears the streak
- `ChannelingState` also flags whether the channel **started** or **stopped** on the current cast so FX/UI can react exactly once.

> ⚠️ Channeling is combat-only. The tracker is non-serialized and automatically reset when a run loads or whenever `CombatDeckManager.ResetChannelingState()` is called.

---

## 2. Combat Deck Integration

`CombatDeckManager` drives the system during card play:

- `UpdateChannelingState()` is called as soon as the card resolves (before card effects are applied).
- New event: `OnChannelingStateChanged(Character owner, ChannelingState state)` fires on every update.
- `CurrentChannelingState` property exposes the most recent snapshot.
- `ResetChannelingState()` is invoked after the initial hand is drawn and is safe to call from turn/phase logic when you need to forcibly end a streak.

### Immediate vs animated plays

Both the animated flow and the fallback “instant play” path update channeling before raising `OnCardPlayed`, so downstream systems always observe fresh data.

---

## 3. Card Runtime Metadata

The temporary `Card` runtime object (used by `CardEffectProcessor`, `ComboSystem`, etc.) now carries channeling metadata:

- `channelingActive` (bool) – true once the streak reaches 2+ casts.
- `channelingStacks` (int) – number of consecutive casts for the current `GroupKey`.
- `channelingStartedThisCast` (bool) – true only on the 2nd cast (entry point into channeling).
- `channelingStoppedThisCast` (bool) – true when the previous streak was broken by this play.
- `channelingGroupKey` (string) – normalized `GroupKey` used for tracking.

> Use these fields inside custom `CardEffect` logic or combo processors to gate bonuses (“if channelingActive…”) or to trigger one-time surges when `channelingStartedThisCast` is true.

---

## 4. Designing Channeling Cards

1. Ensure variants that should count as the same spell share a `GroupKey` in their `CardData` asset.
2. Enable **Channeling Bonuses** on the `CardDataExtended` asset when you want automated scaling:
   - `Channeling Bonus Enabled`: turn the feature on for this card.
   - `Channeling Min Stacks`: how many consecutive casts are required before the bonus kicks in (default 2).
   - `Channeling Damage Increased %`: additive scaling (enter 150 for +150% increased damage).
   - `Channeling Damage More %`: multiplicative scaling (enter 30 for +30% more damage).
   - `Channeling Additional Guard`: flat guard added before percentage bonuses.
   - `Channeling Guard Increased %`: additive scaling for guard.
   - `Channeling Guard More %`: multiplicative scaling for guard.
3. In your effect logic, reference either:
   - `player.Channeling` for full control (manual queries, status UI), or
   - the runtime card metadata for quick checks inside processors/combos.
4. For retroactive rewards (e.g., “on the third cast, deal +X”), inspect `channelingStacks` when the effect fires.
5. For live requirements (“while channeling, …”), query `channelingActive` or `player.Channeling.IsChanneling` before applying.

---

## 5. Resetting Channeling

Channeling ends automatically when:

- The player plays a card with a different `GroupKey` (streak drops to 1 and `IsChanneling` becomes false).
- `ResetChannelingState()` / `CombatDeckManager.ResetChannelingState()` is called (e.g., start of turn, end of combat, scripted cutscene).

Designers can also create cards that intentionally break channeling by invoking `player.Channeling.BreakChannel()` inside custom effects.

---

## 6. Events and Hooks Cheat Sheet

| Location | Hook | Purpose |
|----------|------|---------|
| `CombatDeckManager` | `OnChannelingStateChanged` | Subscribe to update UI, trigger VFX, or play SFX when channeling starts/stops. |
| `Character.Channeling` | `RegisterCast / BreakChannel` | Manual control from bespoke systems. |
| `Card` runtime instance | `channeling*` fields | Quick access during damage/guard calculations or combo effects. |
| `CombatAnimationManager` | `ShowFloatingText` | Reusable popups; already used to announce channel start/break near the player. |

---

## 7. Best Practices

- Group upgrades and alternate art under a shared `GroupKey` so you keep streaks consistent.
- Use `channelingStartedThisCast` for “surge on entry” cards—this prevents duplicate triggers.
- Remember to reset channeling at scene transitions or non-combat interactions if the streak should not persist.
- For UI, combine `OnChannelingStateChanged` with `CurrentChannelingState` to update indicators instantly and avoid polling.

Channeling is now ready for designers to layer unique bonuses on repeated casts—have fun weaving it into your Witch starting deck! 

