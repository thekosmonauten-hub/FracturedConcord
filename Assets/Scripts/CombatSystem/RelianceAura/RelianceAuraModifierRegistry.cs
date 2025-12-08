using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Registry that automatically loads all RelianceAuraModifierDefinition assets from Resources
/// Similar to AscendancyModifierRegistry
/// </summary>
public class RelianceAuraModifierRegistry : MonoBehaviour
{
    private static RelianceAuraModifierRegistry _instance;
    public static RelianceAuraModifierRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RelianceAuraModifierRegistry>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("RelianceAuraModifierRegistry");
                    _instance = go.AddComponent<RelianceAuraModifierRegistry>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Settings")]
    [Tooltip("Resources folder path for RelianceAuraModifierDefinitions (relative to Resources/). Loads recursively from subfolders.")]
    [SerializeField] private string resourcesPath = "RelianceAuraModifiers";
    
    [Header("Debug Info (Read-Only)")]
    [SerializeField] private List<RelianceAuraModifierDefinition> loadedModifiers = new List<RelianceAuraModifierDefinition>();

    private Dictionary<string, RelianceAuraModifierDefinition> modifiersById = new Dictionary<string, RelianceAuraModifierDefinition>();
    private Dictionary<string, List<RelianceAuraModifierDefinition>> modifiersByAuraName = new Dictionary<string, List<RelianceAuraModifierDefinition>>();
    
