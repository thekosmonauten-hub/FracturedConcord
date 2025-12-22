# Ascendancy Integration Analysis

## Integration Options: Pros & Cons

### Option 1: Full Integration with Existing Systems

**Use EncounterManager + CombatSceneManager + EnemyDatabase**

#### Pros:
- ✅ Minimal new code - reuse existing infrastructure
- ✅ Consistent with current architecture
- ✅ EnemyDatabase already has tier-based pools (`GetRandomEncounter(tier)`)
- ✅ EncounterManager handles scene transitions

#### Cons:
- ❌ EncounterManager is designed for linear progression (prerequisites/unlocks), not run-based
- ❌ No support for persistent run state across floors
- ❌ No minimap/floor generation logic
- ❌ No node system (combat/shrine/chest/etc.)
- ❌ Would require significant modifications to EncounterManager
- ❌ Risk of breaking existing encounter system

**Verdict:** ❌ Not recommended - too much mismatch with maze requirements

---

### Option 2: Separate MazeRunManager + Reuse Supporting Systems

**New MazeRunManager singleton + Reuse EnemyDatabase + Adapt CombatSceneManager pattern**

#### Pros:
- ✅ Clean separation - maze logic isolated from regular encounters
- ✅ Reuse EnemyDatabase for enemy pools (already tiered, perfect for floors)
- ✅ Reuse CombatSceneManager pattern but with maze context
- ✅ No risk to existing systems
- ✅ Easy to expand (bosses, modifiers, special nodes)
- ✅ Run state persists independently
- ✅ Follows existing singleton pattern (EncounterManager, EnemyDatabase)

#### Cons:
- ⚠️ More initial setup (but cleaner long-term)
- ⚠️ Need to coordinate between MazeRunManager and CombatSceneManager

**Verdict:** ✅ **RECOMMENDED** - Best balance of reuse and separation

---

### Option 3: Fully Independent System

**Complete new system, minimal reuse**

#### Pros:
- ✅ Complete isolation
- ✅ No dependencies

#### Cons:
- ❌ Code duplication (enemy pools, combat logic)
- ❌ Maintenance burden (two systems doing similar things)
- ❌ Inconsistent architecture

**Verdict:** ❌ Not recommended - too much duplication

---

## Recommended Architecture (Option 2)

```
MazeRunManager (Singleton)
├── Manages run state (floors, current floor, seed, run modifiers)
├── Generates floors (minimap, nodes, connectivity)
├── Handles node interactions (combat/shrine/chest/stairs)
├── Tracks boss progression
└── Coordinates with:
    ├── EnemyDatabase (for enemy pools per floor)
    ├── CombatSceneManager (load combat, return to maze)
    └── MazeScene (UI for minimap, node selection)
```

### Integration Points:

1. **EnemyDatabase** → Used for floor-based enemy pools
   - `GetRandomEncounter(count, tier)` - perfect for combat nodes
   - Tier scaling: Floor 1 = Normal, Floor 2 = Elite, etc.

2. **CombatSceneManager** → Adapted for maze context
   - MazeRunManager sets temporary "current encounter" for combat scene
   - After combat: return to MazeScene (not MainGameUI)
   - Pass maze context (floor, node type, run modifiers)

3. **MazeScene** → New scene for minimap UI
   - Displays floor minimap
   - Handles node selection
   - Shows run state (floor, modifiers, boss progress)

---

## Data Architecture: ScriptableObjects

### Proposed Structure:

```
MazeConfig.asset (ScriptableObject)
├── Floor count (default 8)
├── Node type weights (combat/shrine/chest/trap/forge/etc.)
├── Floor enemy tiers (which tier per floor)
└── Boss spawn floors (every 2 floors, etc.)

MazeNodeType.asset (ScriptableObject)
├── Node type enum
├── Icon/sprite
└── Behavior class reference

MazeBossData.asset (ScriptableObject)
├── Boss enemy data
├── Modifier pools (random each encounter)
└── Retreat threshold (45% HP)
```

---

## Phase 1 Prototype Plan

1. **Create MazeRunManager** (singleton)
   - Run state structure
   - Seed generation
   - Floor generation (12-16 nodes, connectivity)

2. **Create Floor Generation**
   - Grid-based node placement
   - Connectivity validation (BFS)
   - Node type assignment (weighted)

3. **Create MazeScene** (new scene)
   - Minimap UI (grid of nodes)
   - Node click handling
   - Basic visual feedback

4. **Integrate Combat**
   - Node click → CombatScene (with maze context)
   - Combat victory → Return to MazeScene
   - Mark node as visited

5. **Add Entry Point**
   - EncounterButton/Node can trigger MazeRunManager.StartRun()
   - Load MazeScene

---

## Future Expansion Points

- ✅ Shrines (run modifiers)
- ✅ Chests (loot)
- ✅ Boss encounters
- ✅ Special nodes (Forge, Market, etc.)
- ✅ Run modifiers affecting combat
- ✅ Save/load run state
- ✅ Multiple floors
- ✅ Final boss

All designed to plug in without major refactoring.

