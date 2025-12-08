using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class AffixCategory
{
    public string categoryName;
    public List<AffixSubCategory> subCategories = new List<AffixSubCategory>();
    
    public AffixCategory(string name)
    {
        categoryName = name;
    }
    
    public List<Affix> GetAllAffixes()
    {
        List<Affix> allAffixes = new List<Affix>();
        foreach (var subCategory in subCategories)
        {
            allAffixes.AddRange(subCategory.affixes);
        }
        return allAffixes;
    }
}

[System.Serializable]
public class AffixSubCategory
{
    public string subCategoryName;
    public List<Affix> affixes = new List<Affix>();
    
    public AffixSubCategory(string name)
    {
        subCategoryName = name;
    }
}

[CreateAssetMenu(fileName = "Affix Database", menuName = "Dexiled/Items/Affix Database")]
public class AffixDatabase : ScriptableObject
{
    [Header("Weapon Prefixes")]
    public List<AffixCategory> weaponPrefixCategories = new List<AffixCategory>();
    
    [Header("Weapon Suffixes")]
    public List<AffixCategory> weaponSuffixCategories = new List<AffixCategory>();
    
    [Header("Armour Prefixes")]
    public List<AffixCategory> armourPrefixCategories = new List<AffixCategory>();
    
    [Header("Armour Suffixes")]
    public List<AffixCategory> armourSuffixCategories = new List<AffixCategory>();
    
    [Header("Jewellery Prefixes")]
    public List<AffixCategory> jewelleryPrefixCategories = new List<AffixCategory>();
    
    [Header("Jewellery Suffixes")]
    public List<AffixCategory> jewellerySuffixCategories = new List<AffixCategory>();
    
