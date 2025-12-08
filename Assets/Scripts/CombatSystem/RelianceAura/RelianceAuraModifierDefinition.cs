using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Definition of a Reliance Aura modifier - ScriptableObject that defines what happens when an aura is active
/// Similar to AscendancyModifierDefinition but for auras
/// </summary>
[CreateAssetMenu(fileName = "New Reliance Aura Modifier", menuName = "Dexiled/Reliance Aura/Modifier Definition")]
public class RelianceAuraModifierDefinition : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Unique identifier for this modifier (e.g., 'PyreheartMantle_FireDamage')")]
    public string modifierId;
    
    [Tooltip("Name of the aura this modifier is linked to")]
    public string linkedAuraName;
    
    [TextArea(3, 5)]
    [Tooltip("Description of what this modifier does")]
    public string description;
    
    [Header("Effects")]
    [Tooltip("List of effects that trigger on various events")]
    public List<ModifierEffect> effects = new List<ModifierEffect>();
    
    [Header("Variables (Optional)")]
    [Tooltip("Variable keys for dynamic values")]
    public List<string> variableKeys = new List<string>();
    
    [Tooltip("Variable values (corresponds to variableKeys). Note: Unity cannot serialize List<object>, so this is stored as a workaround.")]
    [System.NonSerialized]
    public List<object> variableValues = new List<object>();
    
    [Header("Tags (Optional)")]
    [Tooltip("Tags for categorizing modifiers")]
    public List<string> tags = new List<string>();
    
    [Header("Level Scaling")]
    [Tooltip("Minimum level for this modifier to be active")]
    public int minLevel = 1;
    
    [Tooltip("Maximum level for this modifier (0 = no max)")]
    public int maxLevel = 0;
}

