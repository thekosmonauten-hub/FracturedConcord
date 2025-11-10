using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines enemy stats, abilities, and behavior.
/// Similar to CardData but for enemies.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "Dexiled/Combat/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    [TextArea(2, 4)]
    public string description = "An enemy";
    public Sprite enemySprite;
    
    [Header("Enemy Type")]
    public EnemyTier tier = EnemyTier.Normal;
    public EnemyRarity rarity = EnemyRarity.Normal;
    public EnemyCategory category = EnemyCategory.Melee;
    
    [Header("Base Stats")]
    public int minHealth = 30;
    public int maxHealth = 50;
    public int baseDamage = 6;
    public int baseArmor = 0;
    
    [Header("Combat Stats")]
    [Range(0f, 1f)]
    public float criticalChance = 0.05f;
    public float criticalMultiplier = 1.5f;
    [Range(0f, 1f)]
    public float dodgeChance = 0f;
    [Header("Accuracy/Evasion")]
    public float accuracyRating = 100f;
    public float evasionRating = 0f;
    
    [Header("Resistances (%)")]
    [Range(-100f, 100f)]
    public float physicalResistance = 0f;
    [Range(-100f, 100f)]
    public float fireResistance = 0f;
    [Range(-100f, 100f)]
    public float coldResistance = 0f;
    [Range(-100f, 100f)]
    public float lightningResistance = 0f;
    [Range(-100f, 100f)]
    public float chaosResistance = 0f;
    
    [Header("Display")]
    [Range(0.25f, 3f)] public float displayScale = 1f;
    [Tooltip("Base panel height before scaling; used with LayoutElement.preferredHeight")] public float basePanelHeight = 280f;
    
    [Header("AI Behavior")]
    public EnemyAIPattern aiPattern = EnemyAIPattern.Aggressive;
    [Range(0f, 1f)]
    public float attackPreference = 0.8f; // 80% attack, 20% defend
    public List<string> specialAbilities = new List<string>();

    [Header("Abilities (Scriptable)")]
    public List<EnemyAbility> abilities = new List<EnemyAbility>();
    
    [Header("Summons")]
    [Tooltip("Preferred minions this enemy can summon. Used by Summon abilities and for final-wave co-spawns.")]
    public List<EnemyData> summonPool = new List<EnemyData>();
    
    [Header("Enemy Tags")]
    [Tooltip("Tags that determine spirit drops. Enemies with 'Fire' tag have 3% chance to drop Fire Spirit, etc.")]
    public List<EnemySpiritTag> spiritTags = new List<EnemySpiritTag>();
    
    [Tooltip("If true, this enemy guarantees a spirit drop matching its tags (e.g., Fire Pixie always drops Fire Spirit)")]
    public bool guaranteedSpiritDrop = false;
    
    [Header("Loot")]
    public int minGoldDrop = 5;
    public int maxGoldDrop = 15;
    public int experienceReward = 10; // legacy, superseded by XP formula
    [Range(0f, 1f)]
    public float cardDropChance = 0.1f; // 10% chance to drop a card
    
    [Header("Database Flags")]
    [Tooltip("If true, this enemy will not appear in random encounters. Useful for summon-only or scripted enemies.")]
    public bool excludeFromRandom = false;
    
    [Header("Visual")]
    public Color healthBarColor = Color.red;
    public RuntimeAnimatorController animatorController;
    
    /// <summary>
    /// Create an Enemy instance from this data.
    /// </summary>
    public Enemy CreateEnemy()
    {
        int health = Random.Range(minHealth, maxHealth + 1);
        Enemy enemy = new Enemy(enemyName, health, baseDamage);
        
        // Apply additional stats
        enemy.criticalChance = criticalChance;
        enemy.criticalMultiplier = criticalMultiplier;
        enemy.accuracyRating = accuracyRating;
        enemy.evasionRating = evasionRating;
        // Store tier/rarity on name for UI if needed
        
        // TODO: Apply resistances, abilities, etc.
        
        return enemy;
    }
    
    /// <summary>
    /// Create an Enemy instance and apply a rarity scaling (PoE-like).
    /// </summary>
    public Enemy CreateEnemyWithRarity(EnemyRarity rolledRarity)
    {
        Enemy e = CreateEnemy();
        e.ApplyRarityScaling(rolledRarity);
        return e;
    }
    
    /// <summary>
    /// Get random gold drop amount.
    /// </summary>
    public int GetGoldDrop()
    {
        return Random.Range(minGoldDrop, maxGoldDrop + 1);
    }
    
    /// <summary>
    /// Check if this enemy should drop a card.
    /// </summary>
    public bool ShouldDropCard()
    {
        return Random.Range(0f, 1f) < cardDropChance;
    }
}

[System.Serializable]
public enum EnemyTier
{
    Normal,      // Regular enemies
    Elite,       // Tougher enemies with better loot
    Boss,        // Major encounters
    Miniboss     // Mid-tier bosses
}

[System.Serializable]
public enum EnemyCategory
{
    Melee,       // Close-range attackers
    Ranged,      // Archers, gunners
    Caster,      // Magic users
    Tank,        // High HP, low damage
    Support,     // Buffs other enemies
    Swarm        // Weak but numerous
}

[System.Serializable]
public enum EnemyAIPattern
{
    Aggressive,  // Always attacks
    Defensive,   // Prefers blocking/buffing
    Balanced,    // Mix of attack and defend
    Tactical,    // Uses abilities strategically
    Reactive     // Responds to player actions
}

/// <summary>
/// Enemy spirit tags that determine spirit currency drops
/// </summary>
[System.Serializable]
public enum EnemySpiritTag
{
    Fire,        // Drops Fire Spirit (3% or guaranteed)
    Cold,        // Drops Cold Spirit (3% or guaranteed)
    Lightning,   // Drops Lightning Spirit (3% or guaranteed)
    Chaos,       // Drops Chaos Spirit (3% or guaranteed)
    Physical,    // Drops Physical Spirit (3% or guaranteed)
    Life,        // Drops Life Spirit (3% or guaranteed)
    Defense,     // Drops Defense Spirit (3% or guaranteed)
    Crit         // Drops Crit Spirit (3% or guaranteed)
}

