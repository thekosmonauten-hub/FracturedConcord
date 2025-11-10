# Loot System - Quick Start Guide

## ✅ What Has Been Implemented

### Core Systems Created:
1. **LootTable ScriptableObject** - Define drops for encounters
2. **LootManager** - Singleton that handles generation and distribution
3. **LootReward** - Data structures for different reward types
4. **LootRewardsUI** - UI to display rewards after combat
5. **Combat Integration** - Automatically generates loot on victory
6. **Encounter Integration** - Each encounter can have its own loot table

## 🎮 How to Use (Quick Steps)

### 1. Create a Loot Table (2 minutes)
```
Right-click in Project → Create → Dexiled → Loot System → Loot Table
```
- Name: "Act1_BasicEncounter_LootTable"
- Set Base Experience: 50
- Add Guaranteed Currency: Orb of Generation (1-2 quantity, 100% chance)
- Add Random Currency: Orb of Infusion (1 quantity, 25% chance)

### 2. Assign to Encounter (30 seconds)
```
Open: Assets/Resources/Encounters/Act1/1.WhereNightFirstFell.asset
```
- Find "Loot Rewards" section at bottom
- Drag your loot table into the "Loot Table" field
- Save

### 3. Setup Combat Scene UI (5 minutes)
```
Open: Assets/Scenes/CombatScene.unity
```
- Create UI Panel (Right-click Canvas → UI → Panel)
- Name it "LootRewardsPanel"
- Add `LootRewardsUI` component
- Create reward display area (vertical layout group recommended)
- Create buttons: "Collect Rewards" and "Return"
- Assign all references in inspector

### 4. Test!
- Play the game
- Complete "Where Night First Fell" encounter
- Victory screen should show your loot rewards
- Click "Collect Rewards" to apply them
- Check console for confirmation logs

## 📦 What You Can Drop

### Currency Drops
```csharp
// In LootTable:
- Orb of Generation
- Orb of Infusion
- Orb of Perfection
- Fire Spirit, Cold Spirit, etc.
- All currency types from CurrencyDatabase
```

### Experience Drops
```csharp
// Automatically scales with area level:
experienceAmount = baseExperience + (experiencePerLevel * (areaLevel - 1))
// Example: Level 5 area with base 50 and +10 per level = 90 experience
```

### Item Drops (Future)
```csharp
// Currently logs to console, integration pending
- Weapons
- Armor
- Accessories
```

### Card Drops (Future)
```csharp
// Currently logs to console, integration pending
- Attack cards
- Skill cards
- Equipment cards
```

## 🔧 Configuration Examples

### Easy Encounter
```
Base Experience: 50
Experience Per Level: 10
Guaranteed: 
  - Orb of Generation: 1-2 (100%)
Random:
  - Orb of Infusion: 1 (20%)
```

### Medium Encounter
```
Base Experience: 100
Experience Per Level: 15
Guaranteed:
  - Orb of Generation: 2-3 (100%)
Random:
  - Orb of Infusion: 1 (35%)
  - Fire Spirit: 1 (25%)
  - Orb of Perfection: 1 (15%)
```

### Boss Encounter
```
Base Experience: 250
Experience Per Level: 30
Guaranteed:
  - Orb of Perfection: 2-4 (100%)
  - Divine Spirit: 1 (100%)
Random:
  - Orb of Mutation: 1 (50%)
  - Rare Item: 1 (30%)
  - Orb of the Void: 1 (10%)
```

## 🎯 Key Features

✅ **Automatic Integration**: Loot generates on combat victory  
✅ **Area Level Scaling**: Experience scales with encounter difficulty  
✅ **Guaranteed Drops**: Always drop certain currencies  
✅ **Random Drops**: RNG for variety and excitement  
✅ **Currency Support**: Full integration with existing currency system  
✅ **Experience Support**: Applies XP and triggers level-ups  
✅ **Quantity Ranges**: Min/Max for variable rewards  
✅ **Drop Chances**: 0-100% chance per entry  

## 🐛 Troubleshooting

**No loot appearing?**
- Check that encounter has loot table assigned
- Check console for "[LootManager]" logs
- Verify LootManager exists in scene (auto-creates)

**UI not showing?**
- Verify LootRewardsUI component is on Canvas
- Check all UI references are assigned
- Check panel is initially set to inactive

**Rewards not applying?**
- Check console for "[LootManager] Applied X rewards" message
- Verify CharacterManager exists and has active character
- Check LootManager.ApplyRewards() is being called

## 📝 Next Steps

1. **Create loot tables for all Act 1 encounters** (17 encounters)
2. **Design reward balance** (what should drop where?)
3. **Setup LootRewardsUI** in combat scene with proper layout
4. **Test each encounter** to verify loot generation
5. **Tune drop rates** based on player feedback
6. **Add item drops** once inventory integration is ready

## 💡 Tips

- Use **low drop chances** (10-20%) for rare currencies
- Use **guaranteed drops** for common currencies players need
- **Scale experience** appropriately (later encounters = more XP)
- **Test balance**: Play through encounters and adjust
- **Visual feedback**: Make rewards exciting to collect!

---

**Status**: ✅ Fully Implemented and Ready to Configure
**Integration**: ✅ Combat System, Currency System, Experience System
**Documentation**: ✅ Full Setup Guide Available (LootSystem_Setup_Guide.md)













