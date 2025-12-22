# Embossing Effects - Full Implementation Plan

## 📋 Overview

This document outlines the complete implementation plan for making embossing effects functional in combat. Currently, embossings can be applied to cards via the UI, but they don't affect gameplay yet.

**Current State:**
- ✅ Embossing UI system (tooltips, confirmation, application)
- ✅ Embossing data stored on cards (`card.appliedEmbossings`)
- ✅ EmbossingDatabase loaded and accessible
- ❌ Effects don't modify combat behavior yet

**Goal:**
- ✅ Embossings modify damage, status effects, and card behavior in combat
- ✅ All 30+ effect types functional
- ✅ Proper integration with existing systems

---

## 🎯 Implementation Phases

### Phase 1: Core Processor & Damage Multipliers (START HERE)
**Estimated Time:** 1-2 hours  
**Complexity:** Medium  
**Dependencies:** None

**Components:**
1. Create `EmbossingEffectProcessor.cs` - Core effect processing system
2. Update `DamageCalculator.CalculateCardDamage()` - Add embossing multipliers
3. Implement damage multiplier effects:
   - DamageMultiplier (generic)
   - PhysicalDamageMultiplier
   - ElementalDamageMultiplier
   - SpellDamageMultiplier
   - FlatDamageBonus

**Testing:**
- Apply embossing to card
- Play card in combat
- Verify damage increase

---

### Phase 2: Status Effect Application
**Estimated Time:** 2-3 hours  
**Complexity:** Medium  
**Dependencies:** Phase 1, StatusEffectManager

**Components:**
1. Create `EmbossingStatusEffectHandler.cs`
2. Hook into combat damage application
3. Implement status effect embossings:
   - ApplyBleed
   - ApplyIgnite
   - ApplyPoison
   - ApplyShock
   - ApplyFreeze
   - ApplyChill

**Implementation:**
- Roll chance on hit
- Apply status effect to enemy
- Use existing StatusEffectManager

**Testing:**
- Apply "of Cruelty" (15% bleed)
- Play card multiple times
- Verify bleed applies ~15% of the time

---

### Phase 3: Stat Scaling Effects
**Estimated Time:** 1-2 hours  
**Complexity:** Low  
**Dependencies:** Phase 1

**Components:**
1. Update `EmbossingEffectProcessor`
2. Implement scaling embossings:
   - StrengthScaling (+X% per STR)
   - DexterityScaling (+X% per DEX)
   - IntelligenceScaling (+X% per INT)
   - EmbossingCountScaling (+X% per embossing on card)

**Implementation:**
- Read character stats
- Calculate bonus damage
- Add to damage calculation

**Testing:**
- Apply "of Power" (STR scaling)
- Verify damage increases with character STR

---

### Phase 4: Conversion Effects
**Estimated Time:** 2-3 hours  
**Complexity:** High  
**Dependencies:** Phase 1, DamageType system

**Components:**
1. Create `DamageConversionHandler.cs`
2. Update damage calculation pipeline
3. Implement conversion embossings:
   - PhysicalToFireConversion
   - PhysicalToColdConversion
   - PhysicalToLightningConversion
   - ElementalToChaosConversion

**Implementation:**
- Split damage by type
- Convert X% of one type to another
- Update final damage calculation

**Testing:**
- Apply "of Flames" (50% phys → fire)
- Verify damage splits correctly

---

### Phase 5: Utility Effects
**Estimated Time:** 3-4 hours  
**Complexity:** High  
**Dependencies:** All previous phases

**Components:**
1. Update `EmbossingEffectProcessor`
2. Hook into various game systems
3. Implement utility embossings:
   - ManaCostReduction (already partially done)
   - CriticalChance
   - CriticalMultiplier
   - LifeOnHit
   - LifeLeech
   - CardDuplication
   - PrepareCharges
   - Persistence

**Implementation:**
- Modify mana cost calculation
- Modify crit chance/multiplier
- Add life gain on damage
- Duplicate cards in hand
- Flag cards as persistent

**Testing:**
- Test each effect individually
- Verify interactions work correctly

---

### Phase 6: Defensive Effects
**Estimated Time:** 2-3 hours  
**Complexity:** Medium  
**Dependencies:** Phase 1, Guard system

**Components:**
1. Update guard calculation
2. Implement defensive embossings:
   - GuardOnPlay
   - DamageReflection
   - GuardEffectiveness

**Implementation:**
- Add guard when card played
- Reflect damage back to attacker
- Multiply guard values

**Testing:**
- Apply guard embossings
- Verify guard increases
- Test reflection mechanics

---

### Phase 7: Special/Custom Effects
**Estimated Time:** 4-6 hours  
**Complexity:** Very High  
**Dependencies:** All previous phases

