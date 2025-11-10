using UnityEngine;

/// <summary>
/// Represents a status effect that can be applied to characters or enemies
/// </summary>
[System.Serializable]
public class StatusEffect
{
    [Header("Basic Information")]
    public StatusEffectType effectType;
    public string effectName;
    public string description;
    
    [Header("Effect Properties")]
    public float magnitude = 0f;
    public int duration = 1; // -1 for permanent effects
    public float tickInterval = 1f; // How often the effect triggers (for damage over time, etc.)
    
    [Header("Visual Properties")]
    public Color effectColor = Color.white;
    public string iconName = "";
    
    [Header("Source Information")]
    public string sourceName = "";
    public bool isDebuff = true; // true for debuffs, false for buffs
    public DamageType damageType = DamageType.Physical; // For ticking damage/heal types
    
    // Runtime properties
    public float timeRemaining;
    public float nextTickTime;
    public bool isActive = true;
    
    public StatusEffect()
    {
        timeRemaining = duration;
        nextTickTime = tickInterval;
    }
    
    public StatusEffect(StatusEffectType type, string name, float mag, int dur, bool debuff = true)
    {
        effectType = type;
        effectName = name;
        magnitude = mag;
        duration = dur;
        isDebuff = debuff;
        timeRemaining = duration;
        nextTickTime = tickInterval;
        
        // Set default properties based on type
        SetDefaultProperties();
    }
    
    private void SetDefaultProperties()
    {
        switch (effectType)
        {
            case StatusEffectType.Poison:
                effectColor = Color.green;
                iconName = "Poison"; // Matches Poison.aseprite
                tickInterval = 1f;
                damageType = DamageType.Chaos; // Poison acts as chaos DoT
                break;
            case StatusEffectType.Burn:
                effectColor = Color.red;
                iconName = "Ignite"; // Matches Ignite.aseprite
                tickInterval = 1f;
                damageType = DamageType.Fire;
                break;
            case StatusEffectType.Bleed:
                effectColor = new Color(0.7f, 0f, 0f);
                iconName = "Bleed";
                tickInterval = 1f;
                damageType = DamageType.Physical;
                isDebuff = true;
                break;
            case StatusEffectType.ChaosDot:
                effectColor = new Color(0.5f, 0.2f, 0.6f); // Purple-ish
                iconName = "ChaosDot"; // Provide icon if available
                tickInterval = 1f;
                damageType = DamageType.Chaos;
                isDebuff = true;
                break;
            case StatusEffectType.Chill:
                effectColor = new Color(0.6f, 0.9f, 1f);
                iconName = "Chilled";
                tickInterval = 0f;
                break;
            case StatusEffectType.Freeze:
                effectColor = Color.cyan;
                iconName = "Chilled"; // Reuse chilled icon for freeze if dedicated asset missing
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Stun:
                effectColor = Color.yellow;
                iconName = "Stunned"; // Matches Stunned.aseprite
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Vulnerable:
                effectColor = Color.magenta;
                iconName = "Vulnerability"; // Matches Vulnerability.aseprite
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Strength:
                effectColor = Color.red;
                iconName = "TempStrength"; // Matches TempStrength.aseprite
                isDebuff = false;
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Dexterity:
                effectColor = Color.green;
                iconName = "TempAgility"; // Matches TempAgility.aseprite
                isDebuff = false;
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Intelligence:
                effectColor = Color.blue;
                iconName = "TempIntelligence"; // Matches TempIntelligence.aseprite
                isDebuff = false;
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Shield:
                effectColor = new Color(0.5f, 0.8f, 1f);
                iconName = "Shielded"; // Matches Shielded.aseprite
                isDebuff = false;
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.Crumble:
                effectColor = new Color(0.6f, 0.5f, 0.8f);
                iconName = "Crumble"; // Provide an icon if available
                isDebuff = true;
                tickInterval = 0f; // No ticking; consumed by shout or expires
                break;
            case StatusEffectType.Bolster:
                effectColor = new Color(0.4f, 0.8f, 0.6f);
                iconName = "Bolster"; // Provide an icon if available
                isDebuff = false;
                tickInterval = 0f; // No ticking
                break;
            case StatusEffectType.TempMaxMana:
                effectColor = new Color(0.3f, 0.6f, 1f);
                iconName = "TempMana";
                isDebuff = false;
                tickInterval = 0f;
                break;
            case StatusEffectType.TempEvasion:
                effectColor = new Color(0.3f, 0.85f, 0.6f);
                iconName = "TempEvasion";
                isDebuff = false;
                tickInterval = 0f;
                break;
        }
    }
    
