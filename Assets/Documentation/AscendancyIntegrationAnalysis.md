# Ascendancy Integration Analysis

## Executive Summary

The proposed **Modifier Event System** from `AscendancyIntegrationIdeas.md` is a solid, data-driven approach that aligns well with your existing architecture. This document analyzes what you have, what you need, and how to bridge the gap.

---

## 🎯 Proposed System Overview

The document suggests **three layers**:

1. **Modifier Registry** - Data-driven definitions (JSON-like structure)
2. **Character-Attached Modifier Handler** - Event listener that applies effects
3. **Effect Resolvers** - Reusable functions that execute modifier actions

**Key Events Proposed:**
- `OnAttackUsed`
- `OnSpellCast`
- `OnDamageTaken`
- `OnKill`
- `OnTurnStart`
- `OnTurnEnd`
- `OnDeckShuffle`
- `OnDiscard`
- `OnManaSpent`
- `OnStatusApplied`

---

## ✅ What You Already Have

### 1. **Event System (Partial)**
- ✅ `OnCardPlayed` event exists in `CombatDeckManager.cs` and `CombatManager.cs`
- ✅ `OnTurnEnded` and `OnCombatEnded` events exist
- ❌ Missing: `OnAttack`, `OnSpellCast`, `OnDamageTaken`, `OnKill`, `OnTurnStart`, `OnDiscard`, `OnManaSpent`, `OnStatusApplied`

**Location:**
- `Assets/Scripts/CombatSystem/CombatDeckManager.cs` (line 70)
- `Assets/Scripts/Combat/CombatManager.cs` (line 27)

### 2. **Stack System (Excellent Foundation)**
- ✅ `StackSystem` singleton exists with `StackType` enum
- ✅ Supports: `Agitate`, `Tolerance`, `Potential`, `Momentum`
- ✅ Has `AddStacks()`, `RemoveStacks()`, `GetStacks()`, `ClearStacks()`
- ✅ Events: `OnStacksChanged` for UI updates
- ✅ Helper multipliers already implemented

**Location:**
- `Assets/Scripts/CombatSystem/Stacks/StackSystem.cs`
- `Assets/Scripts/CombatSystem/Stacks/StackType.cs`

**This is PERFECT for the proposed stack-based mechanics!**

### 3. **Status Effect System**
- ✅ `StatusEffectManager` with full lifecycle management
- ✅ Supports stacking rules (Bleed, Poison, Bolster)
- ✅ Events: `OnStatusEffectAdded`, `OnStatusEffectRemoved`, `OnStatusEffectTick`
- ✅ Can apply stat modifications via `ApplyRuntimeStatEffect()`

**Location:**
- `Assets/Scripts/CombatSystem/StatusEffectManager.cs`
- `Assets/Scripts/CombatSystem/StatusEffect.cs`

### 4. **Character Stats System**
- ✅ `CharacterStatsData` with comprehensive stat tracking
- ✅ `Character` class with core attributes (STR/DEX/INT)
- ✅ Damage modifiers system (`DamageModifiers` class)
- ✅ Stat calculation pipeline exists

**Location:**
- `Assets/Scripts/Data/Character.cs`
- `Assets/Scripts/Data/CharacterStatsData.cs`

### 5. **Ascendancy Data Structure**
- ✅ `AscendancyData` ScriptableObject
- ✅ `AscendancyPassive` class with name, description, prerequisites
- ✅ `CharacterAscendancyProgress` tracks unlocked passives
- ✅ Tree structure with branches, floating nodes, choice nodes

**Location:**
- `Assets/Scripts/Data/AscendancyData.cs`
- `Assets/Scripts/Data/CharacterAscendancyProgress.cs`

**However:** Passives currently only store **descriptions** - no actual effect definitions!

### 6. **Combat Systems**
- ✅ Card playing system (`CombatDeckManager`)
- ✅ Damage calculation (`DamageCalculator`, `DamageCalculation`)
- ✅ Card effect processing (`CardEffectProcessor`)

---

## ❌ What's Missing

### 1. **Comprehensive Event System**
You need to add the missing events:
- `OnAttackUsed` (when an attack card is played)
- `OnSpellCast` (when a spell card is played)
- `OnDamageTaken` (when character takes damage)
- `OnKill` (when enemy is killed)
- `OnTurnStart` (at start of player turn)
- `OnDiscard` (when cards are discarded)
- `OnManaSpent` (when mana is consumed)
- `OnStatusApplied` (when status effect is added)

