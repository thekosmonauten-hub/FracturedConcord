# Dexiled - Game Summary

## 🎮 Game Overview

**Dexiled** is a **deckbuilder ARPG** (Action Role-Playing Game) built in Unity, heavily inspired by Path of Exile's character progression and itemization systems. The game combines card-based combat mechanics with deep character customization, passive skill trees, and extensive item/equipment systems.

### Core Gameplay Loop
Players build decks of cards, engage in turn-based combat encounters, defeat enemies to gain experience and loot, and progressively customize their character through multiple progression systems including unique passive tree system called "Warrants" (gem-like socketable items), equipment,  and character classes with ascendancy specializations.

---

## ✅ Currently Implemented Systems

### 1. **Combat System** (Core - Fully Functional)
- **Turn-based card combat** with player and enemy turns
- **Card system** with multiple card types (Attack, Guard, Skill, Power, Spell)
- **Damage calculation system** with stat scaling (Strength, Dexterity, Intelligence)
- **Weapon integration** - cards scale with equipped weapons (melee, projectile, spell weapons)
- **AoE (Area of Effect) cards** that hit multiple enemies
- **Enemy system** with health, damage, intents, and status effects
- **Combat animations** - cards fly to enemies, damage numbers display
- **Guard system** - defensive resource that persists between turns with decay
- **Mana system** - resource management for card costs
- **Status effects** - ailments (Bleed, Poison, Ignite, Chill, Shock, Freeze, Slow) applied to enemies
- **Enemy energy system** - resource bar for bosses/elites that drains from Chill/Slow
- **Wave-based encounters** - multiple waves of enemies per encounter
- **Movement speed compression** - faster movement reduces wave count in encounters

### 2. **Character System** (Core - Fully Functional)
- **6 Character Classes**:
  - **Primary Classes**: Marauder (STR), Ranger (DEX), Witch (INT)
  - **Hybrid Classes**: Brawler (STR/DEX), Thief (DEX/INT), Apostle (STR/INT)
- **Path of Exile-style attributes**: Strength, Dexterity, Intelligence
- **Attribute scaling**:
  - STR → Physical damage, Life, Life regeneration
  - DEX → Accuracy, Evasion, Critical chance
  - INT → Energy Shield, Spell power, Critical multiplier
- **Level progression** with class-specific attribute gains per level
- **Resource systems**: Health, Mana, Guard, Energy Shield, Reliance
- **Character save system** - JSON-based persistence

### 3. **Card System** (Core - Fully Functional)
- **Card data structure** with comprehensive properties:
  - Base damage/guard values
  - Mana costs
  - Damage scaling (STR/DEX/INT multipliers)
  - Weapon scaling (melee/projectile/spell)
  - AoE properties
  - Tags (Spell, Dual, Unleash, etc.)
  - Special effects (ifDiscarded, dualWieldEffect)
- **Deck management** - character-specific deck data
- **Card collection** system
- **Card runtime system** - visual card objects with pooling
- **Card effects processor** - applies damage, status effects, and special mechanics
- **Starter decks** for each class (Ranger deck fully implemented with combos)

### 4. **Speed Meter System** (Implemented)
- **Aggression meter** - builds from attack cards, provides attack speed bonuses
- **Focus meter** - builds from spell cards, provides spell power bonuses
- **Charge-based system** - meters fill and convert to charges
- **Visual rings** around player display showing meter progress
- **Consumption rules** - charges consumed for bonuses (mana discounts, combo requirements)

### 5. **Channeling System** (Implemented)
- **Channeling tracking** - tracks consecutive casts of same spell
- **Streak system** - maintains channeling state across turns
- **Break detection** - detects when channeling stops
- **Metadata on cards** - runtime cards include channeling flags

### 6. **Stack Systems** (Implemented)
- **Agitate/Tolerance/Potential** - Ranger-specific stack mechanics
- **StackSystem singleton** - reusable infrastructure for stack-based mechanics
- **Temporary stat system** - duration-based stat modifications
- **Status effect integration** - stacks interact with status effects

### 7. **Status Effects & Ailments** (Implemented)
- **Ailment types**: Bleed, Poison, Ignite, Chill, Shock, Freeze, Slow
- **Status effect manager** - applies and tracks effects on enemies
- **Duration and magnitude scaling** - scales with character stats
- **Visual display** - status effect icons on enemy displays
- **Energy drain** - Chill/Slow drain enemy energy

