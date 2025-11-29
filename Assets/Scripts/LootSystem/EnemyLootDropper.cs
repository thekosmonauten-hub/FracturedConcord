using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Handles per-enemy loot drops during combat (immediate drops, not end-of-combat)
/// </summary>
public class EnemyLootDropper : MonoBehaviour
{
    private static EnemyLootDropper _instance;
    public static EnemyLootDropper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EnemyLootDropper>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EnemyLootDropper");
                    _instance = go.AddComponent<EnemyLootDropper>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Generate loot drops for a single defeated enemy
    /// </summary>
    public List<LootReward> GenerateEnemyLoot(EnemyData enemyData, int areaLevel = 1)
    {
        List<LootReward> drops = new List<LootReward>();
        
        if (enemyData == null)
            return drops;
        
        // Roll for spirit drops based on tags (3% or guaranteed)
        if (enemyData.spiritTags != null && enemyData.spiritTags.Count > 0)
        {
            foreach (var tag in enemyData.spiritTags)
            {
                bool shouldDrop = false;
                
                if (enemyData.guaranteedSpiritDrop)
                {
                    shouldDrop = true;
                    Debug.Log($"[Enemy Loot] Guaranteed spirit drop from {enemyData.enemyName}");
                }
                else
                {
                    float roll = Random.Range(0f, 100f);
                    shouldDrop = roll <= 3f;
                    if (shouldDrop)
                    {
                        Debug.Log($"[Enemy Loot] Spirit drop rolled: {roll:F2}% <= 3% SUCCESS");
                    }
                }
                
                if (shouldDrop)
                {
                    CurrencyType spiritType = GetSpiritForTag(tag);
                    drops.Add(new LootReward
                    {
                        rewardType = RewardType.Currency,
                        currencyType = spiritType,
                        currencyAmount = 1
                    });
                }
            }
        }
        
        // TODO: Add random currency drops (Orbs) based on enemy tier/rarity
        // TODO: Add item drops based on enemy loot table
        
        return drops;
    }
    
    /// <summary>
    /// Apply loot drops immediately to player
    /// </summary>
    public void ApplyImmediateDrops(List<LootReward> drops)
    {
        if (drops == null || drops.Count == 0)
            return;
        
        LootManager lootManager = LootManager.Instance;
        if (lootManager == null)
        {
            Debug.LogError("[EnemyLootDropper] LootManager not found!");
            return;
        }
        
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[EnemyLootDropper] No character to apply drops to!");
            return;
        }
        
        Character character = charManager.GetCurrentCharacter();
        
        LootDropResult tempResult = new LootDropResult();
        tempResult.rewards.AddRange(drops);
        lootManager.ApplyRewards(tempResult);
    }
    
    private CurrencyType GetSpiritForTag(EnemySpiritTag tag)
    {
        switch (tag)
        {
            case EnemySpiritTag.Fire:
                return CurrencyType.FireSpirit;
            case EnemySpiritTag.Cold:
                return CurrencyType.ColdSpirit;
            case EnemySpiritTag.Lightning:
                return CurrencyType.LightningSpirit;
            case EnemySpiritTag.Chaos:
                return CurrencyType.ChaosSpirit;
            case EnemySpiritTag.Physical:
                return CurrencyType.PhysicalSpirit;
            case EnemySpiritTag.Life:
                return CurrencyType.LifeSpirit;
            case EnemySpiritTag.Defense:
                return CurrencyType.DefenseSpirit;
            case EnemySpiritTag.Crit:
                return CurrencyType.CritSpirit;
            default:
                return CurrencyType.FireSpirit;
        }
    }
}













