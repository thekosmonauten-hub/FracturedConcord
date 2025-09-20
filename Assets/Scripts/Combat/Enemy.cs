using UnityEngine;

[System.Serializable]
public class Enemy
{
    [Header("Basic Information")]
    public string enemyName;
    public int maxHealth;
    public int currentHealth;
    public int baseDamage;
    
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
