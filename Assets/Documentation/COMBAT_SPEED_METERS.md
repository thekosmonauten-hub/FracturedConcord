## Combat Speed Meters

### Overview
- **Aggression Meter** – fills whenever the player plays an `Attack` card. Progress scales with the character’s attack speed multiplier. When the meter reaches 100%, the player earns an Aggression charge that can power future attack-based bonuses (e.g., half-cost attacks). Excess progress rolls over into the next charge.
- **Focus Meter** – fills whenever the player plays a `Skill` or `Power` card. Progress scales with the character’s cast speed multiplier. Focus charges unlock spell-centric benefits (e.g., guaranteed combos).
- **Enemy Energy** – enemy units track a hidden energy budget that Chill/Slow effects can drain. Once fully implemented, energy denial will block high-cost enemy moves.
- **Movement Speed** – compresses encounter wave counts (capped so the boss wave always appears). Implementation is staged after player meters.
  - Baseline values: unarmed attack speed = 1.0 attacks/sec, baseline cast speed = 1.5 casts/sec. Increases from items/passives modify these via the new Character speed helpers.

### Current Implementation (2025-11-07)
- `CombatDeckManager` tracks both meters, emits `OnSpeedMeterChanged` events with normalized progress and stored charges.
- Ring UI wiring lives in `SpeedMeterRing` (referenced by `PlayerCombatDisplay`). Aggression/Focus progress shows as paired arc fills; stored charges are displayed numerically.
- Mechanics that consume charges (e.g., half-cost Attacks, forced combos) are not yet hooked; meters are purely informational while we validate pacing.
- Movement-speed-derived wave compression and enemy energy denial are planned but not active yet.

### Tuning Notes
- Default gain per card is ~3.33% for Aggression (≈30 attacks per charge) and 5% for Focus (≈20 spells per charge) at baseline speed; adjust in `CombatDeckManager` as needed.
- Charges are integer counts; future systems should handle both “first charge grants effect” and “bank multiple charges for larger payoffs”.
- When we introduce charge consumers, use the existing event pipeline to trigger popups/highlights rather than adding new UI clutter.

### UI Guidelines
- Aggression ring color: `#FF8A3C`. Focus ring color: `#4ECDE6`.
- Keep the ring subtle (50% alpha background) so health/guard bars remain readable.
- Use the existing floating combat text helpers (`CombatAnimationManager.ShowFloatingText`) for one-shot notifications instead of permanent HUD elements.

### Building the Meter Ring (Step-by-Step)
1. **Open the combat UI prefab or scene** (e.g. `Assets/UI/Combat/PlayerCombatDisplay.prefab`).
2. **Create the container**
   - Add an empty `RectTransform` named `SpeedMeterRing` as a child of the player display root.
   - Set size ≈ 260×260, anchors to center, and position so it frames the portrait/guard bars.
3. **Background ring**
   - Add an `Image` (`RingBackground`) under the container. Use any circular sprite (Unity built-in `UI/Sprite/Knob` works).
   - `Image Type = Simple`; tint with ~30% alpha to keep it subtle.
4. **Aggression fill**
   - Duplicate the background image, rename to `AggressionFill`.
   - Set `Image Type = Filled`, `Fill Method = Radial 360`, `Fill Origin = Top`, `Clockwise = true`.
   - Tint with `#FF8A3C` (≈70% alpha).
5. **Focus fill**
   - Duplicate `AggressionFill`, rename to `FocusFill`.
   - Change color to `#4ECDE6` and set `Fill Origin = Bottom` (so the fills sweep opposite directions).
6. **Charge counters**
   - Add two `TextMeshProUGUI` children: `AggressionCharges` (anchor top-right) and `FocusCharges` (anchor bottom-right).
   - Use a bold font around 36pt with outline/glow for readability. Leave the text empty by default.
7. **Ready indicators (optional)**
   - Add small `Image` objects (`AggressionReady`, `FocusReady`) near the ring edge.
   - Use a glow/dot sprite, tint to match the meter, and disable the GameObjects by default.
8. **Attach the script**
   - Select the `SpeedMeterRing` GameObject and add the `SpeedMeterRing` component (`Assets/Scripts/UI/Combat/SpeedMeterRing.cs`).
   - Assign the Aggression/Focus fill images, charge texts, and ready indicators in the inspector.
9. **Link PlayerCombatDisplay**
   - In `PlayerCombatDisplay`, ensure the `speedMeterRing` serialized field references the component (drag the GameObject in the inspector).
10. **Verify in Play Mode**
   - Play Attack cards to see the orange fill advance and charges increment.
   - Play Skill/Power cards to confirm the teal fill responds. Ready indicators should light up once at least one charge is stored.

### TODO / Follow-Up
1. Hook Aggression charges into the mana cost calculation for attack cards (half-cost for the next charge spent).
2. Allow Focus charges to auto-satisfy combo requirements or offer a small menu of spell bonuses.
3. Surface enemy energy orbs beneath health bars and integrate Chill to drain them.
4. Add encounter preview logic that displays wave compression from movement speed.
5. Expand documentation once consumption rules are implemented.