    // Singleton instance
    private static AffixDatabase _instance;
    public static AffixDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<AffixDatabase>("AffixDatabase");
                if (_instance == null)
                {
                    Debug.LogError("AffixDatabase not found in Resources folder!");
                }
            }
            return _instance;
        }
    }
    
    // Get random prefix for item type
    public Affix GetRandomPrefix(ItemType itemType, int itemLevel, AffixTier maxTier = AffixTier.Tier1)
    {
        List<Affix> availablePrefixes = GetAvailablePrefixes(itemType, itemLevel, maxTier);
        
        if (availablePrefixes.Count == 0)
            return null;
            
        // Weighted random selection
        float totalWeight = availablePrefixes.Sum(a => a.weight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var affix in availablePrefixes)
        {
            currentWeight += affix.weight;
            if (randomValue <= currentWeight)
                return affix;
        }
        
        return availablePrefixes[0]; // Fallback
    }
    
    // Get random suffix for item type
    public Affix GetRandomSuffix(ItemType itemType, int itemLevel, AffixTier maxTier = AffixTier.Tier1)
    {
        List<Affix> availableSuffixes = GetAvailableSuffixes(itemType, itemLevel, maxTier);
        
        if (availableSuffixes.Count == 0)
            return null;
            
        // Weighted random selection
        float totalWeight = availableSuffixes.Sum(a => a.weight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var affix in availableSuffixes)
        {
            currentWeight += affix.weight;
            if (randomValue <= currentWeight)
                return affix;
        }
        
        return availableSuffixes[0]; // Fallback
    }
    
    // Get available prefixes for item type and level
    private List<Affix> GetAvailablePrefixes(ItemType itemType, int itemLevel, AffixTier maxTier)
    {
        List<Affix> allPrefixes = new List<Affix>();
        
        switch (itemType)
        {
            case ItemType.Weapon:
                foreach (var category in weaponPrefixCategories)
                {
                    allPrefixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Armour:
                foreach (var category in armourPrefixCategories)
                {
                    allPrefixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Accessory:
                foreach (var category in jewelleryPrefixCategories)
                {
                    allPrefixes.AddRange(category.GetAllAffixes());
                }
                break;
        }
        
        // Filter by tier requirements, level requirements, and item tags
        return allPrefixes.Where(a => 
            (int)a.tier <= (int)maxTier && 
            a.minLevel <= itemLevel &&
            a.requiredTags.Any(tag => GetItemTags(itemType).Contains(tag))
        ).ToList();
    }
    
    // Get available suffixes for item type and level
    private List<Affix> GetAvailableSuffixes(ItemType itemType, int itemLevel, AffixTier maxTier)
    {
        List<Affix> allSuffixes = new List<Affix>();
        
        switch (itemType)
        {
            case ItemType.Weapon:
                foreach (var category in weaponSuffixCategories)
                {
                    allSuffixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Armour:
                foreach (var category in armourSuffixCategories)
                {
                    allSuffixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Accessory:
                foreach (var category in jewellerySuffixCategories)
                {
                    allSuffixes.AddRange(category.GetAllAffixes());
                }
                break;
        }
        
        // Filter by tier requirements, level requirements, and item tags
        return allSuffixes.Where(a => 
            (int)a.tier <= (int)maxTier && 
            a.minLevel <= itemLevel &&
            a.requiredTags.Any(tag => GetItemTags(itemType).Contains(tag))
        ).ToList();
    }
    
    // Get item tags for item type
    private List<string> GetItemTags(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                return new List<string> { "weapon", "attack", "melee", "ranged", "spell" };
            case ItemType.Armour:
                return new List<string> { "armour", "defence" };
            case ItemType.Accessory:
                return new List<string> { "jewellery", "accessory" };
            case ItemType.Consumable:
                return new List<string> { "consumable" };
            case ItemType.Material:
                return new List<string> { "material" };
            default:
                return new List<string>();
        }
    }
    
    // Generate random affixes for an item
    public void GenerateRandomAffixes(BaseItem item, int itemLevel, float magicChance = 0.1f, float rareChance = 0.05f)
    {
        if (item.isUnique)
            return; // Unique items have fixed affixes
            
        item.ClearAffixes();
        
        float random = Random.Range(0f, 1f);
        
        if (random < rareChance)
        {
            // Generate Rare item (3-6 affixes)
            GenerateRareAffixes(item, itemLevel);
        }
        else if (random < magicChance)
        {
            // Generate Magic item (1-2 affixes)
            GenerateMagicAffixes(item, itemLevel);
        }
        // Otherwise, Normal item (0 affixes)
    }
    
    // Generate random affixes for an item with forced rarity (for testing)
    public void GenerateRandomAffixes(BaseItem item, int itemLevel, ItemRarity forcedRarity)
    {
        if (item == null)
        {
            Debug.LogError("[AffixDatabase] Cannot generate affixes for null item!");
            return;
        }
        
        if (item.isUnique)
            return; // Unique items have fixed affixes
            
        item.ClearAffixes();
        
        switch (forcedRarity)
        {
            case ItemRarity.Normal:
                // No affixes
                break;
            case ItemRarity.Magic:
                GenerateMagicAffixes(item, itemLevel);
                break;
            case ItemRarity.Rare:
                GenerateRareAffixes(item, itemLevel);
                break;
            case ItemRarity.Unique:
                // Unique items should have pre-defined affixes
                break;
        }
    }
    
    private void GenerateMagicAffixes(BaseItem item, int itemLevel)
    {
        int prefixCount = Random.Range(0, 2); // 0 or 1
        int suffixCount = Random.Range(0, 2); // 0 or 1
        
        // Ensure at least one affix for Magic items
        if (prefixCount == 0 && suffixCount == 0)
        {
            if (Random.Range(0f, 1f) < 0.5f)
                prefixCount = 1;
            else
                suffixCount = 1;
        }
        
        GenerateAffixes(item, itemLevel, prefixCount, suffixCount);
    }
    
    private void GenerateRareAffixes(BaseItem item, int itemLevel)
    {
        int prefixCount = Random.Range(1, 4); // 1 to 3
        int suffixCount = Random.Range(1, 4); // 1 to 3
        
        // Ensure at least 3 total affixes for Rare items
        while (prefixCount + suffixCount < 3)
        {
            if (prefixCount < 3)
                prefixCount++;
            else if (suffixCount < 3)
                suffixCount++;
        }
        
        GenerateAffixes(item, itemLevel, prefixCount, suffixCount);
    }
    
    private void GenerateAffixes(BaseItem item, int itemLevel, int prefixCount, int suffixCount)
    {
        // Generate prefixes
        for (int i = 0; i < prefixCount && item.CanAddPrefix(); i++)
        {
            AffixTier maxTier = GetMaxTierForLevel(itemLevel);
            Affix prefix = GetRandomPrefix(item.itemType, itemLevel, maxTier);
            if (prefix != null)
            {
                item.AddPrefix(prefix);
            }
        }
        
        // Generate suffixes
        for (int i = 0; i < suffixCount && item.CanAddSuffix(); i++)
        {
            AffixTier maxTier = GetMaxTierForLevel(itemLevel);
            Affix suffix = GetRandomSuffix(item.itemType, itemLevel, maxTier);
            if (suffix != null)
            {
                item.AddSuffix(suffix);
            }
        }
    }
    
    private AffixTier GetMaxTierForLevel(int itemLevel)
    {
        // Simple tier calculation based on item level
        if (itemLevel >= 80) return AffixTier.Tier1;
        if (itemLevel >= 70) return AffixTier.Tier2;
        if (itemLevel >= 60) return AffixTier.Tier3;
        if (itemLevel >= 50) return AffixTier.Tier4;
        if (itemLevel >= 40) return AffixTier.Tier5;
        if (itemLevel >= 30) return AffixTier.Tier6;
        if (itemLevel >= 20) return AffixTier.Tier7;
        if (itemLevel >= 10) return AffixTier.Tier8;
        return AffixTier.Tier9;
    }
    
    // Method to populate the database with physical damage affixes
    [ContextMenu("Add Physical Damage Affixes")]
    public void AddPhysicalDamageAffixes()
    {
        // Clear existing weapon prefix categories to avoid duplicates
        weaponPrefixCategories.Clear();
        
        // Create Physical category
        AffixCategory physicalCategory = new AffixCategory("Physical");
        
        // Create sub-categories
        AffixSubCategory increasedCategory = new AffixSubCategory("Increased");
        AffixSubCategory flatCategory = new AffixSubCategory("Flat");
        AffixSubCategory hybridCategory = new AffixSubCategory("Hybrid");
        
        // Squire's series (Increased Physical Damage + Accuracy) - Hybrid
        AddPhysicalDamageAffix(hybridCategory, "Squire's", 1, 15, 19, 16, 20, AffixTier.Tier9);
        AddPhysicalDamageAffix(hybridCategory, "Journeyman's", 11, 20, 24, 21, 46, AffixTier.Tier8);
        AddPhysicalDamageAffix(hybridCategory, "Reaver's", 23, 25, 34, 47, 72, AffixTier.Tier7);
        AddPhysicalDamageAffix(hybridCategory, "Mercenary's", 35, 35, 44, 73, 97, AffixTier.Tier6);
        AddPhysicalDamageAffix(hybridCategory, "Champion's", 46, 45, 54, 98, 123, AffixTier.Tier5);
        AddPhysicalDamageAffix(hybridCategory, "Conqueror's", 60, 55, 64, 124, 149, AffixTier.Tier4);
        AddPhysicalDamageAffix(hybridCategory, "Emperor's", 73, 65, 74, 150, 174, AffixTier.Tier3);
        AddPhysicalDamageAffix(hybridCategory, "Dictator's", 83, 75, 79, 175, 200, AffixTier.Tier2);
        
        // Glinting series (Added Physical Damage) - Flat
        AddAddedPhysicalDamageAffix(flatCategory, "Glinting", 2, 1, 3, AffixTier.Tier9);
        AddAddedPhysicalDamageAffix(flatCategory, "Burnished", 13, 4, 9, AffixTier.Tier8);
        AddAddedPhysicalDamageAffix(flatCategory, "Polished", 21, 6, 15, AffixTier.Tier7);
        AddAddedPhysicalDamageAffix(flatCategory, "Honed", 29, 8, 20, AffixTier.Tier6);
        AddAddedPhysicalDamageAffix(flatCategory, "Gleaming", 36, 11, 25, AffixTier.Tier5);
        AddAddedPhysicalDamageAffix(flatCategory, "Annealed", 46, 13, 31, AffixTier.Tier4);
        AddAddedPhysicalDamageAffix(flatCategory, "Razor-sharp", 54, 16, 38, AffixTier.Tier3);
        AddAddedPhysicalDamageAffix(flatCategory, "Tempered", 65, 19, 45, AffixTier.Tier2);
        AddAddedPhysicalDamageAffix(flatCategory, "Flaring", 77, 22, 52, AffixTier.Tier1);
        
        // Heavy series (Increased Physical Damage only) - Increased
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Heavy", 1, 40, 49, AffixTier.Tier9);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Serrated", 11, 50, 64, AffixTier.Tier8);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Wicked", 23, 65, 84, AffixTier.Tier7);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Vicious", 35, 85, 109, AffixTier.Tier6);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Bloodthirsty", 46, 110, 134, AffixTier.Tier5);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Cruel", 60, 135, 154, AffixTier.Tier4);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Tyrannical", 73, 155, 169, AffixTier.Tier3);
        AddIncreasedPhysicalDamageAffix(increasedCategory, "Merciless", 83, 170, 179, AffixTier.Tier2);
        
        // Add sub-categories to physical category
        physicalCategory.subCategories.Add(increasedCategory);
        physicalCategory.subCategories.Add(flatCategory);
        physicalCategory.subCategories.Add(hybridCategory);
        
        // Add physical category to weapon prefixes
        weaponPrefixCategories.Add(physicalCategory);
        
        int totalAffixes = physicalCategory.GetAllAffixes().Count;
        Debug.Log($"Added {totalAffixes} physical damage affixes to the database in organized categories.");
    }
    
    // Helper method to get or create a category
    private AffixCategory GetOrCreateCategory(List<AffixCategory> categories, string categoryName)
    {
        AffixCategory category = categories.Find(c => c.categoryName == categoryName);
        if (category == null)
        {
            category = new AffixCategory(categoryName);
            categories.Add(category);
        }
        return category;
    }
    
    // Helper method to get or create a sub-category
    private AffixSubCategory GetOrCreateSubCategory(AffixCategory category, string subCategoryName)
    {
        AffixSubCategory subCategory = category.subCategories.Find(sc => sc.subCategoryName == subCategoryName);
        if (subCategory == null)
        {
            subCategory = new AffixSubCategory(subCategoryName);
            category.subCategories.Add(subCategory);
        }
        return subCategory;
    }
    
    private void AddPhysicalDamageAffix(AffixSubCategory category, string name, int itemLevel, int physMin, int physMax, int accMin, int accMax, AffixTier tier)
    {
        Affix affix = new Affix(name, $"{physMin}-{physMax}% increased Physical Damage\n{accMin}-{accMax} to Accuracy Rating", AffixType.Prefix, tier);
        
        // Add physical damage modifier (LOCAL - affects weapon base damage)
        AffixModifier physMod = new AffixModifier("PhysicalDamage", physMin, physMax, ModifierType.Increased, ModifierScope.Local);
        physMod.damageType = DamageType.Physical;
        affix.modifiers.Add(physMod);
        
        // Add accuracy modifier (LOCAL - affects weapon accuracy)
        AffixModifier accMod = new AffixModifier("AccuracyRating", accMin, accMax, ModifierType.Flat, ModifierScope.Local);
        affix.modifiers.Add(accMod);
        
        // Set required tags - compatible with all weapon types that can have physical damage
        affix.requiredTags = new List<string> { "weapon", "attack" };
        
        category.affixes.Add(affix);
    }
    
    private void AddAddedPhysicalDamageAffix(AffixSubCategory category, string name, int itemLevel, int minDamage, int maxDamage, AffixTier tier)
    {
        Affix affix = new Affix(name, $"Adds {minDamage} to {maxDamage} Physical Damage", AffixType.Prefix, tier);
        
        // Add physical damage modifier (LOCAL - affects weapon base damage)
        AffixModifier mod = new AffixModifier("PhysicalDamage", minDamage, maxDamage, ModifierType.Flat, ModifierScope.Local);
        mod.damageType = DamageType.Physical;
        affix.modifiers.Add(mod);
        
        // Set required tags - compatible with all weapon types that can have physical damage
        affix.requiredTags = new List<string> { "weapon", "attack" };
        
        category.affixes.Add(affix);
    }
    
    private void AddIncreasedPhysicalDamageAffix(AffixSubCategory category, string name, int itemLevel, int minPercent, int maxPercent, AffixTier tier)
    {
        Affix affix = new Affix(name, $"{minPercent}-{maxPercent}% increased Physical Damage", AffixType.Prefix, tier);
        
        // Add physical damage modifier (LOCAL - affects weapon base damage)
        AffixModifier mod = new AffixModifier("PhysicalDamage", minPercent, maxPercent, ModifierType.Increased, ModifierScope.Local);
        mod.damageType = DamageType.Physical;
        affix.modifiers.Add(mod);
        
        // Set required tags - compatible with all weapon types that can have physical damage
        affix.requiredTags = new List<string> { "weapon", "attack" };
        
        category.affixes.Add(affix);
    }
    
    [ContextMenu("Add Elemental Damage Affixes")]
    public void AddElementalDamageAffixes()
    {
        // Get or create Elemental category
        AffixCategory elementalCategory = GetOrCreateCategory(weaponPrefixCategories, "Elemental");
        
        // Create sub-categories for different damage types
        AffixSubCategory fireCategory = GetOrCreateSubCategory(elementalCategory, "Fire");
        AffixSubCategory coldCategory = GetOrCreateSubCategory(elementalCategory, "Cold");
        AffixSubCategory lightningCategory = GetOrCreateSubCategory(elementalCategory, "Lightning");
        AffixSubCategory chaosCategory = GetOrCreateSubCategory(elementalCategory, "Chaos");
        
        // Fire Damage Affixes
        AddElementalDamageAffix(fireCategory, "Fiery", 1, 1, 3, DamageType.Fire, AffixTier.Tier9);
        AddElementalDamageAffix(fireCategory, "Burning", 13, 4, 9, DamageType.Fire, AffixTier.Tier8);
        AddElementalDamageAffix(fireCategory, "Blazing", 21, 6, 15, DamageType.Fire, AffixTier.Tier7);
        AddElementalDamageAffix(fireCategory, "Infernal", 29, 8, 20, DamageType.Fire, AffixTier.Tier6);
        AddElementalDamageAffix(fireCategory, "Volcanic", 36, 11, 25, DamageType.Fire, AffixTier.Tier5);
        AddElementalDamageAffix(fireCategory, "Molten", 46, 13, 31, DamageType.Fire, AffixTier.Tier4);
        AddElementalDamageAffix(fireCategory, "Magmatic", 54, 16, 38, DamageType.Fire, AffixTier.Tier3);
        AddElementalDamageAffix(fireCategory, "Pyroclastic", 65, 19, 45, DamageType.Fire, AffixTier.Tier2);
        AddElementalDamageAffix(fireCategory, "Incandescent", 77, 22, 52, DamageType.Fire, AffixTier.Tier1);
        
        // Cold Damage Affixes
        AddElementalDamageAffix(coldCategory, "Frozen", 1, 1, 3, DamageType.Cold, AffixTier.Tier9);
        AddElementalDamageAffix(coldCategory, "Icy", 13, 4, 9, DamageType.Cold, AffixTier.Tier8);
        AddElementalDamageAffix(coldCategory, "Glacial", 21, 6, 15, DamageType.Cold, AffixTier.Tier7);
        AddElementalDamageAffix(coldCategory, "Arctic", 29, 8, 20, DamageType.Cold, AffixTier.Tier6);
        AddElementalDamageAffix(coldCategory, "Polar", 36, 11, 25, DamageType.Cold, AffixTier.Tier5);
        AddElementalDamageAffix(coldCategory, "Frigid", 46, 13, 31, DamageType.Cold, AffixTier.Tier4);
        AddElementalDamageAffix(coldCategory, "Crystalline", 54, 16, 38, DamageType.Cold, AffixTier.Tier3);
        AddElementalDamageAffix(coldCategory, "Permafrost", 65, 19, 45, DamageType.Cold, AffixTier.Tier2);
        AddElementalDamageAffix(coldCategory, "Absolute Zero", 77, 22, 52, DamageType.Cold, AffixTier.Tier1);
        
        // Lightning Damage Affixes
        AddElementalDamageAffix(lightningCategory, "Static", 1, 1, 3, DamageType.Lightning, AffixTier.Tier9);
        AddElementalDamageAffix(lightningCategory, "Charged", 13, 4, 9, DamageType.Lightning, AffixTier.Tier8);
        AddElementalDamageAffix(lightningCategory, "Thunder", 21, 6, 15, DamageType.Lightning, AffixTier.Tier7);
        AddElementalDamageAffix(lightningCategory, "Storm", 29, 8, 20, DamageType.Lightning, AffixTier.Tier6);
        AddElementalDamageAffix(lightningCategory, "Tempest", 36, 11, 25, DamageType.Lightning, AffixTier.Tier5);
        AddElementalDamageAffix(lightningCategory, "Hurricane", 46, 13, 31, DamageType.Lightning, AffixTier.Tier4);
        AddElementalDamageAffix(lightningCategory, "Cyclone", 54, 16, 38, DamageType.Lightning, AffixTier.Tier3);
        AddElementalDamageAffix(lightningCategory, "Typhoon", 65, 19, 45, DamageType.Lightning, AffixTier.Tier2);
        AddElementalDamageAffix(lightningCategory, "Apocalypse", 77, 22, 52, DamageType.Lightning, AffixTier.Tier1);
        
        // Chaos Damage Affixes
        AddElementalDamageAffix(chaosCategory, "Corrupted", 1, 1, 3, DamageType.Chaos, AffixTier.Tier9);
        AddElementalDamageAffix(chaosCategory, "Tainted", 13, 4, 9, DamageType.Chaos, AffixTier.Tier8);
        AddElementalDamageAffix(chaosCategory, "Defiled", 21, 6, 15, DamageType.Chaos, AffixTier.Tier7);
        AddElementalDamageAffix(chaosCategory, "Profane", 29, 8, 20, DamageType.Chaos, AffixTier.Tier6);
        AddElementalDamageAffix(chaosCategory, "Sacrilegious", 36, 11, 25, DamageType.Chaos, AffixTier.Tier5);
        AddElementalDamageAffix(chaosCategory, "Blasphemous", 46, 13, 31, DamageType.Chaos, AffixTier.Tier4);
        AddElementalDamageAffix(chaosCategory, "Heretical", 54, 16, 38, DamageType.Chaos, AffixTier.Tier3);
        AddElementalDamageAffix(chaosCategory, "Abyssal", 65, 19, 45, DamageType.Chaos, AffixTier.Tier2);
        AddElementalDamageAffix(chaosCategory, "Void", 77, 22, 52, DamageType.Chaos, AffixTier.Tier1);
        
        int totalAffixes = elementalCategory.GetAllAffixes().Count;
        Debug.Log($"Added {totalAffixes} elemental damage affixes to the database.");
    }
    
    private void AddElementalDamageAffix(AffixSubCategory category, string name, int itemLevel, int minDamage, int maxDamage, DamageType damageType, AffixTier tier)
    {
        string damageTypeName = damageType.ToString();
        Affix affix = new Affix(name, $"Adds {minDamage} to {maxDamage} {damageTypeName} Damage", AffixType.Prefix, tier);
        
        // Add damage modifier (LOCAL - affects weapon base damage)
        AffixModifier mod = new AffixModifier($"{damageTypeName}Damage", minDamage, maxDamage, ModifierType.Flat, ModifierScope.Local);
        mod.damageType = damageType;
        affix.modifiers.Add(mod);
        
        // Set required tags - compatible with all weapon types
        affix.requiredTags = new List<string> { "weapon" };
        
        category.affixes.Add(affix);
    }

    // ===== SMART COMPATIBILITY METHODS =====
    
    /// <summary>
    /// Enhanced compatibility checking using base item stats and Local vs Global modifier rules
    /// </summary>
    public static bool IsAffixCompatibleWithItem(Affix affix, BaseItem item)
    {
        // Check each modifier in the affix for Local vs Global compatibility
        foreach (AffixModifier modifier in affix.modifiers)
        {
            if (!IsModifierCompatibleWithItem(modifier, item))
                return false;
        }
        
        // Legacy compatibility tag system (keep for backward compatibility)
        if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
            return true; // No restrictions
        
        foreach (string requiredTag in affix.compatibleTags)
        {
            if (requiredTag == "energyshield_base")
            {
                // Only compatible with items that have Energy Shield as base stat
                if (item is Armour armour && armour.energyShield > 0)
                    continue;
                else
                    return false;
            }
            else if (requiredTag == "armour_base")
            {
                // Only compatible with items that have Armour as base stat
                if (item is Armour armour && armour.armour > 0)
                    continue;
                else
                    return false;
            }
            else if (requiredTag == "evasion_base")
            {
                // Only compatible with items that have Evasion as base stat
                if (item is Armour armour && armour.evasion > 0)
                    continue;
                else
                    return false;
            }
            else
            {
                // Check item tags
                if (item.itemTags != null && item.itemTags.Contains(requiredTag))
                    continue;
                else
                    return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks if a specific modifier is compatible with an item based on Local vs Global rules
    /// </summary>
    private static bool IsModifierCompatibleWithItem(AffixModifier modifier, BaseItem item)
    {
        if (modifier.scope == ModifierScope.Global)
        {
            return IsGlobalModifierCompatible(modifier, item);
        }
        else // Local modifier
        {
            return IsLocalModifierCompatible(modifier, item);
        }
    }
    
    /// <summary>
    /// Global modifiers can roll on appropriate item types (jewelry, armor)
    /// but should NOT roll on items where they could be local instead
    /// </summary>
    private static bool IsGlobalModifierCompatible(AffixModifier modifier, BaseItem item)
    {
        string statName = modifier.statName.ToLower();
        
        // Global mods typically can roll on jewelry and armor, but not weapons
        // Exception: Some global mods CAN roll on weapons as implicits (marked separately)
        
        if (item is WeaponItem)
        {
            // Most global mods should not roll on weapons where local versions exist
            if (statName.Contains("physicaldamage") || statName.Contains("firedamage") ||
                statName.Contains("colddamage") || statName.Contains("lightningdamage") ||
                statName.Contains("chaosdamage") || statName.Contains("criticalchance"))
            {
                return false; // These should be local on weapons
            }
            
            // Allow some global mods on weapons (accuracy, resistances, etc.)
            return true;
        }
        
        if (item is Armour || item is Jewellery)
        {
            // Global mods are generally allowed on armor and jewelry
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Local modifiers can only roll on items that have the relevant base stat
    /// </summary>
    private static bool IsLocalModifierCompatible(AffixModifier modifier, BaseItem item)
    {
        string statName = modifier.statName.ToLower();
        
        // Physical damage mods - only on weapons
        if (statName.Contains("physicaldamage") || statName.Contains("addedphysicaldamage"))
        {
            return item is WeaponItem;
        }
        
        // Elemental damage mods - only on weapons  
        if (statName.Contains("firedamage") || statName.Contains("colddamage") ||
            statName.Contains("lightningdamage") || statName.Contains("chaosdamage"))
        {
            return item is WeaponItem;
        }
        
        // Critical strike chance - only on weapons with base crit
        if (statName.Contains("criticalchance") || statName.Contains("criticalstrikechance"))
        {
            return item is WeaponItem weapon && weapon.criticalStrikeChance > 0;
        }
        
        // Attack speed - only on weapons
        if (statName.Contains("attackspeed"))
        {
            return item is WeaponItem;
        }
        
        // Cast speed - only on caster weapons
        if (statName.Contains("castspeed"))
        {
            return item is WeaponItem weapon && weapon.itemTags.Contains("spell");
        }
        
        // Armour - only on armor pieces with base armour
        if (statName.Contains("armour") && !statName.Contains("base"))
        {
            return item is Armour armour && armour.armour > 0;
        }
        
        // Evasion - only on armor pieces with base evasion
        if (statName.Contains("evasion"))
        {
            return item is Armour armour && armour.evasion > 0;
        }
        
        // Energy Shield - only on armor pieces with base ES
        if (statName.Contains("energyshield"))
        {
            return item is Armour armour && armour.energyShield > 0;
        }
        
        // Block chance - only on shields
        if (statName.Contains("blockchance"))
        {
            return item is Armour armour && armour.itemTags.Contains("shield");
        }
        
        // Default: allow if no specific restrictions
        return true;
    }
    
    /// <summary>
    /// Basic compatibility checking for item types (used during affix pool selection)
    /// Enhanced with Local vs Global modifier awareness
    /// </summary>
    private bool IsAffixCompatibleWithItemType(Affix affix, ItemType itemType)
    {
        // Use compatible tags if available
        if (affix.compatibleTags != null && affix.compatibleTags.Count > 0)
        {
            List<string> itemTags = GetItemTags(itemType);
            
            // Check if any compatible tag matches the item's tags
            foreach (string compatibleTag in affix.compatibleTags)
            {
                if (itemTags.Contains(compatibleTag))
                    return true;
                    
                // Handle base-type specific tags
                if (compatibleTag.EndsWith("_base"))
                {
                    // For base-type specific affixes, allow them in the pool
                    // Final compatibility will be checked when applying to specific items
                    string baseType = compatibleTag.Replace("_base", "");
                    if (itemTags.Contains(baseType))
                        return true;
                }
            }
            
            return false;
        }
        
        // Fallback to legacy tag system
        if (affix.requiredTags != null && affix.requiredTags.Count > 0)
        {
            List<string> itemTags = GetItemTags(itemType);
            return affix.requiredTags.Any(tag => itemTags.Contains(tag));
        }
        
        return true; // No restrictions
    }
    
    /// <summary>
    /// Get random prefix that's compatible with the specific item
    /// </summary>
    public Affix GetRandomCompatiblePrefix(BaseItem item, int itemLevel, AffixTier maxTier = AffixTier.Tier1)
    {
        List<Affix> availablePrefixes = GetAvailableCompatiblePrefixes(item, itemLevel, maxTier);
        
        if (availablePrefixes.Count == 0)
            return null;
            
        // Weighted random selection
        float totalWeight = availablePrefixes.Sum(a => a.weight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var affix in availablePrefixes)
        {
            currentWeight += affix.weight;
            if (randomValue <= currentWeight)
                return affix;
        }
        
        return availablePrefixes[0]; // Fallback
    }
    
    /// <summary>
    /// Get random suffix that's compatible with the specific item
    /// </summary>
    public Affix GetRandomCompatibleSuffix(BaseItem item, int itemLevel, AffixTier maxTier = AffixTier.Tier1)
    {
        List<Affix> availableSuffixes = GetAvailableCompatibleSuffixes(item, itemLevel, maxTier);
        
        if (availableSuffixes.Count == 0)
            return null;
            
        // Weighted random selection
        float totalWeight = availableSuffixes.Sum(a => a.weight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var affix in availableSuffixes)
        {
            currentWeight += affix.weight;
            if (randomValue <= currentWeight)
                return affix;
        }
        
        return availableSuffixes[0]; // Fallback
    }
    
    /// <summary>
    /// Get available prefixes that are compatible with the specific item
    /// </summary>
    private List<Affix> GetAvailableCompatiblePrefixes(BaseItem item, int itemLevel, AffixTier maxTier)
    {
        List<Affix> allPrefixes = new List<Affix>();
        
        switch (item.itemType)
        {
            case ItemType.Weapon:
                foreach (var category in weaponPrefixCategories)
                {
                    allPrefixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Armour:
                foreach (var category in armourPrefixCategories)
                {
                    allPrefixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Accessory:
                foreach (var category in jewelleryPrefixCategories)
                {
                    allPrefixes.AddRange(category.GetAllAffixes());
                }
                break;
        }
        
        // Filter by tier requirements, level requirements, and smart compatibility with the specific item
        return allPrefixes.Where(a => 
            (int)a.tier <= (int)maxTier && 
            a.minLevel <= itemLevel &&
            IsAffixCompatibleWithItem(a, item)
        ).ToList();
    }
    
    /// <summary>
    /// Get available suffixes that are compatible with the specific item
    /// </summary>
    private List<Affix> GetAvailableCompatibleSuffixes(BaseItem item, int itemLevel, AffixTier maxTier)
    {
        List<Affix> allSuffixes = new List<Affix>();
        
        switch (item.itemType)
        {
            case ItemType.Weapon:
                foreach (var category in weaponSuffixCategories)
                {
                    allSuffixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Armour:
                foreach (var category in armourSuffixCategories)
                {
                    allSuffixes.AddRange(category.GetAllAffixes());
                }
                break;
            case ItemType.Accessory:
                foreach (var category in jewellerySuffixCategories)
                {
                    allSuffixes.AddRange(category.GetAllAffixes());
                }
                break;
        }
        
        // Filter by tier requirements, level requirements, and smart compatibility with the specific item
        return allSuffixes.Where(a => 
            (int)a.tier <= (int)maxTier && 
            a.minLevel <= itemLevel &&
            IsAffixCompatibleWithItem(a, item)
        ).ToList();
    }

    // ===== PoE CONVERTER METHODS =====
    
    /// <summary>
    /// Converts PoE Lua table format to our import format
    /// </summary>


    

}