**Where to add:**
- `CombatDeckManager.cs` - for card-related events
- `Character.cs` or `CombatManager.cs` - for damage/combat events
- `StatusEffectManager.cs` - for status events

### 2. **Modifier Registry System**
No system exists to:
- Define modifier effects in data (JSON/ScriptableObject)
- Register modifiers to characters
- Activate/deactivate modifiers based on unlocked Ascendancy nodes

**Proposed Structure:**
```csharp
// New ScriptableObject or JSON
[CreateAssetMenu]
public class AscendancyModifierDefinition : ScriptableObject
{
    public string modifierId;
    public string ascendancyNodeName; // Links to AscendancyPassive
    public List<ModifierEffect> effects;
}
```

### 3. **Modifier Handler Component**
No component exists that:
- Attaches to Character
- Listens to combat events
- Checks active modifiers
- Applies their effects

**Proposed Component:**
```csharp
public class AscendancyModifierHandler : MonoBehaviour
{
    private Character character;
    private List<ActiveModifier> activeModifiers = new List<ActiveModifier>();
    
    void OnEnable() { /* Subscribe to events */ }
    void OnDisable() { /* Unsubscribe */ }
    
    void OnAttackUsed(AttackData attack) { /* Process modifiers */ }
    // ... other event handlers
}
```

### 4. **Effect Resolver System**
No centralized system to execute modifier effects like:
- `add_brand_effect`
- `reduce_hit_percent`
- `add_stack`
- `consume_stack`
- `add_flat_damage`
- `apply_status`

**Proposed:**
```csharp
public static class ModifierEffectResolver
{
    public static void ResolveEffect(string effectName, Dictionary<string, object> params, Character character, object context);
}
```

### 5. **Integration Between Ascendancy and Modifiers**
Currently, `AscendancyPassive` only has a `description` field. You need to:
- Link passives to modifier definitions
- Activate modifiers when passives are unlocked
- Deactivate when passives are refunded

---

## 🔧 Integration Strategy

### Phase 1: Extend Event System

**Priority: HIGH** - Foundation for everything else

1. **Add missing events to `CombatDeckManager.cs`:**
```csharp
public System.Action<CardDataExtended> OnAttackUsed;
public System.Action<CardDataExtended> OnSpellCast;
public System.Action<int> OnManaSpent;
public System.Action<CardDataExtended> OnDiscard;
```

2. **Add events to `Character.cs` or create `CombatEventBus`:**
```csharp
public static class CombatEventBus
{
    public static System.Action<Character, float, DamageType> OnDamageTaken;
    public static System.Action<Character> OnTurnStart;
    public static System.Action<Character> OnTurnEnd;
    public static System.Action<Enemy> OnKill;
}
```

3. **Wire up events in existing code:**
   - `CombatDeckManager.PlayCard()` → Fire `OnAttackUsed` or `OnSpellCast`
   - `Character.TakeDamage()` → Fire `OnDamageTaken`
   - `CombatManager` turn logic → Fire `OnTurnStart`/`OnTurnEnd`

### Phase 2: Create Modifier Definition System

**Priority: HIGH** - Core data structure

1. **Create `AscendancyModifierDefinition.cs` ScriptableObject:**
   - Stores effect definitions
   - Links to `AscendancyPassive` by name
   - Can be created in Unity Inspector

2. **Extend `AscendancyPassive.cs`:**
   - Add optional `modifierDefinition` reference field
   - Or use string `modifierId` to look up in registry

3. **Create `ModifierEffect.cs` data structure:**
   - Event trigger (e.g., "OnAttack")
   - Conditions (optional)
   - Effect type and parameters

### Phase 3: Build Modifier Handler

**Priority: HIGH** - Runtime execution

1. **Create `AscendancyModifierHandler.cs` MonoBehaviour:**
   - Attach to player character GameObject
   - Subscribe to all combat events
   - Maintain list of active modifiers
   - Process events through active modifiers

2. **Create `ActiveModifier.cs` class:**
   - Wraps `AscendancyModifierDefinition`
   - Tracks state (stacks, cooldowns, etc.)
   - Executes effects via resolver

