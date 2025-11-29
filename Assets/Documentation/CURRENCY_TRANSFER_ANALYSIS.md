# Currency, Spirits, and Seals Transfer Analysis

## Summary
This document analyzes how currencies (Orbs, Spirits, Seals, Fragments) are transferred to player storage and whether they appear in EquipmentScreen's currency grid.

## Key Findings

### ✅ Currency Transfer Flow (Working Correctly)

**1. CombatScene - Dropped Currencies:**
- Currencies are dropped via `EnemyLootDropper.ApplyImmediateDrops()`
- This calls `LootManager.ApplyRewards()`
- Currencies are added to `LootManager.playerCurrencies` (Dictionary<CurrencyType, int>) via `LootManager.AddCurrency()`

**2. MazeScene - Dropped Currencies:**
- Same flow as CombatScene (uses LootManager)

**3. MazeHub - Purchased Currencies:**
- Vendor purchases should use `LootManager.AddCurrency()` or `LootManager.RemoveCurrency()`
- Need to verify vendor implementation

**Flow Diagram:**
```
Combat/Maze Drop → EnemyLootDropper → LootManager.ApplyRewards() 
→ AddCurrency() → LootManager.playerCurrencies (Dictionary<CurrencyType, int>)
```

### ❌ EquipmentScreen Display Issue (NOT Working)

**Problem:**
- `EquipmentScreen` maintains its own `playerCurrencies` list (type: `List<CurrencyData>`)
- This list is **NOT** synchronized with `LootManager.playerCurrencies` (type: `Dictionary<CurrencyType, int>`)
- EquipmentScreen only loads test currencies with random quantities (0-5) via `InitializePlayerCurrencies()`
- There is **NO** method that loads currencies from `LootManager.playerCurrencies`

**Current State:**
- EquipmentScreen's `InitializeCurrencySystem()` method calls:
  1. `InitializePlayerCurrencies()` - Creates test currencies with random quantities (NOT from LootManager)
  2. `GenerateCurrencyGrids()` - Creates visual slots
  3. `UpdateCurrencyDisplay()` - Displays currencies from its own list

**Missing Functionality:**
- No `LoadCurrenciesFromLootManager()` method
- No conversion from `Dictionary<CurrencyType, int>` to `List<CurrencyData>`
- No subscription to currency change events
- No refresh when returning to EquipmentScreen

## Code References

### Currency Transfer (Working)
```108:110:Assets/Scripts/LootSystem/LootManager.cs
case RewardType.Currency:
    AddCurrency(reward.currencyType, reward.currencyAmount);
    break;
```

```314:323:Assets/Scripts/LootSystem/LootManager.cs
public void AddCurrency(CurrencyType currencyType, int amount)
{
    if (!playerCurrencies.ContainsKey(currencyType))
    {
        playerCurrencies[currencyType] = 0;
    }
    
    playerCurrencies[currencyType] += amount;
    Debug.Log($"[LootManager] Added {amount}x {currencyType}. Total: {playerCurrencies[currencyType]}");
}
```

```34:34:Assets/Scripts/LootSystem/LootManager.cs
private Dictionary<CurrencyType, int> playerCurrencies = new Dictionary<CurrencyType, int>();
```

### EquipmentScreen Issue (Not Loading from LootManager)
```50:50:Assets/Scripts/UI/EquipmentScreen/EquipmentScreen.cs
private List<CurrencyData> playerCurrencies = new List<CurrencyData>();
```

```2001:2019:Assets/Scripts/UI/EquipmentScreen/EquipmentScreen.cs
private void InitializeCurrencySystem()
{
    // Load or create currency database
    currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
    if (currencyDatabase == null)
    {
        currencyDatabase = ScriptableObject.CreateInstance<CurrencyDatabase>();
        currencyDatabase.InitializeDefaultCurrencies();
    }
    
    // Initialize player currencies with some test currencies
    InitializePlayerCurrencies();
    
    // Generate currency grids for all tabs
    GenerateCurrencyGrids();
    
    // Update currency display
    UpdateCurrencyDisplay();
}
```

