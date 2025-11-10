# Currency System - Logic Reference

## ✅ Updated Currency Logic

This document defines the **actual functionality** of each currency type to match their descriptions.

---

## 🔮 ORBS (Crafting Currencies)

### **Orb of Generation** (15% drop)
- **Description**: "Allows you to generate a card"
- **Logic**: Used to generate/create cards, not applicable to equipment
- **Target**: N/A (card system)
- **Effect**: Generate a new card

---

### **Orb of Infusion** (15% drop)
- **Description**: "Reforges Normal equipment, making it Magic and adds a random affix"
- **Logic**:
  1. Requires: Normal rarity item
  2. Clears all existing affixes
  3. Changes rarity: Normal → Magic
  4. Adds: 1 random affix (prefix or suffix)
- **Target**: Normal rarity equipment only
- **Effect**: Upgrades item to Magic with 1 affix

---

### **Orb of Perfection** (7% drop)
- **Description**: "Reforges Magic equipment making it Rare and adds a random affix"
- **Logic**:
  1. Requires: Magic rarity item
  2. Keeps existing affixes
  3. Changes rarity: Magic → Rare
  4. Adds: 1 additional random affix
- **Target**: Magic rarity equipment only
- **Effect**: Upgrades item to Rare with +1 affix

---

### **Orb of Perpetuity** (6% drop)
- **Description**: "Adds a random affix to Rare equipment"
- **Logic**:
  1. Requires: Rare rarity item
  2. Checks: Total affixes < 6
  3. Adds: 1 random affix (prefix or suffix)
  4. Rarity: Stays Rare
- **Target**: Rare rarity equipment (with < 6 affixes)
- **Effect**: Adds 1 affix to existing Rare item

---

### **Orb of Redundancy** (3% drop) ✏️ UPDATED
- **Description**: "Removes a random affix from an equipment"
- **Logic**:
  1. Requires: Item with at least 1 affix
  2. Randomly selects: 1 prefix OR 1 suffix
  3. Removes: The selected affix
  4. Rarity: May downgrade if affixes drop below thresholds
- **Target**: Any item with affixes
- **Effect**: Removes 1 random affix

---

### **Orb of the Void** (2% drop)
- **Description**: "Corrupts an item, adding powerful but unpredictable modifiers"
- **Logic**:
  1. Marks item as corrupted
  2. Adds: 1-2 powerful random modifiers
  3. Risk: May add negative modifiers
  4. Item cannot be modified further after corruption
- **Target**: Any equipment
- **Effect**: High risk, high reward corruption

---

### **Orb of Mutation** (6% drop) ✏️ UPDATED
- **Description**: "Randomizes all affixes on a Magic equipment"
- **Logic**:
  1. Requires: Magic rarity item
  2. Counts current affixes (prefixes + suffixes)
  3. Clears: All existing affixes
  4. Regenerates: Same NUMBER of affixes but completely random
  5. Rarity: Stays Magic
- **Target**: Magic rarity equipment only
- **Effect**: Rerolls all affixes (same count, new types/values)

---

### **Orb of Proliferation** (12% drop) ✏️ UPDATED
- **Description**: "Adds a random affix to a Magic equipment"
- **Logic**:
  1. Requires: Magic rarity item
  2. Checks: Total affixes < 6
  3. Adds: 1 random affix (prefix or suffix)
  4. Rarity: Stays Magic
