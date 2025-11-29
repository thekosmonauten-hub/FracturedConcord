using UnityEngine;

/// <summary>
/// Factory class for creating status effects with proper source damage calculations
/// </summary>
public static class StatusEffectFactory
{
    /// <summary>
    /// Create a Bleeding status effect from physical damage
    /// Base duration: 5 turns
    /// </summary>
    public static StatusEffect CreateBleeding(float physicalDamage, int duration = 5)
    {
        float magnitude = StatusEffectManager.CalculateBleedingMagnitude(physicalDamage);
        StatusEffect effect = StatusEffect.CreateWithSourceDamage(
            StatusEffectType.Bleed,
            "Bleed",
            magnitude,
            duration,
            true,
            physicalDamage
        );
        return effect;
    }
    
    /// <summary>
    /// Create a Poison status effect from physical and/or chaos damage
    /// Base duration: 2 turns
    /// </summary>
    public static StatusEffect CreatePoison(float physicalDamage, float chaosDamage, int duration = 2)
    {
        float magnitude = StatusEffectManager.CalculatePoisonMagnitude(physicalDamage, chaosDamage);
        StatusEffect effect = StatusEffect.CreateWithSourceDamage(
            StatusEffectType.Poison,
            "Poison",
            magnitude,
            duration,
            true,
            physicalDamage,
            0f,
            0f,
            0f,
            chaosDamage
        );
        return effect;
    }
    
    /// <summary>
    /// Create an Ignite status effect from fire damage
    /// Base duration: 4 turns, tickInterval: 1 turn
    /// </summary>
    public static StatusEffect CreateIgnite(float fireDamage, int duration = 4)
    {
        float magnitude = StatusEffectManager.CalculateIgniteMagnitude(fireDamage);
        StatusEffect effect = StatusEffect.CreateWithSourceDamage(
            StatusEffectType.Burn,
            "Ignite",
            magnitude,
            duration,
            true,
            0f,
            fireDamage
        );
        effect.tickInterval = 1f; // Per turn
        return effect;
    }
    
    /// <summary>
    /// Create a Chilled status effect from cold damage
    /// Base duration: 2 turns
    /// </summary>
    public static StatusEffect CreateChilled(float coldDamage, int duration = 2)
    {
        StatusEffect effect = StatusEffect.CreateWithSourceDamage(
            StatusEffectType.Chill,
            "Chilled",
            0f, // No magnitude for Chilled
            duration,
            true,
            0f,
            0f,
            coldDamage
        );
        effect.energyGainReduction = StatusEffectManager.CalculateChilledEnergyReduction(coldDamage);
        return effect;
    }
    
    /// <summary>
    /// Create a Frozen status effect from cold damage
    /// Duration: 1-2 turns based on cold damage as percentage of target's max HP
    /// </summary>
    public static StatusEffect CreateFrozen(float coldDamage, float targetMaxHP)
    {
        int duration = StatusEffectManager.CalculateFrozenDuration(coldDamage, targetMaxHP);
        StatusEffect effect = StatusEffect.CreateWithSourceDamage(
            StatusEffectType.Freeze,
            "Frozen",
            0f, // No magnitude for Frozen
            duration,
            true,
            0f,
            0f,
            coldDamage
        );
        return effect;
    }
    
    /// <summary>
    /// Create a Shocked status effect from lightning damage
    /// Base duration: 2 turns
    /// </summary>
    public static StatusEffect CreateShocked(float lightningDamage, int duration = 2)
    {
        StatusEffect effect = StatusEffect.CreateWithSourceDamage(
            StatusEffectType.Shocked,
            "Shocked",
            0f, // No magnitude for Shocked
            duration,
            true,
            0f,
            0f,
            0f,
            lightningDamage
        );
        effect.damageMultiplier = StatusEffectManager.CalculateShockedDamageMultiplier(lightningDamage);
        return effect;
    }
    
    /// <summary>
    /// Create a Stagger status effect
    /// Base duration: 1 turn
    /// </summary>
    public static StatusEffect CreateStagger(int duration = 1)
    {
        StatusEffect effect = new StatusEffect(
            StatusEffectType.Stagger,
            "Stagger",
            0f, // No magnitude for Stagger
            duration,
            true
        );
        return effect;
    }
    
    /// <summary>
    /// Create a Vulnerability status effect
    /// Consumed after one damage instance
    /// </summary>
    public static StatusEffect CreateVulnerability(int duration = 1)
    {
        StatusEffect effect = new StatusEffect(
            StatusEffectType.Vulnerable,
            "Vulnerability",
            0f, // No magnitude for Vulnerability
            duration,
            true
        );
        effect.vulnerabilityConsumed = false;
        return effect;
    }
    
    /// <summary>
    /// Create a Chaos DoT status effect
    /// </summary>
    public static StatusEffect CreateChaosDot(float magnitude, int duration)
    {
        StatusEffect effect = new StatusEffect(
            StatusEffectType.ChaosDot,
            "Chaos DoT",
            magnitude,
            duration,
            true
        );
        return effect;
    }
    
    /// <summary>
    /// Create a Crumble status effect (stores physical damage)
    /// </summary>
    public static StatusEffect CreateCrumble(float storedDamage, int duration)
    {
        StatusEffect effect = new StatusEffect(
            StatusEffectType.Crumble,
            "Crumble",
            storedDamage, // Stored damage amount
            duration,
            true
        );
        return effect;
    }
    
    /// <summary>
    /// Create a Bolster status effect (damage reduction)
    /// </summary>
    public static StatusEffect CreateBolster(float stacks, int duration)
    {
        // Cap at 10 stacks
        stacks = Mathf.Min(stacks, 10f);
        StatusEffect effect = new StatusEffect(
            StatusEffectType.Bolster,
            "Bolster",
            stacks,
            duration,
            false // Buff
        );
        return effect;
    }
}

