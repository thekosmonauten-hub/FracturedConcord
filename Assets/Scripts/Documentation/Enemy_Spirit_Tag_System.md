# Enemy Spirit Tag System - Guide

## ✨ Overview

Enemies can now have **Spirit Tags** that determine what spirit currencies they can drop! This creates **thematic loot** where fire enemies drop Fire Spirits, physical enemies drop Physical Spirits, etc.

## 🏷️ How It Works

### **1. Tag Assignment**

In any **Enemy Data** asset (e.g., `Coconut Crab.asset`), you'll now see:

```
[Enemy Tags]
  Spirit Tags: [List ▼]
    - Fire
    - Physical
  Guaranteed Spirit Drop: [ ]
```

### **2. Drop Logic**

**Normal Drop (3% chance per tag)**:
- Enemy has `Fire` tag → 3% chance to drop Fire Spirit
- Enemy has `Physical` tag → 3% chance to drop Physical Spirit
- Enemy with BOTH tags → 3% chance for Fire Spirit + 3% chance for Physical Spirit

**Guaranteed Drop (100% chance)**:
- Check ✓ "Guaranteed Spirit Drop"
- Enemy will **always** drop one spirit matching its tags
- Perfect for special enemies like "Fire Pixie", "Cold Elemental", etc.

## 🎯 Available Spirit Tags

### **8 Spirit Tag Types**:

| Tag        | Drops            | Use For                          |
|------------|------------------|----------------------------------|
| Fire       | Fire Spirit      | Fire enemies, pyromancers       |
| Cold       | Cold Spirit      | Ice enemies, frost creatures    |
| Lightning  | Lightning Spirit | Storm enemies, electric beings  |
| Chaos      | Chaos Spirit     | Corrupted enemies, void beings  |
| Physical   | Physical Spirit  | Warriors, beasts, melee enemies |
| Life       | Life Spirit      | Healers, nature enemies         |
| Defense    | Defense Spirit   | Tanks, armored enemies          |
| Crit       | Crit Spirit      | Assassins, precise enemies      |

## 📦 Configuration Examples

### **Example 1: Fire Warrior**
```yaml
Enemy: Fire Sword Knight
Spirit Tags: [Fire, Physical]
Guaranteed Spirit Drop: ☐ (unchecked)

Result:
- 3% chance to drop Fire Spirit
- 3% chance to drop Physical Spirit
- Can drop both, one, or neither
```

### **Example 2: Fire Pixie (Guaranteed)**
```yaml
Enemy: Fire Pixie
Spirit Tags: [Fire]
Guaranteed Spirit Drop: ☑ (checked)

Result:
- 100% chance to drop Fire Spirit
- Always drops when defeated
```

### **Example 3: Chaos Caster**
```yaml
Enemy: Void Mage
Spirit Tags: [Chaos, Crit]
Guaranteed Spirit Drop: ☐ (unchecked)

Result:
- 3% chance to drop Chaos Spirit
- 3% chance to drop Crit Spirit
```

### **Example 4: Tank Enemy**
```yaml
Enemy: Iron Guardian
Spirit Tags: [Physical, Defense, Life]
Guaranteed Spirit Drop: ☐ (unchecked)

Result:
- 3% chance to drop Physical Spirit
- 3% chance to drop Defense Spirit
- 3% chance to drop Life Spirit
```

## 🎮 In-Game Behavior

### **During Combat:**
1. Player defeats enemies
2. System **tracks each enemy's EnemyData**
3. Stores spirit tags for loot calculation

### **On Victory:**
1. Loot system processes all defeated enemies
2. For each unique tag:
   - If enemy had "Guaranteed Spirit Drop" → **100% drop**
   - Otherwise → **3% chance per enemy with that tag**
3. Spirits added to loot rewards
4. Player sees spirits in rewards UI

### **Example Combat:**
```
Wave 1: Defeat 3 enemies
  - Enemy A: [Fire] tag
  - Enemy B: [Fire, Physical] tags
  - Enemy C: [Physical] tag

Loot Calculation:
  Fire tag: 2 enemies → 2 rolls at 3% each
  Physical tag: 2 enemies → 2 rolls at 3% each
  
Possible Results:
  - 0 spirits (bad luck)
  - 1 Fire Spirit (one roll succeeded)
  - 1 Physical Spirit (one roll succeeded)
  - Both spirits (lucky!)
```

## 🔧 Setup Instructions

### **Step 1: Tag Your Enemies**

For each enemy asset in `Assets/Resources/Enemies/`:

1. **Open the enemy asset**
2. **Find "Enemy Tags" section**
3. **Add appropriate Spirit Tags**:
   - Fire enemies → Add `Fire`
   - Melee warriors → Add `Physical`
   - Tanks → Add `Defense`, `Life`
   - Casters → Add element + `Crit`
