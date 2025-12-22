using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton registry for loading and retrieving EmbossingModifierDefinition assets
/// Similar to RelianceAuraModifierRegistry but for embossings
/// Handles level scaling for embossing modifiers (level 1-20)
/// </summary>
public class EmbossingModifierRegistry : MonoBehaviour
{
    private static EmbossingModifierRegistry _instance;
    public static EmbossingModifierRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EmbossingModifierRegistry>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EmbossingModifierRegistry");
                    _instance = go.AddComponent<EmbossingModifierRegistry>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private Dictionary<string, EmbossingModifierDefinition> modifierCache = new Dictionary<string, EmbossingModifierDefinition>();
    private bool isLoaded = false;
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadModifiers();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load all EmbossingModifierDefinition assets from Resources
    /// </summary>
    public void LoadModifiers()
    {
        if (isLoaded) return;
        
        modifierCache.Clear();
        
        EmbossingModifierDefinition[] modifiers = Resources.LoadAll<EmbossingModifierDefinition>("EmbossingModifiers");
        
        foreach (var modifier in modifiers)
        {
            if (modifier != null && !string.IsNullOrEmpty(modifier.modifierId))
            {
                modifierCache[modifier.modifierId] = modifier;
            }
        }
        
        isLoaded = true;
        Debug.Log($"[EmbossingModifierRegistry] Loaded {modifierCache.Count} Embossing Modifier Definitions from Resources/EmbossingModifiers");
    }
    
    /// <summary>
    /// Get a modifier definition by ID
    /// </summary>
    public EmbossingModifierDefinition GetModifier(string modifierId)
    {
        if (!isLoaded) LoadModifiers();
        
        if (modifierCache.TryGetValue(modifierId, out EmbossingModifierDefinition modifier))
        {
            return modifier;
        }
        
        return null;
    }
    
    /// <summary>
    /// Get all modifiers linked to a specific embossing ID
    /// </summary>
    public List<EmbossingModifierDefinition> GetModifiersForEmbossing(string embossingId)
    {
        if (!isLoaded) LoadModifiers();
        
        return modifierCache.Values
            .Where(m => m != null && m.linkedEmbossingId == embossingId)
            .ToList();
    }
    
