# Monster Modifier Implementation Plan

This document categorizes the 30 monster modifier ideas by implementation complexity and provides implementation notes.

## Implementation Categories

### ✅ **Easy - Simple Effect Types** (Can implement immediately)
These can be added as new `ModifierEffectType` values and handled in `ModifierEffectHandler`:

1. **Rot-Slicked** (Common) - `PoisonOnHit` (already exists!)
2. **Smoldering Armor** (Common) - `SmolderingArmor` - Apply fire damage to attackers
3. **Blood-Tither** (Uncommon) - `BloodTither` - Steal life on damage taken
4. **Volatile Pulse** (Uncommon) - `VolatilePulse` - AoE lightning on crit hit
5. **Wail of the Fallen** (Uncommon) - `WailOfTheFallen` - Apply vulnerable on death
6. **Grudge-Forged** (Rare) - `GrudgeForged` - Damage increases enemy's damage next turn

### ⚠️ **Medium - Requires Tracking/State** (Need additional systems)
These need new tracking systems but are straightforward:

7. **Time-Lagged** (Uncommon) - `DelayedActions` - **Requires DelayedAction system** ✅ (Implementing now)
8. **Temporal Slip** (Uncommon) - `TemporalSlip` - 10% chance to avoid damage and heal
9. **Overclocked Core** (Rare) - `OverclockedCore` - Gains 2% speed/damage every turn
10. **Shifting Plates** (Uncommon) - `ShiftingPlates` - Alternates Armored/Evasive
11. **Despairmonger** (Uncommon) - `Despairmonger` - Apply affliction at health thresholds
12. **Paradox-Bound** (Rare) - `ParadoxBound` - Track last damage type, gain resistance
13. **Echo of Missteps** (Uncommon) - `EchoOfMissteps` - Track last card played
14. **Threadbare** (Uncommon) - `Threadbare` - Takes extra damage, attacks twice

### 🔴 **Complex - Requires New Systems** (Need significant work)
These require new game systems or deep integration:

15. **Fractured Echo** (Rare) - `FracturedEcho` - Requires enemy spawning system
16. **Contract-Leech** (Rare) - `ContractLeech` - Requires player buff event system
17. **Inverted Harmonics** (Very rare) - `InvertedHarmonics` - Requires damage threshold system
18. **Whisper-Wrapped** (Rare) - `WhisperWrapped` - Requires deck manipulation system
19. **Insight Drinker** (Uncommon) - `InsightDrinker` - Requires card draw event system
20. **Icebound Roots** (Common) - `IceboundRoots` - Requires mana cost modification system
21. **Scrapstorm** (Rare) - `Scrapstorm` - Requires retaliation system (similar to Smoldering Armor)
22. **Unraveling Aura** (Very rare) - `UnravelingAura` - Requires player buff removal system
23. **Law-Twisted** (Rare) - `LawTwisted` - Requires damage type system integration
24. **Hexweaver** (Uncommon) - `Hexweaver` - Requires curse system
25. **Bound in Silence** (Rare) - `BoundInSilence` - Requires skill silencing system
26. **Contract Hoarder** (Uncommon) - `ContractHoarder` - Requires card draw event + loot multiplier
27. **Clausebreaker** (Rare) - `Clausebreaker` - Requires hit counter + debuff system
28. **Soul-Linked** (Rare) - `SoulLinked` - Requires enemy linking system
29. **Erratic Pulse** (Rare) - `ErraticPulse` - Requires random action system

## Implementation Priority

### Phase 1: Delayed Actions System + Time-Lagged ✅
- Create `DelayedAction` class
- Add delayed action queue to `Enemy`
- Modify `CombatManager` to process delayed actions
- Implement Time-Lagged modifier with damage bonus

### Phase 2: Simple Retaliation Effects
- Smoldering Armor
- Volatile Pulse
- Blood-Tither
- Wail of the Fallen

### Phase 3: Turn-Based Tracking
- Overclocked Core
- Shifting Plates
- Despairmonger (health threshold tracking)

### Phase 4: Damage Type Tracking
- Paradox-Bound
- Echo of Missteps

### Phase 5: Complex Systems (Future)
- Fractured Echo (spawning)
- Contract-Leech (buff events)
- Whisper-Wrapped (deck manipulation)
- etc.

## Notes

- **Delayed Actions**: This is a foundational system that enables Time-Lagged and could be used for other delayed effects
- **Event Systems**: Many modifiers need event hooks (on player buff, on card draw, etc.) - consider creating a combat event system
- **Damage Type System**: Some modifiers need to track damage types - ensure the damage system supports this
- **Enemy Linking**: Soul-Linked requires enemies to know about each other - may need a combat-wide registry

## CSV Format Additions

For modifiers with parameters, add these columns:
- `DelayedActionTurns` - Number of turns to delay (default: 1)
- `DamageBonusPerDelayedAction` - % damage bonus per queued action (default: 10)
- `TemporalSlipChance` - Chance to avoid damage (default: 0.1)
- `TemporalSlipHealPercent` - % of damage to heal (default: 0.1)
- `BloodTitherPercent` - % of damage to steal as life (default: 0.05)
- `GrudgeForgedDamageBonus` - % damage bonus per damage taken (default: 1.0)
- `OverclockedCoreBonusPerTurn` - % speed/damage per turn (default: 2.0)
- `ThreadbareExtraDamage` - % extra damage taken (default: 20)
- `ThreadbareAttackEfficiency` - % efficiency for second attack (default: 50)