- **Target**: Magic rarity equipment (with < 6 affixes)
- **Effect**: Adds 1 affix to Magic item (doesn't upgrade rarity)

---

### **Orb of Amnesia** (4% drop) ✏️ UPDATED
- **Description**: "Removes all affixes while preserving locked affixes"
- **Logic**:
  1. Requires: Item with at least 1 affix
  2. Identifies: All locked affixes (via Inscription Seal)
  3. Removes: All non-locked affixes
  4. Preserves: Any affixes marked as locked
  5. Rarity: Downgrades to Normal (unless locked affixes remain)
- **Target**: Any item with affixes
- **Effect**: Full reset except for locked affixes

---

## ⚡ SPIRITS (Elemental/Stat Modifiers)

### **Fire Spirit** (drop% pending)
- **Description**: "Adds or rerolls fire damage affixes on equipment"
- **Logic**:
  1. Searches for existing fire damage affixes
  2. If found: Rerolls their values
  3. If not found: Adds 1 fire damage affix
- **Target**: Any equipment
- **Effect**: Ensures item has fire damage

### **Cold Spirit** (drop% pending)
- **Description**: "Adds or rerolls cold damage affixes on equipment"
- **Logic**: Same as Fire Spirit but for cold damage

### **Lightning Spirit** (drop% pending)
- **Description**: "Adds or rerolls lightning damage affixes on equipment"
- **Logic**: Same as Fire Spirit but for lightning damage

### **Chaos Spirit** (drop% pending)
- **Description**: "Adds or rerolls chaos damage affixes on equipment"
- **Logic**: Same as Fire Spirit but for chaos damage

### **Physical Spirit** (drop% pending)
- **Description**: "Adds or rerolls physical damage affixes on equipment"
- **Logic**: Same as Fire Spirit but for physical damage

### **Life Spirit** (drop% pending)
- **Description**: "Adds or rerolls life affixes on equipment"
- **Logic**: Same pattern but for life/health stats

### **Defense Spirit** (drop% pending)
- **Description**: "Adds or rerolls defense affixes on equipment"
- **Logic**: Same pattern but for armor/evasion/ES

### **Crit Spirit** (drop% pending)
- **Description**: "Adds or rerolls critical strike affixes on equipment"
- **Logic**: Same pattern but for crit chance/multiplier

### **Divine Spirit** (drop% pending)
- **Description**: "Rerolls the value of a random affix to maximum"
- **Logic**:
  1. Requires: Item with at least 1 affix
  2. Selects: 1 random affix
  3. Rerolls: Its value to the tier's maximum roll
- **Target**: Any item with affixes
- **Effect**: Perfect roll on 1 affix

---

## 🔒 SEALS (Advanced Modifiers)

### **Transposition Seal** (drop% pending)
- **Description**: "Swaps two affixes between two items"
- **Logic**: Requires 2 items, swaps selected affixes

### **Chaos Seal** (drop% pending)
- **Description**: "Randomly shuffles all affixes on an item"
- **Logic**: Randomizes which affixes are prefixes vs suffixes

### **Memory Seal** (drop% pending)
- **Description**: "Saves the current state of an item for later restoration"
- **Logic**: Creates snapshot of item's affixes for rollback

### **Inscription Seal** (drop% pending)
- **Description**: "Locks an affix preventing it from being changed"
- **Logic**: Marks selected affix as locked (immune to rerolls/removes)

### **Adaptation Seal** (drop% pending)
- **Description**: "Changes the type of an affix while keeping its tier"
- **Logic**: Transforms affix type but preserves tier and value range

### **Correction Seal** (drop% pending)
- **Description**: "Removes the lowest tier affix from an item"
- **Logic**: Finds lowest tier affix and removes it

### **Etching Seal** (drop% pending)
- **Description**: "Improves the tier of a random affix by one"
- **Logic**: Upgrades random affix: Tier 1→2, Tier 2→3, etc.

---

## 🧩 FRAGMENTS (Future Use)

### **Fragment 1, 2, 3** (drop% pending)
- **Description**: "Mysterious fragment - purpose unknown"
- **Logic**: Reserved for future mechanics

---

## 📋 Summary of Changes

### **Orb of Redundancy**
- ❌ Old: "Rerolls values of all affixes"
- ✅ New: "Removes a random affix"

### **Orb of Mutation**
- ❌ Old: "Transforms one affix into another"
- ✅ New: "Randomizes all affixes on Magic equipment"

### **Orb of Proliferation**
- ❌ Old: "Duplicates a random affix"
- ✅ New: "Adds a random affix to Magic equipment"

### **Orb of Amnesia**
- ❌ Old: "Removes a random affix while preserving locked"
- ✅ New: "Removes ALL affixes while preserving locked"

---

## 🔧 Implementation Status

✅ **Logic Defined**: All orb behaviors specified  
✅ **Validation**: Can/cannot apply checks implemented  
⏳ **Affix System Integration**: Needs AffixDatabase hookup  
⏳ **Locked Affix Tracking**: Needs implementation  
⏳ **Spirit Effects**: Need element-specific affix addition  
⏳ **Seal Effects**: Need advanced modifier logic  

---

**File**: `Assets/Scripts/Data/Items/CurrencyEffectProcessor.cs`  
**Status**: Core logic implemented, awaiting affix system integration













