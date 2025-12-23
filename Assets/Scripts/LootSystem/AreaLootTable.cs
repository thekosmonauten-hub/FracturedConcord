using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Area-based loot table system that generates items based on area level requirements
/// 
/// Level Logic:
/// - Area level 9: drops items level 1-9
/// - Area level 24: drops items level 1-24  
/// - Area level 34: drops items level 9-34 (area_level - 25)
/// - Area level 75: drops items level 50-75 (area_level - 25)
/// </summary>
[CreateAssetMenu(fileName = "New Area Loot Table", menuName = "Dexiled/Loot/Area Loot Table")]
public class AreaLootTable : ScriptableObject
{
    [Header("Area Configuration")]
    [Tooltip("The level of this area (determines item level ranges)")]
    public int areaLevel = 1;
    
    [Header("Drop Rates")]
    [Tooltip("Chance for any item to drop (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float baseDropChance = 0.80f; // Increased from 0.45f to 0.80f (80%) for more frequent drops
    
    [Header("Item Type Weights")]
    [Tooltip("Relative chance for each item type to drop. Effigy weight controls overall effigy drop rate - all effigies in effigyDrops share this weight equally.")]
    public ItemTypeWeight[] itemTypeWeights = new ItemTypeWeight[]
    {
        new ItemTypeWeight { itemType = ItemType.Weapon, weight = 30f },
        new ItemTypeWeight { itemType = ItemType.Armour, weight = 40f },
        new ItemTypeWeight { itemType = ItemType.Accessory, weight = 20f },
        new ItemTypeWeight { itemType = ItemType.Consumable, weight = 10f },
        new ItemTypeWeight { itemType = ItemType.Effigy, weight = 20f },  // Increased from 5f to 20f for more frequent effigy drops
        new ItemTypeWeight { itemType = ItemType.Warrant, weight = 20f }  // Increased from 5f to 20f for more frequent warrant drops
    };
    
    [Header("Rarity Distribution")]
    [Tooltip("Rarity chances (should add up to ~1.0)")]
    public RarityWeight[] rarityWeights = new RarityWeight[]
    {
        new RarityWeight { rarity = ItemRarity.Normal, weight = 0.65f },  // Tuned: Slightly reduced
        new RarityWeight { rarity = ItemRarity.Magic, weight = 0.28f },   // Tuned: Slightly increased
        new RarityWeight { rarity = ItemRarity.Rare, weight = 0.065f },   // Tuned: Slightly increased
        new RarityWeight { rarity = ItemRarity.Unique, weight = 0.002f }  // Tuned: Doubled for better excitement
    };
    
    [Header("Currency Drops")]
    [Tooltip("Enable currency drops alongside item drops")]
    public bool enableCurrencyDrops = true;
    
    [Tooltip("Currency drop configuration with individual drop chances")]
    public CurrencyDropWeight[] currencyDrops = new CurrencyDropWeight[]
    {
        // Currency drop rates (TUNED for better balance)
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfGeneration, dropChance = 0.05f, minQuantity = 1, maxQuantity = 1 },  // Tuned: Slightly increased, can drop 1-2
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfInfusion, dropChance = 0.045f, minQuantity = 1, maxQuantity = 2 },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfPerfection, dropChance = 0.025f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfPerpetuity, dropChance = 0.02f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfRedundancy, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfTheVoid, dropChance = 0.008f, minQuantity = 1, maxQuantity = 1 },  // Tuned: Slightly increased
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfMutation, dropChance = 0.02f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfProliferation, dropChance = 0.035f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfAmnesia, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },
        
        // Spirits (TUNED for better balance)
        new CurrencyDropWeight { currencyType = CurrencyType.FireSpirit, dropChance = 0.03f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.ColdSpirit, dropChance = 0.03f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.LightningSpirit, dropChance = 0.03f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.ChaosSpirit, dropChance = 0.025f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.PhysicalSpirit, dropChance = 0.03f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.LifeSpirit, dropChance = 0.03f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.DefenseSpirit, dropChance = 0.03f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.CritSpirit, dropChance = 0.025f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.DivineSpirit, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },  // Tuned: Slightly increased
        
        // Seals (TUNED for better balance)
        new CurrencyDropWeight { currencyType = CurrencyType.TranspositionSeal, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.ChaosSeal, dropChance = 0.02f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.MemorySeal, dropChance = 0.012f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.InscriptionSeal, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.AdaptationSeal, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.CorrectionSeal, dropChance = 0.015f, minQuantity = 1, maxQuantity = 1 },
        new CurrencyDropWeight { currencyType = CurrencyType.EtchingSeal, dropChance = 0.02f, minQuantity = 1, maxQuantity = 1 }
    };

    [Header("Effigy Drops")]
    [Tooltip("Optional effigy drops evaluated against area level")]
    public EffigyDropWeight[] effigyDrops = new EffigyDropWeight[0];

    [Header("Warrant Drops")]
    [Tooltip("Optional warrant drops evaluated against area level")]
    public WarrantDropWeight[] warrantDrops = new WarrantDropWeight[0];

    [Header("Name Generation")]
    [Tooltip("Data for generating Magic and Rare item names")]
    public NameGenerationData nameGenerationData;
    
    [Header("Debug")]
    [Tooltip("Show debug logs when generating loot")]
    public bool enableDebugLogs = false;
    
    /// <summary>
    /// Calculate the valid item level range for this area
    /// </summary>
    public (int minLevel, int maxLevel) GetValidLevelRange()
    {
        int minLevel = Mathf.Max(1, areaLevel - 25);
        int maxLevel = areaLevel;
        return (minLevel, maxLevel);
    }
    
    /// <summary>
    /// Generate a single random item for this area
    /// </summary>
    public BaseItem GenerateRandomItem()
    {
        if (Random.Range(0f, 1f) > baseDropChance)
        {
            if (enableDebugLogs) Debug.Log($"[AreaLoot] No drop (failed base chance {baseDropChance:P0})");
            return null;
        }
        
        // Get valid level range
        var (minLevel, maxLevel) = GetValidLevelRange();
        
        // Select item type based on weights
        ItemType selectedType = SelectRandomItemType();
        if (enableDebugLogs) Debug.Log($"[AreaLoot] Selected type: {selectedType}");
        
        // Get eligible items from database
        List<BaseItem> eligibleItems = GetEligibleItems(selectedType, minLevel, maxLevel);
        
        if (eligibleItems.Count == 0)
        {
            if (enableDebugLogs) Debug.Log($"[AreaLoot] No eligible {selectedType} items for levels {minLevel}-{maxLevel}");
            return null;
        }
        
        // Select random item
        BaseItem baseItem = eligibleItems[Random.Range(0, eligibleItems.Count)];
        
        // Create a copy and apply random affixes
        BaseItem generatedItem = CreateItemCopy(baseItem);
        ApplyRandomAffixes(generatedItem);
        
        if (enableDebugLogs) 
        {
            Debug.Log($"[AreaLoot] Generated: {generatedItem.GetDisplayName()} (Level {generatedItem.requiredLevel})");
        }
        
        return generatedItem;
    }
    
    /// <summary>
    /// Generate a single random item for this area with forced rarity (for testing)
    /// </summary>
    public BaseItem GenerateRandomItem(ItemRarity forcedRarity)
    {
        // Get valid level range
        var (minLevel, maxLevel) = GetValidLevelRange();
        
        // Select item type based on weights
        ItemType selectedType = SelectRandomItemType();
        if (enableDebugLogs) Debug.Log($"[AreaLoot] Selected type: {selectedType} with forced rarity: {forcedRarity}");
        
        // Get eligible items from database
        List<BaseItem> eligibleItems = GetEligibleItems(selectedType, minLevel, maxLevel);
        
        if (eligibleItems.Count == 0)
        {
            if (enableDebugLogs) Debug.Log($"[AreaLoot] No eligible {selectedType} items for levels {minLevel}-{maxLevel}");
            return null;
        }
        
        // Select random item
        BaseItem baseItem = eligibleItems[Random.Range(0, eligibleItems.Count)];
        
        if (baseItem == null)
        {
            Debug.LogError("[AreaLoot] baseItem is null after selection from eligible items!");
            return null;
        }
        
        // Create a copy and apply affixes with forced rarity
        BaseItem generatedItem = CreateItemCopy(baseItem);
        
        if (generatedItem == null)
        {
            Debug.LogError($"[AreaLoot] Failed to create copy of {baseItem.itemName} (itemType: {baseItem.itemType})");
            return null;
        }
        
        ApplyRandomAffixes(generatedItem, forcedRarity);
        
        if (enableDebugLogs) 
        {
            Debug.Log($"[AreaLoot] Generated: {generatedItem.GetDisplayName()} (Level {generatedItem.requiredLevel}) [Forced {forcedRarity}]");
        }
        
        return generatedItem;
    }
    
    /// <summary>
    /// Generate multiple items for this area
    /// </summary>
    public List<BaseItem> GenerateMultipleItems(int count)
    {
        List<BaseItem> items = new List<BaseItem>();
        
        for (int i = 0; i < count; i++)
        {
            BaseItem item = GenerateRandomItem();
            if (item != null)
            {
                items.Add(item);
            }
        }
        
        return items;
    }
    
    private ItemType SelectRandomItemType()
    {
        float totalWeight = itemTypeWeights.Sum(w => w.weight);
        float random = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var weight in itemTypeWeights)
        {
            currentWeight += weight.weight;
            if (random <= currentWeight)
            {
                return weight.itemType;
            }
        }
        
        // Fallback
        return ItemType.Weapon;
    }
    
    private List<BaseItem> GetEligibleItems(ItemType itemType, int minLevel, int maxLevel)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[AreaLoot] ItemDatabase.Instance is null!");
            return new List<BaseItem>();
        }
        
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        
        // Filter items, skipping null entries (defensive programming)
        return allItems.Where(item => 
            item != null && // Null check to prevent NullReferenceException
            item.itemType == itemType && 
            item.requiredLevel >= minLevel && 
            item.requiredLevel <= maxLevel &&
            !item.isUnique // Don't drop uniques randomly (handle separately)
        ).ToList();
    }
    
    private BaseItem CreateItemCopy(BaseItem original)
    {
        // Create a runtime copy of the item
        BaseItem copy;
        
        switch (original.itemType)
        {
            case ItemType.Weapon:
                copy = CreateWeaponCopy(original as WeaponItem);
                break;
            case ItemType.Armour:
                copy = CreateArmourCopy(original as Armour);
                break;
            case ItemType.Accessory:
                copy = CreateAccessoryCopy(original as Jewellery);
                break;
            case ItemType.Consumable:
                copy = CreateConsumableCopy(original as Consumable);
                break;
            default:
                // Generic copy
                copy = ScriptableObject.CreateInstance<BaseItem>();
                CopyBaseProperties(original, copy);
                break;
        }
        
        return copy;
    }
    
    private WeaponItem CreateWeaponCopy(WeaponItem original)
    {
        if (original == null) return null;
        
        WeaponItem copy = ScriptableObject.CreateInstance<WeaponItem>();
        CopyBaseProperties(original, copy);
        
        // Copy weapon-specific properties
        copy.weaponType = original.weaponType;
        copy.handedness = original.handedness;
        copy.minDamage = original.minDamage;
        copy.maxDamage = original.maxDamage;
        
        // ROLL base damage between min and max (whole numbers only)
        copy.rolledBaseDamage = Random.Range((int)original.minDamage, (int)original.maxDamage + 1); // +1 to make maxDamage inclusive
        
        copy.attackSpeed = original.attackSpeed;
        copy.criticalStrikeChance = original.criticalStrikeChance;
        copy.criticalStrikeMultiplier = original.criticalStrikeMultiplier;
        copy.primaryDamageType = original.primaryDamageType;
        copy.requiredStrength = original.requiredStrength;
        copy.requiredDexterity = original.requiredDexterity;
        copy.requiredIntelligence = original.requiredIntelligence;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[AreaLoot] Rolled weapon damage: {copy.itemName} → {copy.rolledBaseDamage:F1} (from {original.minDamage}-{original.maxDamage})");
        }
        
        return copy;
    }
    
    private Armour CreateArmourCopy(Armour original)
    {
        if (original == null) return null;
        
        Armour copy = ScriptableObject.CreateInstance<Armour>();
        CopyBaseProperties(original, copy);
        
        // Copy armour-specific properties
        copy.armourSlot = original.armourSlot;
        copy.armourType = original.armourType;
        copy.armour = original.armour;
        copy.evasion = original.evasion;
        copy.energyShield = original.energyShield;
        copy.ward = original.ward;
        copy.requiredStrength = original.requiredStrength;
        copy.requiredDexterity = original.requiredDexterity;
        copy.requiredIntelligence = original.requiredIntelligence;
        copy.movementSpeedPenalty = original.movementSpeedPenalty;
        copy.attackSpeedPenalty = original.attackSpeedPenalty;
        
        return copy;
    }
    
    private Jewellery CreateAccessoryCopy(Jewellery original)
    {
        if (original == null) return null;
        
        Jewellery copy = ScriptableObject.CreateInstance<Jewellery>();
        CopyBaseProperties(original, copy);
        
        // Copy jewellery-specific properties if needed
        // (Add specific properties when Jewellery class is examined)
        
        return copy;
    }
    
    private Consumable CreateConsumableCopy(Consumable original)
    {
        if (original == null) return null;
        
        Consumable copy = ScriptableObject.CreateInstance<Consumable>();
        CopyBaseProperties(original, copy);
        
        // Copy consumable-specific properties if needed
        // (Add specific properties when Consumable class is examined)
        
        return copy;
    }
    
    private void CopyBaseProperties(BaseItem original, BaseItem copy)
    {
        copy.itemName = original.itemName;
        copy.description = original.description;
        copy.itemIcon = original.itemIcon;
        copy.rarity = original.rarity;
        copy.requiredLevel = original.requiredLevel;
        
        // Set itemLevel based on area level (for affix tier gating)
        // Item level determines which affix tiers can roll
        var (minLevel, maxLevel) = GetValidLevelRange();
        copy.itemLevel = Random.Range(minLevel, maxLevel + 1);
        
        copy.itemType = original.itemType;
        copy.equipmentType = original.equipmentType;
        copy.isStackable = original.isStackable;
        copy.maxStackSize = original.maxStackSize;
        copy.quality = original.quality;
        copy.isUnique = original.isUnique;
        
        // Copy implicit modifiers (these are fixed)
        copy.implicitModifiers = new List<Affix>();
        foreach (var affix in original.implicitModifiers)
        {
            copy.implicitModifiers.Add(affix);
        }
        
        // Copy item tags (with null check)
        if (original.itemTags != null)
        {
            copy.itemTags = new List<string>(original.itemTags);
        }
        else
        {
            copy.itemTags = new List<string>();
            Debug.LogWarning($"[AreaLoot] Item '{original.itemName}' has null itemTags! This may prevent affix compatibility.");
        }
        
        // Clear prefixes/suffixes (will be generated fresh)
        copy.prefixes = new List<Affix>();
        copy.suffixes = new List<Affix>();
    }
    
    private void ApplyRandomAffixes(BaseItem item)
    {
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogWarning("[AreaLoot] AffixDatabase_Modern.Instance is null, skipping affix generation");
            return;
        }
        
        // Select rarity based on weights
        ItemRarity targetRarity = SelectRandomRarity();
        
        // Generate affixes using existing system
        // Use item.itemLevel (not areaLevel) to determine which affix tiers can roll
        AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, item.itemLevel, GetMagicChance(), GetRareChance());
        
        // Generate display name for Magic/Rare items
        GenerateItemName(item);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[AreaLoot] Applied {item.prefixes.Count} prefixes, {item.suffixes.Count} suffixes to {item.itemName}");
        }
    }
    
    private void ApplyRandomAffixes(BaseItem item, ItemRarity forcedRarity)
    {
        if (item == null)
        {
            Debug.LogError("[AreaLoot] Cannot apply affixes to null item!");
            return;
        }
        
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogWarning("[AreaLoot] AffixDatabase_Modern.Instance is null, skipping affix generation");
            return;
        }
        
        // Generate affixes with forced rarity
        // Use item.itemLevel (not areaLevel) to determine which affix tiers can roll
        AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, item.itemLevel, forcedRarity);
        
        // Generate display name for Magic/Rare items
        GenerateItemName(item);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[AreaLoot] Applied {item.prefixes.Count} prefixes, {item.suffixes.Count} suffixes to {item.itemName} [Forced {forcedRarity}]");
        }
    }
    
    /// <summary>
    /// Generate display name for Magic and Rare items
    /// </summary>
    private void GenerateItemName(BaseItem item)
    {
        if (item == null) return;
        
        // Only generate names for Magic and Rare items
        if (item.rarity == ItemRarity.Magic || item.rarity == ItemRarity.Rare)
        {
            item.generatedName = ItemNameGenerator.GenerateItemName(item, nameGenerationData);
            
            if (enableDebugLogs && !string.IsNullOrEmpty(item.generatedName))
            {
                Debug.Log($"[AreaLoot] Generated name for {item.rarity} item: '{item.generatedName}'");
            }
        }
    }
    
    private ItemRarity SelectRandomRarity()
    {
        float random = Random.Range(0f, 1f);
        float currentWeight = 0f;
        
        foreach (var weight in rarityWeights)
        {
            currentWeight += weight.weight;
            if (random <= currentWeight)
            {
                return weight.rarity;
            }
        }
        
        // Fallback
        return ItemRarity.Normal;
    }
    
    private float GetMagicChance()
    {
        var magicWeight = rarityWeights.FirstOrDefault(r => r.rarity == ItemRarity.Magic);
        return magicWeight != null ? magicWeight.weight : 0.25f;
    }
    
    private float GetRareChance()
    {
        var rareWeight = rarityWeights.FirstOrDefault(r => r.rarity == ItemRarity.Rare);
        return rareWeight != null ? rareWeight.weight : 0.05f;
    }
    
    /// <summary>
    /// Validate this loot table configuration
    /// </summary>
    [ContextMenu("Validate Configuration")]
    public void ValidateConfiguration()
    {
        var (minLevel, maxLevel) = GetValidLevelRange();
        Debug.Log($"[AreaLoot] Area Level {areaLevel}: Item levels {minLevel}-{maxLevel}");
        
        // Check each item type
        foreach (var typeWeight in itemTypeWeights)
        {
            List<BaseItem> eligible = GetEligibleItems(typeWeight.itemType, minLevel, maxLevel);
            Debug.Log($"  {typeWeight.itemType}: {eligible.Count} eligible items (weight: {typeWeight.weight})");
        }
        
        // Check rarity weights
        float totalRarityWeight = rarityWeights.Sum(r => r.weight);
        Debug.Log($"  Total rarity weight: {totalRarityWeight:F3} (should be ~1.0)");
        
        // Check currency drops if enabled
        if (enableCurrencyDrops && currencyDrops != null)
        {
            Debug.Log($"  Currency drops: {currencyDrops.Length} configured");
            float totalCurrencyChance = currencyDrops.Sum(c => c.dropChance);
            Debug.Log($"  Total currency drop chance: {totalCurrencyChance:F3}");
        }
    }
    
    /// <summary>
    /// Generate random currency drops for this area
    /// </summary>
    public List<LootReward> GenerateCurrencyDrops()
    {
        List<LootReward> currencyRewards = new List<LootReward>();
        
        if (!enableCurrencyDrops || currencyDrops == null || currencyDrops.Length == 0)
        {
            return currencyRewards;
        }
        
        foreach (var currencyDrop in currencyDrops)
        {
            if (Random.Range(0f, 1f) <= currencyDrop.dropChance)
            {
                int quantity = Random.Range(currencyDrop.minQuantity, currencyDrop.maxQuantity + 1);
                
                LootReward currencyReward = new LootReward
                {
                    rewardType = RewardType.Currency,
                    currencyType = currencyDrop.currencyType,
                    currencyAmount = quantity
                };
                
                currencyRewards.Add(currencyReward);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[AreaLoot] Currency drop: {quantity}x {currencyDrop.currencyType}");
                }
            }
        }
        
        return currencyRewards;
    }
    
    /// <summary>
    /// Generate combined loot (items + currencies) for this area
    /// </summary>
    public List<LootReward> GenerateAllLoot(int maxItems = 3)
    {
        return GenerateAllLoot(areaLevel, maxItems);
    }

    /// <summary>
    /// Generate combined loot (items + currencies + effigies) for a specific area level
    /// </summary>
    public List<LootReward> GenerateAllLoot(int currentAreaLevel, int maxItems = 3)
    {
        List<LootReward> allRewards = new List<LootReward>();
        
        // Generate item drops
        List<BaseItem> items = GenerateMultipleItems(maxItems);
        foreach (var item in items)
        {
            if (item != null)
            {
                ItemData itemData = ConvertBaseItemToItemData(item);
                LootReward itemReward = new LootReward
                {
                    rewardType = RewardType.Item,
                    itemData = itemData,
                    itemInstance = item
                };
                allRewards.Add(itemReward);
            }
        }
        
        // Generate currency drops
        List<LootReward> currencies = GenerateCurrencyDrops();
        allRewards.AddRange(currencies);

        // Generate effigy drops
        List<LootReward> effigyRewards = GenerateEffigyDrops(currentAreaLevel);
        allRewards.AddRange(effigyRewards);

        // Generate warrant drops
        List<LootReward> warrantRewards = GenerateWarrantDrops(currentAreaLevel);
        allRewards.AddRange(warrantRewards);
        
        if (enableDebugLogs && allRewards.Count > 0)
        {
            Debug.Log($"[AreaLoot] Generated {allRewards.Count} total rewards ({items.Count} items, {currencies.Count} currencies, {effigyRewards.Count} effigies, {warrantRewards.Count} warrants)");
        }
        
        return allRewards;
    }

    private List<LootReward> GenerateEffigyDrops(int currentAreaLevel)
    {
        List<LootReward> effigyRewards = new List<LootReward>();

        if (effigyDrops == null || effigyDrops.Length == 0)
            return effigyRewards;

        // Get Effigy weight from itemTypeWeights
        float effigyWeight = GetItemTypeWeight(ItemType.Effigy);
        if (effigyWeight <= 0f)
        {
            // Effigy drops disabled (weight is 0 or not found)
            return effigyRewards;
        }

        // Calculate total weight of all item types for probability
        float totalItemWeight = itemTypeWeights.Sum(w => w.weight);
        if (totalItemWeight <= 0f)
        {
            return effigyRewards;
        }

        // Calculate probability of dropping an effigy (based on weight ratio)
        float effigyDropProbability = effigyWeight / totalItemWeight;

        // First roll: Should we drop an effigy at all? (based on baseDropChance and effigy weight)
        if (Random.Range(0f, 1f) > baseDropChance * effigyDropProbability)
        {
            return effigyRewards;
        }

        // Second roll: Which effigy to drop? (equal weight among all effigies in effigyDrops)
        // Filter eligible effigies for this area level
        List<EffigyDropWeight> eligibleDrops = new List<EffigyDropWeight>();
        foreach (var drop in effigyDrops)
        {
            if (drop == null || drop.effigyBlueprint == null)
                continue;

            // Check if this effigy is eligible for this area level
            if (currentAreaLevel >= drop.minAreaLevel && currentAreaLevel <= drop.maxAreaLevel)
            {
                eligibleDrops.Add(drop);
            }
        }

        if (eligibleDrops.Count == 0)
        {
            return effigyRewards;
        }

        // Select a random eligible effigy (equal probability for all)
        EffigyDropWeight selectedDrop = eligibleDrops[Random.Range(0, eligibleDrops.Count)];

        // Roll the selected effigy (using area-level scaling for drop chance)
        float chance = selectedDrop.GetDropChance(currentAreaLevel);
        if (chance > 0f && Random.Range(0f, 1f) <= chance)
        {
            Effigy instance = EffigyFactory.CreateInstance(selectedDrop.effigyBlueprint, EffigyAffixDatabase.Instance);
            if (instance != null)
            {
                effigyRewards.Add(new LootReward
                {
                    rewardType = RewardType.Effigy,
                    effigyInstance = instance
                });

                if (enableDebugLogs)
                {
                    Debug.Log($"[AreaLoot] Generated effigy: {instance.effigyName} (weight: {effigyWeight}, probability: {effigyDropProbability:P2})");
                }
            }
        }

        return effigyRewards;
    }

    /// <summary>
    /// Get the weight for a specific item type
    /// </summary>
    private float GetItemTypeWeight(ItemType itemType)
    {
        var weight = itemTypeWeights.FirstOrDefault(w => w.itemType == itemType);
        return weight != null ? weight.weight : 0f;
    }

    private List<LootReward> GenerateWarrantDrops(int currentAreaLevel)
    {
        List<LootReward> warrantRewards = new List<LootReward>();

        if (warrantDrops == null || warrantDrops.Length == 0)
            return warrantRewards;

        // Get Warrant weight from itemTypeWeights
        float warrantWeight = GetItemTypeWeight(ItemType.Warrant);
        if (warrantWeight <= 0f)
        {
            // Warrant drops disabled (weight is 0 or not found)
            return warrantRewards;
        }

        // Calculate total weight of all item types for probability
        float totalItemWeight = itemTypeWeights.Sum(w => w.weight);
        if (totalItemWeight <= 0f)
        {
            return warrantRewards;
        }

        // Calculate probability of dropping a warrant (based on weight ratio)
        float warrantDropProbability = warrantWeight / totalItemWeight;

        // First roll: Should we drop a warrant at all? (based on baseDropChance and warrant weight)
        if (Random.Range(0f, 1f) > baseDropChance * warrantDropProbability)
        {
            return warrantRewards;
        }

        // Get WarrantDatabase (needed to create instances from blueprints)
        WarrantDatabase warrantDatabase = Resources.Load<WarrantDatabase>("WarrantDatabase");
        if (warrantDatabase == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[AreaLoot] WarrantDatabase not found in Resources. Warrant drops disabled.");
            return warrantRewards;
        }

        // Second roll: Which warrant to drop? (equal weight among all warrants in warrantDrops)
        // Filter eligible warrants for this area level
        List<WarrantDropWeight> eligibleDrops = new List<WarrantDropWeight>();
        foreach (var drop in warrantDrops)
        {
            if (drop == null || drop.warrantBlueprint == null)
                continue;

            // Check if this warrant is eligible for this area level
            if (currentAreaLevel >= drop.minAreaLevel && currentAreaLevel <= drop.maxAreaLevel)
            {
                eligibleDrops.Add(drop);
            }
        }

        if (eligibleDrops.Count == 0)
        {
            return warrantRewards;
        }

        // Select a random eligible warrant (equal probability for all)
        WarrantDropWeight selectedDrop = eligibleDrops[Random.Range(0, eligibleDrops.Count)];

        // Roll the selected warrant (using area-level scaling for drop chance)
        float chance = selectedDrop.GetDropChance(currentAreaLevel);
        if (chance > 0f && Random.Range(0f, 1f) <= chance)
        {
            // Create rolled instance from blueprint
            WarrantDefinition rolledInstance = warrantDatabase.CreateInstanceFromBlueprint(
                selectedDrop.warrantBlueprint, 
                selectedDrop.minAffixes, 
                selectedDrop.maxAffixes
            );
            
            if (rolledInstance != null)
            {
                warrantRewards.Add(new LootReward
                {
                    rewardType = RewardType.Warrant,
                    warrantBlueprint = selectedDrop.warrantBlueprint,
                    warrantInstance = rolledInstance
                });

                if (enableDebugLogs)
                {
                    Debug.Log($"[AreaLoot] Generated warrant: {rolledInstance.displayName} (weight: {warrantWeight}, probability: {warrantDropProbability:P2})");
                }
            }
        }

        return warrantRewards;
    }
    
    /// <summary>
    /// Helper method to convert BaseItem to ItemData (same as AreaLootManager)
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
            requiredLevel = baseItem.requiredLevel,
            sourceItem = baseItem
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

