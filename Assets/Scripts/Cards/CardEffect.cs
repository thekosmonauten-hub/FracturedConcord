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
            case EffectType.ApplyStatus:
                ApplyStatusEffect(caster, target);
                break;
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
    
    /// <summary>
    /// Apply a status effect to a target
    /// </summary>
    private void ApplyStatusEffect(Character caster, Character target)
    {
        if (target == null) return;
        
        // Create status effect based on effect name
        StatusEffectType statusType = GetStatusEffectType(effectName);
        if (statusType != StatusEffectType.Poison || effectName.ToLower() == "poison") // Check for poison specifically
        {
            StatusEffect statusEffect = new StatusEffect(statusType, effectName, value, duration);
            
            // Find the target's StatusEffectManager
            // Note: Character is a data class, not a MonoBehaviour, so we need to find the manager differently
            StatusEffectManager statusManager = null;
            
            // Try to find in display components
            PlayerCombatDisplay playerDisplay = GameObject.FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                statusManager = playerDisplay.GetStatusEffectManager();
            }
            
            // Also try to find in enemy displays
            if (statusManager == null)
            {
                EnemyCombatDisplay[] enemyDisplays = GameObject.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                foreach (var enemyDisplay in enemyDisplays)
                {
                    if (enemyDisplay != null && enemyDisplay.name.Contains(target.characterName))
                    {
                        statusManager = enemyDisplay.GetComponent<StatusEffectManager>();
                        break;
                    }
                }
            }
            
            if (statusManager != null)
            {
                statusManager.AddStatusEffect(statusEffect);
                Debug.Log($"Applied status effect {effectName} to {target.characterName}");
            }
            else
            {
                Debug.LogWarning($"No StatusEffectManager found for {target.characterName}");
            }
        }
    }
    
    /// <summary>
    /// Get StatusEffectType from effect name
    /// </summary>
    private StatusEffectType GetStatusEffectType(string effectName)
    {
        // Map effect names to status effect types
        switch (effectName.ToLower())
        {
            case "poison":
            case "poisoned":
                return StatusEffectType.Poison;
            case "burn":
            case "burning":
                return StatusEffectType.Burn;
            case "chill":
            case "chilled":
                return StatusEffectType.Chill;
            case "freeze":
            case "frozen":
                return StatusEffectType.Freeze;
            case "stun":
            case "stunned":
                return StatusEffectType.Stun;
            case "strength":
                return StatusEffectType.Strength;
            case "dexterity":
                return StatusEffectType.Dexterity;
            case "intelligence":
                return StatusEffectType.Intelligence;
            case "shield":
                return StatusEffectType.Shield;
            case "regeneration":
            case "regen":
                return StatusEffectType.Regeneration;
            case "mana regen":
            case "mana regeneration":
                return StatusEffectType.ManaRegen;
            case "vulnerable":
                return StatusEffectType.Vulnerable;
            case "weak":
                return StatusEffectType.Weak;
            case "frail":
                return StatusEffectType.Frail;
            default:
                return StatusEffectType.Poison; // Default fallback
        }
    }
}
