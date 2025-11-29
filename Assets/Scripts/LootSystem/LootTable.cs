using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Defines what can drop from an enemy or encounter
/// </summary>
[System.Serializable]
public class LootEntry
{
    [Header("Drop Settings")]
    [Tooltip("Chance to drop this entry (0-100%)")]
    [Range(0, 100)]
    public float dropChance = 50f;
    
    [Tooltip("Minimum quantity to drop")]
    public int minQuantity = 1;
    
    [Tooltip("Maximum quantity to drop")]
    public int maxQuantity = 1;
    
    [Header("Reward Type")]
    public RewardType rewardType;
    
    [Header("Currency (if RewardType = Currency)")]
    public CurrencyType currencyType;
    
    [Header("Item (if RewardType = Item)")]
    public ItemData itemData;
    
    [Header("Card (if RewardType = Card)")]
    public string cardName;
    
    /// <summary>
    /// Roll to see if this entry drops
    /// </summary>
    public bool RollDrop()
    {
        return Random.Range(0f, 100f) <= dropChance;
    }
    
    /// <summary>
    /// Get the quantity that drops
    /// </summary>
    public int GetDropQuantity()
    {
        return Random.Range(minQuantity, maxQuantity + 1);
    }
}

/// <summary>
/// ScriptableObject that defines loot drops for encounters or enemies
/// </summary>
[CreateAssetMenu(fileName = "New Loot Table", menuName = "Dexiled/Loot System/Loot Table", order = 0)]
public class LootTable : ScriptableObject
{
    [Header("Loot Table Settings")]
    [Tooltip("Name of this loot table")]
    public string tableName = "Default Loot Table";
    
    [Header("Database References")]
    [Tooltip("Currency database for currency drops")]
    public CurrencyDatabase currencyDatabase;
    
    [Tooltip("Item database for item drops")]
    public ItemDatabase itemDatabase;
    
    [Header("Base Experience")]
    [Tooltip("Base experience awarded (scales with area level)")]
    public int baseExperience = 50;
    
    [Tooltip("Experience multiplier per area level")]
    public float experiencePerLevel = 10f;
    
    [Header("Guaranteed Currency Drops")]
    [Tooltip("Currency that always drops")]
    public List<LootEntry> guaranteedCurrencyDrops = new List<LootEntry>();
    
    [Header("Random Currency Drops")]
    [Tooltip("Currency that has a chance to drop")]
    public List<LootEntry> randomCurrencyDrops = new List<LootEntry>();
    
    [Header("Item Drops")]
    [Tooltip("Items that can drop")]
    public List<LootEntry> itemDrops = new List<LootEntry>();
    
    [Header("Card Drops")]
    [Tooltip("Cards that can drop (future)")]
    public List<LootEntry> cardDrops = new List<LootEntry>();
    
    /// <summary>
    /// Generate loot drops from this table
    /// </summary>
    public LootDropResult GenerateLoot(int areaLevel = 1, List<EnemyData> defeatedEnemies = null)
    {
        LootDropResult result = new LootDropResult();
        
        // Add experience (scaled by area level)
        int experienceAmount = Mathf.RoundToInt(baseExperience + (experiencePerLevel * (areaLevel - 1)));
        result.AddExperience(experienceAmount);
        
        // Process guaranteed currency drops
        foreach (var entry in guaranteedCurrencyDrops)
        {
            if (entry.rewardType == RewardType.Currency)
            {
                int quantity = entry.GetDropQuantity();
                result.AddCurrency(entry.currencyType, quantity);
            }
        }
        
        // Process random currency drops
        foreach (var entry in randomCurrencyDrops)
        {
            if (entry.RollDrop() && entry.rewardType == RewardType.Currency)
            {
                int quantity = entry.GetDropQuantity();
                result.AddCurrency(entry.currencyType, quantity);
            }
        }
        
        // Process item drops
        foreach (var entry in itemDrops)
        {
            if (entry.RollDrop() && entry.rewardType == RewardType.Item && entry.itemData != null)
            {
                result.AddItem(entry.itemData, entry.itemData != null ? entry.itemData.sourceItem : null);
            }
        }
        
        // Process card drops (future)
        foreach (var entry in cardDrops)
        {
            if (entry.RollDrop() && entry.rewardType == RewardType.Card && !string.IsNullOrEmpty(entry.cardName))
            {
                result.AddReward(new LootReward
                {
                    rewardType = RewardType.Card,
                    cardName = entry.cardName
                });
            }
        }
        
        // Process enemy tag-based spirit drops
        if (defeatedEnemies != null && defeatedEnemies.Count > 0)
        {
            ProcessEnemySpiritTagDrops(result, defeatedEnemies);
        }
        
        return result;
    }
    
