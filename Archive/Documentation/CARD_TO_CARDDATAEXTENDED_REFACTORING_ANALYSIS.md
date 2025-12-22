# Card → CardDataExtended Refactoring Analysis

## 📊 **Scope Assessment**

### **Current State**
- **Card class**: Runtime serializable class (172 files reference it)
- **CardDataExtended**: ScriptableObject asset (34 files reference it)
- **Conversion**: `CardDataExtended.ToCard()` creates Card instances at runtime
- **Problem**: Dual source of truth, unnecessary conversions, data loss risk

### **Goal**
Make `CardDataExtended` the single source of truth, eliminate `Card` class.

---

## 🔍 **Key Differences & Challenges**

### **1. Runtime Data Problem** ⚠️

`Card` stores **runtime state** that cannot be in a ScriptableObject:

```csharp
// Card.cs - Runtime data
public List<EmbossingInstance> appliedEmbossings;  // Player modifications
public int cardLevel = 1;                          // Card progression
public int cardExperience = 0;                    // Experience tracking
public bool channelingActive;                     // Combat state
```

**ScriptableObjects are shared assets** - they cannot store per-instance runtime data.

### **2. Type System Difference**

- **Card**: `[System.Serializable]` class (can be serialized in JSON/save files)
- **CardDataExtended**: `ScriptableObject` (Unity asset, shared across instances)

### **3. Usage Patterns**

**Card is used for:**
- ✅ Combat calculations (`CardEffectProcessor`)
- ✅ Deck storage (`CharacterDeckData`)
- ✅ UI display (`DeckBuilderCardUI`, `CombatCardAdapter`)
- ✅ Preparation system (`PreparedCard`)
- ✅ Embossing system (applied embossings)
- ✅ Card leveling/experience
- ✅ Save/Load (serialization)

**CardDataExtended is used for:**
- ✅ Asset definition (ScriptableObject)
- ✅ Dynamic descriptions (`GetDynamicDescription()`)
- ✅ Card database lookups
- ✅ Some UI systems (newer code)

---

## 📋 **Refactoring Strategy**

### **Option A: Runtime Wrapper Class** (RECOMMENDED)

Create a lightweight runtime wrapper that holds:
1. Reference to `CardDataExtended` (the asset)
2. Runtime state (embossings, level, experience)

```csharp
[System.Serializable]
public class CardInstance
{
    public CardDataExtended cardData;  // Asset reference
    public List<EmbossingInstance> appliedEmbossings;
    public int cardLevel = 1;
    public int cardExperience = 0;
    
    // Convenience properties that delegate to cardData
    public string cardName => cardData.cardName;
    public int manaCost => cardData.playCost;
    public float baseDamage => cardData.damage;
    // ... etc
}
```

**Pros:**
- ✅ Single source of truth (CardDataExtended)
- ✅ Runtime data separate from asset
- ✅ Can serialize for save/load
- ✅ Minimal changes to existing code

**Cons:**
- ⚠️ Need to update all `Card` references to `CardInstance`
- ⚠️ Need convenience properties/methods

### **Option B: Move Runtime Data to CharacterDeckData**

Store runtime state in `CharacterDeckData` as dictionaries:

```csharp
public class CharacterDeckData
{
    public List<CardDataExtended> cards;  // Assets only
    public Dictionary<string, List<EmbossingInstance>> cardEmbossings;
    public Dictionary<string, int> cardLevels;
    public Dictionary<string, int> cardExperience;
}
```

**Pros:**
- ✅ Clean separation
- ✅ No wrapper class needed

**Cons:**
- ❌ More complex lookups
- ❌ Harder to maintain consistency
- ❌ Large refactoring

### **Option C: Hybrid Approach**

Keep `Card` but make it a thin wrapper around `CardDataExtended`:

```csharp
[System.Serializable]
public class Card
{
    [System.NonSerialized]
    public CardDataExtended sourceCardData;  // Primary source
    
    // Runtime-only fields
    public List<EmbossingInstance> appliedEmbossings;
    public int cardLevel = 1;
    public int cardExperience = 0;
    
    // All other properties delegate to sourceCardData
    public string cardName => sourceCardData?.cardName ?? "";
    // ... etc
}
```

**Pros:**
- ✅ Minimal code changes
- ✅ Backward compatible

**Cons:**
- ⚠️ Still two classes
- ⚠️ Need to ensure sourceCardData is always set

---

## 📊 **Refactoring Scope Breakdown**

### **Phase 1: Core Systems** (HIGH PRIORITY)
- [ ] `CardEffectProcessor.cs` - 11 methods use `Card`
- [ ] `CombatDeckManager.cs` - Hand management, card playing
- [ ] `PreparationManager.cs` - Prepared cards
- [ ] `CharacterDeckData.cs` - Deck storage
- [ ] `CardRuntimeManager.cs` - Card GameObject creation

**Estimated Files**: ~15-20 files
**Estimated Time**: 4-6 hours

### **Phase 2: UI Systems** (MEDIUM PRIORITY)
- [ ] `DeckBuilderCardUI.cs` - Card display
- [ ] `CombatCardAdapter.cs` - Combat card UI
- [ ] `AnimatedCombatUI.cs` - Combat animations
- [ ] `CardHoverEffect.cs` - Hover tooltips
- [ ] `CardTooltipSystem.cs` - Tooltip display

