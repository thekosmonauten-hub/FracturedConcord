# Area-Based Loot System Guide

## 🎯 Overview

The **Area-Based Loot System** generates items based on area level requirements, ensuring players find appropriately leveled gear as they progress through the game.

### ✨ **Key Features**:
- **📊 Level-Based Drops**: Items drop based on area level with smart range calculation
- **🎲 Configurable Rarity**: Customizable drop rates for Normal/Magic/Rare/Unique items
- **⚖️ Item Type Weighting**: Control the relative frequency of weapon/armor/accessory drops
- **🔗 Seamless Integration**: Works with existing ItemDatabase, AffixDatabase, and LootReward systems
- **🛠️ Runtime Generation**: Creates properly affixed items on-the-fly

## 📊 Level Range Logic

The system uses a **25-level sliding window** with a minimum floor of level 1:

| Area Level | Item Level Range | Logic |
|------------|------------------|-------|
| **9** | 1-9 | `max(1, 9-25) = 1` to `9` |
| **24** | 1-24 | `max(1, 24-25) = 1` to `24` |
| **34** | 9-34 | `max(1, 34-25) = 9` to `34` |
| **75** | 50-75 | `max(1, 75-25) = 50` to `75` |

### **Why This Works:**
- **Early Game** (levels 1-25): Players can find any item from level 1 up to their area level
- **Mid/Late Game** (25+): Players find items within a 25-level range, keeping loot relevant without being too restrictive

## 🏗️ System Components

### **1. AreaLootTable** (ScriptableObject)
Defines loot configuration for areas:

```csharp
[Header("Area Configuration")]
public int areaLevel = 1;                    // The area's level
public float baseDropChance = 0.15f;         // 15% chance for any item

[Header("Item Type Weights")]  
public ItemTypeWeight[] itemTypeWeights;     // Weapon: 30%, Armor: 40%, etc.

[Header("Rarity Distribution")]
public RarityWeight[] rarityWeights;         // Normal: 70%, Magic: 25%, etc.
```

### **2. AreaLootManager** (Singleton)
Handles loot generation and integration:

```csharp
// Generate items for an area
List<BaseItem> items = AreaLootManager.Instance.GenerateLootForArea(areaLevel, count);

// Check if item can drop in area
bool canDrop = AreaLootManager.Instance.CanItemDropInArea(item, areaLevel);

// Get level range for area
var (minLevel, maxLevel) = AreaLootManager.Instance.GetValidLevelRangeForArea(areaLevel);
```

## 🚀 How to Use

### **Step 1: Create Area Loot Table**

1. **Right-click** in Project → **Create** → **Dexiled** → **Loot** → **Area Loot Table**
2. **Configure the table**:
   ```
   Area Level: 34
   Base Drop Chance: 15%
   
   Item Type Weights:
   - Weapons: 30%
   - Armor: 40% 
   - Accessories: 20%
   - Consumables: 10%
   
   Rarity Weights:
   - Normal: 70%
   - Magic: 25%
   - Rare: 5%
   - Unique: 0.1%
   ```

### **Step 2: Set Up Area Loot Manager**

1. **Add AreaLootManager** to a scene GameObject (or let it auto-create)
2. **Assign your default loot table**
3. **Enable debug logs** for testing

### **Step 3: Generate Loot**

```csharp
// Generate loot for area level 34
List<BaseItem> loot = AreaLootManager.Instance.GenerateLootForArea(34, 3);

// Or generate a single item
BaseItem item = AreaLootManager.Instance.GenerateSingleItemForArea(34);

// The system will:
// 1. Calculate valid range: levels 9-34
// 2. Filter ItemDatabase for eligible items
// 3. Apply weighted random selection
// 4. Generate appropriate affixes using AffixDatabase
```

## 🔗 Integration Examples

### **With Existing Enemy Loot System**

```csharp
// In your combat/enemy defeat code:
public void OnEnemyDefeated(EnemyData enemy, int areaLevel)
{
    // Generate area-based loot
    List<BaseItem> areaLoot = AreaLootManager.Instance.GenerateLootForArea(areaLevel, 2);
    
    // Convert to LootRewards
    foreach (var item in areaLoot)
    {
        LootReward reward = new LootReward
        {
            rewardType = RewardType.Item,
            itemData = item
        };
        
        // Add to existing drop system
        CombatLog.Instance.AddLootDrop(enemy.enemyName, reward);
    }
}
```