[System.Serializable]
public class ItemTypeWeight
{
    public ItemType itemType;
    [Range(0f, 100f)]
    public float weight = 10f;
}

[System.Serializable]
public class RarityWeight
{
    public ItemRarity rarity;
    [Range(0f, 1f)]
    public float weight = 0.1f;
}

[System.Serializable]
public class CurrencyDropWeight
{
    public CurrencyType currencyType;
    [Tooltip("Individual drop chance for this currency (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float dropChance = 0.05f;
    [Tooltip("Minimum quantity to drop")]
    public int minQuantity = 1;
    [Tooltip("Maximum quantity to drop")]
    public int maxQuantity = 1;
}

[System.Serializable]
public class EffigyDropWeight
{
    [Tooltip("Effigy blueprint to roll when this drop succeeds")]
    public Effigy effigyBlueprint;

    [Tooltip("Lowest area level this drop can appear")]
    public int minAreaLevel = 1;

    [Tooltip("Highest area level this drop can appear")]
    public int maxAreaLevel = 100;

    [Tooltip("Drop chance when the area level equals minAreaLevel (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float dropChanceAtMinLevel = 0f;

    [Tooltip("Drop chance when the area level equals maxAreaLevel (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float dropChanceAtMaxLevel = 0.05f;

    [Tooltip("Number of times to roll this effigy when the drop chance succeeds")]
    public int maxQuantity = 1;

    public float GetDropChance(int areaLevel)
    {
        if (effigyBlueprint == null)
            return 0f;

        if (areaLevel < minAreaLevel || areaLevel > maxAreaLevel)
            return 0f;

        if (Mathf.Approximately(minAreaLevel, maxAreaLevel))
            return dropChanceAtMaxLevel;

        float t = Mathf.InverseLerp(minAreaLevel, maxAreaLevel, areaLevel);
        return Mathf.Lerp(dropChanceAtMinLevel, dropChanceAtMaxLevel, t);
    }
}

[System.Serializable]
public class WarrantDropWeight
{
    [Tooltip("Warrant blueprint to roll when this drop succeeds")]
    public WarrantDefinition warrantBlueprint;

    [Tooltip("Lowest area level this drop can appear")]
    public int minAreaLevel = 1;

    [Tooltip("Highest area level this drop can appear")]
    public int maxAreaLevel = 100;

    [Tooltip("Drop chance when the area level equals minAreaLevel (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float dropChanceAtMinLevel = 0f;

    [Tooltip("Drop chance when the area level equals maxAreaLevel (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float dropChanceAtMaxLevel = 0.05f;

    [Tooltip("Number of times to roll this warrant when the drop chance succeeds")]
    public int maxQuantity = 1;

    [Tooltip("Minimum number of affixes to roll on the warrant")]
    public int minAffixes = 1;

    [Tooltip("Maximum number of affixes to roll on the warrant")]
    public int maxAffixes = 3;

    public float GetDropChance(int areaLevel)
    {
        if (warrantBlueprint == null)
            return 0f;

        if (areaLevel < minAreaLevel || areaLevel > maxAreaLevel)
            return 0f;

        if (Mathf.Approximately(minAreaLevel, maxAreaLevel))
            return dropChanceAtMaxLevel;

        float t = Mathf.InverseLerp(minAreaLevel, maxAreaLevel, areaLevel);
        return Mathf.Lerp(dropChanceAtMinLevel, dropChanceAtMaxLevel, t);
    }
}