    void Awake()
    {
        // Singleton pattern
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
    
    [ContextMenu("Reload Modifiers")]
    public void ReloadModifiers()
    {
        LoadModifiers();
    }

    private void LoadModifiers()
    {
        modifiersById.Clear();
        modifiersByAuraName.Clear();
        loadedModifiers.Clear();

        // Resources.LoadAll loads recursively from subfolders
        RelianceAuraModifierDefinition[] loadedAssets = Resources.LoadAll<RelianceAuraModifierDefinition>(resourcesPath);

        if (loadedAssets != null && loadedAssets.Length > 0)
        {
            foreach (var modifier in loadedAssets)
            {
                if (modifier == null || string.IsNullOrEmpty(modifier.modifierId))
                {
                    Debug.LogWarning($"[RelianceAuraModifierRegistry] Found null or invalid modifier asset in {resourcesPath}. Skipping.");
                    continue;
                }

                if (modifiersById.ContainsKey(modifier.modifierId))
                {
                    Debug.LogWarning($"[RelianceAuraModifierRegistry] Duplicate modifierId found: {modifier.modifierId}. Overwriting with the last one loaded.");
                }
                modifiersById[modifier.modifierId] = modifier;
                loadedModifiers.Add(modifier);

                if (!string.IsNullOrEmpty(modifier.linkedAuraName))
                {
                    if (!modifiersByAuraName.ContainsKey(modifier.linkedAuraName))
                    {
                        modifiersByAuraName[modifier.linkedAuraName] = new List<RelianceAuraModifierDefinition>();
                    }
                    modifiersByAuraName[modifier.linkedAuraName].Add(modifier);
                }
            }
            Debug.Log($"[RelianceAuraModifierRegistry] Loaded {loadedModifiers.Count} Reliance Aura Modifier Definitions from Resources/{resourcesPath}");
        }
        else
        {
            Debug.LogWarning($"[RelianceAuraModifierRegistry] No Reliance Aura Modifier Definitions found in Resources/{resourcesPath}");
        }
    }
    
    /// <summary>
    /// Get a specific modifier by its unique ID.
    /// </summary>
    public RelianceAuraModifierDefinition GetModifier(string modifierId)
    {
        if (modifiersById.TryGetValue(modifierId, out RelianceAuraModifierDefinition modifier))
        {
            return modifier;
        }
        Debug.LogWarning($"[RelianceAuraModifierRegistry] Modifier with ID '{modifierId}' not found.");
        return null;
    }

    /// <summary>
    /// Get all modifiers linked to a specific aura name.
    /// </summary>
    public List<RelianceAuraModifierDefinition> GetModifiersForAura(string auraName)
    {
        if (string.IsNullOrEmpty(auraName))
        {
            return new List<RelianceAuraModifierDefinition>();
        }

        if (modifiersByAuraName.TryGetValue(auraName, out List<RelianceAuraModifierDefinition> modifiers))
        {
            return new List<RelianceAuraModifierDefinition>(modifiers);
        }

        return new List<RelianceAuraModifierDefinition>();
    }

    /// <summary>
    /// Get all loaded modifier definitions.
    /// </summary>
    public List<RelianceAuraModifierDefinition> GetAllModifiers()
    {
        return loadedModifiers;
    }

    /// <summary>
    /// Check if a modifier with the given ID exists.
    /// </summary>
    public bool HasModifier(string modifierId)
    {
        return modifiersById.ContainsKey(modifierId);
    }
    
    /// <summary>
    /// Get all active modifiers for a character based on their active auras.
    /// </summary>
    public List<RelianceAuraModifierDefinition> GetActiveModifiers(Character character)
    {
        if (character == null || character.activeRelianceAuras == null)
        {
            return new List<RelianceAuraModifierDefinition>();
        }
        
        var activeModifiers = new List<RelianceAuraModifierDefinition>();
        
        foreach (var auraName in character.activeRelianceAuras)
        {
            if (string.IsNullOrEmpty(auraName))
                continue;
            
            var modifiers = GetModifiersForAura(auraName);
            activeModifiers.AddRange(modifiers);
        }
        
        return activeModifiers;
    }
    
    /// <summary>
    /// Get all active modifier effects for a character, scaled based on aura levels.
    /// This is the main method used by the combat system to get scaled effects.
    /// </summary>
    public List<ModifierEffect> GetActiveEffects(Character character)
    {
        if (character == null || character.activeRelianceAuras == null)
        {
            return new List<ModifierEffect>();
        }
        
        var activeEffects = new List<ModifierEffect>();
        AuraExperienceManager auraExpManager = AuraExperienceManager.Instance;
        
        foreach (var auraName in character.activeRelianceAuras)
        {
            if (string.IsNullOrEmpty(auraName))
                continue;
            
            // Get aura level (defaults to 1 if not found)
            int auraLevel = 1;
            if (auraExpManager != null)
            {
                auraLevel = auraExpManager.GetAuraLevel(auraName);
            }
            
            // Get modifiers for this aura
            var modifiers = GetModifiersForAura(auraName);
            
            foreach (var modifierDef in modifiers)
            {
                if (modifierDef != null && modifierDef.effects != null)
                {
                    // Scale effects based on aura level
                    List<ModifierEffect> scaledEffects = ScaleEffectsForLevel(modifierDef.effects, auraLevel);
                    activeEffects.AddRange(scaledEffects);
                }
            }
        }
        
        return activeEffects;
    }
    
    /// <summary>
    /// Scale modifier effects based on aura level (1-20).
    /// Interpolates numeric parameters between Level 1 and Level 20 values.
    /// </summary>
    private List<ModifierEffect> ScaleEffectsForLevel(List<ModifierEffect> baseEffects, int level)
    {
        if (baseEffects == null || level <= 1)
        {
            return baseEffects ?? new List<ModifierEffect>();
        }
        
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
    /// Supports two approaches:
    /// 1. If parameter has a "_level20" version, interpolate between base and level20
    /// 2. If no level20 version, assume base is level1 and scale proportionally (for backwards compatibility)
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
                    // No level20 version - assume base is level1, scale proportionally
                    // This maintains backwards compatibility with existing modifiers
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
                // Non-numeric values are copied as-is (strings, bools, etc.)
                scaledParams.SetParameter(key, value);
            }
        }
        
        return scaledParams;
    }
}

