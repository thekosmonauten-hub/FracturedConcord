# Loot System - Database Integration Guide

## ✨ Enhanced Features

The loot system now integrates with your **CurrencyDatabase** and **ItemDatabase** for easy drop configuration!

## 📦 Database-Driven Setup

### 1. Assign Databases to Loot Table

When you create a loot table, you'll see:

```
[Header("Database References")]
- Currency Database: Drag CurrencyDatabase asset here
- Item Database: Drag ItemDatabase asset here
```

**Location**: `Assets/Resources/CurrencyDatabase.asset`

### 2. Use the Custom Editor

The custom editor makes adding drops super easy! When you select a loot table, you'll see:

#### **Quick Add Currency Drop Tool**
```
┌─────────────────────────────────┐
│ Add Currency Drop               │
├─────────────────────────────────┤
│ Currency Type: [Dropdown]       │
│ Description: [Auto-shows here]  │
│ Guaranteed Drop: [✓]            │
│ Drop Chance %: [slider 0-100]   │
│ Quantity Range: [1] to [3]      │
│ [Add Currency Drop] Button      │
└─────────────────────────────────┘
```

**Features**:
- ✅ Dropdown shows ALL currency types from database
- ✅ Auto-displays currency description
- ✅ Toggle between guaranteed (100%) or random chance
- ✅ Set min/max quantity range
- ✅ One-click add to loot table

#### **Quick Setup Presets**
Three preset buttons for instant configuration:

1. **Basic Encounter Rewards**
   - 1-3x Orb of Generation (guaranteed)
   - 25% chance for Orb of Infusion
   - 15% chance for Fire Spirit
   - 50 base XP + 10 per level

2. **Boss Encounter Rewards**
   - 3-5x Orb of Generation (guaranteed)
   - 2-3x Orb of Perfection (guaranteed)
   - 50% chance for Orb of Mutation
   - 10% chance for Divine Spirit
   - 250 base XP + 30 per level

3. **Clear All Drops**
   - Removes all entries (with confirmation)

## 🎯 Workflow Examples

### Example 1: Create Basic Encounter Loot
```
1. Create new Loot Table
2. Assign CurrencyDatabase from Resources
3. Click "Basic Encounter Rewards"
4. Done! Ready to use
```

### Example 2: Custom Currency Mix
```
1. Create new Loot Table
2. Assign CurrencyDatabase
3. Use "Add Currency Drop" tool:
   - Select "OrbOfGeneration" → Guaranteed → 2-4 qty → Add
   - Select "OrbOfInfusion" → 30% chance → 1 qty → Add
   - Select "ColdSpirit" → 20% chance → 1 qty → Add
4. Save
```

### Example 3: Boss Loot with Items
```
1. Create new Loot Table
2. Assign both CurrencyDatabase and ItemDatabase
3. Click "Boss Encounter Rewards" for currencies
4. Manually add item drops:
   - Create LootEntry
   - Set RewardType = Item
   - Drag ItemData from ItemDatabase
   - Set drop chance (e.g., 30%)
5. Save
```

## 🛠️ Helper Methods (For Scripts)

If you want to create loot tables programmatically:

```csharp
// Create guaranteed currency drop
var entry = LootTableHelper.CreateGuaranteedCurrencyDrop(
    CurrencyType.OrbOfGeneration, 
    minQty: 1, 
    maxQty: 3
);

// Create random currency drop
var entry = LootTableHelper.CreateRandomCurrencyDrop(
    CurrencyType.OrbOfInfusion,
    dropChance: 25f,
    minQty: 1,
    maxQty: 1
);

// Create item drop
var entry = LootTableHelper.CreateItemDrop(
    itemData,
    dropChance: 30f
);

// Quick setup presets
var basicRewards = LootTableHelper.CreateBasicCurrencyRewards();
var bossRewards = LootTableHelper.CreateBossCurrencyRewards();
```

## 📋 Currency Types Available

From your CurrencyDatabase:

### Orbs
- OrbOfGeneration
- OrbOfInfusion
- OrbOfPerfection
- OrbOfPerpetuity
- OrbOfRedundancy
- OrbOfTheVoid
- OrbOfMutation
- OrbOfProliferation
- OrbOfAmnesia

### Spirits
- FireSpirit
- ColdSpirit
- LightningSpirit
- ChaosSpirit
- PhysicalSpirit
- LifeSpirit
- DefenseSpirit
- CritSpirit
- DivineSpirit

### Seals
- TranspositionSeal
- ChaosSeal
- MemorySeal
- InscriptionSeal
- AdaptationSeal
- CorrectionSeal
- EtchingSeal

### Fragments
- Fragment1
- Fragment2
- Fragment3

## 💡 Best Practices

### ✅ DO:
- Always assign CurrencyDatabase to loot tables
- Use the custom editor for quick setup
- Use presets as starting points, then customize
- Test drop rates in-game and adjust
- Group similar currencies (e.g., all spirits together)

### ❌ DON'T:
- Leave databases unassigned (you'll lose helper features)
- Set drop chances above 100%
- Set min quantity > max quantity
- Forget to save after changes

## 🎮 Testing Your Loot

1. Create/configure loot table
2. Assign to encounter
3. Play through encounter
4. Check console for loot generation logs:
   ```
   [Combat Victory] Generated 4 rewards:
     - 2x OrbOfGeneration
     - 50 Experience
     - 1x FireSpirit
     - 1x OrbOfInfusion
   ```
5. Adjust drop rates based on feel

## 🔧 Troubleshooting

**Currency descriptions not showing?**
- Make sure CurrencyDatabase is assigned in the loot table

**Preset buttons not working?**
- Database must be assigned first
- Check console for error messages

**Drops not appearing in game?**
- Verify loot table is assigned to encounter
- Check drop chances (0% won't drop!)
- Look for "[LootManager]" logs in console

---

**Status**: ✅ Database Integration Complete  
**Benefits**: Faster setup, fewer errors, better workflow!













