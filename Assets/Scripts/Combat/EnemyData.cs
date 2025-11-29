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
    
    [Header("Energy System")]
    [Tooltip("If true, enemy uses energy for actions (Attack: 5 energy, Defend: 15 energy)")]
    public bool enableEnergy = true; // Default to true so enemies use energy
    [Min(0f)] public float baseMaxEnergy = 100f;
    [Min(0f)] public float energyRegenPerTurn = 10f;
    [Tooltip("Energy drained per point of Chill magnitude")]
    [Min(0f)] public float chillDrainPerMagnitude = 2f;
    [Tooltip("Energy drained per point of Slow magnitude")]
    [Min(0f)] public float slowDrainPerMagnitude = 3f;
    
    [Header("Stagger System")]
    [Tooltip("Amount of stagger needed to trigger stun (0 = cannot be staggered)")]
    [Min(0f)] public float staggerThreshold = 100f;
    [Tooltip("How much stagger decays per turn (0 = no decay)")]
    [Min(0f)] public float staggerDecayPerTurn = 0f;
    
    [Header("Guard System")]
    [Tooltip("Percentage of max health gained as guard when defending (0.1 = 10%, 0.2 = 20%, etc.)")]
    [Range(0f, 1f)] public float defendGuardPercent = 0.1f; // Default 10% of max health
    
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

    [Header("Initial Stacks")]
    [Min(0)] public int initialAgitateStacks;
    [Min(0)] public int initialToleranceStacks;
    [Min(0)] public int initialPotentialStacks;
    
    [Header("Database Flags")]
    [Tooltip("If true, this enemy will not appear in random encounters. Useful for summon-only or scripted enemies.")]
    public bool excludeFromRandom = false;
    
    [Header("Visual")]
    public Color healthBarColor = Color.red;
    public RuntimeAnimatorController animatorController;
    
    /// <summary>
    /// Create an Enemy instance from this data.
    /// </summary>
    /// <param name="areaLevel">Area level for scaling (1 = no scaling, higher = stronger enemies)</param>
    public Enemy CreateEnemy(int areaLevel = 1)
    {
        int health = Random.Range(minHealth, maxHealth + 1);
        Enemy enemy = new Enemy(enemyName, health, baseDamage);
        enemy.ResetStacks();
        enemy.ConfigureEnergy(enableEnergy ? baseMaxEnergy : 0f,
                              enableEnergy ? energyRegenPerTurn : 0f,
                              chillDrainPerMagnitude,
                              slowDrainPerMagnitude);
        
        // Apply stagger system
        enemy.staggerThreshold = staggerThreshold;
        enemy.currentStagger = 0f;
        enemy.staggerDecayPerTurn = staggerDecayPerTurn;
        
        // Initialize guard system
        enemy.maxGuard = enemy.maxHealth;
        enemy.currentGuard = 0f;
        enemy.guardPersistenceFraction = 0.25f; // Same as player default
        enemy.defendGuardPercent = defendGuardPercent; // Configurable guard amount per enemy
        
        // Apply additional stats
        enemy.criticalChance = criticalChance;
        enemy.criticalMultiplier = criticalMultiplier;
        enemy.accuracyRating = accuracyRating;
        enemy.evasionRating = evasionRating;
        
        // Apply area level scaling (similar to rarity scaling)
        // Area level 1 = base stats, each level adds ~15% more health and ~10% more damage
        if (areaLevel > 1)
        {
            float healthMultiplier = 1f + 0.15f * (areaLevel - 1);
            float damageMultiplier = 1f + 0.10f * (areaLevel - 1);
            
            enemy.maxHealth = Mathf.CeilToInt(enemy.maxHealth * healthMultiplier);
            enemy.currentHealth = enemy.maxHealth;
            enemy.baseDamage = Mathf.CeilToInt(enemy.baseDamage * damageMultiplier);
        }
        
        // Store tier/rarity on name for UI if needed
        
        if (initialAgitateStacks > 0) enemy.AddStacks(StackType.Agitate, initialAgitateStacks);
        if (initialToleranceStacks > 0) enemy.AddStacks(StackType.Tolerance, initialToleranceStacks);
        if (initialPotentialStacks > 0) enemy.AddStacks(StackType.Potential, initialPotentialStacks);
        
        // TODO: Apply resistances, abilities, etc.
        
        return enemy;
    }
    
    /// <summary>
    /// Create an Enemy instance and apply a rarity scaling (PoE-like).
    /// </summary>
    /// <param name="rolledRarity">Enemy rarity for scaling</param>
    /// <param name="areaLevel">Area level for additional scaling (1 = no scaling)</param>
    public Enemy CreateEnemyWithRarity(EnemyRarity rolledRarity, int areaLevel = 1)
    {
        Enemy e = CreateEnemy(areaLevel);
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

