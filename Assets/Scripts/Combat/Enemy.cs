using UnityEngine;

[System.Serializable]
public class Enemy
{
    [Header("Basic Information")]
    public string enemyName;
    public int maxHealth;
    public int currentHealth;
    public int baseDamage;
    public float accuracyRating = 100f; // for hit chance vs player evasion
    public float evasionRating = 0f; // reserved for future
    public EnemyRarity rarity = EnemyRarity.Normal;
    
    [Header("Combat Stats")]
    public float criticalChance = 0.05f; // 5% base
    public float criticalMultiplier = 1.5f;
    
    [Header("AI Behavior")]
    public EnemyIntent currentIntent;
    public int intentDamage;
    
    public Enemy(string name, int health, int damage)
    {
        enemyName = name;
        maxHealth = health;
        currentHealth = health;
        baseDamage = damage;
        
        // Set initial intent
        SetIntent();
    }

    public void ApplyRarityScaling(EnemyRarity r)
    {
        rarity = r;
        switch (rarity)
        {
            case EnemyRarity.Magic:
                // Slightly tougher: ~60% more life, ~20% more damage
                maxHealth = Mathf.CeilToInt(maxHealth * 1.6f);
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                baseDamage = Mathf.CeilToInt(baseDamage * 1.2f);
                break;
            case EnemyRarity.Rare:
                // Much tougher: ~150% more life, ~50% more damage
                maxHealth = Mathf.CeilToInt(maxHealth * 2.5f);
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                baseDamage = Mathf.CeilToInt(baseDamage * 1.5f);
                break;
            case EnemyRarity.Unique:
                // Boss-like: ~400% more life, 2x damage
                maxHealth = Mathf.CeilToInt(maxHealth * 5.0f);
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                baseDamage = Mathf.CeilToInt(baseDamage * 2.0f);
                break;
            case EnemyRarity.Normal:
            default:
                break;
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - Mathf.RoundToInt(damage));
        
        if (currentHealth <= 0)
        {
            Debug.Log($"{enemyName} has been defeated!");
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
    
    public int GetAttackDamage()
    {
        // Accuracy vs Player evasion (area level parity assumed elsewhere)
        var cm = CharacterManager.Instance;
        var player = cm != null ? cm.GetCurrentCharacter() : null;
        if (player != null)
        {
            float playerEvasion = Mathf.Max(0f, player.baseEvasionRating) * (1f + Mathf.Max(0f, player.increasedEvasion));
            // Area parity: if area level > player level, scale evasion down slightly; if lower, scale up slightly
            int areaLevel = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentAreaLevel() : player.level;
            float parity = Mathf.Clamp((float)player.level / Mathf.Max(1, areaLevel), 0.5f, 1.5f);
            playerEvasion *= parity;
            float chanceToHit = accuracyRating / (accuracyRating + Mathf.Max(1f, playerEvasion));
            if (Random.value >= Mathf.Clamp01(chanceToHit))
            {
                return 0; // Miss
            }
        }

        // Check for critical hit
        bool isCritical = Random.Range(0f, 1f) < criticalChance;
        float damage = baseDamage;
        
        if (isCritical)
        {
            damage *= criticalMultiplier;
            Debug.Log($"{enemyName} landed a critical hit!");
        }
        
        return Mathf.RoundToInt(damage);
    }
    
    public void SetIntent()
    {
        // Simple AI: randomly choose between attack and defend
        float random = Random.Range(0f, 1f);
        
        if (random < 0.8f) // 80% chance to attack
        {
            currentIntent = EnemyIntent.Attack;
            intentDamage = GetAttackDamage();
        }
        else // 20% chance to defend
        {
            currentIntent = EnemyIntent.Defend;
            intentDamage = 0;
        }
    }
    
    public string GetIntentDescription()
    {
        switch (currentIntent)
        {
            case EnemyIntent.Attack:
                return $"Attack ({intentDamage})";
            case EnemyIntent.Defend:
                return "Defend";
            default:
                return "Unknown";
        }
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}

public enum EnemyIntent
{
    Attack,
    Defend
}
