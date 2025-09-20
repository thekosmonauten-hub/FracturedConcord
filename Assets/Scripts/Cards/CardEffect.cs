using UnityEngine;

public enum EffectType
{
    Damage,
    Heal,
    Guard,
    Draw,
    Discard,
    ApplyStatus,
    RemoveStatus,
    GainMana,
    GainReliance,
    TemporaryStatBoost,
    PermanentStatBoost
}

[System.Serializable]
public class CardEffect
{
    [Header("Effect Information")]
    public EffectType effectType;
    public string effectName;
    public string description;
    
    [Header("Effect Values")]
    public float value = 0f;
    public int duration = 1; // For temporary effects
    public DamageType damageType = DamageType.Physical;
    
    [Header("Target Information")]
    public bool targetsSelf = true;
    public bool targetsEnemy = false;
    public bool targetsAllEnemies = false;
    public bool targetsAll = false;
    
    [Header("Conditions")]
    public string condition = ""; // e.g., "ifDiscarded", "ifCritical"
    
    // Apply the effect to a target
    public void ApplyEffect(Character caster, Character target = null)
    {
        switch (effectType)
        {
            case EffectType.Damage:
                if (target != null)
                {
                    float damage = value;
                    target.TakeDamage(damage, damageType);
                }
                break;
                
            case EffectType.Heal:
                if (target != null)
                {
                    target.Heal(value);
                }
                break;
                
            case EffectType.Guard:
                if (target != null)
                {
                    target.AddGuard(value);
                }
                break;
                
            case EffectType.Draw:
                // This would be handled by the card system
                break;
                
            case EffectType.Discard:
                // This would be handled by the card system
                break;
                
            case EffectType.GainMana:
                if (target != null)
                {
                    target.RestoreMana((int)value);
                }
                break;
                
            case EffectType.GainReliance:
                if (target != null)
                {
                    target.AddReliance((int)value);
                }
                break;
                
            case EffectType.TemporaryStatBoost:
                if (target != null)
                {
                    // Apply temporary stat boost
                    // This would need to be implemented in the Character class
                }
                break;
        }
    }
    
    // Get effect description
    public string GetEffectDescription()
    {
        string desc = description;
        
        if (!string.IsNullOrEmpty(condition))
        {
            desc = $"{condition}: {desc}";
        }
        
        return desc;
    }
}