```2021:2046:Assets/Scripts/UI/EquipmentScreen/EquipmentScreen.cs
private void InitializePlayerCurrencies()
{
    playerCurrencies.Clear();
    
    // Add some test currencies
    foreach (var currency in currencyDatabase.currencies)
    {
        CurrencyData playerCurrency = new CurrencyData
        {
            currencyType = currency.currencyType,
            currencyName = currency.currencyName,
            description = currency.description,
            rarity = currency.rarity,
            currencySprite = currency.currencySprite,
            quantity = UnityEngine.Random.Range(0, 5), // Random quantity for testing
            canTargetCards = currency.canTargetCards,
            canTargetEquipment = currency.canTargetEquipment,
            validEquipmentRarities = currency.validEquipmentRarities,
            maxAffixesForTarget = currency.maxAffixesForTarget,
            isCorruption = currency.isCorruption,
            preservesLockedAffixes = currency.preservesLockedAffixes
        };
        
        playerCurrencies.Add(playerCurrency);
    }
}
```

```2406:2414:Assets/Scripts/UI/EquipmentScreen/EquipmentScreen.cs
private CurrencyData GetPlayerCurrencyForSlot(int slotIndex, string tabName)
{
    // Get the base currency for this slot
    CurrencyData baseCurrency = currencyDatabase.GetCurrencyBySlotIndex(slotIndex, tabName);
    if (baseCurrency == null) return null;
    
    // Find the player's currency with the same type
    return playerCurrencies.Find(c => c.currencyType == baseCurrency.currencyType);
}
```

## Currency Types

### Orbs (9 types)
- OrbOfGeneration
- OrbOfInfusion
- OrbOfPerfection
- OrbOfPerpetuity
- OrbOfRedundancy
- OrbOfTheVoid
- OrbOfMutation
- OrbOfProliferation
- OrbOfAmnesia

### Spirits (9 types)
- FireSpirit
- ColdSpirit
- LightningSpirit
- ChaosSpirit
- PhysicalSpirit
- LifeSpirit
- DefenseSpirit
- CritSpirit
- DivineSpirit

### Seals (7 types)
- TranspositionSeal
- ChaosSeal
- MemorySeal
- InscriptionSeal
- AdaptationSeal
- CorrectionSeal
- EtchingSeal

### Fragments (3 types)
- Fragment1
- Fragment2
- Fragment3

### Maze-Specific Currencies (4 types)
- MandateFragment
- ShatteredSigil
- ContradictionCore
- CollapseMotif

## Recommendations

### To Fix EquipmentScreen Currency Display:

1. **Add LoadCurrenciesFromLootManager() method:**
   - Get all currencies from `LootManager.Instance.GetAllCurrencies()`
   - Convert `Dictionary<CurrencyType, int>` to `List<CurrencyData>`
   - Match quantities from LootManager to CurrencyData entries
   - Call this in `InitializeCurrencySystem()` or `OnEnable()`

2. **Create Currency Sync Method:**
   - Convert LootManager's dictionary format to EquipmentScreen's list format
   - Preserve all currency metadata (name, description, sprite, etc.) from CurrencyDatabase
   - Update quantities from LootManager

3. **Add Currency Refresh:**
   - Refresh currency display when EquipmentScreen is opened
   - Optionally subscribe to currency change events (if implemented)

4. **Handle Currency Updates:**
   - When currencies are added/removed in LootManager, update EquipmentScreen display
   - Consider adding events to LootManager for currency changes

## Data Structure Comparison

### LootManager Storage:
```csharp
Dictionary<CurrencyType, int> playerCurrencies
// Key: CurrencyType enum
// Value: Quantity (int)
```

### EquipmentScreen Storage:
```csharp
List<CurrencyData> playerCurrencies
// CurrencyData contains:
// - currencyType (CurrencyType)
// - currencyName (string)
// - description (string)
// - rarity (ItemRarity)
// - currencySprite (Sprite)
// - quantity (int) ← This needs to sync with LootManager
// - canTargetCards (bool)
// - canTargetEquipment (bool)
// - validEquipmentRarities (ItemRarity[])
// - maxAffixesForTarget (int)
// - isCorruption (bool)
// - preservesLockedAffixes (bool)
```

## Conclusion

**Currency Transfer:** ✅ Working correctly - currencies are added to `LootManager.playerCurrencies`

**EquipmentScreen Display:** ❌ **NOT working** - EquipmentScreen does not load currencies from LootManager, only displays test currencies with random quantities

**Action Required:** Implement currency loading in EquipmentScreen to sync with LootManager's currency storage.

## Additional Notes

- LootManager's `SavePlayerCurrencies()` and `LoadPlayerCurrencies()` methods have TODO comments indicating persistence is not yet implemented
- EquipmentScreen displays currencies in tabs (Orbs, Spirits, Seals, Fragments) with 3x3 grids (9 slots per tab)
- Currency quantities are displayed as labels on currency slots when > 0
- Currency sprites are extracted from texture atlases in `UpdateCurrencyDisplay()`