4. **Check "Guaranteed Spirit Drop"** for special enemies (Pixies, Elementals)

### **Step 2: Test in Combat**

1. Start an encounter with tagged enemies
2. Defeat all enemies
3. Check console for loot tracking:
   ```
   [Loot Tracking] Defeated enemy tracked: Fire Warrior (Tags: Fire, Physical)
   [Loot] Random spirit drop (3% chance): FireSpirit
   ```
4. See spirits in rewards UI

## 💡 Design Guidelines

### **Common Enemies (Normal Rarity)**
- **1-2 tags** max
- **No guaranteed drops**
- Examples:
  - Goblin → `Physical`
  - Fire Imp → `Fire`
  - Skeleton Archer → `Physical`

### **Elite Enemies**
- **2-3 tags**
- **No guaranteed drops** (keep them rare)
- Examples:
  - Fire Knight → `Fire, Physical`
  - Ice Mage → `Cold, Crit`
  - Armored Tank → `Physical, Defense, Life`

### **Special Enemies (Guaranteed Drops)**
- **1 tag** (focused theme)
- **✓ Guaranteed Spirit Drop**
- Examples:
  - Fire Pixie → `Fire` + guaranteed
  - Cold Elemental → `Cold` + guaranteed
  - Life Spirit → `Life` + guaranteed

### **Boss Enemies**
- **2-4 tags** (multi-faceted)
- **Usually no guaranteed** (rely on 3% but more tags = more chances)
- Examples:
  - Chaos Lord → `Chaos, Physical, Crit`
  - Dragon → `Fire, Physical, Life, Defense`

## 📊 Drop Rate Math

### **Single Enemy with 1 Tag**
- 3% chance = ~1 in 33 kills

### **Multiple Enemies with Same Tag**
- 2 enemies with Fire tag = 2 rolls × 3% = ~5.9% overall
- 3 enemies with Fire tag = 3 rolls × 3% = ~8.7% overall

### **Enemy with Multiple Tags**
- Enemy with 3 tags = 3 different spirit types, each at 3%
- Not cumulative (can drop multiple spirits from one enemy!)

## 🎲 Balancing Tips

### ✅ DO:
- Tag enemies thematically (fire enemies → Fire tag)
- Use multiple tags for hybrid enemies
- Reserve guaranteed drops for rare/special enemies
- Give bosses 3-4 tags for variety
- Test drop rates in actual gameplay

### ❌ DON'T:
- Tag every enemy with every tag (dilutes theme)
- Make all drops guaranteed (too much loot)
- Forget to tag enemies (no spirit drops!)
- Give common enemies guaranteed drops (economy break)

## 🔍 Debugging

### **Console Logs to Watch:**
```
[Loot Tracking] Defeated enemy tracked: Fire Warrior (Tags: Fire, Physical)
[Loot] Random spirit drop (3% chance): FireSpirit (from enemy with Fire tag)
[Loot] Guaranteed spirit drop: ColdSpirit (from enemy with Cold tag)
[Combat Victory] Generated 5 rewards (from 3 defeated enemies):
  - 50 Experience
  - 1x FireSpirit
  - 2x OrbOfGeneration
```

### **Verification:**
1. Check enemy asset has tags assigned
2. Check console shows "Defeated enemy tracked"
3. Check console shows spirit drop attempts
4. Verify spirits appear in loot rewards

## 🚀 Integration Summary

### **What's Connected:**
✅ **EnemyData** - Stores spirit tags  
✅ **EnemyCombatDisplay** - Exposes EnemyData via GetEnemyData()  
✅ **CombatDisplayManager** - Tracks defeated enemies  
✅ **LootTable** - Processes tags for spirit drops  
✅ **LootManager** - Passes enemy data to loot generation  

### **Flow:**
```
Enemy Defeated → Track EnemyData → 
Combat Victory → Process All Tags → 
Roll 3% (or 100% if guaranteed) → 
Add Spirits to Loot → Display Rewards
```

## 📝 Example Enemy Configurations

### **Coconut Crab**
```
Spirit Tags: [Physical, Defense]
Guaranteed: ☐
→ 3% Physical Spirit, 3% Defense Spirit
```

### **Fire Pixie** (Special)
```
Spirit Tags: [Fire]
Guaranteed: ☑
→ 100% Fire Spirit
```

### **Drowned Dead**
```
Spirit Tags: [Cold, Life]
Guaranteed: ☐
→ 3% Cold Spirit, 3% Life Spirit
```

### **BOSS: First To Fall**
```
Spirit Tags: [Chaos, Physical, Crit, Life]
Guaranteed: ☐
→ 3% for each (4 chances for different spirits!)
```

---

**Status**: ✅ Fully Implemented  
**Drop Base Rate**: 3% per enemy per tag  
**Special Rate**: 100% if guaranteed flag set  
**Tag Types**: 8 available spirit tags













