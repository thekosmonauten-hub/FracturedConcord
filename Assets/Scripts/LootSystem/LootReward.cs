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
    Card
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
    
    // Experience reward
    public int experienceAmount;
    
    // Card reward (future)
    public string cardName;
    
    // Display info
    public string GetDisplayName()
    {
        switch (rewardType)
        {
            case RewardType.Currency:
                return $"{currencyAmount}x {currencyType}";
            case RewardType.Item:
                return itemData != null ? itemData.itemName : "Unknown Item";
            case RewardType.Experience:
                return $"{experienceAmount} Experience";
            case RewardType.Card:
                return cardName;
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
            case RewardType.Experience:
                // TODO: Add experience icon
                return null;
            case RewardType.Card:
                // TODO: Add card icon
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
        rewards.Add(new LootReward
        {
            rewardType = RewardType.Item,
            itemData = itemData
        });
    }
    
    public int GetRewardCount()
    {
        return rewards.Count;
    }
}
