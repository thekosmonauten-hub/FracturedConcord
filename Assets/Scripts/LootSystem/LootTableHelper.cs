using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Helper methods for configuring loot tables
/// </summary>
public static class LootTableHelper
{
    /// <summary>
    /// Create a guaranteed currency drop entry
    /// </summary>
    public static LootEntry CreateGuaranteedCurrencyDrop(CurrencyType currencyType, int minQty = 1, int maxQty = 1)
    {
        return new LootEntry
        {
            rewardType = RewardType.Currency,
            currencyType = currencyType,
            dropChance = 100f,
            minQuantity = minQty,
            maxQuantity = maxQty
        };
    }
    
    /// <summary>
    /// Create a random currency drop entry
    /// </summary>
    public static LootEntry CreateRandomCurrencyDrop(CurrencyType currencyType, float dropChance, int minQty = 1, int maxQty = 1)
    {
        return new LootEntry
        {
            rewardType = RewardType.Currency,
            currencyType = currencyType,
            dropChance = dropChance,
            minQuantity = minQty,
            maxQuantity = maxQty
        };
    }
    
    /// <summary>
    /// Create an item drop entry
    /// </summary>
    public static LootEntry CreateItemDrop(ItemData itemData, float dropChance)
    {
        return new LootEntry
        {
            rewardType = RewardType.Item,
            itemData = itemData,
            dropChance = dropChance,
            minQuantity = 1,
            maxQuantity = 1
        };
    }
    
    /// <summary>
    /// Quick setup for common currency rewards
    /// </summary>
    public static List<LootEntry> CreateBasicCurrencyRewards()
    {
        return new List<LootEntry>
        {
            CreateGuaranteedCurrencyDrop(CurrencyType.OrbOfGeneration, 1, 3),
            CreateRandomCurrencyDrop(CurrencyType.OrbOfInfusion, 25f, 1, 1),
            CreateRandomCurrencyDrop(CurrencyType.FireSpirit, 15f, 1, 1)
        };
    }
    
    /// <summary>
    /// Quick setup for boss currency rewards
    /// </summary>
    public static List<LootEntry> CreateBossCurrencyRewards()
    {
        return new List<LootEntry>
        {
            CreateGuaranteedCurrencyDrop(CurrencyType.OrbOfGeneration, 3, 5),
            CreateGuaranteedCurrencyDrop(CurrencyType.OrbOfPerfection, 2, 3),
            CreateRandomCurrencyDrop(CurrencyType.OrbOfMutation, 50f, 1, 1),
            CreateRandomCurrencyDrop(CurrencyType.DivineSpirit, 10f, 1, 1)
        };
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor utilities for loot tables
/// </summary>
public static class LootTableEditorHelper
{
    /// <summary>
    /// Get all currency types as display names
    /// </summary>
    public static string[] GetCurrencyNames()
    {
        return System.Enum.GetNames(typeof(CurrencyType));
    }
    
    /// <summary>
    /// Get currency type description from database
    /// </summary>
    public static string GetCurrencyDescription(CurrencyType currencyType)
    {
        var db = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        if (db != null)
        {
            var currency = db.GetCurrency(currencyType);
            return currency?.description ?? "No description";
        }
        return "Database not found";
    }
}
#endif