**Estimated Files**: ~10-15 files
**Estimated Time**: 3-4 hours

### **Phase 3: Data & Save Systems** (MEDIUM PRIORITY)
- [ ] `Character.cs` - Character deck references
- [ ] `DeckManager.cs` - Deck management
- [ ] Save/Load systems - Serialization
- [ ] `CardExperienceManager.cs` - Leveling system

**Estimated Files**: ~10-15 files
**Estimated Time**: 2-3 hours

### **Phase 4: Supporting Systems** (LOW PRIORITY)
- [ ] `ComboSystem.cs` - Combo detection
- [ ] `CardAbilityRouter.cs` - Card abilities
- [ ] `EmbossingEffectProcessor.cs` - Embossing system
- [ ] Various UI components

**Estimated Files**: ~30-40 files
**Estimated Time**: 4-6 hours

### **Phase 5: Cleanup** (LOW PRIORITY)
- [ ] Remove `Card` class
- [ ] Remove `ToCard()` method
- [ ] Update documentation
- [ ] Test all systems

**Estimated Files**: ~5-10 files
**Estimated Time**: 2-3 hours

---

## ⏱️ **Total Estimated Time**

**Conservative Estimate**: 15-22 hours
**Optimistic Estimate**: 10-15 hours
**With Testing**: 20-30 hours

---

## 🎯 **Recommended Approach**

### **Step 1: Create CardInstance Wrapper** (2 hours)
```csharp
[System.Serializable]
public class CardInstance
{
    // Asset reference (primary source of truth)
    public CardDataExtended cardData;
    
    // Runtime state
    public List<EmbossingInstance> appliedEmbossings = new List<EmbossingInstance>();
    public int cardLevel = 1;
    public int cardExperience = 0;
    
    // Convenience properties
    public string cardName => cardData?.cardName ?? "";
    public int manaCost => cardData?.playCost ?? 0;
    public float baseDamage => cardData != null ? (float)cardData.damage : 0f;
    // ... add all Card properties as delegates
}
```

### **Step 2: Update Core Combat Systems** (4-6 hours)
- Replace `Card` with `CardInstance` in:
  - `CardEffectProcessor`
  - `CombatDeckManager`
  - `PreparationManager`

### **Step 3: Update Data Storage** (2-3 hours)
- Change `CharacterDeckData.cards` from `List<Card>` to `List<CardInstance>`
- Update save/load serialization

### **Step 4: Update UI Systems** (3-4 hours)
- Update all UI components to use `CardInstance`
- Ensure `cardData` reference is always set

### **Step 5: Gradual Migration** (4-6 hours)
- Keep `Card` class temporarily with `[Obsolete]` attribute
- Add conversion methods for backward compatibility
- Migrate systems one by one

### **Step 6: Remove Card Class** (1-2 hours)
- Once all systems migrated, remove `Card` class
- Remove `ToCard()` method

---

## ⚠️ **Risks & Considerations**

### **High Risk Areas**
1. **Save/Load System**: Serialization format changes
2. **Embossing System**: Runtime modifications need careful handling
3. **Card Leveling**: Experience/level data must persist
4. **Combat State**: `channelingActive` and other runtime flags

### **Testing Requirements**
- ✅ Combat system (all card types)
- ✅ Preparation system
- ✅ Embossing system
- ✅ Card leveling/experience
- ✅ Save/Load functionality
- ✅ UI display (all screens)
- ✅ Deck building

---

## 📝 **Migration Checklist**

### **Pre-Migration**
- [ ] Create `CardInstance` wrapper class
- [ ] Add convenience properties/methods
- [ ] Create migration utility to convert existing `Card` instances
- [ ] Backup save files

### **Phase 1: Core Combat**
- [ ] Update `CardEffectProcessor`
- [ ] Update `CombatDeckManager`
- [ ] Update `PreparationManager`
- [ ] Test combat system

### **Phase 2: Data Storage**
- [ ] Update `CharacterDeckData`
- [ ] Update save/load serialization
- [ ] Test save/load

### **Phase 3: UI Systems**
- [ ] Update `DeckBuilderCardUI`
- [ ] Update `CombatCardAdapter`
- [ ] Update `AnimatedCombatUI`
- [ ] Test all UI screens

### **Phase 4: Supporting Systems**
- [ ] Update embossing system
- [ ] Update card leveling
- [ ] Update combo system
- [ ] Test all features

### **Post-Migration**
- [ ] Remove `Card` class
- [ ] Remove `ToCard()` method
- [ ] Update documentation
- [ ] Full system test

---

## 🎯 **Conclusion**

**Refactoring Size**: **LARGE** (15-30 hours)

**Recommendation**: 
- ✅ **Proceed with Option A (CardInstance wrapper)**
- ✅ **Gradual migration** (keep Card temporarily)
- ✅ **Thorough testing** at each phase
- ⚠️ **Consider doing this in a separate branch**

**Benefits**:
- ✅ Single source of truth
- ✅ Eliminates conversion overhead
- ✅ Reduces data loss risk
- ✅ Cleaner architecture

**Challenges**:
- ⚠️ Large codebase changes
- ⚠️ Runtime data handling
- ⚠️ Save/Load compatibility
- ⚠️ Testing requirements