### Phase 4: Implement Effect Resolvers

**Priority: MEDIUM** - Reusable effect execution

1. **Create `ModifierEffectResolver.cs` static class:**
   - Dictionary of effect name → resolver function
   - Handles: `add_brand_effect`, `reduce_damage`, `add_stack`, etc.

2. **Leverage existing systems:**
   - Use `StackSystem` for stack effects
   - Use `StatusEffectManager` for status effects
   - Use `DamageModifiers` for damage modifications

### Phase 5: Integration Points

**Priority: MEDIUM** - Connect everything

1. **On Ascendancy Passive Unlock:**
   - Check if passive has modifier definition
   - Register modifier to `AscendancyModifierHandler`
   - Activate modifier

2. **On Ascendancy Passive Refund:**
   - Deactivate and unregister modifier

3. **Combat Start:**
   - Load all active modifiers from character's unlocked passives
   - Initialize modifier states (e.g., set initial stacks)

---

## 🎨 Recommended Architecture

### Option A: ScriptableObject-Based (Recommended)

**Pros:**
- Unity Inspector integration
- Easy to create/modify in Unity
- Type-safe
- Can reference other assets

**Structure:**
```
AscendancyModifierDefinition (SO)
  ├─ modifierId: "Mirrorsteel_Guard"
  ├─ linkedPassiveName: "Mirrorsteel Guard"
  ├─ effects: List<ModifierEffect>
  └─ variables: Dictionary<string, float>

ModifierEffect
  ├─ event: "OnDamageTaken"
  ├─ conditions: List<Condition>
  └─ actions: List<Action>
```

### Option B: JSON-Based (Alternative)

**Pros:**
- Easy to edit outside Unity
- Version control friendly
- Can be loaded at runtime

**Cons:**
- Less type safety
- Requires JSON parser
- No Inspector integration

---

## 🔗 Integration with Existing Systems

### StackSystem Integration

**Perfect fit!** The proposed system already uses stacks. Your `StackSystem` can handle:
- `Mirrorsteel` stacks (from example)
- `Wardweave` stacks
- `Corruption` stacks
- Any custom stack type

**Just extend `StackType` enum:**
```csharp
public enum StackType
{
    Agitate,
    Tolerance,
    Potential,
    Momentum,
    Mirrorsteel,  // New
    Wardweave,    // New
    Corruption    // New
}
```

### StatusEffectManager Integration

Modifiers can apply status effects via existing system:
```csharp
// In ModifierEffectResolver
void ApplyStatus(string statusName, float magnitude, int duration)
{
    StatusEffectType type = ParseStatusType(statusName);
    StatusEffect effect = new StatusEffect(type, statusName, magnitude, duration);
    StatusEffectManager.Instance.AddStatusEffect(effect);
}
```

### Damage Calculation Integration

Modifiers can modify damage via `DamageModifiers`:
```csharp
// In ModifierEffectResolver
void AddBrandEffect(Character character, AttackData attack, string elementType)
{
    DamageModifiers mods = character.GetDamageModifiers();
    float bonus = character.intelligence * 0.6f;
    mods.AddAddedDamage(ParseDamageType(elementType), bonus);
}
```

---

## 📋 Implementation Checklist

### Foundation (Week 1)
- [ ] Create `CombatEventBus` or extend existing event system
- [ ] Add missing events: `OnAttackUsed`, `OnSpellCast`, `OnDamageTaken`, etc.
- [ ] Wire events into existing combat code
- [ ] Test event firing

### Data Layer (Week 1-2)
- [ ] Create `AscendancyModifierDefinition` ScriptableObject
- [ ] Create `ModifierEffect` data structure
- [ ] Create `ModifierCondition` and `ModifierAction` classes
- [ ] Extend `AscendancyPassive` to reference modifiers
- [ ] Create example modifier definitions in Unity

### Runtime Layer (Week 2-3)
- [ ] Create `AscendancyModifierHandler` component
- [ ] Create `ActiveModifier` wrapper class
- [ ] Implement event subscription/unsubscription
- [ ] Implement modifier activation/deactivation

