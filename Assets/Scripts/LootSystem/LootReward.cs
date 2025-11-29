using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Types of rewards that can drop from combat
/// </summary>
public enum RewardType
{
    Currency,
    Item,
    Experience,
    Card,
    Effigy,
    Warrant
}

/// <summary>
/// A single loot reward (currency, item, exp, etc.)
/// </summary>
[System.Serializable]
public class LootReward
{
    public RewardType rewardType;
    
    // Currency reward
    public CurrencyType currencyType;
    public int currencyAmount;
    
    // Item reward
    public ItemData itemData;
    public BaseItem itemInstance;

    // Effigy reward
    public Effigy effigyInstance;
    
    // Experience reward
    public int experienceAmount;
    
    // Card reward (future)
    public string cardName;
    
    // Warrant reward
    public WarrantDefinition warrantBlueprint; // The blueprint to roll from
    public WarrantDefinition warrantInstance; // The rolled instance (set after rolling)
    
    // Display info
    public string GetDisplayName()
    {
        switch (rewardType)
        {
            case RewardType.Currency:
                return $"{currencyAmount}x {currencyType}";
            case RewardType.Item:
                return itemData != null ? itemData.itemName : "Unknown Item";
            case RewardType.Effigy:
                if (effigyInstance == null) return "Unknown Effigy";
                string alias = string.IsNullOrEmpty(effigyInstance.displayAlias) ? effigyInstance.effigyName : effigyInstance.displayAlias;
                return $"{effigyInstance.GetRarityName()} {alias}";
            case RewardType.Experience:
                return $"{experienceAmount} Experience";
            case RewardType.Card:
                return cardName;
            case RewardType.Warrant:
                if (warrantInstance != null)
                    return $"Warrant: {warrantInstance.displayName}";
                if (warrantBlueprint != null)
                    return $"Warrant Blueprint: {warrantBlueprint.displayName}";
                return "Warrant";
            default:
                return "Unknown Reward";
        }
    }
    
    public Sprite GetIcon()
    {
        switch (rewardType)
        {
            case RewardType.Currency:
                var currencyDb = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
                if (currencyDb != null)
                {
                    var currencyData = currencyDb.GetCurrency(currencyType);
                    return currencyData?.currencySprite;
                }
                return null;
            case RewardType.Item:
                return itemData?.itemSprite;
            case RewardType.Effigy:
                return effigyInstance != null ? effigyInstance.icon : null;
            case RewardType.Experience:
                // TODO: Add experience icon
                return null;
            case RewardType.Card:
                // TODO: Add card icon
                return null;
            case RewardType.Warrant:
                if (warrantInstance != null)
                    return warrantInstance.icon;
                if (warrantBlueprint != null)
                    return warrantBlueprint.icon;
                return null;
            default:
                return null;
        }
    }
}

/// <summary>
/// Collection of rewards from a combat encounter
/// </summary>
[System.Serializable]
public class LootDropResult
{
    public List<LootReward> rewards = new List<LootReward>();
    public int totalExperience = 0;
    
    public void AddReward(LootReward reward)
    {
        rewards.Add(reward);
        
        // Track total experience
        if (reward.rewardType == RewardType.Experience)
        {
            totalExperience += reward.experienceAmount;
        }
    }
    
    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        rewards.Add(new LootReward
        {
            rewardType = RewardType.Currency,
            currencyType = currencyType,
            currencyAmount = amount
        });
    }
    
    public void AddExperience(int amount)
    {
        rewards.Add(new LootReward
        {
            rewardType = RewardType.Experience,
            experienceAmount = amount
        });
        totalExperience += amount;
    }
    
    public void AddItem(ItemData itemData)
    {
        AddItem(itemData, null);
    }

    public void AddItem(ItemData itemData, BaseItem baseItem)
    {
        rewards.Add(new LootReward
        {
            rewardType = RewardType.Item,
            itemData = itemData,
            itemInstance = baseItem
        });
    }
    
    public void AddEffigy(Effigy effigy)
    {
        if (effigy == null)
            return;
        
        rewards.Add(new LootReward
        {
            rewardType = RewardType.Effigy,
            effigyInstance = effigy
        });
    }
    
    public void AddWarrant(WarrantDefinition blueprint)
    {
        if (blueprint == null)
            return;
        
        rewards.Add(new LootReward
        {
            rewardType = RewardType.Warrant,
            warrantBlueprint = blueprint
        });
    }
    
    public int GetRewardCount()
    {
        return rewards.Count;
    }
    
    /// <summary>
    /// Apply quantity multiplier to all currency and item rewards
    /// Multiplies currency amounts and duplicates items
    /// </summary>
    public void ApplyQuantityMultiplier(float multiplier)
    {
        if (multiplier <= 1f) return; // No change if multiplier is 1 or less
        
        List<LootReward> newRewards = new List<LootReward>();
        
        foreach (var reward in rewards)
        {
            if (reward.rewardType == RewardType.Currency)
            {
                // Multiply currency amounts
                int newAmount = Mathf.RoundToInt(reward.currencyAmount * multiplier);
                if (newAmount > 0)
                {
                    newRewards.Add(new LootReward
                    {
                        rewardType = RewardType.Currency,
                        currencyType = reward.currencyType,
                        currencyAmount = newAmount
                    });
                }
            }
            else if (reward.rewardType == RewardType.Item)
            {
                // Duplicate items based on multiplier (round up for partial multipliers)
                int itemCount = Mathf.CeilToInt(multiplier);
                for (int i = 0; i < itemCount; i++)
                {
                    newRewards.Add(new LootReward
                    {
                        rewardType = RewardType.Item,
                        itemData = reward.itemData,
                        itemInstance = reward.itemInstance
                    });
                }
            }
            else
            {
                // Other reward types are not multiplied (Experience, Cards, etc.)
                newRewards.Add(reward);
            }
        }
        
        rewards = newRewards;
        
        Debug.Log($"[LootDropResult] Applied quantity multiplier {multiplier:F2}x: {rewards.Count} rewards");
    }
    
    /// <summary>
    /// Apply rarity multiplier to item rewards
    /// This affects the chance of higher rarity items, not the items themselves
    /// Note: This is typically applied during item generation, not after
    /// </summary>
    public void ApplyRarityMultiplier(float multiplier)
    {
        if (multiplier <= 1f) return; // No change if multiplier is 1 or less
        
        // Rarity multiplier is typically applied during item generation
        // This method is here for consistency, but the actual application
        // should happen in the loot generation system
        Debug.Log($"[LootDropResult] Rarity multiplier {multiplier:F2}x noted (applied during generation)");
    }
}