    /// <summary>
    /// Process spirit drops based on defeated enemy tags
    /// </summary>
    private void ProcessEnemySpiritTagDrops(LootDropResult result, List<EnemyData> defeatedEnemies)
    {
        Dictionary<EnemySpiritTag, int> tagCounts = new Dictionary<EnemySpiritTag, int>();
        Dictionary<EnemySpiritTag, bool> guaranteedTags = new Dictionary<EnemySpiritTag, bool>();
        
        // Count tags and check for guaranteed drops
        foreach (var enemyData in defeatedEnemies)
        {
            if (enemyData == null || enemyData.spiritTags == null) continue;
            
            foreach (var tag in enemyData.spiritTags)
            {
                // Count this tag
                if (!tagCounts.ContainsKey(tag))
                    tagCounts[tag] = 0;
                tagCounts[tag]++;
                
                // Check for guaranteed drop
                if (enemyData.guaranteedSpiritDrop)
                {
                    guaranteedTags[tag] = true;
                }
            }
        }
        
        // Process each tag for spirit drops
        foreach (var kvp in tagCounts)
        {
            EnemySpiritTag tag = kvp.Key;
            int count = kvp.Value;
            bool isGuaranteed = guaranteedTags.ContainsKey(tag) && guaranteedTags[tag];
            
            // Convert tag to currency type
            Dexiled.Data.Items.CurrencyType spiritType = GetSpiritForTag(tag);
            
            // Roll for drop
            if (isGuaranteed)
            {
                // Guaranteed drop (e.g., from Pixies)
                result.AddCurrency(spiritType, 1);
                Debug.Log($"[Loot] Guaranteed spirit drop: {spiritType} (from enemy with {tag} tag)");
            }
            else
            {
                // 3% chance per enemy with this tag
                for (int i = 0; i < count; i++)
                {
                    if (Random.Range(0f, 100f) <= 3f)
                    {
                        result.AddCurrency(spiritType, 1);
                        Debug.Log($"[Loot] Random spirit drop (3% chance): {spiritType} (from enemy with {tag} tag)");
                        break; // Only drop once per tag type
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Convert enemy spirit tag to currency type
    /// </summary>
    private Dexiled.Data.Items.CurrencyType GetSpiritForTag(EnemySpiritTag tag)
    {
        switch (tag)
        {
            case EnemySpiritTag.Fire:
                return Dexiled.Data.Items.CurrencyType.FireSpirit;
            case EnemySpiritTag.Cold:
                return Dexiled.Data.Items.CurrencyType.ColdSpirit;
            case EnemySpiritTag.Lightning:
                return Dexiled.Data.Items.CurrencyType.LightningSpirit;
            case EnemySpiritTag.Chaos:
                return Dexiled.Data.Items.CurrencyType.ChaosSpirit;
            case EnemySpiritTag.Physical:
                return Dexiled.Data.Items.CurrencyType.PhysicalSpirit;
            case EnemySpiritTag.Life:
                return Dexiled.Data.Items.CurrencyType.LifeSpirit;
            case EnemySpiritTag.Defense:
                return Dexiled.Data.Items.CurrencyType.DefenseSpirit;
            case EnemySpiritTag.Crit:
                return Dexiled.Data.Items.CurrencyType.CritSpirit;
            default:
                return Dexiled.Data.Items.CurrencyType.FireSpirit; // Fallback
        }
    }
}