**Components:**
1. Create custom effect handlers
2. Implement special embossings:
   - ConditionalDamage
   - ComboScaling
   - CustomEffect (AoE radius, Bolster, etc.)

**Implementation:**
- Check conditions (enemy HP, player HP, etc.)
- Scale with combo counters
- Handle unique custom effects per embossing

**Testing:**
- Test each custom effect
- Verify condition checking
- Validate edge cases

---

## 📁 Files to Create

### New Files:

1. **`EmbossingEffectProcessor.cs`**
   - Location: `Assets/Scripts/CombatSystem/Embossing/`
   - Purpose: Core embossing effect processing
   - Size: ~500-800 lines

2. **`EmbossingStatusEffectHandler.cs`**
   - Location: `Assets/Scripts/CombatSystem/Embossing/`
   - Purpose: Handle status effect application
   - Size: ~200-300 lines

3. **`DamageConversionHandler.cs`**
   - Location: `Assets/Scripts/CombatSystem/Embossing/`
   - Purpose: Element conversion logic
   - Size: ~150-250 lines

4. **`EmbossingUtilityHandler.cs`**
   - Location: `Assets/Scripts/CombatSystem/Embossing/`
   - Purpose: Utility effects (life on hit, crit, etc.)
   - Size: ~300-400 lines

5. **`EmbossingCustomEffectHandler.cs`**
   - Location: `Assets/Scripts/CombatSystem/Embossing/`
   - Purpose: Custom/special effects
   - Size: ~200-400 lines

---

## 📝 Files to Modify

### Major Updates:

1. **`DamageCalculator.cs` or `DamageCalculation.cs`**
   - Add: `ProcessEmbossingEffects(Card card, Character character)`
   - Add: Damage multiplier application
   - Add: Element conversion

2. **`CardEffectProcessor.cs`**
   - Add: Call to EmbossingEffectProcessor
   - Add: Status effect application after damage
   - Add: Guard/utility effect hooks

3. **`Card.cs`**
   - Add: `GetEmbossingMultiplier()` helper
   - Add: `GetEmbossingCritBonus()` helper
   - Add: `HasEmbossingEffect(EmbossingEffectType type)` checker

4. **`Character.cs` (if needed)**
   - May need embossing-based stat modifications
   - Life gain hooks

---

## 🔧 Technical Architecture

### Effect Processing Flow:

```
Card Played
    ↓
DamageCalculator.CalculateCardDamage()
    ↓
EmbossingEffectProcessor.ProcessEmbossings(card)
    ↓
┌─────────────────────────────────────────┐
│ 1. Get all appliedEmbossings from card  │
│ 2. Load EmbossingEffect data from DB    │
│ 3. Process each effect type:            │
│    - Damage multipliers → damage calc   │
│    - Conversions → damage type split    │
│    - Scaling → stat-based bonuses       │
│    - Critical → crit chance/mult        │
│ 4. Return modified damage/effects       │
└─────────────────────────────────────────┘
    ↓
Apply Damage to Enemy
    ↓
EmbossingStatusEffectHandler.RollStatusEffects(card, target)
    ↓
Apply Status Effects (if roll succeeds)
    ↓
EmbossingUtilityHandler.OnHit(card, damage, player)
    ↓
Apply Life Gain / Other On-Hit Effects
```

---

## 📊 Effect Processing Methods

### EmbossingEffectProcessor Core Methods:

```csharp
// Main entry point
public static EmbossingProcessedData ProcessCardEmbossings(Card card, Character character)

// Damage processing
public static float ApplyDamageMultipliers(Card card, float baseDamage)
public static float ApplyScalingBonuses(Card card, Character character, float baseDamage)
public static Dictionary<DamageType, float> ApplyConversions(Card card, float damage, DamageType originalType)

// Utility processing
public static float ApplyCriticalModifiers(Card card, float baseCritChance, float baseCritMult)
public static void ApplyOnPlayEffects(Card card, Character player)
public static void ApplyOnHitEffects(Card card, Character player, Enemy target, float damageDealt)

// Status effects
public static void RollAndApplyStatusEffects(Card card, Enemy target)

// Validation
public static bool HasEmbossingOfType(Card card, EmbossingEffectType type)
public static List<EmbossingEffect> GetEmbossingsByType(Card card, EmbossingEffectType type)
```

---

## 🧪 Testing Strategy

### Unit Testing Checklist:

**Phase 1 - Damage Multipliers:**
- [ ] Card with no embossings = base damage
- [ ] Card with +25% damage = base × 1.25
- [ ] Card with multiple embossings = stack correctly
- [ ] Physical/Elemental/Spell multipliers apply to correct damage types

**Phase 2 - Status Effects:**
- [ ] 15% bleed applies ~15% of the time (test 100 hits)
- [ ] Multiple status embossings stack chances
- [ ] Status effects appear on enemy