### **With Chest/Container System**

```csharp
public class TreasureChest : MonoBehaviour
{
    [SerializeField] private int areaLevel = 20;
    [SerializeField] private int itemCount = 3;
    
    public void OpenChest()
    {
        List<BaseItem> treasure = AreaLootManager.Instance.GenerateLootForArea(areaLevel, itemCount);
        
        foreach (var item in treasure)
        {
            // Add to player inventory
            PlayerInventory.Instance.AddItem(item);
            
            // Show pickup notification
            ShowLootNotification(item);
        }
    }
}
```

### **With Quest Rewards**

```csharp
public class QuestReward
{
    public void GenerateQuestReward(int playerLevel)
    {
        // Generate reward slightly above player level
        int rewardLevel = playerLevel + 2;
        
        BaseItem reward = AreaLootManager.Instance.GenerateSingleItemForArea(rewardLevel);
        
        // Guarantee at least Magic quality for quest rewards
        if (reward != null && reward.rarity == ItemRarity.Normal)
        {
            AffixDatabase.Instance.GenerateRandomAffixes(reward, rewardLevel, 1.0f, 0.3f);
        }
        
        QuestManager.Instance.GiveReward(reward);
    }
}
```

## 🎛️ Configuration Examples

### **Early Game Area (Level 5)**
```
Area Level: 5
Base Drop Chance: 20%    # Higher chance for new players

Item Weights:
- Weapons: 40%           # Focus on weapons for progression  
- Armor: 35%
- Accessories: 15%
- Consumables: 10%

Rarity Weights:
- Normal: 85%            # Mostly normal items
- Magic: 15%
- Rare: 0%
- Unique: 0%
```

### **Mid Game Area (Level 40)**
```
Area Level: 40
Base Drop Chance: 15%    # Standard drop rate

Item Weights:
- Weapons: 30%           # Balanced distribution
- Armor: 40%
- Accessories: 20%
- Consumables: 10%

Rarity Weights:
- Normal: 60%            # More variety in rarity
- Magic: 30%
- Rare: 10%
- Unique: 0.1%
```

### **End Game Area (Level 80)**
```
Area Level: 80
Base Drop Chance: 12%    # Lower chance, higher quality

Item Weights:
- Weapons: 25%
- Armor: 35%
- Accessories: 30%       # More accessories for min-maxing
- Consumables: 10%

Rarity Weights:
- Normal: 40%            # Favor higher rarities
- Magic: 35%
- Rare: 24%
- Unique: 1%
```

## 💰 Currency Drops Integration

The Area Loot System now includes **full currency drop support** based on your existing loot table configurations!

### **Currency Configuration**

Each AreaLootTable includes currency drops with individual drop rates:

```csharp
[Header("Currency Drops")]
public bool enableCurrencyDrops = true;

public CurrencyDropWeight[] currencyDrops = new CurrencyDropWeight[]
{
    // Orbs (based on 1-9Loot.asset rates)
    new CurrencyDropWeight { currencyType = CurrencyType.OrbOfGeneration, dropChance = 0.15f },
    new CurrencyDropWeight { currencyType = CurrencyType.OrbOfInfusion, dropChance = 0.15f },
    new CurrencyDropWeight { currencyType = CurrencyType.OrbOfPerfection, dropChance = 0.07f },
    
    // Spirits 
    new CurrencyDropWeight { currencyType = CurrencyType.FireSpirit, dropChance = 0.10f },
    new CurrencyDropWeight { currencyType = CurrencyType.ColdSpirit, dropChance = 0.10f },
    
    // Seals
    new CurrencyDropWeight { currencyType = CurrencyType.TranspositionSeal, dropChance = 0.05f },
    // ... and 20+ more currencies!
};
```

### **Drop Rate Examples** (from your 1-9Loot.asset):

