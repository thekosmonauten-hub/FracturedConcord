using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Defines affixes that can be crafted onto equipment at the Maze Forge.
    /// These are predetermined affixes that players can select and apply using maze currencies.
    /// </summary>
    [System.Serializable]
    public class MazeForgeAffix
    {
        [Header("Affix Information")]
        [Tooltip("Display name of the affix")]
        public string affixName;
        
        [Tooltip("Description of what this affix does")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("The actual Affix data to apply to equipment")]
        public Affix affixData;
        
        [Header("Crafting Requirements")]
        [Tooltip("Currency tier required to craft this affix (1-4)")]
        [Range(1, 4)]
        public int requiredCurrencyTier = 1;
        
        [Tooltip("Amount of currency required to craft")]
        [Range(1, 100)]
        public int currencyCost = 10;
        
        [Tooltip("Can this be a prefix?")]
        public bool canBePrefix = true;
        
        [Tooltip("Can this be a suffix?")]
        public bool canBeSuffix = true;
        
        [Header("Item Restrictions")]
        [Tooltip("Item types this affix can be applied to")]
        public List<ItemType> allowedItemTypes = new List<ItemType>();
        
        [Tooltip("Equipment types this affix can be applied to (if applicable)")]
        public List<EquipmentType> allowedEquipmentTypes = new List<EquipmentType>();
        
        [Tooltip("Minimum item level required")]
        [Range(1, 100)]
        public int minItemLevel = 1;
        
        [Header("Visual")]
        [Tooltip("Icon for this affix in the forge UI")]
        public Sprite affixIcon;
        
        [Tooltip("Rarity color for display")]
        public Color affixColor = Color.white;
    }
    
    /// <summary>
    /// Database of affixes available for crafting at the Maze Forge.
    /// Organized by currency tier and item type.
    /// </summary>
    [CreateAssetMenu(fileName = "Maze Forge Affix Database", menuName = "Dexiled/Maze Forge Affix Database", order = 4)]
    public class MazeForgeAffixDatabase : ScriptableObject
    {
        [Header("Tier 1 Affixes (Mandate Fragments)")]
        [Tooltip("Affixes craftable with Mandate Fragments")]
        public List<MazeForgeAffix> tier1Affixes = new List<MazeForgeAffix>();
        
        [Header("Tier 2 Affixes (Shattered Sigils)")]
        [Tooltip("Affixes craftable with Shattered Sigils")]
        public List<MazeForgeAffix> tier2Affixes = new List<MazeForgeAffix>();
        
        [Header("Tier 3 Affixes (Contradiction Cores)")]
        [Tooltip("Affixes craftable with Contradiction Cores")]
        public List<MazeForgeAffix> tier3Affixes = new List<MazeForgeAffix>();
        
        [Header("Tier 4 Affixes (Collapse Motifs)")]
        [Tooltip("Affixes craftable with Collapse Motifs")]
        public List<MazeForgeAffix> tier4Affixes = new List<MazeForgeAffix>();
        
        // Singleton instance
        private static MazeForgeAffixDatabase _instance;
        public static MazeForgeAffixDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<MazeForgeAffixDatabase>("MazeForgeAffixDatabase");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[MazeForgeAffixDatabase] Database not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Gets all affixes available for a specific currency tier.
        /// </summary>
        public List<MazeForgeAffix> GetAffixesForTier(int tier)
        {
            switch (tier)
            {
                case 1: return tier1Affixes;
                case 2: return tier2Affixes;
                case 3: return tier3Affixes;
                case 4: return tier4Affixes;
                default: return new List<MazeForgeAffix>();
            }
        }
        
        /// <summary>
        /// Gets affixes that can be applied to a specific item type and equipment type.
        /// </summary>
        public List<MazeForgeAffix> GetAffixesForItem(BaseItem item, int maxTier = 4)
        {
            if (item == null) return new List<MazeForgeAffix>();
            
            List<MazeForgeAffix> compatibleAffixes = new List<MazeForgeAffix>();
            
            // Check all tiers up to maxTier
            for (int tier = 1; tier <= maxTier; tier++)
            {
                List<MazeForgeAffix> tierAffixes = GetAffixesForTier(tier);
                
                foreach (MazeForgeAffix forgeAffix in tierAffixes)
                {
                    // Check item type compatibility
                    if (forgeAffix.allowedItemTypes != null && forgeAffix.allowedItemTypes.Count > 0)
                    {
                        if (!forgeAffix.allowedItemTypes.Contains(item.itemType))
                            continue;
                    }
                    
                    // Check equipment type compatibility (if applicable)
                    if (item is BaseItem baseItem && forgeAffix.allowedEquipmentTypes != null && forgeAffix.allowedEquipmentTypes.Count > 0)
                    {
                        // Only check equipment type if the affix has restrictions
                        if (!forgeAffix.allowedEquipmentTypes.Contains(baseItem.equipmentType))
                            continue;
                    }
                    
                    // Check item level requirement
                    if (item.requiredLevel < forgeAffix.minItemLevel)
                        continue;
                    
                    compatibleAffixes.Add(forgeAffix);
                }
            }
            
            return compatibleAffixes;
        }
        
        /// <summary>
        /// Gets affixes that can be applied as a prefix to a specific item.
        /// </summary>
        public List<MazeForgeAffix> GetPrefixAffixesForItem(BaseItem item, int maxTier = 4)
        {
            return GetAffixesForItem(item, maxTier).Where(a => a.canBePrefix).ToList();
        }
        
        /// <summary>
        /// Gets affixes that can be applied as a suffix to a specific item.
        /// </summary>
        public List<MazeForgeAffix> GetSuffixAffixesForItem(BaseItem item, int maxTier = 4)
        {
            return GetAffixesForItem(item, maxTier).Where(a => a.canBeSuffix).ToList();
        }
        
        /// <summary>
        /// Gets the total count of affixes in the database.
        /// </summary>
        public int GetTotalAffixCount()
        {
            return tier1Affixes.Count + tier2Affixes.Count + tier3Affixes.Count + tier4Affixes.Count;
        }
    }
}