**Phase 3 - Scaling:**
- [ ] STR scaling increases with character STR
- [ ] Multiple scaling types stack
- [ ] Embossing count scaling works

**Phase 4 - Conversions:**
- [ ] 50% conversion splits damage correctly
- [ ] Multiple conversions stack properly
- [ ] Can't convert > 100%

**Phase 5-7:**
- [ ] All utility effects tested individually
- [ ] Custom effects work as intended
- [ ] No conflicts between effect types

---

## ⚠️ Potential Challenges

### Challenge 1: Effect Stacking
**Problem:** How do multiple embossings interact?
**Solution:** 
- Damage multipliers: Additive (25% + 30% = 55%)
- Status chances: Separate rolls
- Conversions: Apply in sequence

### Challenge 2: Mana Cost
**Problem:** Already implemented in UI, need combat integration
**Solution:** Use `Card.GetCurrentManaCost()` method (already exists)

### Challenge 3: Element Conversion
**Problem:** Current system uses single `primaryDamageType`
**Solution:** May need to track damage as Dictionary<DamageType, float>

### Challenge 4: Custom Effects
**Problem:** Each custom effect needs unique logic
**Solution:** Use customEffectId switch statement

---

## 📈 Priority Order (Recommended)

### High Priority (Combat Impact):
1. ✅ Damage multipliers (Phase 1) - **START HERE**
2. ✅ Status effects (Phase 2)
3. ✅ Stat scaling (Phase 3)

### Medium Priority (Quality of Life):
4. Utility effects (Phase 5)
5. Defensive effects (Phase 6)

### Low Priority (Advanced):
6. Conversions (Phase 4)
7. Custom effects (Phase 7)

---

## 🚀 Phase 1 Implementation (NEXT)

### Step 1: Create EmbossingEffectProcessor.cs

**Location:** `Assets/Scripts/CombatSystem/Embossing/EmbossingEffectProcessor.cs`

**Core Methods:**
```csharp
public static float ApplyDamageMultipliers(Card card, float baseDamage, DamageType damageType)
{
    if (card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
        return baseDamage;
    
    float totalMultiplier = 1.0f;
    
    foreach (var instance in card.appliedEmbossings)
    {
        EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
        if (effect == null) continue;
        
        // Apply level bonus
        float levelBonus = instance.GetLevelBonusMultiplier();
        
        switch (effect.effectType)
        {
            case EmbossingEffectType.DamageMultiplier:
                totalMultiplier += effect.effectValue * levelBonus;
                break;
                
            case EmbossingEffectType.PhysicalDamageMultiplier:
                if (damageType == DamageType.Physical)
                    totalMultiplier += effect.effectValue * levelBonus;
                break;
                
            // ... other damage types
        }
    }
    
    return baseDamage * totalMultiplier;
}
```

### Step 2: Update DamageCalculator

**File:** `Assets/Scripts/Combat/DamageCalculation.cs` (line ~184)

**Modify:** `CalculateCardDamage()` method

**Add after attribute scaling:**
```csharp
// Apply embossing effects
if (card.appliedEmbossings != null && card.appliedEmbossings.Count > 0)
{
    totalDamage = EmbossingEffectProcessor.ApplyDamageMultipliers(
        card, 
        totalDamage, 
        card.primaryDamageType
    );
    
    Debug.Log($"  After embossing multipliers: {totalDamage}");
}
```

### Step 3: Test

**Testing Steps:**
1. Create test card with base damage 10
2. Apply "of Amplification" (+25% damage)
3. Play card in combat
4. Verify damage = 12.5 (10 × 1.25)

---

## 📚 Full Effect Type Implementation Guide

### Damage Multipliers (Phase 1):
```csharp
DamageMultiplier           → baseDamage × (1 + effectValue)
PhysicalDamageMultiplier   → if damageType == Physical
ElementalDamageMultiplier  → if damageType is elemental
SpellDamageMultiplier      → if card has spell tag
FlatDamageBonus            → baseDamage + effectValue
```

### Status Effects (Phase 2):
```csharp
ApplyBleed     → Roll statusEffectChance, apply Bleed(effectValue duration)
ApplyIgnite    → Roll statusEffectChance, apply Ignite(effectValue duration)
ApplyPoison    → Roll statusEffectChance, apply Poison(effectValue duration)
ApplyShock     → Roll statusEffectChance, apply Shock(effectValue duration)
ApplyFreeze    → Roll statusEffectChance, apply Freeze(effectValue duration)
ApplyChill     → Roll statusEffectChance, apply Chill(effectValue duration)
```

### Scaling (Phase 3):
```csharp
StrengthScaling     → damage += character.strength × effectValue
DexterityScaling    → damage += character.dexterity × effectValue
IntelligenceScaling → damage += character.intelligence × effectValue
EmbossingCountScaling → damage × (1 + effectValue × embossingCount)
```

