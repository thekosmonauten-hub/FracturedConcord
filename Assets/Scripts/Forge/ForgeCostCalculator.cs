using UnityEngine;
using Dexiled.Data.Items;

/// <summary>
/// Calculates recommended material costs for crafting recipes based on item level and rarity
/// Uses a hybrid formula that scales with level and rarity
/// </summary>
public static class ForgeCostCalculator
{
    /// <summary>
    /// Cost calculation configuration
    /// </summary>
    public struct CostConfig
    {
        public float baseCost;
        public float levelScaling; // How much cost increases per level
        public float normalMultiplier;
        public float magicMultiplier;
        public float rareMultiplier;
        public float uniqueMultiplier;
        
        public static CostConfig Default => new CostConfig
        {
            baseCost = 3f,
            levelScaling = 0.2f, // +0.2 per level (so level 10 = +2, level 20 = +4)
            normalMultiplier = 1.0f,
            magicMultiplier = 1.2f,
            rareMultiplier = 1.8f,
            uniqueMultiplier = 2.5f
        };
        
        public static CostConfig Alternative => new CostConfig
        {
            baseCost = 3f,
            levelScaling = 0.1f, // Gentler scaling
            normalMultiplier = 1.0f,
            magicMultiplier = 1.1f,
            rareMultiplier = 1.5f,
            uniqueMultiplier = 2.0f
        };
        
        public static CostConfig Expensive => new CostConfig
        {
            baseCost = 5f,
            levelScaling = 0.3f,
            normalMultiplier = 1.0f,
            magicMultiplier = 1.3f,
            rareMultiplier = 2.0f,
            uniqueMultiplier = 3.0f
        };
    }
    
    /// <summary>
    /// Calculate recommended material cost for crafting an item
    /// </summary>
    /// <param name="itemLevel">Required level of the item to craft</param>
    /// <param name="itemRarity">Rarity of the item to craft</param>
    /// <param name="config">Cost configuration (uses Default if null)</param>
    /// <returns>Recommended material cost</returns>
    public static int CalculateCost(int itemLevel, ItemRarity itemRarity, CostConfig? config = null)
    {
        if (itemLevel < 1) itemLevel = 1;
        
        CostConfig cfg = config ?? CostConfig.Default;
        
        // Base cost with level scaling: baseCost + (level * levelScaling)
        float baseCost = cfg.baseCost + (itemLevel * cfg.levelScaling);
        
        // Apply rarity multiplier
        float rarityMultiplier = GetRarityMultiplier(itemRarity, cfg);
        
        // Final cost
        float finalCost = baseCost * rarityMultiplier;
        
        // Round up to ensure it's always meaningful
        return Mathf.Max(1, Mathf.CeilToInt(finalCost));
    }
    
    /// <summary>
    /// Calculate recommended material cost for a crafting recipe
    /// Uses the recipe's minItemLevel and craftedRarity
    /// </summary>
    public static int CalculateCostForRecipe(CraftingRecipe recipe, CostConfig? config = null)
    {
        if (recipe == null) return 3; // Default fallback
        
        int itemLevel = recipe.minItemLevel;
        ItemRarity rarity = recipe.craftedRarity;
        
        return CalculateCost(itemLevel, rarity, config);
    }
    
    /// <summary>
    /// Calculate recommended material cost for a specific item
    /// Uses the item's requiredLevel and rarity
    /// </summary>
    public static int CalculateCostForItem(BaseItem item, CostConfig? config = null)
    {
        if (item == null) return 3; // Default fallback
        
        int itemLevel = item.requiredLevel;
        ItemRarity rarity = item.rarity;
        
        return CalculateCost(itemLevel, rarity, config);
    }
    
    /// <summary>
    /// Get rarity multiplier from config
    /// </summary>
    private static float GetRarityMultiplier(ItemRarity rarity, CostConfig config)
    {
        switch (rarity)
        {
            case ItemRarity.Normal:
                return config.normalMultiplier;
            case ItemRarity.Magic:
                return config.magicMultiplier;
            case ItemRarity.Rare:
                return config.rareMultiplier;
            case ItemRarity.Unique:
                return config.uniqueMultiplier;
            default:
                return config.normalMultiplier;
        }
    }
    
    /// <summary>
    /// Get estimated number of items needed to salvage to craft
    /// Uses average salvage yields for the given level and rarity
    /// </summary>
    public static float EstimateItemsToSalvage(int itemLevel, ItemRarity itemRarity)
    {
        // Use the salvage system's formula to estimate yield per item
        float baseQuantity = 1f;
        float rarityMultiplier = GetSalvageRarityMultiplier(itemRarity);
        float levelMultiplier = 1f + (itemLevel * 0.1f);
        
        // Average yield per item (assuming normal quality = 0)
        float averageYield = baseQuantity * rarityMultiplier * levelMultiplier;
        
        // Calculate cost for this item
        int cost = CalculateCost(itemLevel, itemRarity);
        
        // How many items needed
        if (averageYield <= 0) return float.MaxValue;
        return cost / averageYield;
    }
    
    /// <summary>
    /// Get salvage rarity multiplier (matches ForgeSalvageSystem)
    /// </summary>
    private static float GetSalvageRarityMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal:
                return 1f;
            case ItemRarity.Magic:
                return 1.5f;
            case ItemRarity.Rare:
                return 2.5f;
            case ItemRarity.Unique:
                return 4f;
            default:
                return 1f;
        }
    }
    
    /// <summary>
    /// Generate a cost breakdown string for display
    /// </summary>
    public static string GetCostBreakdown(int itemLevel, ItemRarity itemRarity, CostConfig? config = null)
    {
        int cost = CalculateCost(itemLevel, itemRarity, config);
        float itemsNeeded = EstimateItemsToSalvage(itemLevel, itemRarity);
        
        return $"Cost: {cost} materials (~{itemsNeeded:F1} items to salvage)";
    }
}


