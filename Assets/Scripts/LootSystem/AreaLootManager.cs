using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Manages area-based loot generation and integrates with existing loot systems
/// </summary>
public class AreaLootManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Default loot table to use if no specific table is found")]
    public AreaLootTable defaultLootTable;
    
    [Header("Name Generation")]
    [Tooltip("Data for generating Magic and Rare item names")]
    public NameGenerationData nameGenerationData;
    
    [Header("Area-Specific Loot Tables")]
    [Tooltip("Loot tables configured for specific area level ranges")]
    public List<AreaLootTableRange> areaLootTableRanges = new List<AreaLootTableRange>();

    [Tooltip("Convenience bindings for story acts. Leave lootTable empty to skip an act.")]
    public List<ActLootTableConfig> actLootTables = new List<ActLootTableConfig>
    {
        new ActLootTableConfig { actName = "Act 1", minAreaLevel = 1,  maxAreaLevel = 20 },
        new ActLootTableConfig { actName = "Act 2", minAreaLevel = 21, maxAreaLevel = 40 },
        new ActLootTableConfig { actName = "Act 3", minAreaLevel = 41, maxAreaLevel = 60 }
    };
    
    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = false;
    
    // Cache for quick lookup
    private Dictionary<int, AreaLootTable> lootTableCache = new Dictionary<int, AreaLootTable>();
    
    // Singleton pattern
    private static AreaLootManager _instance;
    public static AreaLootManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AreaLootManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AreaLootManager");
                    _instance = go.AddComponent<AreaLootManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        
        // Build cache on awake
        BuildLootTableCache();
    }
    
    /// <summary>
    /// Build cache for quick area level to loot table lookup
    /// </summary>
    private void BuildLootTableCache()
    {
        lootTableCache.Clear();
        
        foreach (var range in areaLootTableRanges)
        {
            if (range.lootTable == null)
            {
                if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] Loot table range has null loot table (min: {range.minAreaLevel}, max: {range.maxAreaLevel})");
                continue;
            }
            
            // Cache all levels in this range
            for (int level = range.minAreaLevel; level <= range.maxAreaLevel; level++)
            {
                if (lootTableCache.ContainsKey(level))
                {
                    if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] Area level {level} is covered by multiple loot tables! Using most recently added.");
                }
                lootTableCache[level] = range.lootTable;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"[AreaLootManager] Cached loot table '{range.lootTable.name}' for area levels {range.minAreaLevel}-{range.maxAreaLevel}");
            }
        }

        // Register act bindings (evaluated after manual ranges so they can fill gaps)
        foreach (var actConfig in actLootTables)
        {
            if (actConfig == null || actConfig.lootTable == null)
                continue;

            for (int level = actConfig.minAreaLevel; level <= actConfig.maxAreaLevel; level++)
            {
                lootTableCache[level] = actConfig.lootTable;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[AreaLootManager] Cached act loot table '{actConfig.actName}' ({actConfig.lootTable.name}) for area levels {actConfig.minAreaLevel}-{actConfig.maxAreaLevel}");
            }
        }
    }
    
    /// <summary>
    /// Generate loot for a specific area level
    /// </summary>
    public List<BaseItem> GenerateLootForArea(int areaLevel, int itemCount = 1)
    {
        AreaLootTable lootTable = GetLootTableForArea(areaLevel);
        
        if (lootTable == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] No loot table found for area level {areaLevel}");
            return new List<BaseItem>();
        }
        
        return lootTable.GenerateMultipleItems(itemCount);
    }
    
    /// <summary>
    /// Generate a single item for a specific area level
    /// </summary>
    public BaseItem GenerateSingleItemForArea(int areaLevel)
    {
        AreaLootTable lootTable = GetLootTableForArea(areaLevel);
        
        if (lootTable == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] No loot table found for area level {areaLevel}");
            return null;
        }
        
        return lootTable.GenerateRandomItem();
    }
    
    /// <summary>
    /// Generate a single item for a specific area level with forced rarity (for testing)
    /// </summary>
    public BaseItem GenerateSingleItemForArea(int areaLevel, ItemRarity forcedRarity)
    {
        AreaLootTable lootTable = GetLootTableForArea(areaLevel);
        
        if (lootTable == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] No loot table found for area level {areaLevel}");
            return null;
        }
        
        return lootTable.GenerateRandomItem(forcedRarity);
    }
    
    /// <summary>
    /// Get valid item level range for an area
    /// </summary>
    public (int minLevel, int maxLevel) GetValidLevelRangeForArea(int areaLevel)
    {
        int minLevel = Mathf.Max(1, areaLevel - 25);
        int maxLevel = areaLevel;
        return (minLevel, maxLevel);
    }
    
    /// <summary>
    /// Check if an item can drop in a specific area
    /// </summary>
    public bool CanItemDropInArea(BaseItem item, int areaLevel)
    {
        if (item == null) return false;
        
        var (minLevel, maxLevel) = GetValidLevelRangeForArea(areaLevel);
        return item.requiredLevel >= minLevel && item.requiredLevel <= maxLevel;
    }
    
    /// <summary>
    /// Get all items that can drop in a specific area
    /// </summary>
    public List<BaseItem> GetEligibleItemsForArea(int areaLevel, ItemType? filterType = null)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[AreaLootManager] ItemDatabase.Instance is null!");
            return new List<BaseItem>();
        }
        
        var (minLevel, maxLevel) = GetValidLevelRangeForArea(areaLevel);
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        List<BaseItem> eligibleItems = new List<BaseItem>();
        
        foreach (var item in allItems)
        {
            if (item.requiredLevel >= minLevel && item.requiredLevel <= maxLevel)
            {
                if (filterType == null || item.itemType == filterType)
                {
                    if (!item.isUnique) // Exclude uniques from general drops
                    {
                        eligibleItems.Add(item);
                    }
                }
            }
        }
        
        return eligibleItems;
    }
    
    /// <summary>
    /// Generate loot rewards compatible with existing LootReward system (items + currencies)
    /// </summary>
    public List<LootReward> GenerateLootRewardsForArea(int areaLevel, int maxItems = 3)
    {
        AreaLootTable lootTable = GetLootTableForArea(areaLevel);
        
        if (lootTable == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] No loot table found for area level {areaLevel}");
            return new List<LootReward>();
        }
        
        // Use the loot table's comprehensive generation method
        List<LootReward> allRewards = lootTable.GenerateAllLoot(areaLevel, maxItems);
        
        if (enableDebugLogs && allRewards.Count > 0)
        {
            int itemCount = allRewards.Count(r => r.rewardType == RewardType.Item);
            int currencyCount = allRewards.Count(r => r.rewardType == RewardType.Currency);
            Debug.Log($"[AreaLootManager] Generated {allRewards.Count} total rewards for area level {areaLevel} ({itemCount} items, {currencyCount} currencies)");
        }
        
        return allRewards;
    }
    
    /// <summary>
    /// Generate only currency drops for an area
    /// </summary>
    public List<LootReward> GenerateCurrencyDropsForArea(int areaLevel)
    {
        AreaLootTable lootTable = GetLootTableForArea(areaLevel);
        
        if (lootTable == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"[AreaLootManager] No loot table found for area level {areaLevel}");
            return new List<LootReward>();
        }
        
        return lootTable.GenerateCurrencyDrops();
    }
    
    /// <summary>
    /// Integration with existing EnemyLootDropper system
    /// </summary>
    public void AddAreaLootToEnemyDrops(EnemyData enemyData, int areaLevel, List<LootReward> existingRewards)
    {
        // Generate additional area-based loot
        List<LootReward> areaRewards = GenerateLootRewardsForArea(areaLevel, 1);
        
        if (areaRewards.Count > 0)
        {
            existingRewards.AddRange(areaRewards);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[AreaLootManager] Added {areaRewards.Count} area-based drops to {enemyData.enemyName}");
            }
        }
    }
    
    public AreaLootTable GetLootTableForArea(int areaLevel)
    {
        AreaLootTable sourceTable = null;
        string sourceDescription = "";
        
        // First, try to find a specific loot table for this area level
        if (lootTableCache.TryGetValue(areaLevel, out AreaLootTable cachedTable))
        {
            sourceTable = cachedTable;
            sourceDescription = $"cached table '{cachedTable.name}'";
        }
        else
        {
            // Fallback: Try to find a matching range (in case cache wasn't built)
            foreach (var range in areaLootTableRanges)
            {
                if (range.lootTable != null && areaLevel >= range.minAreaLevel && areaLevel <= range.maxAreaLevel)
                {
                    sourceTable = range.lootTable;
                    sourceDescription = $"range table '{range.lootTable.name}' (levels {range.minAreaLevel}-{range.maxAreaLevel})";
                    break;
                }
            }
            
            // Last resort: use default table
            if (sourceTable == null && defaultLootTable != null)
            {
                sourceTable = defaultLootTable;
                sourceDescription = $"default table '{defaultLootTable.name}'";
            }
        }
        
        if (sourceTable != null)
        {
            // Log which loot table is being used (only if debug logs are enabled)
            if (enableDebugLogs)
            {
                Debug.Log($"[AreaLootManager] Area Level {areaLevel}: Using {sourceDescription}");
            }
            
            // Create a runtime copy with the correct area level
            return CreateRuntimeLootTable(sourceTable, areaLevel);
        }
        
        Debug.LogWarning($"[AreaLootManager] No loot table found for area level {areaLevel}");
        return null;
    }
    
    private AreaLootTable CreateRuntimeLootTable(AreaLootTable sourceTable, int areaLevel)
    {
        if (sourceTable == null) return null;
        
        // Create a runtime copy
        AreaLootTable runtimeTable = ScriptableObject.CreateInstance<AreaLootTable>();
        
        // Copy properties from source table
        runtimeTable.areaLevel = areaLevel; // Set to the actual area level being used
        runtimeTable.baseDropChance = sourceTable.baseDropChance;
        
        // Copy item type weights
        runtimeTable.itemTypeWeights = new ItemTypeWeight[sourceTable.itemTypeWeights.Length];
        for (int i = 0; i < sourceTable.itemTypeWeights.Length; i++)
        {
            runtimeTable.itemTypeWeights[i] = new ItemTypeWeight
            {
                itemType = sourceTable.itemTypeWeights[i].itemType,
                weight = sourceTable.itemTypeWeights[i].weight
            };
        }
        
        // Copy rarity weights
        runtimeTable.rarityWeights = new RarityWeight[sourceTable.rarityWeights.Length];
        for (int i = 0; i < sourceTable.rarityWeights.Length; i++)
        {
            runtimeTable.rarityWeights[i] = new RarityWeight
            {
                rarity = sourceTable.rarityWeights[i].rarity,
                weight = sourceTable.rarityWeights[i].weight
            };
        }
        
        // Copy currency drops
        runtimeTable.enableCurrencyDrops = sourceTable.enableCurrencyDrops;
        runtimeTable.currencyDrops = new CurrencyDropWeight[sourceTable.currencyDrops.Length];
        for (int i = 0; i < sourceTable.currencyDrops.Length; i++)
        {
            runtimeTable.currencyDrops[i] = new CurrencyDropWeight
            {
                currencyType = sourceTable.currencyDrops[i].currencyType,
                dropChance = sourceTable.currencyDrops[i].dropChance,
                minQuantity = sourceTable.currencyDrops[i].minQuantity,
                maxQuantity = sourceTable.currencyDrops[i].maxQuantity
            };
        }
        
        runtimeTable.enableDebugLogs = sourceTable.enableDebugLogs || enableDebugLogs;
        
        return runtimeTable;
    }
    
    /// <summary>
    /// Rebuild the loot table cache (useful if loot tables are changed at runtime)
    /// </summary>
    [ContextMenu("Rebuild Loot Table Cache")]
    public void RebuildLootTableCache()
    {
        BuildLootTableCache();
        Debug.Log("[AreaLootManager] Loot table cache rebuilt!");
    }
    
    /// <summary>
    /// Auto-load all AreaLootTable assets from Resources and configure default ranges
    /// This will overwrite existing range configurations!
    /// </summary>
    [ContextMenu("Auto-Load Loot Tables from Resources")]
    public void AutoLoadLootTablesFromResources()
    {
        areaLootTableRanges.Clear();
        
        // Load all AreaLootTable assets from Resources/LootTables
        AreaLootTable[] allTables = Resources.LoadAll<AreaLootTable>("LootTables");
        
        if (allTables.Length == 0)
        {
            Debug.LogWarning("[AreaLootManager] No AreaLootTable assets found in Resources/LootTables!");
            return;
        }
        
        // Sort by area level
        System.Array.Sort(allTables, (a, b) => a.areaLevel.CompareTo(b.areaLevel));
        
        // Configure ranges: each table covers from previous max+1 to its areaLevel
        int previousMax = 0;
        
        foreach (var table in allTables)
        {
            int minLevel = previousMax + 1;
            int maxLevel = table.areaLevel;
            
            // If this would create an invalid range, use the table's areaLevel as both min and max
            if (minLevel > maxLevel)
            {
                minLevel = table.areaLevel;
                maxLevel = table.areaLevel;
            }
            
            areaLootTableRanges.Add(new AreaLootTableRange
            {
                lootTable = table,
                minAreaLevel = minLevel,
                maxAreaLevel = maxLevel
            });
            
            Debug.Log($"[AreaLootManager] Auto-configured '{table.name}': Area Levels {minLevel}-{maxLevel}");
            previousMax = maxLevel;
        }
        
        // Rebuild cache
        BuildLootTableCache();
        Debug.Log($"[AreaLootManager] Auto-loaded {areaLootTableRanges.Count} loot tables!");
    }
    
    /// <summary>
    /// Display current loot table configuration in console
    /// </summary>
    [ContextMenu("Display Loot Table Configuration")]
    public void DisplayLootTableConfiguration()
    {
        Debug.Log("=== AreaLootManager Configuration ===");
        
        if (areaLootTableRanges.Count == 0)
        {
            Debug.Log("No area-specific loot tables configured.");
        }
        else
        {
            Debug.Log($"Configured {areaLootTableRanges.Count} loot table ranges:");
            foreach (var range in areaLootTableRanges)
            {
                string tableName = range.lootTable != null ? range.lootTable.name : "NULL";
                Debug.Log($"  Area {range.minAreaLevel}-{range.maxAreaLevel}: {tableName}");
            }
        }
        
        Debug.Log($"Cache contains {lootTableCache.Count} cached level mappings");
        
        if (defaultLootTable != null)
        {
            Debug.Log($"Default loot table: {defaultLootTable.name}");
        }
        else
        {
            Debug.LogWarning("No default loot table configured!");
        }
    }
    
    /// <summary>
    /// Verify and log which loot table will be used for a given area level
    /// Useful for debugging encounter loot configuration
    /// </summary>
    public void VerifyLootTableForArea(int areaLevel, string context = "")
    {
        Debug.Log($"=== Loot Table Verification for Area Level {areaLevel} ===");
        if (!string.IsNullOrEmpty(context))
        {
            Debug.Log($"Context: {context}");
        }
        
        AreaLootTable lootTable = GetLootTableForArea(areaLevel);
        
        if (lootTable == null)
        {
            Debug.LogError($"❌ No loot table found for area level {areaLevel}!");
            Debug.LogError("   Make sure you have configured loot table ranges or a default table.");
            return;
        }
        
        Debug.Log($"✅ Loot table loaded: Runtime table (configured for area level {lootTable.areaLevel})");
        
        // Check if we can find the source table
        if (lootTableCache.TryGetValue(areaLevel, out AreaLootTable sourceTable))
        {
            Debug.Log($"   Source table: {sourceTable.name}");
            var range = areaLootTableRanges.FirstOrDefault(r => r.lootTable == sourceTable);
            if (range != null)
            {
                Debug.Log($"   Covers area levels: {range.minAreaLevel}-{range.maxAreaLevel}");
            }
        }
        else if (defaultLootTable != null)
        {
            Debug.Log($"   Using default table: {defaultLootTable.name}");
        }
        
        // Show loot table configuration
        var (minLevel, maxLevel) = lootTable.GetValidLevelRange();
        Debug.Log($"   Item level range: {minLevel}-{maxLevel}");
        Debug.Log($"   Base drop chance: {lootTable.baseDropChance:P0}");
        Debug.Log($"   Currency drops enabled: {lootTable.enableCurrencyDrops}");
        
        // Test generation capability
        List<BaseItem> testItems = GetEligibleItemsForArea(areaLevel);
        Debug.Log($"   Eligible items in database: {testItems.Count}");
        
        Debug.Log("==========================================");
    }
    
    /// <summary>
    /// Debug method to test loot generation for different area levels
    /// </summary>
    [ContextMenu("Test Loot Generation")]
    public void TestLootGeneration()
    {
        int[] testAreas = { 9, 24, 34, 75 };
        
        foreach (int areaLevel in testAreas)
        {
            Debug.Log($"\n=== TESTING AREA LEVEL {areaLevel} ===");
            var (minLevel, maxLevel) = GetValidLevelRangeForArea(areaLevel);
            Debug.Log($"Valid item level range: {minLevel}-{maxLevel}");
            
            // Test each item type
            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                List<BaseItem> eligible = GetEligibleItemsForArea(areaLevel, itemType);
                Debug.Log($"  {itemType}: {eligible.Count} eligible items");
            }
            
            // Generate some test items
            List<BaseItem> testItems = GenerateLootForArea(areaLevel, 5);
            Debug.Log($"Generated {testItems.Count} test items:");
            foreach (var item in testItems)
            {
                if (item != null)
                {
                    Debug.Log($"  - {item.GetDisplayName()} (Level {item.requiredLevel})");
                }
            }
        }
    }
    
    /// <summary>
    /// Converts a BaseItem (ScriptableObject) to ItemData (serializable data structure)
    /// </summary>
    private ItemData ConvertBaseItemToItemData(BaseItem baseItem)
    {
        if (baseItem == null) return null;
        
        ItemData itemData = new ItemData
        {
            itemName = baseItem.itemName,
            itemType = baseItem.itemType,
            equipmentType = baseItem.equipmentType,
            rarity = baseItem.GetCalculatedRarity(),
            level = baseItem.requiredLevel,
            itemSprite = baseItem.itemIcon,
            requiredLevel = baseItem.requiredLevel
        };
        
        // Convert weapon-specific properties
        if (baseItem is WeaponItem weapon)
        {
            itemData.baseDamageMin = weapon.minDamage;
            itemData.baseDamageMax = weapon.maxDamage;
            itemData.criticalStrikeChance = weapon.criticalStrikeChance;
            itemData.attackSpeed = weapon.attackSpeed;
            itemData.requiredStrength = weapon.requiredStrength;
            itemData.requiredDexterity = weapon.requiredDexterity;
            itemData.requiredIntelligence = weapon.requiredIntelligence;
        }
        
        // Convert armor-specific properties
        if (baseItem is Armour armor)
        {
            itemData.baseArmour = armor.armour;
            itemData.baseEvasion = armor.evasion;
            itemData.baseEnergyShield = armor.energyShield;
            itemData.requiredStrength = armor.requiredStrength;
            itemData.requiredDexterity = armor.requiredDexterity;
            itemData.requiredIntelligence = armor.requiredIntelligence;
        }
        
        // Convert modifiers to string format
        foreach (Affix affix in baseItem.implicitModifiers)
        {
            itemData.implicitModifiers.Add(affix.description);
        }
        
        foreach (Affix affix in baseItem.prefixes)
        {
            itemData.prefixModifiers.Add(affix.description);
        }
        
        foreach (Affix affix in baseItem.suffixes)
        {
            itemData.suffixModifiers.Add(affix.description);
        }
        
        return itemData;
    }
}

/// <summary>
/// Defines a range of area levels that use a specific loot table
/// </summary>
[System.Serializable]
public class AreaLootTableRange
{
    [Tooltip("The loot table asset to use for this range")]
    public AreaLootTable lootTable;
    
    [Tooltip("Minimum area level (inclusive) for this loot table")]
    public int minAreaLevel = 1;
    
    [Tooltip("Maximum area level (inclusive) for this loot table")]
    public int maxAreaLevel = 10;
    
    /// <summary>
    /// Check if a given area level falls within this range
    /// </summary>
    public bool ContainsLevel(int areaLevel)
    {
        return areaLevel >= minAreaLevel && areaLevel <= maxAreaLevel;
    }
}

/// <summary>
/// Convenience binding for story acts to quickly assign loot tables per act.
/// </summary>
[System.Serializable]
public class ActLootTableConfig
{
    public string actName = "Act 1";
    public int minAreaLevel = 1;
    public int maxAreaLevel = 20;
    public AreaLootTable lootTable;
}
