using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Manages loot generation and reward distribution
/// </summary>
public class LootManager : MonoBehaviour
{
    private static LootManager _instance;
    public static LootManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LootManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("LootManager");
                    _instance = go.AddComponent<LootManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Default Loot Tables")]
    [Tooltip("Default loot table for encounters without specific tables")]
    public LootTable defaultLootTable;
    
    [Header("Currency Storage")]
    private Dictionary<CurrencyType, int> playerCurrencies = new Dictionary<CurrencyType, int>();
    
    private CurrencyDatabase currencyDatabase;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeCurrencySystem();
    }

    private void InitializeCurrencySystem()
    {
        // Load currency database
        currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        if (currencyDatabase == null)
        {
            Debug.LogWarning("[LootManager] CurrencyDatabase not found in Resources folder!");
        }
        
        // Load saved currencies from CharacterManager
        LoadPlayerCurrencies();
    }

    /// <summary>
    /// Generate loot from a loot table
    /// </summary>
    public LootDropResult GenerateLoot(LootTable lootTable, int areaLevel = 1, List<EnemyData> defeatedEnemies = null)
    {
        if (lootTable == null)
        {
            Debug.LogWarning("[LootManager] No loot table provided, using default");
            lootTable = defaultLootTable;
        }
        
        if (lootTable == null)
        {
            Debug.LogError("[LootManager] No loot table available!");
            return new LootDropResult();
        }
        
        return lootTable.GenerateLoot(areaLevel, defeatedEnemies);
    }

    /// <summary>
    /// Apply rewards to the player
    /// </summary>
    public void ApplyRewards(LootDropResult lootResult)
    {
        if (lootResult == null || lootResult.rewards.Count == 0)
        {
            Debug.Log("[LootManager] No rewards to apply");
            return;
        }

        CharacterManager charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[LootManager] No character to apply rewards to!");
            return;
        }

        Character character = charManager.GetCurrentCharacter();

        foreach (var reward in lootResult.rewards)
        {
            switch (reward.rewardType)
            {
                case RewardType.Currency:
                    AddCurrency(reward.currencyType, reward.currencyAmount);
                    break;
                    
                case RewardType.Experience:
                    character.AddExperience(reward.experienceAmount);
                    Debug.Log($"[LootManager] Awarded {reward.experienceAmount} experience");
                    break;
                    
                case RewardType.Item:
                    // TODO: Add item to inventory
                    Debug.Log($"[LootManager] Awarded item: {reward.itemData?.itemName}");
                    break;
                    
                case RewardType.Card:
                    // TODO: Add card to collection
                    Debug.Log($"[LootManager] Awarded card: {reward.cardName}");
                    break;
            }
        }
        
        // Save currencies
        SavePlayerCurrencies();
        
        // Auto-save character
        charManager.SaveCharacter();
        
        Debug.Log($"[LootManager] Applied {lootResult.rewards.Count} rewards to {character.characterName}");
    }

    /// <summary>
    /// Add currency to player's stash
    /// </summary>
    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        if (!playerCurrencies.ContainsKey(currencyType))
        {
            playerCurrencies[currencyType] = 0;
        }
        
        playerCurrencies[currencyType] += amount;
        Debug.Log($"[LootManager] Added {amount}x {currencyType}. Total: {playerCurrencies[currencyType]}");
    }

    /// <summary>
    /// Get player's currency amount
    /// </summary>
    public int GetCurrencyAmount(CurrencyType currencyType)
    {
        if (playerCurrencies.ContainsKey(currencyType))
        {
            return playerCurrencies[currencyType];
        }
        return 0;
    }

    /// <summary>
    /// Remove currency from player (for spending)
    /// </summary>
    public bool RemoveCurrency(CurrencyType currencyType, int amount)
    {
        if (!playerCurrencies.ContainsKey(currencyType))
        {
            return false;
        }
        
        if (playerCurrencies[currencyType] < amount)
        {
            return false;
        }
        
        playerCurrencies[currencyType] -= amount;
        SavePlayerCurrencies();
        return true;
    }

    /// <summary>
    /// Get all player currencies
    /// </summary>
    public Dictionary<CurrencyType, int> GetAllCurrencies()
    {
        return new Dictionary<CurrencyType, int>(playerCurrencies);
    }

    /// <summary>
    /// Load player currencies from save data
    /// </summary>
    private void LoadPlayerCurrencies()
    {
        // TODO: Load from save system
        // For now, initialize with 0 for all currencies
        if (currencyDatabase != null)
        {
            foreach (var currencyData in currencyDatabase.currencies)
            {
                if (!playerCurrencies.ContainsKey(currencyData.currencyType))
                {
                    playerCurrencies[currencyData.currencyType] = 0;
                }
            }
        }
        
        Debug.Log($"[LootManager] Loaded {playerCurrencies.Count} currency types");
    }

    /// <summary>
    /// Save player currencies to save data
    /// </summary>
    private void SavePlayerCurrencies()
    {
        // TODO: Save to persistent storage
        // For now, just keep in memory
        Debug.Log($"[LootManager] Saved {playerCurrencies.Count} currency types");
    }
}
