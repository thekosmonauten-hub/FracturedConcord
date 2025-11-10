using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Modern Affix Database - Uses only compatibleTags and Local/Global modifier system
/// Created to replace legacy AffixDatabase with clean, modern architecture
/// </summary>
[CreateAssetMenu(fileName = "Affix Database Modern", menuName = "Dexiled/Items/Affix Database (Modern)")]
public class AffixDatabase_Modern : ScriptableObject
{
    [Header("Weapon Affixes")]
    public List<AffixCategory> weaponPrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> weaponSuffixCategories = new List<AffixCategory>();
    
    [Header("Armour Affixes")]
    public List<AffixCategory> armourPrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> armourSuffixCategories = new List<AffixCategory>();
    
    [Header("Jewellery Affixes")]
    public List<AffixCategory> jewelleryPrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> jewellerySuffixCategories = new List<AffixCategory>();
    
    // Singleton instance
    private static AffixDatabase_Modern _instance;
    public static AffixDatabase_Modern Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<AffixDatabase_Modern>("AffixDatabase");
                if (_instance == null)
                {
                    Debug.LogError("[AffixDatabase_Modern] AffixDatabase not found in Resources folder! Make sure it's named 'AffixDatabase.asset'");
                }
                else
                {
                    Debug.Log($"[AffixDatabase_Modern] Loaded database with {_instance.GetTotalAffixCount()} total affixes");
                }
            }
            return _instance;
        }
    }
    
    #region Public API - Item Affix Generation
    
    /// <summary>
    /// Generate random affixes for an item based on rarity chances
    /// </summary>
    public void GenerateRandomAffixes(BaseItem item, int itemLevel, float magicChance = 0.25f, float rareChance = 0.05f)
    {
        if (item == null)
        {
            Debug.LogError("[AffixDatabase_Modern] Cannot generate affixes for null item!");
            return;
        }
        
        if (item.isUnique)
            return; // Unique items have fixed affixes
            
        item.ClearAffixes();
        
        float random = Random.Range(0f, 1f);
        
        if (random < rareChance)
        {
            GenerateRareAffixes(item, itemLevel);
        }
        else if (random < magicChance)
        {
            GenerateMagicAffixes(item, itemLevel);
        }
        // Otherwise, Normal item (0 affixes)
    }
    
    /// <summary>
    /// Generate affixes with forced rarity (for testing)
    /// </summary>
    public void GenerateRandomAffixes(BaseItem item, int itemLevel, ItemRarity forcedRarity)
    {
        if (item == null)
        {
            Debug.LogError("[AffixDatabase_Modern] Cannot generate affixes for null item!");
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
    
    #endregion
    
    #region Private - Affix Generation
    
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
        AffixTier maxTier = GetMaxTierForLevel(itemLevel);
        
        // Generate prefixes
        for (int i = 0; i < prefixCount && item.CanAddPrefix(); i++)
        {
            Affix prefix = GetRandomCompatiblePrefix(item, itemLevel, maxTier);
            if (prefix != null)
            {
                item.AddPrefix(prefix);
            }
            else
            {
                Debug.LogWarning($"[AffixDatabase_Modern] No compatible prefix found for {item.itemName} (Level {itemLevel})");
            }
        }
        
        // Generate suffixes
        for (int i = 0; i < suffixCount && item.CanAddSuffix(); i++)
        {
            Affix suffix = GetRandomCompatibleSuffix(item, itemLevel, maxTier);
            if (suffix != null)
            {
                item.AddSuffix(suffix);
            }
            else
            {
                Debug.LogWarning($"[AffixDatabase_Modern] No compatible suffix found for {item.itemName} (Level {itemLevel})");
            }
        }
    }
    
    private AffixTier GetMaxTierForLevel(int itemLevel)
    {
        // Tier calculation based on item level
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
    
    #endregion
    
    #region Public API - Random Affix Selection
    
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
    
    #endregion
    
    #region Private - Available Affix Filtering
    
    /// <summary>
    /// Get available prefixes that are compatible with the specific item
    /// </summary>
    private List<Affix> GetAvailableCompatiblePrefixes(BaseItem item, int itemLevel, AffixTier maxTier)
    {
        List<Affix> allPrefixes = new List<Affix>();
        
        // Get all prefixes for the item type
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
        
        // Filter by tier, level, and smart compatibility
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
        
        // Get all suffixes for the item type
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
        
        // Filter by tier, level, and smart compatibility
        return allSuffixes.Where(a => 
            (int)a.tier <= (int)maxTier && 
            a.minLevel <= itemLevel &&
            IsAffixCompatibleWithItem(a, item)
        ).ToList();
    }
    
    #endregion
    
    #region Smart Compatibility System
    
    /// <summary>
    /// MODERN: Smart compatibility checking using compatibleTags and Local/Global modifier rules
    /// This is the ONLY compatibility method - no legacy requiredTags!
    /// </summary>
    public static bool IsAffixCompatibleWithItem(Affix affix, BaseItem item)
    {
        // Step 1: Check Local/Global modifier compatibility
        foreach (AffixModifier modifier in affix.modifiers)
        {
            if (!IsModifierCompatibleWithItem(modifier, item))
            {
                return false;
            }
        }
        
        // Step 2: Check compatible tags (modern tag system)
        if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
        {
            // No tags = no restrictions (universal affix)
            return true;
        }
        
        // Check if item matches ANY compatible tag
        bool hasMatchingTag = false;
        
        foreach (string requiredTag in affix.compatibleTags)
        {
            // Check base stat requirements (e.g., "armour_base", "energyshield_base")
            if (requiredTag.EndsWith("_base"))
            {
                if (HasRequiredBaseStat(item, requiredTag))
                {
                    hasMatchingTag = true;
                    break;
                }
            }
            // Check item tags (e.g., "helmet", "weapon", "shield")
            else if (item.itemTags != null && item.itemTags.Contains(requiredTag))
            {
                hasMatchingTag = true;
                break;
            }
        }
        
        return hasMatchingTag;
    }
    
    /// <summary>
    /// Check if item has the required base stat for base-type specific affixes
    /// </summary>
    private static bool HasRequiredBaseStat(BaseItem item, string baseTag)
    {
        if (!(item is Armour armour))
            return false;
        
        switch (baseTag)
        {
            case "armour_base":
                return armour.armour > 0;
            case "evasion_base":
                return armour.evasion > 0;
            case "energyshield_base":
                return armour.energyShield > 0;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Check if a specific modifier is compatible based on Local vs Global rules
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
    /// Global modifiers - affect character stats, typically roll on armor/jewelry
    /// </summary>
    private static bool IsGlobalModifierCompatible(AffixModifier modifier, BaseItem item)
    {
        string statName = modifier.statName.ToLower();
        
        // Global damage mods should NOT roll on weapons (use local instead)
        if (item is WeaponItem)
        {
            if (statName.Contains("physicaldamage") || statName.Contains("firedamage") ||
                statName.Contains("colddamage") || statName.Contains("lightningdamage") ||
                statName.Contains("chaosdamage") || statName.Contains("criticalchance") ||
                statName.Contains("attackspeed") || statName.Contains("castspeed"))
            {
                return false; // These should be local on weapons
            }
        }
        
        // Global mods are generally allowed on armor and jewelry
        return true;
    }
    
    /// <summary>
    /// Local modifiers - directly modify item base stats, must have relevant base stat
    /// </summary>
    private static bool IsLocalModifierCompatible(AffixModifier modifier, BaseItem item)
    {
        string statName = modifier.statName.ToLower();
        
        // Weapon-specific local mods
        if (statName.Contains("physicaldamage") || statName.Contains("firedamage") ||
            statName.Contains("colddamage") || statName.Contains("lightningdamage") ||
            statName.Contains("chaosdamage"))
        {
            return item is WeaponItem;
        }
        
        if (statName.Contains("criticalchance") || statName.Contains("criticalstrikechance"))
        {
            return item is WeaponItem weapon && weapon.criticalStrikeChance > 0;
        }
        
        if (statName.Contains("attackspeed"))
        {
            return item is WeaponItem;
        }
        
        if (statName.Contains("castspeed"))
        {
            return item is WeaponItem weapon && weapon.itemTags.Contains("spell");
        }
        
        // Armor-specific local mods
        if (statName == "armour" || statName.Contains("increased") && statName.Contains("armour"))
        {
            return item is Armour armour && armour.armour > 0;
        }
        
        if (statName == "evasion" || statName.Contains("increased") && statName.Contains("evasion"))
        {
            return item is Armour armour && armour.evasion > 0;
        }
        
        if (statName == "energyshield" || statName.Contains("increased") && statName.Contains("energyshield"))
        {
            return item is Armour armour && armour.energyShield > 0;
        }
        
        if (statName.Contains("blockchance"))
        {
            return item is Armour armour && armour.itemTags.Contains("shield");
        }
        
        // Default: allow if no specific restrictions
        return true;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get total affix count for diagnostics
    /// </summary>
    public int GetTotalAffixCount()
    {
        int count = 0;
        
        foreach (var category in weaponPrefixCategories)
            count += category.GetAllAffixes().Count;
        foreach (var category in weaponSuffixCategories)
            count += category.GetAllAffixes().Count;
        foreach (var category in armourPrefixCategories)
            count += category.GetAllAffixes().Count;
        foreach (var category in armourSuffixCategories)
            count += category.GetAllAffixes().Count;
        foreach (var category in jewelleryPrefixCategories)
            count += category.GetAllAffixes().Count;
        foreach (var category in jewellerySuffixCategories)
            count += category.GetAllAffixes().Count;
        
        return count;
    }
    
    /// <summary>
    /// Get affix counts by category for diagnostics
    /// </summary>
    public void GetAffixCounts(out int weaponPrefixes, out int weaponSuffixes, 
                               out int armourPrefixes, out int armourSuffixes,
                               out int jewelleryPrefixes, out int jewellerySuffixes)
    {
        weaponPrefixes = weaponPrefixCategories.Sum(c => c.GetAllAffixes().Count);
        weaponSuffixes = weaponSuffixCategories.Sum(c => c.GetAllAffixes().Count);
        armourPrefixes = armourPrefixCategories.Sum(c => c.GetAllAffixes().Count);
        armourSuffixes = armourSuffixCategories.Sum(c => c.GetAllAffixes().Count);
        jewelleryPrefixes = jewelleryPrefixCategories.Sum(c => c.GetAllAffixes().Count);
        jewellerySuffixes = jewellerySuffixCategories.Sum(c => c.GetAllAffixes().Count);
    }
    
    #endregion
}








