# Master Checklist

## Combat Systems & Player Mechanics
- [x] Integrate Agitate/Tolerance/Potential mechanic to Enemy
- [x] Integrate Buff mechanics to enemies (Bolster, temp stats etc).
- [x] Hook Aggression charges into attack mana cost discounts
- [x] Allow Focus charges to satisfy combo requirements / spell bonuses
- [x] Surface enemy energy UI and wire Chill/Slow drains
- [x] Add encounter preview showing wave compression from movement speed
- [x] Make sure that enemies can have ailments applied to them their effects are working and that it's displayed in the StatusEffectsContainer
- [x] Extend combat speed meter documentation after consumption rules implemented
- [x] Fix Experience gain from killing enemies.
- [x] Implement Stagger system (meter, threshold, stun on fill, guard protection, energy costs for enemy actions)
- [ ] Test and verify stagger system: confirm stagger values display correctly, guard reduces stagger by 50%, enemies/player skip turns when staggered, stagger bars update properly
- [x] Fix Vulnerability debuff: enemies now take 20% more damage and the debuff is consumed after one damage instance
- [x] Sometime during combat, the discard or drawpile is cleared, leaving the player with very few cards at wave 4-5. (Duplicate cards in discard caused the card to be destroyed instead of discarded) 

## Combat Effects
- [ ] Implement Curses
- [ ] Fix floating damage text timing and impact effect synchronization (deferred for later)
- [ ]
- [ ]
- [ ]
- [ ]
- [ ]
- [ ]
- [ ]
- [ ]