### Effect Resolvers (Week 3-4)
- [ ] Create `ModifierEffectResolver` static class
- [ ] Implement common effects:
  - [ ] `add_brand_effect`
  - [ ] `reduce_damage_percent`
  - [ ] `add_stack` / `consume_stack`
  - [ ] `apply_status`
  - [ ] `add_flat_damage`
  - [ ] `modify_stat`
- [ ] Test each resolver

### Integration (Week 4)
- [ ] Hook into `CharacterAscendancyProgress.UnlockPassive()`
- [ ] Auto-register modifiers on unlock
- [ ] Auto-unregister on refund
- [ ] Initialize modifiers on combat start
- [ ] Test full flow: Unlock → Activate → Trigger → Apply

### Testing & Polish (Week 5)
- [ ] Create test Ascendancy with modifiers
- [ ] Test all event types
- [ ] Test stack interactions
- [ ] Test status effect interactions
- [ ] Performance testing
- [ ] Debug console for active modifiers (as suggested)

---

## 🚨 Potential Challenges

### 1. **Event Ordering**
Some modifiers might need to fire before/after others. Consider:
- Priority system for modifiers
- Event phases (Pre/Post)

### 2. **Performance**
Many modifiers listening to events could impact performance. Consider:
- Modifier filtering (only active modifiers process events)
- Event pooling
- Batch processing

### 3. **State Management**
Modifiers might need to track state between events. Consider:
- `ActiveModifier` state dictionary
- Persistent stacks (already handled by `StackSystem`)

### 4. **Conditional Logic**
Complex conditions (e.g., "if previous card was Spell") need context. Consider:
- Event context objects (e.g., `AttackData`, `SpellData`)
- History tracking (e.g., last played card type)

---

## 💡 Recommendations

1. **Start Small:** Implement one event type (`OnAttackUsed`) and one modifier first
2. **Leverage Existing:** Use `StackSystem`, `StatusEffectManager` extensively
3. **Data-Driven:** Make everything configurable via ScriptableObjects
4. **Debug Tools:** Build a debug panel to inspect active modifiers (as suggested)
5. **Documentation:** Document each effect resolver with examples

---

## 📚 Example Implementation Flow

### 1. Define Modifier (Unity Inspector)
```
AscendancyModifierDefinition: "Mirrorsteel_Guard"
  ├─ Linked Passive: "Mirrorsteel Guard"
  ├─ Effect 1:
  │   ├─ Event: "OnCombatStart"
  │   └─ Action: "set_stack" { id: "Mirrorsteel", amount: 3 }
  ├─ Effect 2:
  │   ├─ Event: "OnDamageTaken"
  │   ├─ Condition: "has_stack" { id: "Mirrorsteel" }
  │   └─ Action: "reduce_hit_percent" { value: 40 }
  │   └─ Action: "consume_stack" { id: "Mirrorsteel", amount: 1 }
```

### 2. Unlock Passive (Game Code)
```csharp
character.ascendancyProgress.UnlockPassive(passive, ascendancyData);
// → Automatically registers modifier if passive has one
```

### 3. Combat Starts
```csharp
// AscendancyModifierHandler loads all active modifiers
// Fires OnCombatStart event
// Modifier "Mirrorsteel_Guard" Effect 1 triggers
// → Sets Mirrorsteel stacks to 3
```

### 4. Player Takes Damage
```csharp
character.TakeDamage(100);
// → Fires OnDamageTaken event
// Modifier "Mirrorsteel_Guard" Effect 2 checks condition
// → Has Mirrorsteel stacks? Yes (3)
// → Reduces damage by 40% (100 → 60)
// → Consumes 1 stack (3 → 2)
```

---

## ✅ Conclusion

The proposed system is **excellent** and fits your architecture well. You already have:
- ✅ Stack system (perfect for the example)
- ✅ Status effect system
- ✅ Event system (partial, needs extension)
- ✅ Character stats system
- ✅ Ascendancy data structure

**Main gaps:**
- Missing events (easy to add)
- No modifier registry/handler (needs to be built)
- No effect resolvers (needs to be built)
- No integration between Ascendancy unlock and modifier activation (needs to be built)

**Estimated effort:** 3-5 weeks for full implementation, depending on complexity of effect resolvers.

**Recommendation:** Start with Phase 1 (events) and Phase 2 (data structures), then build one complete modifier end-to-end to validate the approach before scaling up.

