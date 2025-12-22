using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a visual effect with all its properties and requirements
/// </summary>
[CreateAssetMenu(fileName = "New Effect", menuName = "Dexiled/Effects/Effect Data")]
public class EffectData : ScriptableObject
{
    [Header("Basic Info")]
    public string effectName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Card Association")]
    [Tooltip("If set, this effect will be used when this specific card is played")]
    public string associatedCardName = "";
    
    [Header("Effect Type")]
    public VisualEffectType effectType;
    public EffectCategory category;
    
    [Header("Damage Type (for elemental effects)")]
    public DamageType damageType = DamageType.Physical;
    
    [Header("Effect Prefab")]
    [Tooltip("The prefab containing ParticleSystem and/or other visual components")]
    public GameObject effectPrefab;
    
    [Header("UI Particle Settings")]
    [Tooltip("If true, automatically adds UIParticle component to prefab")]
    public bool useUIParticle = true;
    [Tooltip("Scale multiplier for UI particles")]
    public float uiParticleScale = 1f;
    
    [Header("Projectile Settings")]
    [Tooltip("Is this a projectile effect?")]
    public bool isProjectile = false;
    [Tooltip("Speed for projectile effects")]
    public float projectileSpeed = 800f;
    [Tooltip("Use arc trajectory for projectiles")]
    public bool useArcTrajectory = true;
    [Tooltip("Arc height for curved projectiles")]
    public float arcHeight = 50f;
    
    [Header("Impact Settings")]
    [Tooltip("Should this effect play an impact effect on arrival?")]
    public bool hasImpactEffect = false;
    [Tooltip("Impact effect to play (if different from this effect)")]
    public EffectData impactEffect;
    
    [Header("Timing")]
    public float duration = 2f;
    public float delay = 0f;
    
    [Header("Requirements")]
    [Tooltip("What conditions must be met to use this effect")]
    public List<EffectRequirement> requirements = new List<EffectRequirement>();
    
    [Header("Tags")]
    [Tooltip("Tags for filtering and searching effects")]
    public List<string> tags = new List<string>();
    
    /// <summary>
    /// Check if this effect meets the given requirements
    /// </summary>
    public bool MeetsRequirements(EffectQuery query)
    {
        if (query == null) return true;
        
        // Check effect type
        if (query.effectType != VisualEffectType.Any && query.effectType != effectType)
            return false;
        
        // Check category
        if (query.category != EffectCategory.Any && query.category != category)
            return false;
        
        // Check damage type
        if (query.damageType != DamageType.None && query.damageType != damageType)
            return false;
        
        // Check tags
        if (query.requiredTags != null && query.requiredTags.Count > 0)
        {
            foreach (string tag in query.requiredTags)
            {
                if (!tags.Contains(tag))
                    return false;
            }
        }
        
        // Check custom requirements
        foreach (var req in requirements)
        {
            if (!req.IsMet(query))
                return false;
        }
        
        return true;
    }
}

/// <summary>
/// Types of visual effects
/// </summary>
public enum VisualEffectType
{
    Any,
    Impact,      // Static effect at location
    Projectile,  // Traveling effect
    Area,        // Area of effect
    Buff,        // Positive effect
    Debuff,      // Negative effect
    Heal,
    Damage
}

/// <summary>
/// Categories for organizing effects
/// </summary>
public enum EffectCategory
{
    Any,
    Physical,
    Fire,
    Cold,
    Lightning,
    Chaos,
    Critical,
    Status,
    Projectile,
    Melee,
    Spell
}

/// <summary>
/// Requirements for an effect
/// </summary>
[System.Serializable]
public class EffectRequirement
{
    public RequirementType type;
    public string value;
    
    public bool IsMet(EffectQuery query)
    {
        if (query == null) return true;
        
        switch (type)
        {
            case RequirementType.IsCritical:
                return query.isCritical;
            case RequirementType.HasStatusEffect:
                // Implement status effect checking if needed
                return true;
            case RequirementType.MinDamage:
                if (float.TryParse(value, out float minDmg))
                    return query.minDamage >= minDmg;
                return true;
            case RequirementType.MaxDamage:
                if (float.TryParse(value, out float maxDmg))
                    return query.maxDamage <= maxDmg;
                return true;
            default:
                return true;
        }
    }
}

public enum RequirementType
{
    None,
    IsCritical,
    HasStatusEffect,
    MinDamage,
    MaxDamage
}

/// <summary>
/// Query for finding effects
/// </summary>
[System.Serializable]
public class EffectQuery
{
    public VisualEffectType effectType = VisualEffectType.Any;
    public EffectCategory category = EffectCategory.Any;
    public DamageType damageType = DamageType.None;
    public List<string> requiredTags = new List<string>();
    public bool isCritical = false;
    public float minDamage = 0f;
    public float maxDamage = float.MaxValue;
}