### 8. **Warrant System** (Partially Implemented)
- **Warrant boards** - grid-based socket system (similar to Path of Exile's gem system)
- **Socket/Effect node architecture** - warrants socketed into nodes, affect nearby effect nodes
- **Multi-page boards** - multiple warrant board loadouts per character
- **Drag-and-drop interface** - warrants can be dragged from locker to sockets
- **Warrant locker** - inventory system for warrants
- **Tooltip system** - shows warrant modifiers on hover
- **Warrant databases**:
  - `WarrantAffixDatabase` - stat modifiers for warrants
  - `WarrantNotableDatabase` - special notable effects
- **Effect range system** - warrants affect nodes within range (3-node influence rule)
- **Icon library** - visual representation of warrants

### 10. **Equipment System** (Partially Implemented)
- **10 Equipment slots**: Helmet, Body Armour, Belt, Gloves, Boots, Ring1, Ring2, Amulet, Weapon, Offhand
- **Item types**: Weapons, Armour, Jewellery, Off-Hand Equipment
- **Item rarity system**: Normal, Magic, Rare, Unique
- **Affix system**: Implicit modifiers, Prefixes, Suffixes
- **Quality system** (0-20) - affects all stats
- **Equipment manager** - calculates and applies stats from equipped items
- **Stat calculation** - equipment stats integrated with character stats
- **Requirement checking** - level and attribute requirements

### 11. **Item System** (Partially Implemented)
- **ScriptableObject-based items** - BaseItem, WeaponItem, Armour, Jewellery
- **Item database** - centralized item collection
- **Weapon types**: Melee, Projectile, Spell weapons
- **Damage ranges** - min/max damage for weapons
- **Item tags** - categorization system

### 12. **UI Systems** (Partially Implemented)
- **Main menu** - UI Toolkit-based with character selection
- **Combat UI** - card hand, health/mana bars, enemy displays
- **Equipment screen** - Unity UI-based equipment management
- **Character stats panel** - displays character statistics
- **Tooltip system** - item and warrant tooltips
- **Encounter selection** - world map with encounter buttons
- **Scene navigation** - scene transitions between main menu, world map, and combat

### 13. **Encounter System** (Implemented)
- **Encounter data** - encounter definitions with scene references
- **Encounter manager** - singleton managing encounter state
- **Encounter buttons** - UI for selecting encounters
- **Wave preview** - shows compressed wave count based on movement speed
- **Completion tracking** - unlocks subsequent encounters

### 14. **Damage & Stats Calculation** (Implemented)
- **StatAggregator** - centralized stat calculation system
- **DamageCalculator** - comprehensive damage calculation with context
- **DamageContext** - contextual information for damage calculations (card tags, target state, etc.)
- **Increased vs More** - proper additive vs multiplicative modifiers
- **Elemental damage** - Fire, Cold, Lightning, Chaos damage types
- **Resistance system** - elemental and physical resistances

### 15. **Card Ability Routing** (Implemented)
- **CardAbilityRouter** - centralized card-specific logic
- **Class-specific combos** - Ranger combos (Focus, Dodge, Pack Hunter, etc.)
- **Combo system** - card combinations with special effects

### 16. **Preparation System** (Implemented)
- **Prepared cards** - cards that accumulate bonuses over turns
- **Unleash mechanic** - unleash all prepared cards at once
- **Turn-based accumulation** - prepared cards gain power each turn

---

## 🚧 Systems in Progress / Planned

### 1. **Ascendancy System** (Planned)
- **18 Ascendancy classes** - 3 per base class
- **Ascendancy trees** - specialized passive trees for each ascendancy
- **Splash art** - visual representation for each ascendancy
- **Signature cards** - unique cards for each ascendancy
- **Core mechanics** - unique gameplay mechanics per ascendancy
- **Unlock requirements** - level/quest-based unlocks

### 2. **Embossing System** (Partially Implemented)
- **Embossing effects** - card enhancements (similar to Path of Exile's support gems)
- **7 Phases planned**:
  - Phase 1: Damage multipliers ✅
  - Phase 2: Status Effects (Planned)
  - Phase 3: Stat Scaling (Planned)
  - Phase 4: Conversions (Planned)
  - Phase 5: Utility (Planned)
  - Phase 6: Defensive (Planned)
  - Phase 7: Custom (Planned)
- **UI implemented** - embossing icons on card slots
- **Integration pending** - effects need to be integrated into combat

### 3. **Town Scene** (Planned)
- **NPCs**: Seer, Questgiver, Forge, Peacekeepers Faction
- **Seer functions**: Card generation, Card vendor
- **Questgiver**: Quest system
- **Forge**: Item salvage
- **Peacekeepers Faction**: Warrant management

### 4. **Warrant System Completion** (In Progress)
- **Notable conditional triggers** - special effects that trigger on events:
  - Focus charge triggers
  - Turn-based spell cost reduction
  - Card draw triggers
  - Card play event triggers
  - Ailment duration extension
  - Echo/repeat system
  - Guard triggers
  - Combat start triggers
  - Life cost triggers
  - Kill triggers
  - Projectile triggers
  - Health threshold triggers
  - Card play counter triggers
  - Skill card triggers
  - Discard cost reduction
  - Consecutive attack triggers
  - Damage conversion triggers
  - Spell casting triggers
  - Area damage triggers
- **Warrant upgrading** - kill requirements to upgrade warrants (Common → Magic)
- **Master Blueprint system** - copyable warrants with rolled affixes
- **Peacekeeper fusion** - 3→1 fusion UI with modifier locking

### 5. **Missing Stat Keys** (Planned - High Priority)
Many stat modifiers from Notable effects need implementation:
- **Critical strike stats** (generic + conditional)
- **Life/Mana regeneration increased**
- **Guard system stats** (retention, effectiveness, damage reduction)
- **Block system stats** (chance, effectiveness)
- **Resistance & penetration**
- **Card system stats** (draw chance, hand size, mana efficiency, etc.)
- **Life/Mana steal**
- **Conditional damage modifiers** (vs cursed, vs blocked, while in shadow, etc.)
- **Ailment & status effect stats** (duration, application chance, effectiveness)
- **Damage reduction & mitigation**
- **Stun & crowd control**
- **Movement & mobility**

### 6. **Deck Building Features** (Planned)
- **Deck templates**
- **Deck statistics panel**
- **Deck code sharing** (export/import)
- **Drag-and-drop card management**

### 7. **Card Leveling System** (Planned)
- Card progression and leveling mechanics

### 8. **Curses System** (Planned)
- Curse mechanics for enemies

---

## 🏗️ Technical Architecture

### **Design Patterns**
- **Singleton pattern** - used extensively for managers (CombatManager, EncounterManager, etc.)
- **Event-driven architecture** - loose coupling via events
- **ScriptableObject-based data** - items, cards, enemies defined as SOs
- **Centralized managers** - Option 1 integration approach (CombatSceneManager pattern)

### **UI Framework**
- **UI Toolkit** - modern UI system for main menu and combat UI
- **Unity UI (Legacy)** - used for equipment screen and some panels
- **USS styling** - Unity Style Sheets (not CSS) for UI Toolkit

### **Data Persistence**
- **JSON-based saves** - character data, equipment, passive trees
- **PlayerPrefs** - for some UI state
- **ScriptableObjects** - for game data (items, cards, enemies)

### **Combat Architecture**
- **Turn-based system** - player turn → enemy turn cycle
- **Card effect processing** - centralized card effect application
- **Damage calculation** - stat-based with context awareness
- **Status effect system** - duration and magnitude scaling

### **Progression Systems**
- **Multiple progression paths**: Passive tree, Equipment, Warrants, Character level
- **Stat aggregation** - centralized stat calculation from all sources
- **Real-time stat updates** - stats update when equipment/passives change

---

## 🎯 Game Design Philosophy

### **Path of Exile Influences**
- **Attribute system** - STR/DEX/INT with PoE-style scaling
- **Passive tree** - 
- **Warrants** - gem-like socketable item passive tree system (inspired by PoE's passive tree + Sockets)
- **Item rarity** - Normal/Magic/Rare/Unique
- **Ascendancy classes** - specialized class variants

### **Deckbuilder Mechanics**
- **Card-based combat** - turn-based card playing
- **Deck building** - collect and customize decks
- **Card synergies** - combo system for card interactions
- **Resource management** - mana and card draw management

### **ARPG Elements**
- **Character progression** - leveling, attributes, skills
- **Equipment system** - 10-slot equipment with stat bonuses
- **Enemy encounters** - wave-based combat encounters
- **Loot system** - items with random affixes

---

## 📊 Current Development Status

### **Core Systems**: ✅ ~80% Complete
- Combat system fully functional
- Character system complete
- Card system operational
- Damage calculation working
- Status effects implemented

### **Progression Systems**: 🚧 ~50% Complete
- Passive tree partially implemented (modular architecture in place)
- Warrant system partially implemented (UI complete, effects pending)
- Equipment system partially implemented (slots and stats working)
- Ascendancy system planned but not started

### **Content Systems**: 🚧 ~30% Complete
- Starter decks for classes (Ranger complete, others partial)
- Enemy system functional but needs more variety
- Item database structure in place, needs content
- Encounter system functional, needs more encounters

### **UI/UX**: 🚧 ~60% Complete
- Main menu and character selection complete
- Combat UI functional
- Equipment screen implemented
- Passive tree UI partially implemented
- Warrant board UI complete
- Town scene UI not started

---

## 🔮 Future Vision

The game aims to be a comprehensive deckbuilder ARPG with:
- **Deep character customization** through multiple progression systems
- **Extensive card collection** with hundreds of cards
- **Complex build possibilities** via passive trees, warrants, and equipment
- **Replayability** through multiple classes, ascendancies, and build paths
- **Strategic combat** with card synergies and combo systems
- **Progressive difficulty** through acts and encounter scaling

---

## 📝 Key Documentation Files

- `MasterChecklist.md` - High-level task tracking
- `DevelopmentLog.md` - Detailed development history
- `StatsAndDamageGuidelines.md` - Stat calculation guidelines
- `MissingStatKeys_Analysis.md` - Planned stat implementations
- Various system-specific guides in `Assets/Documentation/` and `Assets/Scripts/Documentation/`

---

*Last Updated: Based on codebase analysis as of current date*
*Game Status: Active Development - Core systems functional, content and polish in progress*