    /// <summary>
    /// Get active effects for a card's embossings, scaled by embossing levels
    /// </summary>
    public List<ModifierEffect> GetActiveEffects(Card card, Character character = null)
    {
        if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
        {
            return new List<ModifierEffect>();
        }
        
        if (!isLoaded) LoadModifiers();
        
        List<ModifierEffect> allEffects = new List<ModifierEffect>();
        
        foreach (var embossingInstance in card.appliedEmbossings)
        {
            if (embossingInstance == null) continue;
            
            int embossingLevel = embossingInstance.level;
            
            // Get all modifiers for this embossing
            var modifiers = GetModifiersForEmbossing(embossingInstance.embossingId);
            
            foreach (var modifierDef in modifiers)
            {
                // Check level requirements
                if (embossingLevel < modifierDef.minLevel) continue;
                if (modifierDef.maxLevel > 0 && embossingLevel > modifierDef.maxLevel) continue;
                
                // Check card tag requirements
                if (modifierDef.requiredCardTags != null && modifierDef.requiredCardTags.Count > 0)
                {
                    bool tagRequirementMet = false;
                    
                    if (modifierDef.requireAllTags)
                    {
                        // AND logic: ALL tags must be present
                        tagRequirementMet = true;
                        foreach (string requiredTag in modifierDef.requiredCardTags)
                        {
                            if (card.tags == null || !card.tags.Contains(requiredTag))
                            {
                                tagRequirementMet = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // OR logic: At least ONE tag must be present (default)
                        foreach (string requiredTag in modifierDef.requiredCardTags)
                        {
                            if (card.tags != null && card.tags.Contains(requiredTag))
                            {
                                tagRequirementMet = true;
                                break;
                            }
                        }
                    }
                    
                    if (!tagRequirementMet) continue;
                }
                
                // Scale effects based on embossing level (1-20)
                if (modifierDef.effects != null && modifierDef.effects.Count > 0)
                {
                    List<ModifierEffect> scaledEffects = ScaleEffectsForLevel(modifierDef.effects, embossingLevel);
                    allEffects.AddRange(scaledEffects);
                }
            }
        }
        
        return allEffects;
    }
    
    /// <summary>
    /// Scale modifier effects based on embossing level (1-20)
    /// Uses same scaling logic as RelianceAuraModifierRegistry
    /// </summary>
    private List<ModifierEffect> ScaleEffectsForLevel(List<ModifierEffect> baseEffects, int level)
    {
        if (baseEffects == null || baseEffects.Count == 0)
            return new List<ModifierEffect>();
        
        // Clamp level to 1-20 range
        level = Mathf.Clamp(level, 1, 20);
        
        // Calculate interpolation factor (0.0 at level 1, 1.0 at level 20)
        float t = (level - 1) / 19f;
        
        var scaledEffects = new List<ModifierEffect>();
        
        foreach (var effect in baseEffects)
        {
            // Create a copy of the effect
            ModifierEffect scaledEffect = new ModifierEffect
            {
                eventType = effect.eventType,
                conditions = effect.conditions != null ? new List<ModifierCondition>(effect.conditions) : new List<ModifierCondition>(),
                actions = new List<ModifierAction>()
            };
            
            // Scale each action's parameters
            if (effect.actions != null)
            {
                foreach (var action in effect.actions)
                {
                    ModifierAction scaledAction = new ModifierAction
                    {
                        actionType = action.actionType,
                        executionOrder = action.executionOrder,
                        parameterDict = ScaleActionParameters(action.parameterDict, t)
                    };
                    scaledEffect.actions.Add(scaledAction);
                }
            }
            
            scaledEffects.Add(scaledEffect);
        }
        
        return scaledEffects;
    }
    
    /// <summary>
    /// Scale action parameters based on level interpolation factor.
    /// Supports _level20 suffix for scaling (same as RelianceAuraModifierRegistry)
    /// </summary>
    private SerializableParameterDictionary ScaleActionParameters(SerializableParameterDictionary parameters, float t)
    {
        if (parameters == null) return new SerializableParameterDictionary();
        
        var scaledParams = new SerializableParameterDictionary();
        
        // Get all parameters
        var allParams = parameters.GetAllParameters();
        
        foreach (var kvp in allParams)
        {
            string key = kvp.Key;
            object value = kvp.Value;
            
            // Skip level20 parameters (they're only used for scaling)
            if (key.EndsWith("_level20"))
            {
                continue;
            }
            
            // Try to scale numeric values
            if (value is float floatValue)
            {
                // Check if there's a level20 version
                string level20Key = key + "_level20";
                if (parameters.ContainsKey(level20Key))
                {
                    float level20Value = parameters.GetParameter<float>(level20Key);
                    float scaledValue = Mathf.Lerp(floatValue, level20Value, t);
                    scaledParams.SetParameter(key, scaledValue);
                }
                else
                {
                    // No level20 version - use base value
                    scaledParams.SetParameter(key, floatValue);
                }
            }
            else if (value is int intValue)
            {
                // Check if there's a level20 version
                string level20Key = key + "_level20";
                if (parameters.ContainsKey(level20Key))
                {
                    int level20Value = parameters.GetParameter<int>(level20Key);
                    int scaledValue = Mathf.RoundToInt(Mathf.Lerp(intValue, level20Value, t));
                    scaledParams.SetParameter(key, scaledValue);
                }
                else
                {
                    // No level20 version - use base value
                    scaledParams.SetParameter(key, intValue);
                }
            }
            else
            {
                // Non-numeric values are copied as-is
                scaledParams.SetParameter(key, value);
            }
        }
        
        return scaledParams;
    }
}

