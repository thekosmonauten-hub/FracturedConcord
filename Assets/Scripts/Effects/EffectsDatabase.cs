using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Database of all visual effects in the game
/// </summary>
[CreateAssetMenu(fileName = "Effects Database", menuName = "Dexiled/Effects/Effects Database")]
public class EffectsDatabase : ScriptableObject
{
    [Header("All Effects")]
    public List<EffectData> allEffects = new List<EffectData>();
    
    [Header("Effect Collections (Auto-populated)")]
    public List<EffectData> impactEffects = new List<EffectData>();
    public List<EffectData> projectileEffects = new List<EffectData>();
    public List<EffectData> areaEffects = new List<EffectData>();
    
    [Header("By Damage Type")]
    public List<EffectData> fireEffects = new List<EffectData>();
    public List<EffectData> coldEffects = new List<EffectData>();
    public List<EffectData> lightningEffects = new List<EffectData>();
    public List<EffectData> chaosEffects = new List<EffectData>();
    public List<EffectData> physicalEffects = new List<EffectData>();
    
    // Singleton pattern
    private static EffectsDatabase _instance;
    public static EffectsDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try Resources folder first
                _instance = Resources.Load<EffectsDatabase>("EffectsDatabase");
                
                // If not in Resources, try to find it in the scene (assigned to CombatEffectManager)
                if (_instance == null)
                {
                    var combatEffectManager = UnityEngine.Object.FindFirstObjectByType<CombatEffectManager>();
                    if (combatEffectManager != null)
                    {
                        _instance = combatEffectManager.GetEffectsDatabase();
                    }
                }
                
                // If still not found, try FindObjectOfType as last resort
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindFirstObjectByType<EffectsDatabase>();
                }
                
                if (_instance == null)
                {
                    Debug.LogWarning("EffectsDatabase not found! Please either:\n" +
                        "1. Create EffectsDatabase asset and save it to Resources/EffectsDatabase.asset, OR\n" +
                        "2. Assign EffectsDatabase to CombatEffectManager's 'Effects Database' field in the Inspector.");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Find effect matching query
    /// </summary>
    public EffectData FindEffect(EffectQuery query)
    {
        if (query == null) return null;
        
        // Try to find exact match first
        var matches = allEffects.Where(e => e != null && e.MeetsRequirements(query)).ToList();
        
        if (matches.Count == 0)
        {
            Debug.LogWarning($"No effect found matching query: {query.effectType}, {query.category}, {query.damageType}");
            return null;
        }
        
        // If multiple matches, prefer more specific ones
        // Priority: Critical > Non-critical, then by category match
        var prioritized = matches.OrderByDescending(e => 
        {
            int priority = 0;
            if (query.isCritical && e.tags.Contains("critical")) priority += 10;
            if (e.category != EffectCategory.Any && e.category == query.category) priority += 5;
            if (e.damageType == query.damageType) priority += 3;
            return priority;
        }).ToList();
        
        return prioritized[0];
    }
    
    /// <summary>
    /// Find effect by name
    /// </summary>
    public EffectData FindEffectByName(string name)
    {
        return allEffects.FirstOrDefault(e => e != null && e.effectName == name);
    }
    
    /// <summary>
    /// Find effect by card name (for card-specific effects)
    /// </summary>
    public EffectData FindEffectByCardName(string cardName)
    {
        if (string.IsNullOrEmpty(cardName)) return null;
        
        return allEffects.FirstOrDefault(e => 
            e != null &&
            !string.IsNullOrEmpty(e.associatedCardName) && 
            e.associatedCardName.Equals(cardName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Find effect by tag and effect type (for tag-based effects like "Warcry")
    /// </summary>
    public EffectData FindEffectByTag(string tag, VisualEffectType effectType)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        
        return allEffects.FirstOrDefault(e => 
            e != null &&
            e.effectType == effectType &&
            e.tags != null &&
            e.tags.Contains(tag, System.StringComparer.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Find effects by damage type
    /// </summary>
    public List<EffectData> FindEffectsByDamageType(DamageType damageType)
    {
        return allEffects.Where(e => e != null && e.damageType == damageType).ToList();
    }
    
    /// <summary>
    /// Find projectile effect for damage type
    /// </summary>
    public EffectData FindProjectileEffect(DamageType damageType, bool isCritical = false)
    {
        var query = new EffectQuery
        {
            effectType = VisualEffectType.Projectile,
            damageType = damageType,
            isCritical = isCritical
        };
        
        return FindEffect(query);
    }
    
    /// <summary>
    /// Find impact effect for damage type
    /// </summary>
    public EffectData FindImpactEffect(DamageType damageType, bool isCritical = false)
    {
        var query = new EffectQuery
        {
            effectType = VisualEffectType.Impact,
            damageType = damageType,
            isCritical = isCritical
        };
        
        Debug.Log($"[EffectsDatabase] FindImpactEffect - Looking for: effectType={query.effectType}, damageType={damageType}, isCritical={isCritical}");
        Debug.Log($"[EffectsDatabase] Total effects in database: {allEffects.Count(e => e != null)}");
        
        var result = FindEffect(query);
        
        if (result == null)
        {
            Debug.LogWarning($"[EffectsDatabase] No impact effect found for {damageType}. Available Physical effects:");
            var physicalEffects = allEffects.Where(e => e != null && e.damageType == DamageType.Physical).ToList();
            foreach (var effect in physicalEffects)
            {
                Debug.Log($"  - {effect.effectName} (effectType: {effect.effectType}, isProjectile: {effect.isProjectile})");
            }
        }
        else
        {
            Debug.Log($"[EffectsDatabase] ✓ Found impact effect: {result.effectName}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Auto-categorize effects
    /// </summary>
    [ContextMenu("Categorize Effects")]
    public void CategorizeEffects()
    {
        impactEffects.Clear();
        projectileEffects.Clear();
        areaEffects.Clear();
        fireEffects.Clear();
        coldEffects.Clear();
        lightningEffects.Clear();
        chaosEffects.Clear();
        physicalEffects.Clear();
        
        foreach (var effect in allEffects)
        {
            if (effect == null) continue;
            
            // By type
            if (effect.effectType == VisualEffectType.Impact) impactEffects.Add(effect);
            if (effect.effectType == VisualEffectType.Projectile) projectileEffects.Add(effect);
            if (effect.effectType == VisualEffectType.Area) areaEffects.Add(effect);
            
            // By damage type
            switch (effect.damageType)
            {
                case DamageType.Fire: fireEffects.Add(effect); break;
                case DamageType.Cold: coldEffects.Add(effect); break;
                case DamageType.Lightning: lightningEffects.Add(effect); break;
                case DamageType.Chaos: chaosEffects.Add(effect); break;
                case DamageType.Physical: physicalEffects.Add(effect); break;
            }
        }
        
        Debug.Log($"Categorized {allEffects.Count(e => e != null)} effects");
    }
    
    private void OnValidate()
    {
        CategorizeEffects();
    }
}