### Conversions (Phase 4):
```csharp
PhysicalToFireConversion → 
    physicalDamage × (1 - effectValue) as Physical
    physicalDamage × effectValue as Fire

ElementalToChaosConversion →
    (fire + cold + lightning) × (1 - effectValue) as elemental
    (fire + cold + lightning) × effectValue as Chaos
```

### Utility (Phase 5):
```csharp
ManaCostReduction  → card.manaCost × (1 - effectValue)
CriticalChance     → baseCritChance + effectValue
CriticalMultiplier → baseCritMult + effectValue
LifeOnHit          → player.Heal(effectValue) after damage
LifeLeech          → player.Heal(damage × effectValue) after damage
CardDuplication    → Roll effectValue, duplicate card in hand
PrepareCharges     → Add card to preparation with effectValue charges
Persistence        → Don't discard card after play
```

### Defensive (Phase 6):
```csharp
GuardOnPlay         → player.AddGuard(effectValue) when played
DamageReflection    → attacker.TakeDamage(damage × effectValue)
GuardEffectiveness  → guard × (1 + effectValue)
```

### Custom (Phase 7):
```csharp
CustomEffect → Switch on customEffectId:
    "aoe_radius"   → card.aoeTargets += (int)effectValue
    "bolster"      → Apply Bolster(effectValue stacks)
    "draw_card"    → Draw effectValue cards
    [Add more as needed]
```

---

## 🔗 Integration Points

### 1. Combat Damage Flow:
```
PlayCard() 
  → DamageCalculator.CalculateCardDamage()
    → EmbossingEffectProcessor.ApplyDamageMultipliers()
    → EmbossingEffectProcessor.ApplyScalingBonuses()
  → Apply damage to enemy
  → EmbossingStatusEffectHandler.RollStatusEffects()
  → EmbossingUtilityHandler.OnHit()
```

### 2. Mana Cost Integration:
```
Card.GetCurrentManaCost() [Already exists]
  → Reads appliedEmbossings
  → Applies manaCostMultiplier
  → Returns modified cost
```

### 3. Guard Calculation:
```
CalculateGuard()
  → Base guard from card
  → Apply guard scaling
  → EmbossingEffectProcessor.ApplyGuardModifiers()
  → Return total guard
```

---

## 📊 Data Flow Example

### Example: Playing Card with Embossings

**Card:** Heavy Strike (10 base damage)  
**Embossings:**
1. "of Ferocity" (+25% damage)
2. "of Cruelty" (15% bleed chance)
3. "of Power" (+3% per 50 STR, player has 100 STR)

**Calculation:**
```
Base Damage: 10
  ↓
STR Scaling: 10 + (100 × 0.03) = 13
  ↓
Damage Multiplier: 13 × 1.25 = 16.25
  ↓
Apply 16 damage to enemy
  ↓
Roll Bleed: Random(0-1) < 0.15? → Apply Bleed if true
  ↓
Complete
```

---

## 🎓 Best Practices

### Performance:
- Cache EmbossingDatabase.Instance
- Don't recreate lists every frame
- Use object pooling for effect instances

### Maintainability:
- Keep each effect type in separate method
- Use switch statements for clarity
- Document expected values for each effect

### Testing:
- Unit test each effect type
- Integration test effect combinations
- Edge case testing (no embossings, max embossings, etc.)

---

## 📋 Checklist for Each Phase

**Before Starting:**
- [ ] Read relevant existing code
- [ ] Identify integration points
- [ ] Plan data structures

**During Implementation:**
- [ ] Create new files in correct folder
- [ ] Update existing files minimally
- [ ] Add debug logging for testing
- [ ] Document public methods

**After Implementation:**
- [ ] Test basic functionality
- [ ] Test edge cases
- [ ] Remove excessive debug logs
- [ ] Update documentation

---

## 🚦 Current Status

**Phase 1: Damage Multipliers** - 🟡 READY TO IMPLEMENT  
**Phase 2: Status Effects** - ⚪ Pending  
**Phase 3: Stat Scaling** - ⚪ Pending  
**Phase 4: Conversions** - ⚪ Pending  
**Phase 5: Utility** - ⚪ Pending  
**Phase 6: Defensive** - ⚪ Pending  
**Phase 7: Custom** - ⚪ Pending  

---

## 🎯 Next Steps

1. ✅ Review this plan
2. 🔄 Create `EmbossingEffectProcessor.cs` (Phase 1)
3. 🔄 Update `DamageCalculator` (Phase 1)
4. 🔄 Test damage multipliers (Phase 1)
5. ⏸️ Continue with Phase 2 after Phase 1 works

---

**Let's start with Phase 1: Damage Multipliers!** 🚀


