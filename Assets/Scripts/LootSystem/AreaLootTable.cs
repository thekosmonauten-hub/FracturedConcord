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
    public float baseDropChance = 0.15f;
    
    [Header("Item Type Weights")]
    [Tooltip("Relative chance for each item type to drop")]
    public ItemTypeWeight[] itemTypeWeights = new ItemTypeWeight[]
    {
        new ItemTypeWeight { itemType = ItemType.Weapon, weight = 30f },
        new ItemTypeWeight { itemType = ItemType.Armour, weight = 40f },
        new ItemTypeWeight { itemType = ItemType.Accessory, weight = 20f },
        new ItemTypeWeight { itemType = ItemType.Consumable, weight = 10f }
    };
    
    [Header("Rarity Distribution")]
    [Tooltip("Rarity chances (should add up to ~1.0)")]
    public RarityWeight[] rarityWeights = new RarityWeight[]
    {
        new RarityWeight { rarity = ItemRarity.Normal, weight = 0.70f },
        new RarityWeight { rarity = ItemRarity.Magic, weight = 0.25f },
        new RarityWeight { rarity = ItemRarity.Rare, weight = 0.05f },
        new RarityWeight { rarity = ItemRarity.Unique, weight = 0.001f }
    };
    
    [Header("Currency Drops")]
    [Tooltip("Enable currency drops alongside item drops")]
    public bool enableCurrencyDrops = true;
    
    [Tooltip("Currency drop configuration with individual drop chances")]
    public CurrencyDropWeight[] currencyDrops = new CurrencyDropWeight[]
    {
        // Based on 1-9Loot.asset currency distribution
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfGeneration, dropChance = 0.15f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfInfusion, dropChance = 0.15f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfPerfection, dropChance = 0.07f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfPerpetuity, dropChance = 0.06f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfRedundancy, dropChance = 0.03f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfTheVoid, dropChance = 0.02f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfMutation, dropChance = 0.06f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfProliferation, dropChance = 0.12f },
        new CurrencyDropWeight { currencyType = CurrencyType.OrbOfAmnesia, dropChance = 0.04f },
        
        // Spirits (area 1-9 rates)
        new CurrencyDropWeight { currencyType = CurrencyType.FireSpirit, dropChance = 0.10f },
        new CurrencyDropWeight { currencyType = CurrencyType.ColdSpirit, dropChance = 0.10f },
        new CurrencyDropWeight { currencyType = CurrencyType.LightningSpirit, dropChance = 0.10f },
        new CurrencyDropWeight { currencyType = CurrencyType.ChaosSpirit, dropChance = 0.08f },
        new CurrencyDropWeight { currencyType = CurrencyType.PhysicalSpirit, dropChance = 0.10f },
        new CurrencyDropWeight { currencyType = CurrencyType.LifeSpirit, dropChance = 0.10f },
        new CurrencyDropWeight { currencyType = CurrencyType.DefenseSpirit, dropChance = 0.10f },
        new CurrencyDropWeight { currencyType = CurrencyType.CritSpirit, dropChance = 0.08f },
        new CurrencyDropWeight { currencyType = CurrencyType.DivineSpirit, dropChance = 0.03f },
        
        // Seals (area 1-9 rates)
        new CurrencyDropWeight { currencyType = CurrencyType.TranspositionSeal, dropChance = 0.05f },
        new CurrencyDropWeight { currencyType = CurrencyType.ChaosSeal, dropChance = 0.07f },
        new CurrencyDropWeight { currencyType = CurrencyType.MemorySeal, dropChance = 0.03f },
        new CurrencyDropWeight { currencyType = CurrencyType.InscriptionSeal, dropChance = 0.05f },
        new CurrencyDropWeight { currencyType = CurrencyType.AdaptationSeal, dropChance = 0.05f },
        new CurrencyDropWeight { currencyType = CurrencyType.CorrectionSeal, dropChance = 0.05f },
        new CurrencyDropWeight { currencyType = CurrencyType.EtchingSeal, dropChance = 0.07f }
    };

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
        
        return allItems.Where(item => 
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
        copy.attackSpeed = original.attackSpeed;
        copy.criticalStrikeChance = original.criticalStrikeChance;
        copy.criticalStrikeMultiplier = original.criticalStrikeMultiplier;
        copy.primaryDamageType = original.primaryDamageType;
        copy.requiredStrength = original.requiredStrength;
        copy.requiredDexterity = original.requiredDexterity;
        copy.requiredIntelligence = original.requiredIntelligence;
        
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
        AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, areaLevel, GetMagicChance(), GetRareChance());
        
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
        AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, areaLevel, forcedRarity);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[AreaLoot] Applied {item.prefixes.Count} prefixes, {item.suffixes.Count} suffixes to {item.itemName} [Forced {forcedRarity}]");
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
        List<LootReward> allRewards = new List<LootReward>();
        
        // Generate item drops
        List<BaseItem> items = GenerateMultipleItems(maxItems);
        foreach (var item in items)
        {
            if (item != null)
            {
                LootReward itemReward = new LootReward
                {
                    rewardType = RewardType.Item,
                    itemData = ConvertBaseItemToItemData(item)
                };
                allRewards.Add(itemReward);
            }
        }
        
        // Generate currency drops
        List<LootReward> currencies = GenerateCurrencyDrops();
        allRewards.AddRange(currencies);
        
        if (enableDebugLogs && allRewards.Count > 0)
        {
            Debug.Log($"[AreaLoot] Generated {allRewards.Count} total rewards ({items.Count} items, {currencies.Count} currencies)");
        }
        
        return allRewards;
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