| Currency Type | Drop Chance | Rarity Class |
|---------------|-------------|--------------|
| **Orb of Generation** | 15% | Common |
| **Orb of Infusion** | 15% | Common |
| **Fire/Cold/Lightning Spirit** | 10% | Uncommon |
| **Orb of Perfection** | 7% | Uncommon |
| **Chaos Spirit** | 8% | Uncommon |
| **Divine Spirit** | 3% | Rare |
| **Orb of The Void** | 2% | Very Rare |

### **Usage Examples**

#### **Generate Items + Currencies**
```csharp
// Get both items and currencies (recommended)
List<LootReward> allLoot = AreaLootManager.Instance.GenerateLootRewardsForArea(areaLevel: 34, maxItems: 3);

foreach (var reward in allLoot)
{
    if (reward.rewardType == RewardType.Currency)
    {
        Debug.Log($"Currency drop: {reward.currencyAmount}x {reward.currencyType}");
        PlayerCurrency.AddCurrency(reward.currencyType, reward.currencyAmount);
    }
    else if (reward.rewardType == RewardType.Item)
    {
        Debug.Log($"Item drop: {reward.itemData.itemName}");
        PlayerInventory.AddItem(reward.itemData);
    }
}
```

#### **Generate Only Currencies**
```csharp
// Get only currency drops
List<LootReward> currencies = AreaLootManager.Instance.GenerateCurrencyDropsForArea(areaLevel: 50);
```

#### **Integration with Combat**
```csharp
public void OnEnemyDefeated(EnemyData enemy, int areaLevel)
{
    // Generate items + currencies automatically
    List<LootReward> drops = AreaLootManager.Instance.GenerateLootRewardsForArea(areaLevel, 2);
    
    foreach (var drop in drops)
    {
        CombatLog.Instance.AddLootDrop(enemy.enemyName, drop);
        
        // Automatically handles both items and currencies!
        if (drop.rewardType == RewardType.Currency)
        {
            ShowCurrencyPickup(drop.currencyType, drop.currencyAmount);
        }
    }
}
```

### **Area-Specific Currency Scaling**

You can create different currency configurations for different area ranges:

- **Early Game (1-20)**: High common orb rates, low rare currency rates
- **Mid Game (21-50)**: Balanced distribution, better spirits drop rates  
- **End Game (51+)**: Lower overall rates, higher rare currency chances

### **Configuration Benefits**

✅ **Uses Your Existing Rates**: Based on your proven 1-9Loot.asset distribution  
✅ **Individual Drop Chances**: Each currency has its own independent chance  
✅ **Flexible Quantities**: Configure min/max amounts per currency  
✅ **Easy Balancing**: Adjust rates per area level without code changes  
✅ **Debug Friendly**: Built-in logging shows exactly what dropped  

## 🔧 Advanced Features

### **Custom Item Filtering**

```csharp
// Get only weapons that can drop in area 50
List<BaseItem> eligibleWeapons = AreaLootManager.Instance.GetEligibleItemsForArea(50, ItemType.Weapon);

// Check if specific item can drop
bool canDrop = AreaLootManager.Instance.CanItemDropInArea(myWeapon, 50);
```

### **Loot Table Validation**

Use the **"Validate Configuration"** context menu on any AreaLootTable to:
- Check how many items are eligible for each type
- Verify rarity weights add up properly
- Preview the level range calculation

### **Debug Testing**

Use the **"Test Loot Generation"** context menu on AreaLootManager to:
- Test loot generation for areas 9, 24, 34, and 75
- See eligible item counts for each type
- Generate sample items with full debug output

## 🎯 Benefits

### **For Players**:
- **Relevant Loot**: Always find gear appropriate for your level
- **Progression Clarity**: Understand why certain items drop in certain areas  
- **Build Diversity**: Access to variety of item types and rarities

### **For Developers**:
- **Easy Configuration**: Tweak drop rates without code changes
- **Scalable**: Works for any number of areas and levels
- **Integrated**: Leverages existing item and affix systems
- **Debuggable**: Built-in testing and validation tools

---

The Area-Based Loot System ensures your players always find meaningful upgrades while maintaining the excitement of discovering rare and powerful items! 🎲⚔️