## Passive Tree - Warrant System
- [x] Setup WarrantTree.scene
- [x] Create canvas + scroll view container for tree layout
- [x] Finalize board geometry (socket/effect node counts, adjacency rules)
- [x] Implement multi-page board data model & save slots
- [x] Build warrant socket/effect node prefabs with drag-drop interaction
- [x] Follow prefab plan (see `Assets/Documentation/WarrantPrefabSetup.md`)
- [x] Follow prefab plan (Socket prefab visuals + WarrantSocketView drag-drop)
- [x] Wire Warrant Locker inventory + free swapping flow
- [x] Create a WarrantAffixDatabase (Please make sure that all offensive and defensive CharacterStats have % Increased modifiers that we can add to the WarrantAffixDatabse) (`Asset/Documentation/WarrantDatabase.md`)
- [x] Create WarrantNotableDatabase (separate database for Notable effects with nested modifiers)
- [X] Adjust WarrantPrefab to Contain "Notable" data (Only the specific Warrant Socket has this buff, doesn't apply to Effect Range, See JSON at bottom of `Asset/Documentation/WarrantDatabase.md`)
- [X] Allow A "Master Blueprint" warrant to be copied and rolled with affixes on Drop/Quest reward.
- [ ] Implement Peacekeeper 3→1 fusion UI & modifier locking
- [ ] Author Unique/Keystone warrant behaviors documentation
- [ ] Implement "Upgrading Socketed warrants" - Required to kill X amount of Y enemies to upgrade from Common -> Magic - Once upgrade is ready, Peacekeepers faction will help you "Improve it" (Selection of 3 random affixes.)
- [ ] **Implement Notable Conditional Triggers** - The following Notable effects require special trigger/event systems to be implemented:
  - [ ] Focus charge system: "Gain double Focus charges when you Shock an enemy (once per turn)"
  - [ ] Turn-based spell cost reduction: "first spell each turn costs 10% less mana"
  - [ ] Conditional card draw triggers: "+1 card draw next turn after you play a skill", "Draw 1 card when Marked enemy dies"
  - [ ] Card play event triggers: "3% chance for played cards to refund their mana", "2% chance to play an extra random card"
  - [ ] Ailment duration extension: "Chill inflicted lasts additional 2 turns"
  - [ ] Echo/repeat system: "first card each turn repeats at 25% power", "echoed cards have 25% increased effect", "cards that echo, echo +1 time with 25% effect"
  - [ ] Guard trigger + temporary buff: "on guard, gain +1% life per turn for 2 turns"
  - [ ] Combat start triggers: "first spell each combat gains +10% damage", "first spell each combat grants 5% of damage done as guard"
  - [ ] Combat trigger + damage avoidance: "10% chance to immaterialize once per combat (avoid one hit)"
  - [ ] Life cost trigger + temporary buff: "when you spend life to cast a card, gain +10% damage for 1 turn"
  - [ ] Kill trigger + card return: "on kill, 10% chance to return the card to your hand with 20% effectiveness"
  - [ ] Projectile trigger + debuff: "projectile attacks reduce enemy damage by 6% for 2 turns"
  - [ ] Health threshold trigger: "when below 40% life, +25% damage"
  - [ ] Card play counter trigger: "every 5th card played this combat gains +10% effect", "+1 draw when this triggers"
  - [ ] Skill card trigger + cost reduction: "when you use a skill card, reduce next card cost by 50% (once)"
  - [ ] Multi-target spell trigger: "spells that hit multiple targets gain +12% damage"
  - [ ] Discard cost reduction: "Reduce discard cost by 1 for one card per turn"
  - [ ] Consecutive attack trigger: "after 4 consecutive attacks, next attack deals +60% increased damage"
  - [ ] Damage conversion trigger: "your damage randomly converts to another element for +10% extra damage"
  - [ ] Generic damage (needs context): "+8% damage"
  - [ ] Spell casting trigger: "when you cast a spell, 6% chance to store a free delayed cast"
  - [ ] Area damage trigger: "area attacks deal +12% increased damage"
  - [ ] Fix WarrantScene and make sure that Allocated points are saved after click, even if points are allocated during Tutorial.




## TownScene
- [x] Setup TownScene
- [x] Implement "Seer" panel
- [ ] Implement "Questgiver" panel
- [x] Implement "Forge" panel
- [x] Implement "PeacekeepersFaction" Panel
- [x] Implement Card Generation from "The Seer"
- [ ] Implement Card Vendor from "The Seer"
- [ ] Implement Quests from "Questgiver"
- [x] Implement Salvage from "Forge"
- [x] Implement "Warrants" for the "PeacekeepersFaction"
- [ ] 
- [ ] 


## Ascendancy Pipeline
- [ ] Author 18 AscendancyData assets with splash art
- [ ] Create signature card ScriptableObjects
- [ ] Implement ascendancy core mechanics in code
- [ ] Implement ascendancy passive effects in code
- [ ] Add detailed ascendancy info panel UI
- [ ] Implement in-game ascendancy selection flow (save/apply bonuses/unlock tree)
- [ ] Add dynamic unlock requirement checks (level/quest)

## Embossing & Card Progression
- [ ] Complete Embossing Phase 1: create EmbossingEffectProcessor, update DamageCalculator, test damage multipliers
- [ ] Deliver Embossing Phase 2: Status Effects
- [ ] Deliver Embossing Phase 3: Stat Scaling
- [ ] Deliver Embossing Phase 4: Conversions
- [ ] Deliver Embossing Phase 5: Utility
- [ ] Deliver Embossing Phase 6: Defensive
- [ ] Deliver Embossing Phase 7: Custom
- [X] Implement embossing UI
- [x] Display embossing icons on card slot UI
- [ ] Integrate embossing effects into combat
- [ ] Enhance embossing tooltips for persistence/future pass
- [ ] Add embossing level badge overlay (optional enhancement)
- [ ] Write `EMBOSSING_SYSTEM.md` documentation
- [ ] Explore groupKey future uses (collection, filters, stats, trading)

## Embossing System Revamp (In Progress)
- [x] Create EmbossingModifierDefinition ScriptableObject (similar to RelianceAuraModifierDefinition)
- [x] Create EmbossingModifierRegistry singleton to load and manage embossing modifiers
- [x] Create CardStatCalculator to calculate card stats with embossings immediately when applied
- [x] Update Card.GetDynamicDescription() to use CardStatCalculator and show embossing effects
- [x] Update CardTooltipView to show damage breakdown with embossing contributions
- [x] Standardize all embossing names in "Embossing revisit.md" based on their effects
- [x] Create EmbossingModifierEventProcessor to handle embossing events in combat
- [x] Create editor tool to generate EmbossingModifierDefinition assets from "Embossing revisit.md" (EmbossingModifierGenerator)
- [x] Create editor tool to import EmbossingEffect assets from TSV (EmbossingTSVImporter)
- [x] Create editor tool to link modifiers to embossing assets (EmbossingModifierLinker)
- [x] Process new embossing list from "Embossing revisit.md" and create modifier definitions
- [ ] Map EmbossingEffectType enum values to ModifierActionType for migration (if needed)
- [ ] Review and rebalance existing embossing effects for game balance
- [ ] Test embossing effects work immediately when applied (CardStatCalculator integration)
- [ ] Test embossing effects work in combat (event-driven modifier system)

## Card & Deck UX
- [ ] Wire card icon sprite context for inventory/reward displays
- [ ] Increase the scale of CardGrid cards in Deck Builder scene.
- [ ] Implement deck templates
- [ ] Implement deck statistics panel
- [ ] Implement deck code sharing (export/import)
- [ ] Add drag-and-drop card management in deck builder


## Current Priority Tasks
### Embossing System Revamp
- [x] Create EmbossingModifierEventProcessor to handle embossing events in combat
- [x] Create editor tool to generate EmbossingModifierDefinition assets from "Embossing revisit.md"
- [x] Process all 90 embossings from "Embossing revisit.md" and create modifier definitions
- [ ] Test that embossing effects update card damage immediately when applied
- [ ] Test that embossing effects work correctly in combat via event system
- [ ] Review and rebalance embossing power levels if needed
- [ ] Map EmbossingEffectType enum values to ModifierActionType for migration (if needed)

### Equipment System
- [x] Allow 1-handed weapons to be equipped in both MainHand and OffHand slots
- [x] Prevent equipping OffHand weapon if MainHand is empty
- [x] Auto-move OffHand weapon to MainHand when MainHand is unequipped

## Miscellaneous Follow-ups
- [ ] Update remaining systems to consume CardDataExtended directly
- [ ] Review Compilation/Migration documentation future items
- [ ] Verify outstanding notes for future development logs
- [x] Adopt `StatAggregator.BuildTotals()` + `DamageCalculator` for all card damage computations (see `Assets/Documentation/StatsAndDamageGuidelines.md`)



