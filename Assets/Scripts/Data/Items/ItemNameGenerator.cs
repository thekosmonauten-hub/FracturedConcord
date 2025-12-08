using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Dexiled.Data.Items
{
    /// <summary>
    /// Utility class for generating item names based on rarity and affixes
    /// Magic items: Use affix names (e.g., "Tainted Worn Hatchet of the Cat")
    /// Rare items: Use random 2-word names (e.g., "Forsaken Edge")
    /// </summary>
    public static class ItemNameGenerator
    {
        /// <summary>
        /// Generate a full display name for an item based on its rarity
        /// </summary>
        public static string GenerateItemName(BaseItem item, NameGenerationData nameData = null)
        {
            if (item == null) return "Unknown Item";
            
            switch (item.rarity)
            {
                case ItemRarity.Normal:
                    // Normal items just use their base name
                    return item.itemName;
                
                case ItemRarity.Magic:
                    // Magic items use affix names: "[Prefix] BaseName [of Suffix]"
                    return GenerateMagicItemName(item);
                
                case ItemRarity.Rare:
                    // Rare items use random 2-word names: "[RandomPrefix] [RandomSuffix]"
                    return GenerateRareItemName(item, nameData);
                
                case ItemRarity.Unique:
                    // Unique items should have predefined names
                    return item.itemName;
                
                default:
                    return item.itemName;
            }
        }
        
        /// <summary>
        /// Generate name for Magic (blue) items using affix names
        /// Format: "[Prefix] BaseName [of Suffix]"
        /// </summary>
        private static string GenerateMagicItemName(BaseItem item)
        {
            StringBuilder nameBuilder = new StringBuilder();
            
            // Get first prefix and suffix
            Affix firstPrefix = null;
            Affix firstSuffix = null;
            
            if (item is WeaponItem weapon)
            {
                if (weapon.prefixes != null && weapon.prefixes.Count > 0)
                    firstPrefix = weapon.prefixes[0];
                if (weapon.suffixes != null && weapon.suffixes.Count > 0)
                    firstSuffix = weapon.suffixes[0];
            }
            else if (item is Armour armour)
            {
                if (armour.prefixes != null && armour.prefixes.Count > 0)
                    firstPrefix = armour.prefixes[0];
                if (armour.suffixes != null && armour.suffixes.Count > 0)
                    firstSuffix = armour.suffixes[0];
            }
            
            // Build name: "[Prefix] BaseName [of Suffix]"
            if (firstPrefix != null)
            {
                nameBuilder.Append(firstPrefix.name);
                nameBuilder.Append(" ");
            }
            
            nameBuilder.Append(item.itemName);
            
            if (firstSuffix != null)
            {
                nameBuilder.Append(" of ");
                nameBuilder.Append(firstSuffix.name);
            }
            
            return nameBuilder.ToString();
        }
        
        /// <summary>
        /// Generate name for Rare (gold) items using random prefix + suffix
        /// Format: "[RandomPrefix] [RandomSuffix]"
        /// </summary>
        private static string GenerateRareItemName(BaseItem item, NameGenerationData nameData)
        {
            if (nameData == null)
            {
                Debug.LogWarning("[ItemNameGenerator] NameGenerationData is null. Using fallback name.");
                return $"Rare {item.itemName}";
            }
            
            // Validate prefix pool
            if (nameData.rarePrefixes == null || nameData.rarePrefixes.Count == 0)
            {
                Debug.LogWarning("[ItemNameGenerator] Rare prefix pool is empty!");
                return $"Rare {item.itemName}";
            }
            
            // Get appropriate suffix pool for this item type
            List<string> suffixPool = nameData.GetSuffixPoolForItem(item);
            
            if (suffixPool == null || suffixPool.Count == 0)
            {
                Debug.LogWarning($"[ItemNameGenerator] No suffix pool found for item type: {item.GetType().Name}");
                return $"Rare {item.itemName}";
            }
            
            // Pick random prefix and suffix
            string randomPrefix = nameData.rarePrefixes[Random.Range(0, nameData.rarePrefixes.Count)];
            string randomSuffix = suffixPool[Random.Range(0, suffixPool.Count)];
            
            return $"{randomPrefix} {randomSuffix}";
        }
        
        /// <summary>
        /// Generate name with seed for reproducible results
        /// </summary>
        public static string GenerateItemName(BaseItem item, NameGenerationData nameData, int seed)
        {
            if (item == null) return "Unknown Item";
            
            // Use seed for rare items only (magic items use affix names)
            if (item.rarity == ItemRarity.Rare && nameData != null)
            {
                System.Random random = new System.Random(seed);
                
                if (nameData.rarePrefixes.Count > 0)
                {
                    List<string> suffixPool = nameData.GetSuffixPoolForItem(item);
                    if (suffixPool != null && suffixPool.Count > 0)
                    {
                        string randomPrefix = nameData.rarePrefixes[random.Next(0, nameData.rarePrefixes.Count)];
                        string randomSuffix = suffixPool[random.Next(0, suffixPool.Count)];
                        return $"{randomPrefix} {randomSuffix}";
                    }
                }
            }
            
            // Fallback to non-seeded generation
            return GenerateItemName(item, nameData);
        }
    }
}