    /// <summary>
    /// Update the status effect (called each frame for ticking effects)
    /// </summary>
    public void UpdateEffect(float deltaTime)
    {
        if (!isActive) return;
        
        // Handle ticking effects that occur during turns (like poison damage)
        if (tickInterval > 0f)
        {
            nextTickTime -= deltaTime;
        }
    }
    
    /// <summary>
    /// Advance the status effect by one turn
    /// </summary>
    public void AdvanceTurn()
    {
        if (!isActive) return;
        
        // Reduce duration by 1 turn
        if (duration > 0) // Only if not permanent
        {
            timeRemaining -= 1f;
            Debug.Log($"<color=yellow>AdvanceTurn: {effectName} - Duration remaining: {timeRemaining}/{duration}</color>");
            
            if (timeRemaining <= 0f)
            {
                isActive = false;
                Debug.Log($"<color=red>{effectName} expired!</color>");
                return;
            }
        }
        
        // Reset tick timer for next turn
        if (tickInterval > 0f)
        {
            nextTickTime = tickInterval;
        }
    }
    
    /// <summary>
    /// Check if the effect should tick this frame
    /// </summary>
    public bool ShouldTick()
    {
        return isActive && tickInterval > 0f && nextTickTime <= 0f;
    }
    
    /// <summary>
    /// Reset the tick timer
    /// </summary>
    public void ResetTick()
    {
        nextTickTime = tickInterval;
    }
    
    /// <summary>
    /// Get the current magnitude of the effect (can be modified by other effects)
    /// </summary>
    public float GetCurrentMagnitude()
    {
        return magnitude;
    }
    
    /// <summary>
    /// Create a copy of this status effect
    /// </summary>
    public StatusEffect Clone()
    {
        StatusEffect clone = new StatusEffect(effectType, effectName, magnitude, duration, isDebuff);
        clone.description = description;
        clone.effectColor = effectColor;
        clone.iconName = iconName;
        clone.sourceName = sourceName;
        clone.tickInterval = tickInterval;
        return clone;
    }
}

/// <summary>
/// Types of status effects available in the game
/// </summary>
public enum StatusEffectType
{
    // Debuffs
    Poison,         // Damage over time
    Burn,           // Fire damage over time
    Bleed,          // Physical damage over time
    ChaosDot,       // Chaos damage over time
    Chill,          // Action speed reduction / partial freeze
    Freeze,         // Skip next turn
    Stun,           // Skip next turn
    Vulnerable,     // Take increased damage
    Weak,           // Deal reduced damage
    Frail,          // Take increased damage from attacks
    Slow,           // Reduced action speed
    Crumble,        // Stores portion of physical damage dealt; consumed by Shout
    
    // Buffs
    Strength,       // Increased physical damage
    Dexterity,      // Increased accuracy and evasion
    Intelligence,   // Increased spell damage
    Shield,         // Damage absorption
    Bolster,        // Less damage taken per stack (2% per stack)
    Block,          // Chance to block attacks
    Evasion,        // Chance to avoid attacks
    Regeneration,   // Health over time
    ManaRegen,      // Mana over time
    Draw,           // Extra cards drawn
    Energy,         // Extra energy/mana
    TempMaxMana,    // Temporary maximum mana increase
    TempEvasion,    // Temporary increased evasion
    
    // Special
    Invisible,      // Cannot be targeted
    Invulnerable,   // Cannot take damage
    Confused,       // Random actions
    Charmed,        // Temporary ally
    Curse,          // Various negative effects
}
